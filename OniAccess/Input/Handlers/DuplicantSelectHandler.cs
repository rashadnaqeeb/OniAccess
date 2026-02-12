using System.Collections.Generic;
using HarmonyLib;

namespace OniAccess.Input.Handlers
{
    /// <summary>
    /// Handler for MinionSelectScreen (initial colony start duplicant selection)
    /// and Printing Pod selection (recurring every 3 cycles).
    ///
    /// The screen has 3 duplicant slots (CharacterContainer instances). Tab/Shift+Tab
    /// switches between slots. Within each slot, Up/Down navigates a flat list of:
    /// name, interests, traits (with full info), attributes, interest filter dropdown,
    /// and reroll button.
    ///
    /// Per Pitfall 4: CharacterContainer inherits KScreen but is NOT pushed to
    /// KScreenManager -- ContextDetector ignores it. Navigation is handled entirely
    /// within this handler.
    ///
    /// Per locked decisions:
    /// - Traits: full info upfront (name, effect, description all spoken together)
    /// - Attributes: one per arrow press ("Athletics 3")
    /// - After reroll: speak new name, interests, and traits automatically
    /// - Shift+I re-reads the full trait/attribute info (same as normal navigation)
    /// </summary>
    public class DuplicantSelectHandler : BaseMenuHandler
    {
        private int _currentSlot;
        private UnityEngine.Component[] _containers;
        private bool _pendingRerollAnnounce;

        public override string DisplayName => STRINGS.ONIACCESS.HANDLERS.DUPLICANT_SELECT;

        public override IReadOnlyList<HelpEntry> HelpEntries { get; }

        public DuplicantSelectHandler(KScreen screen) : base(screen)
        {
            _currentSlot = 0;
            var entries = new List<HelpEntry>();
            entries.AddRange(CommonHelpEntries);
            entries.AddRange(MenuHelpEntries);
            entries.AddRange(ListNavHelpEntries);
            entries.Add(new HelpEntry("Tab/Shift+Tab", STRINGS.ONIACCESS.HELP.SWITCH_DUPE_SLOT));
            HelpEntries = entries;
        }

        // ========================================
        // TAB NAVIGATION (switch between dupe slots)
        // ========================================

        protected override void NavigateTabForward()
        {
            if (_containers == null || _containers.Length == 0) return;
            int prev = _currentSlot;
            _currentSlot = (_currentSlot + 1) % _containers.Length;
            if (_currentSlot == 0) PlayWrapSound();

            RediscoverAndSpeakSlot();
        }

        protected override void NavigateTabBackward()
        {
            if (_containers == null || _containers.Length == 0) return;
            int prev = _currentSlot;
            _currentSlot = (_currentSlot - 1 + _containers.Length) % _containers.Length;
            if (_currentSlot == _containers.Length - 1 && prev == 0) PlayWrapSound();

            RediscoverAndSpeakSlot();
        }

        /// <summary>
        /// Rediscover widgets for the current slot and speak the dupe name + first widget.
        /// </summary>
        private void RediscoverAndSpeakSlot()
        {
            DiscoverWidgets(_screen);
            _currentIndex = 0;
            if (_widgets.Count > 0)
            {
                Speech.SpeechPipeline.SpeakInterrupt(GetWidgetSpeechText(_widgets[0]));
                if (_widgets.Count > 1)
                {
                    Speech.SpeechPipeline.SpeakQueued(GetWidgetSpeechText(_widgets[1]));
                }
            }
        }

        // ========================================
        // WIDGET DISCOVERY
        // ========================================

        public override void DiscoverWidgets(KScreen screen)
        {
            _widgets.Clear();

            // Find CharacterContainer instances
            // CharacterContainer inherits KScreen but is used as a child component
            _containers = screen.GetComponentsInChildren<CharacterContainer>(true);
            if (_containers == null || _containers.Length == 0) return;

            // Clamp slot index
            if (_currentSlot >= _containers.Length) _currentSlot = 0;
            var container = _containers[_currentSlot] as CharacterContainer;
            if (container == null) return;

            // Only process active containers
            if (!container.gameObject.activeInHierarchy) return;

            DiscoverSlotWidgets(container);
        }

        /// <summary>
        /// Build the widget list for a single CharacterContainer slot.
        /// Order: name, interests, traits (one per), attributes (one per),
        /// interest filter dropdown, reroll button.
        /// </summary>
        private void DiscoverSlotWidgets(CharacterContainer container)
        {
            var traverse = Traverse.Create(container);

            // (a) Name: get from characterNameTitle or EditableTitleBar
            DiscoverNameWidget(container, traverse);

            // (b) Interests: combined as one label
            DiscoverInterestsWidget(container, traverse);

            // (c) Traits: one per trait, with full info (name + effect + description)
            DiscoverTraitWidgets(container, traverse);

            // (d) Attributes: one per attribute
            DiscoverAttributeWidgets(container, traverse);

            // (e) Interest filter dropdown (archetypeDropDown)
            DiscoverFilterDropdown(container, traverse);

            // (f) Reroll button (reshuffleButton)
            DiscoverRerollButton(container, traverse);
        }

        /// <summary>
        /// Discover the duplicant name widget from the CharacterContainer.
        /// Tries characterNameTitle (EditableTitleBar) first, then falls back
        /// to searching for LocText children with a name-like pattern.
        /// </summary>
        private void DiscoverNameWidget(CharacterContainer container, Traverse traverse)
        {
            string name = null;
            UnityEngine.GameObject nameGo = null;

            // Try EditableTitleBar.text via characterNameTitle field
            try
            {
                var titleBar = traverse.Field("characterNameTitle").GetValue<object>();
                if (titleBar != null)
                {
                    var titleBarTraverse = Traverse.Create(titleBar);
                    // EditableTitleBar stores its text in a LocText child
                    var locText = titleBarTraverse.Field("titleText")
                        .GetValue<LocText>();
                    if (locText != null && !string.IsNullOrEmpty(locText.text))
                    {
                        name = locText.text;
                        nameGo = locText.gameObject;
                    }
                }
            }
            catch (System.Exception)
            {
                // Field may not exist or be a different type
            }

            // Fallback: look for any prominent LocText that might be the name
            if (string.IsNullOrEmpty(name))
            {
                try
                {
                    var nameTitle = traverse.Field("nameTitle")
                        .GetValue<LocText>();
                    if (nameTitle != null && !string.IsNullOrEmpty(nameTitle.text))
                    {
                        name = nameTitle.text;
                        nameGo = nameTitle.gameObject;
                    }
                }
                catch (System.Exception) { }
            }

            if (!string.IsNullOrEmpty(name))
            {
                _widgets.Add(new WidgetInfo
                {
                    Label = name,
                    Component = null,
                    Type = WidgetType.Label,
                    GameObject = nameGo ?? container.gameObject
                });
            }
        }

        /// <summary>
        /// Discover the interests/aptitudes label. Combines all interests into
        /// one label: "Interests: Research, Cooking".
        /// </summary>
        private void DiscoverInterestsWidget(CharacterContainer container, Traverse traverse)
        {
            try
            {
                // CharacterContainer stores aptitude/interest info.
                // Try to find aptitude label text from the container hierarchy.
                var aptitudeLabel = traverse.Field("aptitudeLabel")
                    .GetValue<LocText>();
                if (aptitudeLabel != null && !string.IsNullOrEmpty(aptitudeLabel.text))
                {
                    _widgets.Add(new WidgetInfo
                    {
                        Label = aptitudeLabel.text,
                        Component = null,
                        Type = WidgetType.Label,
                        GameObject = aptitudeLabel.gameObject
                    });
                    return;
                }
            }
            catch (System.Exception) { }

            // Fallback: look for interest entries in the container hierarchy
            try
            {
                var aptitudeEntries = traverse.Field("aptitudeEntries")
                    .GetValue<System.Collections.IList>();
                if (aptitudeEntries != null && aptitudeEntries.Count > 0)
                {
                    var interestNames = new List<string>();
                    foreach (var entry in aptitudeEntries)
                    {
                        var entryTraverse = Traverse.Create(entry);
                        var locText = entryTraverse.Field("label")
                            .GetValue<LocText>();
                        if (locText != null && !string.IsNullOrEmpty(locText.text))
                        {
                            interestNames.Add(locText.text);
                        }
                    }

                    if (interestNames.Count > 0)
                    {
                        string combined = "Interests: " + string.Join(", ", interestNames);
                        _widgets.Add(new WidgetInfo
                        {
                            Label = combined,
                            Component = null,
                            Type = WidgetType.Label,
                            GameObject = container.gameObject
                        });
                    }
                }
            }
            catch (System.Exception) { }
        }

        /// <summary>
        /// Discover trait widgets. Each trait gets its own widget with full info:
        /// name + effect + description combined into one label.
        /// Per locked decision: "Traits: full info upfront."
        /// </summary>
        private void DiscoverTraitWidgets(CharacterContainer container, Traverse traverse)
        {
            // Traits are dynamically created LocText elements in the CharacterContainer.
            // They have ToolTip components with the full description.
            // Find the trait container/panel and walk its LocText children.
            try
            {
                // Try traitEntries field first
                var traitEntries = traverse.Field("traitEntries")
                    .GetValue<System.Collections.IList>();
                if (traitEntries != null)
                {
                    foreach (var entry in traitEntries)
                    {
                        AddTraitWidget(entry);
                    }
                    return;
                }
            }
            catch (System.Exception) { }

            // Fallback: look for a traits panel and walk its LocText children
            try
            {
                var traitPanel = traverse.Field("traitsPanel")
                    .GetValue<UnityEngine.GameObject>();
                if (traitPanel == null)
                {
                    traitPanel = traverse.Field("traitsList")
                        .GetValue<UnityEngine.GameObject>();
                }
                if (traitPanel != null)
                {
                    var locTexts = traitPanel.GetComponentsInChildren<LocText>(false);
                    foreach (var lt in locTexts)
                    {
                        if (lt == null || string.IsNullOrEmpty(lt.text)) continue;
                        AddTraitFromLocText(lt);
                    }
                }
            }
            catch (System.Exception) { }
        }

        /// <summary>
        /// Add a trait widget from a trait entry object, combining LocText and ToolTip.
        /// </summary>
        private void AddTraitWidget(object entry)
        {
            if (entry == null) return;

            try
            {
                var entryTraverse = Traverse.Create(entry);

                // Try to get LocText from the entry
                LocText locText = null;
                UnityEngine.GameObject go = null;

                // Entry might be a GameObject or a component
                if (entry is UnityEngine.GameObject entryGo)
                {
                    locText = entryGo.GetComponentInChildren<LocText>();
                    go = entryGo;
                }
                else if (entry is UnityEngine.Component entryComp)
                {
                    locText = entryComp.GetComponentInChildren<LocText>();
                    go = entryComp.gameObject;
                }
                else
                {
                    // Try to get a label field
                    locText = entryTraverse.Field("label").GetValue<LocText>();
                    if (locText != null) go = locText.gameObject;
                }

                if (locText != null)
                {
                    AddTraitFromLocText(locText);
                }
            }
            catch (System.Exception) { }
        }

        /// <summary>
        /// Build a composite trait label from a LocText (name + effect) and its ToolTip
        /// (description). Result: "Mole Hands, +2 Digging, Moves through tiles faster".
        /// If tooltip is too long, truncates to first sentence.
        /// </summary>
        private void AddTraitFromLocText(LocText locText)
        {
            if (locText == null || string.IsNullOrEmpty(locText.text)) return;

            string traitText = locText.text.Trim();
            string tooltipText = null;

            // Get ToolTip description for the full trait info
            var tooltip = locText.GetComponent<ToolTip>();
            if (tooltip != null)
            {
                try
                {
                    tooltipText = tooltip.GetMultiString(0);
                }
                catch (System.Exception) { }
            }

            // Build composite label
            string label;
            if (!string.IsNullOrEmpty(tooltipText))
            {
                // Truncate tooltip to first sentence if too long
                string trimmedTooltip = TruncateToFirstSentence(tooltipText, 120);
                label = $"{traitText}, {trimmedTooltip}";
            }
            else
            {
                label = traitText;
            }

            _widgets.Add(new WidgetInfo
            {
                Label = label,
                Component = null,
                Type = WidgetType.Label,
                GameObject = locText.gameObject
            });
        }

        /// <summary>
        /// Truncate text to the first sentence if it exceeds maxLength.
        /// </summary>
        private static string TruncateToFirstSentence(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength) return text;

            int periodIdx = text.IndexOf('.');
            if (periodIdx > 0 && periodIdx < maxLength)
            {
                return text.Substring(0, periodIdx + 1);
            }

            return text.Substring(0, maxLength);
        }

        /// <summary>
        /// Discover attribute widgets. Each attribute gets its own widget.
        /// Reads the LocText text directly (e.g., "+3 Athletics").
        /// Shift+I reads the full description from ToolTip.
        /// </summary>
        private void DiscoverAttributeWidgets(CharacterContainer container, Traverse traverse)
        {
            // Attributes are in iconGroups -- dynamically created with LocText
            try
            {
                // Try to find attribute entries via various field names
                var iconGroups = traverse.Field("iconGroups")
                    .GetValue<System.Collections.IList>();
                if (iconGroups != null)
                {
                    foreach (var group in iconGroups)
                    {
                        AddAttributeFromGroup(group);
                    }
                    return;
                }
            }
            catch (System.Exception) { }

            // Fallback: look for attributeLabels or similar
            try
            {
                var attrLabels = traverse.Field("attributeLabels")
                    .GetValue<LocText[]>();
                if (attrLabels != null)
                {
                    foreach (var lt in attrLabels)
                    {
                        if (lt == null || string.IsNullOrEmpty(lt.text)) continue;
                        _widgets.Add(new WidgetInfo
                        {
                            Label = lt.text,
                            Component = null,
                            Type = WidgetType.Label,
                            GameObject = lt.gameObject
                        });
                    }
                    return;
                }
            }
            catch (System.Exception) { }

            // Fallback: look for an attributes panel
            try
            {
                var attrPanel = traverse.Field("attributesPanel")
                    .GetValue<UnityEngine.GameObject>();
                if (attrPanel == null)
                {
                    attrPanel = traverse.Field("attributesList")
                        .GetValue<UnityEngine.GameObject>();
                }
                if (attrPanel != null)
                {
                    var locTexts = attrPanel.GetComponentsInChildren<LocText>(false);
                    foreach (var lt in locTexts)
                    {
                        if (lt == null || string.IsNullOrEmpty(lt.text)) continue;
                        _widgets.Add(new WidgetInfo
                        {
                            Label = lt.text,
                            Component = null,
                            Type = WidgetType.Label,
                            GameObject = lt.gameObject
                        });
                    }
                }
            }
            catch (System.Exception) { }
        }

        /// <summary>
        /// Extract attribute info from an iconGroup entry.
        /// </summary>
        private void AddAttributeFromGroup(object group)
        {
            if (group == null) return;
            try
            {
                LocText locText = null;
                UnityEngine.GameObject go = null;

                if (group is UnityEngine.GameObject groupGo)
                {
                    locText = groupGo.GetComponentInChildren<LocText>();
                    go = groupGo;
                }
                else if (group is UnityEngine.Component groupComp)
                {
                    locText = groupComp.GetComponentInChildren<LocText>();
                    go = groupComp.gameObject;
                }
                else
                {
                    var groupTraverse = Traverse.Create(group);
                    locText = groupTraverse.Field("label").GetValue<LocText>();
                    if (locText != null) go = locText.gameObject;
                }

                if (locText != null && !string.IsNullOrEmpty(locText.text))
                {
                    _widgets.Add(new WidgetInfo
                    {
                        Label = locText.text,
                        Component = null,
                        Type = WidgetType.Label,
                        GameObject = go ?? locText.gameObject
                    });
                }
            }
            catch (System.Exception) { }
        }

        /// <summary>
        /// Discover the interest filter dropdown (archetypeDropDown).
        /// Left/Right cycles the filter options.
        /// </summary>
        private void DiscoverFilterDropdown(CharacterContainer container, Traverse traverse)
        {
            try
            {
                var dropdown = traverse.Field("archetypeDropDown")
                    .GetValue<UnityEngine.Component>();
                if (dropdown != null && dropdown.gameObject.activeInHierarchy)
                {
                    // Get current dropdown label
                    string label = "Interest filter";
                    var locText = dropdown.GetComponentInChildren<LocText>();
                    if (locText != null && !string.IsNullOrEmpty(locText.text))
                    {
                        label = locText.text;
                    }

                    _widgets.Add(new WidgetInfo
                    {
                        Label = label,
                        Component = dropdown,
                        Type = WidgetType.Dropdown,
                        GameObject = dropdown.gameObject
                    });
                }
            }
            catch (System.Exception) { }
        }

        /// <summary>
        /// Discover the reroll/reshuffle button. Only added if present and active
        /// (Printing Pod variant may not have reroll).
        /// </summary>
        private void DiscoverRerollButton(CharacterContainer container, Traverse traverse)
        {
            try
            {
                var reshuffleButton = traverse.Field("reshuffleButton")
                    .GetValue<KButton>();
                if (reshuffleButton != null && reshuffleButton.gameObject.activeInHierarchy
                    && reshuffleButton.isInteractable)
                {
                    _widgets.Add(new WidgetInfo
                    {
                        Label = "Reroll",
                        Component = reshuffleButton,
                        Type = WidgetType.Button,
                        GameObject = reshuffleButton.gameObject
                    });
                }
            }
            catch (System.Exception) { }
        }

        // ========================================
        // WIDGET ACTIVATION (Enter key)
        // ========================================

        /// <summary>
        /// Override to handle reroll button post-click: after rerolling, rediscover
        /// widgets and speak the new dupe name, interests, and traits.
        /// </summary>
        protected override void ActivateCurrentWidget()
        {
            if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
            var widget = _widgets[_currentIndex];

            if (widget.Type == WidgetType.Button && widget.Label == "Reroll")
            {
                // Click the reroll button
                var kbutton = widget.Component as KButton;
                kbutton?.SignalClick(KKeyCode.Mouse0);

                // Set flag for pending re-announcement
                // The CharacterContainer updates synchronously on click in most cases,
                // but if async, we check on next HandleUnboundKey
                _pendingRerollAnnounce = true;
                AnnounceAfterReroll();
            }
            else
            {
                base.ActivateCurrentWidget();
            }
        }

        /// <summary>
        /// After reroll, rediscover widgets and announce the new duplicant.
        /// Per decision: "speak new dupe name, interests, and traits."
        /// </summary>
        private void AnnounceAfterReroll()
        {
            DiscoverWidgets(_screen);
            _currentIndex = 0;

            // Speak name, interests, and traits (the first few Label widgets)
            bool first = true;
            for (int i = 0; i < _widgets.Count; i++)
            {
                var w = _widgets[i];
                // Stop speaking at first non-Label widget (attributes come after traits,
                // and the user navigates to read those manually per decision)
                if (w.Type != WidgetType.Label
                    && w.Type != WidgetType.Dropdown
                    && w.Type != WidgetType.Button)
                {
                    break;
                }

                // Speak name, interests, traits but stop before attributes
                // Name and interests are first, then traits -- detect the transition
                // by checking if we've hit the attributes section.
                // Since attributes come after traits in widget order, we simply
                // speak until we see something that looks like an attribute value
                // (starts with + or - and a digit). Or more simply: speak all Label
                // items until we encounter the first attribute-style text.
                if (w.Type == WidgetType.Label && LooksLikeAttribute(w.Label))
                {
                    break;
                }

                if (first)
                {
                    Speech.SpeechPipeline.SpeakInterrupt(GetWidgetSpeechText(w));
                    first = false;
                }
                else
                {
                    Speech.SpeechPipeline.SpeakQueued(GetWidgetSpeechText(w));
                }
            }

            _pendingRerollAnnounce = false;
        }

        /// <summary>
        /// Heuristic: does this label look like an attribute value?
        /// Attributes typically have format like "+3 Athletics" or "Athletics 3".
        /// </summary>
        private static bool LooksLikeAttribute(string label)
        {
            if (string.IsNullOrEmpty(label)) return false;
            // If it starts with + or - followed by a digit, it's an attribute
            if (label.Length >= 2 && (label[0] == '+' || label[0] == '-')
                && char.IsDigit(label[1]))
            {
                return true;
            }
            return false;
        }

        // ========================================
        // UNBOUND KEY HANDLING
        // ========================================

        /// <summary>
        /// Check for pending reroll announcement on next key press.
        /// </summary>
        public override bool HandleUnboundKey(UnityEngine.KeyCode keyCode)
        {
            if (_pendingRerollAnnounce)
            {
                AnnounceAfterReroll();
            }
            return base.HandleUnboundKey(keyCode);
        }
    }
}
