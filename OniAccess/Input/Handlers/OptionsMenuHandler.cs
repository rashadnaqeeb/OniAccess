using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using OniAccess.Util;
using UnityEngine.UI;

namespace OniAccess.Input.Handlers
{
    /// <summary>
    /// Handler for options screens: OptionsMenuScreen (top-level menu),
    /// AudioOptionsScreen, GraphicsOptionsScreen, and GameOptionsScreen.
    ///
    /// OptionsMenuScreen inherits KModalButtonMenu and uses the buttons array pattern.
    /// Sub-screens (Audio, Graphics, Game) inherit KModalScreen and contain sliders,
    /// toggles, and buttons discovered via GetComponentsInChildren.
    ///
    /// Display name is computed from the screen type on activation.
    /// BaseMenuHandler already handles slider speech ("label, value"),
    /// toggle speech ("label, on/off"), and adjustment via Left/Right.
    /// </summary>
    public class OptionsMenuHandler : BaseMenuHandler
    {
        private string _displayName;

        public override string DisplayName => _displayName ?? (string)STRINGS.ONIACCESS.HANDLERS.OPTIONS;

        public override IReadOnlyList<HelpEntry> HelpEntries { get; }

        public OptionsMenuHandler(KScreen screen) : base(screen)
        {
            var entries = new List<HelpEntry>();
            entries.AddRange(CommonHelpEntries);
            entries.AddRange(MenuHelpEntries);
            entries.AddRange(ListNavHelpEntries);
            HelpEntries = entries;
        }

        private UnityEngine.Coroutine _rediscoverCoroutine;

        /// <summary>
        /// Labels that are generic button text, not meaningful toggle descriptions.
        /// Used to reject HierRef Label ref text that's bleeding through from Done/Close buttons.
        /// </summary>
        private static readonly HashSet<string> _ambiguousLabels = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            "Done", "Close", "OK", "Cancel", "Apply", "Back", "Yes", "No"
        };

        /// <summary>
        /// Maps sliders to their game-managed value display LocText (e.g., "Camera Pan Speed: 100%").
        /// Used by FormatSliderValue to read the game's formatted value after stripping it from the label.
        /// </summary>
        private readonly Dictionary<KSlider, LocText> _sliderValueLabels = new Dictionary<KSlider, LocText>();

        /// <summary>
        /// Represents a group of HierRef radio toggles collapsed into a single cycleable widget.
        /// </summary>
        private class RadioGroupInfo
        {
            public List<RadioMember> Members;
            public int CurrentIndex;
        }

        private class RadioMember
        {
            public string Label;
            public KButton Button;
            public HierarchyReferences HierRef;
        }

        public override void OnActivate()
        {
            _displayName = GetDisplayNameForScreen(_screen);
            base.OnActivate();

            // Sub-screens need deferred re-discovery: OnSpawn (Unity Start) hasn't
            // run yet during Activate, so labels aren't set and UIPool controls
            // don't exist. Wait one frame for OnSpawn to complete, then re-discover.
            string screenTypeName = _screen.GetType().Name;
            if (screenTypeName != "OptionsMenuScreen")
            {
                _rediscoverCoroutine = _screen.StartCoroutine(RediscoverAfterFrame());
            }
        }

        public override void OnDeactivate()
        {
            if (_rediscoverCoroutine != null)
            {
                _screen.StopCoroutine(_rediscoverCoroutine);
                _rediscoverCoroutine = null;
            }
            base.OnDeactivate();
        }

        private IEnumerator RediscoverAfterFrame()
        {
            yield return null; // wait one frame for OnSpawn to complete
            _rediscoverCoroutine = null;

            int before = _widgets.Count;
            DiscoverWidgets(_screen);
            Log.Debug($"Rediscovery: {before} -> {_widgets.Count} widgets");

            _currentIndex = 0;
            if (_widgets.Count > 0)
            {
                Speech.SpeechPipeline.SpeakInterrupt(
                    $"{DisplayName}, {_widgets.Count} items. {GetWidgetSpeechText(_widgets[0])}");
            }
        }

        public override void DiscoverWidgets(KScreen screen)
        {
            _widgets.Clear();
            _sliderValueLabels.Clear();

            string screenTypeName = screen.GetType().Name;

            // OptionsMenuScreen is a KModalButtonMenu -- use buttons array pattern
            if (screenTypeName == "OptionsMenuScreen")
            {
                DiscoverButtonMenuWidgets(screen);
            }
            else
            {
                // Audio, Graphics, Game options sub-screens: discover sliders, toggles, buttons
                DiscoverOptionWidgets(screen);
            }
        }

        /// <summary>
        /// Discover widgets for KModalButtonMenu-derived screens (OptionsMenuScreen).
        /// Uses the buttons array and buttonObjects pattern.
        /// </summary>
        private void DiscoverButtonMenuWidgets(KScreen screen)
        {
            var buttons = Traverse.Create(screen).Field("buttons")
                .GetValue<System.Collections.IList>();
            var buttonObjects = Traverse.Create(screen).Field("buttonObjects")
                .GetValue<UnityEngine.GameObject[]>();

            if (buttons == null || buttonObjects == null) return;

            int count = System.Math.Min(buttons.Count, buttonObjects.Length);
            for (int i = 0; i < count; i++)
            {
                if (buttonObjects[i] == null || !buttonObjects[i].activeInHierarchy) continue;

                var kbutton = buttonObjects[i].GetComponent<KButton>();
                if (kbutton == null || !kbutton.isInteractable) continue;

                string label = Traverse.Create(buttons[i]).Field("text")
                    .GetValue<string>();
                if (string.IsNullOrEmpty(label)) continue;

                _widgets.Add(new WidgetInfo
                {
                    Label = label,
                    Component = kbutton,
                    Type = WidgetType.Button,
                    GameObject = buttonObjects[i]
                });
            }
        }

        /// <summary>
        /// Discover widgets for options sub-screens (Audio, Graphics, Game).
        /// Finds sliders, toggles (KToggle, MultiToggle, HierarchyReferences pattern),
        /// dropdowns, and buttons with associated labels.
        /// Filters out mouse-only controls (drag handles, resize handles, scrollbars).
        /// </summary>
        private void DiscoverOptionWidgets(KScreen screen)
        {
            string screenName = screen.GetType().Name;
            Log.Debug($"DiscoverOptionWidgets: {screenName}");

            // Track KButtons already captured as part of HierarchyReferences toggles
            // so the KButton loop below doesn't duplicate them as plain buttons.
            var hierToggleButtons = new HashSet<KButton>();

            // ------------------------------------------------------------------
            // 1. HierarchyReferences-based toggles (KButton + CheckMark pattern)
            //    Used by AudioOptionsScreen (alwaysPlayMusic, alwaysPlayAutomation,
            //    muteOnFocusLost) and GameOptionsScreen (defaultToCloudSaveToggle).
            //    Must run before KButton discovery so these get toggle semantics.
            // ------------------------------------------------------------------
            var hierRefs = screen.GetComponentsInChildren<HierarchyReferences>(true);
            Log.Debug($"  HierarchyReferences found: {hierRefs.Length}");
            foreach (var hr in hierRefs)
            {
                if (hr == null) { Log.Debug($"    skip: null"); continue; }
                if (!hr.gameObject.activeInHierarchy) { Log.Debug($"    skip inactive: {hr.gameObject.name}"); continue; }

                // Must have a CheckMark/Checkmark reference to qualify as a toggle
                bool hasCheck = hr.HasReference("CheckMark") || hr.HasReference("Checkmark");
                if (!hasCheck)
                {
                    Log.Debug($"    skip {hr.gameObject.name}: no CheckMark/Checkmark ref");
                    continue;
                }

                // Find the KButton: prefer named "Button" reference, fall back to child search
                // (GameOptionsScreen's toggles use GetComponentInChildren instead of a named ref)
                KButton kbutton = null;
                if (hr.HasReference("Button"))
                {
                    var btnRef = hr.GetReference("Button");
                    if (btnRef != null) kbutton = btnRef.gameObject.GetComponent<KButton>();
                }
                if (kbutton == null)
                    kbutton = hr.GetComponentInChildren<KButton>(true);
                if (kbutton == null) { Log.Debug($"    skip {hr.gameObject.name}: no KButton found"); continue; }

                // Resolve label: Label ref → FindWidgetLabel → GameObject name
                string label = null;
                if (hr.HasReference("Label"))
                {
                    var labelRef = hr.GetReference("Label");
                    var locText = labelRef as LocText;
                    if (locText == null && labelRef != null)
                        locText = labelRef.gameObject.GetComponent<LocText>();
                    if (locText != null)
                    {
                        label = CleanLabel(locText.text);
                        Log.Debug($"    Label ref text='{locText.text}' cleaned='{label}' for {hr.gameObject.name}");
                    }
                }
                // Reject ambiguous labels (e.g. "Done" bleeding from close button)
                if (label != null && _ambiguousLabels.Contains(label))
                {
                    Log.Debug($"    Label '{label}' is ambiguous for {hr.gameObject.name}, trying alternatives");
                    label = null;
                }
                if (label == null)
                {
                    label = FindWidgetLabel(hr.gameObject);
                    // Same ambiguous check on the fallback result
                    if (label != null && _ambiguousLabels.Contains(label))
                    {
                        Log.Debug($"    FindWidgetLabel also returned ambiguous '{label}', trying name");
                        label = null;
                    }
                }
                if (label == null)
                    label = LabelFromGameObjectName(hr.gameObject.name);
                if (label == null)
                {
                    Log.Debug($"    skip {hr.gameObject.name}: no label");
                    continue;
                }

                hierToggleButtons.Add(kbutton);
                _widgets.Add(new WidgetInfo
                {
                    Label = label,
                    Component = kbutton,
                    Type = WidgetType.Toggle,
                    GameObject = hr.gameObject,
                    Tag = hr  // Store HierarchyReferences for reading CheckMark state
                });
                Log.Debug($"    + HierToggle: '{label}' ({hr.gameObject.name})");
            }

            // Post-process: collapse HierRef toggles sharing the same parent into
            // a single radio-group widget (e.g., Celsius/Kelvin/Fahrenheit → Temperature Units).
            CollapseRadioGroups();

            // ------------------------------------------------------------------
            // 2. Sliders (volume controls, camera speed, UI scale, etc.)
            // ------------------------------------------------------------------
            var sliders = screen.GetComponentsInChildren<KSlider>(true);
            Log.Debug($"  KSlider found: {sliders.Length}");
            foreach (var slider in sliders)
            {
                if (slider == null || !slider.gameObject.activeInHierarchy)
                {
                    Log.Debug($"    skip slider: null or inactive ({slider?.gameObject.name})");
                    continue;
                }
                if (IsMouseOnlyControl(slider.gameObject)) { Log.Debug($"    skip mouse-only: {slider.gameObject.name}"); continue; }

                // Prefer SliderContainer's nameLabel (audio volume sliders)
                string label = null;
                var container = slider.GetComponentInParent<SliderContainer>();
                if (container != null && container.nameLabel != null)
                {
                    label = CleanLabel(container.nameLabel.text);
                    Log.Debug($"    SliderContainer nameLabel='{container.nameLabel.text}' cleaned='{label}'");
                }
                if (label == null)
                    label = FindWidgetLabel(slider.gameObject);
                // Broader search: grandparent's children (label may be in a sibling container)
                // For sliders, also capture the LocText ref if it contains "Label: Value" pattern
                // so FormatSliderValue can read the game's formatted value.
                LocText sliderValueLt = null;
                if (label == null)
                {
                    var foundLt = FindGrandparentLocText(slider.gameObject);
                    if (foundLt != null)
                    {
                        string stripped = StripValueSuffix(foundLt.text);
                        if (stripped != null)
                        {
                            label = stripped;
                            sliderValueLt = foundLt;
                        }
                        else
                        {
                            label = CleanLabel(foundLt.text);
                        }
                    }
                }
                if (label == null) { Log.Debug($"    skip no label: {slider.gameObject.name}"); continue; }

                if (sliderValueLt != null)
                    _sliderValueLabels[slider] = sliderValueLt;

                _widgets.Add(new WidgetInfo
                {
                    Label = label,
                    Component = slider,
                    Type = WidgetType.Slider,
                    GameObject = slider.gameObject
                });
                Log.Debug($"    + Slider: '{label}' ({slider.gameObject.name})");
            }

            // ------------------------------------------------------------------
            // 3. KToggle controls (standard checkboxes)
            // ------------------------------------------------------------------
            var toggles = screen.GetComponentsInChildren<KToggle>(true);
            Log.Debug($"  KToggle found: {toggles.Length}");
            foreach (var toggle in toggles)
            {
                if (toggle == null || !toggle.gameObject.activeInHierarchy) continue;
                if (IsMouseOnlyControl(toggle.gameObject)) continue;

                string label = FindWidgetLabel(toggle.gameObject);
                if (string.IsNullOrEmpty(label)) { Log.Debug($"    skip no label: {toggle.gameObject.name}"); continue; }

                _widgets.Add(new WidgetInfo
                {
                    Label = label,
                    Component = toggle,
                    Type = WidgetType.Toggle,
                    GameObject = toggle.gameObject
                });
                Log.Debug($"    + KToggle: '{label}' ({toggle.gameObject.name})");
            }

            // ------------------------------------------------------------------
            // 4. MultiToggle controls (fullscreen, low-res in GraphicsOptionsScreen)
            //    Different component from KToggle — has onClick delegate + CurrentState.
            // ------------------------------------------------------------------
            var multiToggles = screen.GetComponentsInChildren<MultiToggle>(true);
            Log.Debug($"  MultiToggle found: {multiToggles.Length}");
            foreach (var mt in multiToggles)
            {
                if (mt == null || !mt.gameObject.activeInHierarchy)
                {
                    Log.Debug($"    skip inactive: {mt?.gameObject.name}");
                    continue;
                }
                if (IsMouseOnlyControl(mt.gameObject)) { Log.Debug($"    skip mouse-only: {mt.gameObject.name}"); continue; }

                string label = FindWidgetLabel(mt.gameObject);
                if (label == null)
                    label = LabelFromGameObjectName(mt.gameObject.name);
                if (label == null) { Log.Debug($"    skip no label: {mt.gameObject.name}"); continue; }

                _widgets.Add(new WidgetInfo
                {
                    Label = label,
                    Component = mt,
                    Type = WidgetType.Toggle,
                    GameObject = mt.gameObject
                });
                Log.Debug($"    + MultiToggle: '{label}' ({mt.gameObject.name})");
            }

            // ------------------------------------------------------------------
            // 5. Unity Dropdowns (resolution, color mode, audio device)
            // ------------------------------------------------------------------
            var dropdowns = screen.GetComponentsInChildren<Dropdown>(true);
            Log.Debug($"  Dropdown found: {dropdowns.Length}");
            foreach (var dd in dropdowns)
            {
                if (dd == null || !dd.gameObject.activeInHierarchy)
                {
                    Log.Debug($"    skip inactive: {dd?.gameObject.name}");
                    continue;
                }
                if (IsMouseOnlyControl(dd.gameObject)) { Log.Debug($"    skip mouse-only: {dd.gameObject.name}"); continue; }

                // For dropdowns, search SIBLINGS for label (avoid captionText inside dropdown)
                string label = FindSiblingLabel(dd.gameObject);
                if (label == null)
                    label = LabelFromGameObjectName(dd.gameObject.name);
                if (label == null) { Log.Debug($"    skip no label: {dd.gameObject.name}"); continue; }

                _widgets.Add(new WidgetInfo
                {
                    Label = label,
                    Component = dd,
                    Type = WidgetType.Dropdown,
                    GameObject = dd.gameObject
                });
                Log.Debug($"    + Dropdown: '{label}' ({dd.gameObject.name})");
            }

            // ------------------------------------------------------------------
            // 6. KButton controls (apply, close, done, standalone buttons)
            //    Skip buttons already captured as part of other widget types.
            // ------------------------------------------------------------------
            var kbuttons = screen.GetComponentsInChildren<KButton>(true);
            Log.Debug($"  KButton found: {kbuttons.Length}");
            foreach (var kb in kbuttons)
            {
                if (kb == null || !kb.gameObject.activeInHierarchy)
                {
                    Log.Debug($"    skip inactive: {kb?.gameObject.name}");
                    continue;
                }
                if (!kb.isInteractable) { Log.Debug($"    skip not interactable: {kb.gameObject.name}"); continue; }
                if (IsMouseOnlyControl(kb.gameObject)) { Log.Debug($"    skip mouse-only: {kb.gameObject.name}"); continue; }

                // Skip buttons already captured as HierarchyReferences toggles
                if (hierToggleButtons.Contains(kb)) { Log.Debug($"    skip hierToggle: {kb.gameObject.name}"); continue; }

                // Skip buttons that are children of sliders, toggles, or dropdowns (already captured)
                if (kb.GetComponentInParent<KSlider>() != null) { Log.Debug($"    skip child-of-slider: {kb.gameObject.name}"); continue; }
                if (kb.GetComponentInParent<KToggle>() != null) { Log.Debug($"    skip child-of-toggle: {kb.gameObject.name}"); continue; }
                if (kb.GetComponentInParent<MultiToggle>() != null) { Log.Debug($"    skip child-of-multitoggle: {kb.gameObject.name}"); continue; }
                if (kb.GetComponentInParent<Dropdown>() != null) { Log.Debug($"    skip child-of-dropdown: {kb.gameObject.name}"); continue; }

                string label = GetButtonLabel(kb);
                if (string.IsNullOrEmpty(label)) { Log.Debug($"    skip no label: {kb.gameObject.name}"); continue; }

                _widgets.Add(new WidgetInfo
                {
                    Label = label,
                    Component = kb,
                    Type = WidgetType.Button,
                    GameObject = kb.gameObject
                });
                Log.Debug($"    + Button: '{label}' ({kb.gameObject.name})");
            }

            Log.Debug($"  Total widgets discovered: {_widgets.Count}");
        }

        /// <summary>
        /// Validate widget for navigation. Extends base with MultiToggle,
        /// HierarchyReferences toggle (KButton), and Dropdown support.
        /// Base rejects Toggle widgets whose Component isn't KToggle.
        /// </summary>
        protected override bool IsWidgetValid(WidgetInfo widget)
        {
            if (widget == null || widget.GameObject == null) return false;
            if (!widget.GameObject.activeInHierarchy) return false;

            switch (widget.Type)
            {
                case WidgetType.Toggle:
                {
                    // KToggle
                    var toggle = widget.Component as KToggle;
                    if (toggle != null) return toggle.IsInteractable();
                    // MultiToggle
                    var mt = widget.Component as MultiToggle;
                    if (mt != null) return true;
                    // HierRef toggle (Component is KButton)
                    var kb = widget.Component as KButton;
                    if (kb != null) return kb.isInteractable;
                    return false;
                }
                case WidgetType.Dropdown:
                {
                    if (widget.Tag is RadioGroupInfo)
                        return true;
                    var dd = widget.Component as Dropdown;
                    return dd != null && dd.interactable;
                }
                default:
                    return base.IsWidgetValid(widget);
            }
        }

        /// <summary>
        /// Build speech text with state for MultiToggle, HierRef toggles, and Dropdowns.
        /// Base only handles KToggle and KSlider.
        /// </summary>
        protected override string GetWidgetSpeechText(WidgetInfo widget)
        {
            switch (widget.Type)
            {
                case WidgetType.Toggle:
                {
                    // MultiToggle: read CurrentState
                    var mt = widget.Component as MultiToggle;
                    if (mt != null)
                    {
                        string state = mt.CurrentState == 1 ? "on" : "off";
                        return $"{widget.Label}, {state}";
                    }
                    // HierRef toggle: read CheckMark active state
                    if (widget.Tag is HierarchyReferences hr)
                    {
                        string checkRef = hr.HasReference("CheckMark") ? "CheckMark" : "Checkmark";
                        bool isOn = hr.GetReference(checkRef)?.gameObject.activeSelf ?? false;
                        string state = isOn ? "on" : "off";
                        return $"{widget.Label}, {state}";
                    }
                    // KToggle — fall through to base
                    break;
                }
                case WidgetType.Dropdown:
                {
                    // Radio group: read live checkmark state
                    if (widget.Tag is RadioGroupInfo radio)
                    {
                        for (int i = 0; i < radio.Members.Count; i++)
                        {
                            var rhr = radio.Members[i].HierRef;
                            string ckRef = rhr.HasReference("CheckMark") ? "CheckMark" : "Checkmark";
                            if (rhr.GetReference(ckRef)?.gameObject.activeSelf ?? false)
                            {
                                radio.CurrentIndex = i;
                                return $"{widget.Label}, {radio.Members[i].Label}";
                            }
                        }
                        return $"{widget.Label}, {radio.Members[radio.CurrentIndex].Label}";
                    }
                    var dd = widget.Component as Dropdown;
                    if (dd != null && dd.options.Count > 0)
                    {
                        string optionText = dd.options[dd.value].text;
                        return $"{widget.Label}, {optionText}";
                    }
                    return widget.Label;
                }
            }
            return base.GetWidgetSpeechText(widget);
        }

        /// <summary>
        /// Activate the current widget. Extends base behavior with support for
        /// MultiToggle and HierarchyReferences-based toggle patterns.
        /// </summary>
        protected override void ActivateCurrentWidget()
        {
            if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
            var widget = _widgets[_currentIndex];

            if (widget.Type == WidgetType.Toggle)
            {
                // MultiToggle: invoke its onClick delegate (game code hooks toggle logic here)
                var multiToggle = widget.Component as MultiToggle;
                if (multiToggle != null)
                {
                    multiToggle.onClick?.Invoke();
                    string state = multiToggle.CurrentState == 1 ? "on" : "off";
                    Speech.SpeechPipeline.SpeakInterrupt($"{widget.Label}, {state}");
                    return;
                }

                // HierarchyReferences toggle: click the inner KButton, read CheckMark state
                if (widget.Tag is HierarchyReferences hr)
                {
                    var kbutton = widget.Component as KButton;
                    kbutton?.SignalClick(KKeyCode.Mouse0);
                    // Read state after click — CheckMark active means "on"
                    string checkRef = hr.HasReference("CheckMark") ? "CheckMark" : "Checkmark";
                    bool isOn = hr.GetReference(checkRef)?.gameObject.activeSelf ?? false;
                    string state = isOn ? "on" : "off";
                    Speech.SpeechPipeline.SpeakInterrupt($"{widget.Label}, {state}");
                    return;
                }
            }

            // KToggle, KButton, TextInput — handled by base
            base.ActivateCurrentWidget();
        }

        /// <summary>
        /// Cycle a Unity Dropdown or radio group's selected value and speak the new selection.
        /// </summary>
        protected override void CycleDropdown(WidgetInfo widget, int direction)
        {
            // Radio group: click the next/prev member's button
            if (widget.Tag is RadioGroupInfo radio)
            {
                int count = radio.Members.Count;
                int newIndex = (radio.CurrentIndex + direction + count) % count;
                radio.Members[newIndex].Button.SignalClick(KKeyCode.Mouse0);
                radio.CurrentIndex = newIndex;
                Speech.SpeechPipeline.SpeakInterrupt($"{widget.Label}, {radio.Members[newIndex].Label}");
                return;
            }

            var dropdown = widget.Component as Dropdown;
            if (dropdown == null) return;

            int ddCount = dropdown.options.Count;
            if (ddCount == 0) return;

            int ddNewIndex = (dropdown.value + direction + ddCount) % ddCount;
            dropdown.value = ddNewIndex;  // Fires onValueChanged

            string optionText = dropdown.options[ddNewIndex].text;
            Speech.SpeechPipeline.SpeakInterrupt($"{widget.Label}, {optionText}");
        }

        /// <summary>
        /// Read tooltip for the current widget. For radio groups, reads the tooltip
        /// of the currently selected member rather than the parent container.
        /// </summary>
        protected override void SpeakTooltip()
        {
            if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
            var widget = _widgets[_currentIndex];

            if (widget.Tag is RadioGroupInfo radio)
            {
                var member = radio.Members[radio.CurrentIndex];
                var go = member.HierRef.gameObject;
                var tooltip = go.GetComponent<ToolTip>();
                if (tooltip == null)
                    tooltip = go.GetComponentInChildren<ToolTip>();
                if (tooltip == null) return;

                string text = null;
                if (tooltip.multiStringCount > 0)
                    text = tooltip.GetMultiString(0);
                if (string.IsNullOrEmpty(text) && tooltip.OnToolTip != null)
                    text = tooltip.OnToolTip();
                if (string.IsNullOrEmpty(text)) return;

                Speech.SpeechPipeline.SpeakInterrupt(text);
                return;
            }

            base.SpeakTooltip();
        }

        /// <summary>
        /// Detect HierRef toggles that share the same parent and collapse them
        /// into a single radio-group Dropdown widget (cycleable with Left/Right).
        /// e.g., Celsius + Kelvin + Fahrenheit → "Temperature Units, Celsius"
        /// </summary>
        private void CollapseRadioGroups()
        {
            // Group HierRef toggle widgets by parent transform
            var groups = new Dictionary<UnityEngine.Transform, List<int>>();
            for (int i = 0; i < _widgets.Count; i++)
            {
                if (_widgets[i].Type == WidgetType.Toggle && _widgets[i].Tag is HierarchyReferences)
                {
                    var parent = _widgets[i].GameObject.transform.parent;
                    if (!groups.ContainsKey(parent))
                        groups[parent] = new List<int>();
                    groups[parent].Add(i);
                }
            }

            var toRemove = new HashSet<int>();
            foreach (var kvp in groups)
            {
                if (kvp.Value.Count < 2) continue;

                // Only collapse toggles instantiated from the same prefab (same GameObject name).
                // Independent toggles like AlwaysPlayMusic/MuteOnFocusLost have distinct names.
                string firstName = _widgets[kvp.Value[0]].GameObject.name;
                bool allSameName = true;
                for (int j = 1; j < kvp.Value.Count; j++)
                {
                    if (_widgets[kvp.Value[j]].GameObject.name != firstName)
                    {
                        allSameName = false;
                        break;
                    }
                }
                if (!allSameName)
                {
                    Log.Debug($"    Skipping radio collapse for parent '{kvp.Key.name}': members have different names");
                    continue;
                }

                // Build member list and find the currently active one
                var members = new List<RadioMember>();
                int activeIndex = 0;
                for (int j = 0; j < kvp.Value.Count; j++)
                {
                    var w = _widgets[kvp.Value[j]];
                    var hr = (HierarchyReferences)w.Tag;
                    string checkRef = hr.HasReference("CheckMark") ? "CheckMark" : "Checkmark";
                    bool isOn = hr.GetReference(checkRef)?.gameObject.activeSelf ?? false;
                    if (isOn) activeIndex = j;
                    members.Add(new RadioMember
                    {
                        Label = w.Label,
                        Button = w.Component as KButton,
                        HierRef = hr
                    });
                }

                // Find group label: prefer preceding sibling (section header above the group)
                Log.Debug($"    Radio group parent: '{kvp.Key.name}', parent.parent: '{kvp.Key.parent?.name}'");
                string groupLabel = FindPrecedingLabel(kvp.Key.gameObject);
                Log.Debug($"    FindPrecedingLabel returned: '{groupLabel}'");
                if (groupLabel == null)
                    groupLabel = LabelFromGameObjectName(kvp.Key.name);

                // Replace first widget with radio group dropdown
                int firstIdx = kvp.Value[0];
                _widgets[firstIdx] = new WidgetInfo
                {
                    Label = groupLabel ?? _widgets[firstIdx].Label,
                    Component = members[activeIndex].Button,
                    Type = WidgetType.Dropdown,
                    GameObject = kvp.Key.gameObject,
                    Tag = new RadioGroupInfo { Members = members, CurrentIndex = activeIndex }
                };
                Log.Debug($"    Collapsed {kvp.Value.Count} toggles into radio group '{_widgets[firstIdx].Label}'");

                // Mark the rest for removal
                for (int j = 1; j < kvp.Value.Count; j++)
                    toRemove.Add(kvp.Value[j]);
            }

            // Remove collapsed widgets in reverse order to preserve indices
            var sorted = new List<int>(toRemove);
            sorted.Sort();
            sorted.Reverse();
            foreach (int idx in sorted)
                _widgets.RemoveAt(idx);
        }

        /// <summary>
        /// Format slider value for options screens. Reads SliderContainer's own value display
        /// for audio volume sliders, and handles 0-1 range correctly as percent.
        /// </summary>
        protected override string FormatSliderValue(KSlider slider)
        {
            // If inside a SliderContainer (audio volume sliders), read the game's formatted value
            var container = slider.GetComponentInParent<SliderContainer>();
            if (container != null && container.valueLabel != null)
            {
                string gameValue = container.valueLabel.text;
                if (!string.IsNullOrEmpty(gameValue))
                    return gameValue;
            }

            // 0-1 range (non-wholeNumbers): format as percent
            if (!slider.wholeNumbers && slider.maxValue <= 1.01f && slider.minValue >= -0.01f)
            {
                return $"{UnityEngine.Mathf.RoundToInt(slider.value * 100f)}%";
            }

            // Non-percentage sliders (range > 1): format as integer, not percent
            if (slider.maxValue > 1.01f)
            {
                return UnityEngine.Mathf.RoundToInt(slider.value).ToString();
            }

            return base.FormatSliderValue(slider);
        }

        /// <summary>
        /// Find a speakable label for a widget by checking self and sibling LocText components.
        /// Uses CleanLabel to handle MISSING.STRINGS keys by extracting readable names.
        /// </summary>
        private string FindWidgetLabel(UnityEngine.GameObject widgetObj)
        {
            // Check for a LocText on the widget itself or its children
            var locText = widgetObj.GetComponentInChildren<LocText>();
            if (locText != null)
            {
                string cleaned = CleanLabel(locText.text);
                if (cleaned != null) return cleaned;
            }

            // Check parent's children for a sibling LocText (not inside the widget itself)
            if (widgetObj.transform.parent != null)
            {
                var parentTexts = widgetObj.transform.parent.GetComponentsInChildren<LocText>();
                foreach (var lt in parentTexts)
                {
                    if (lt == null) continue;
                    // Skip LocTexts that are inside the widget itself (already checked above)
                    if (lt.transform.IsChildOf(widgetObj.transform)) continue;
                    string cleaned = CleanLabel(lt.text);
                    if (cleaned != null) return cleaned;
                }
            }

            return null;
        }

        /// <summary>
        /// Find a label by searching only sibling LocTexts (same parent, not children of widgetObj).
        /// Used for dropdowns to avoid picking up captionText as the label.
        /// </summary>
        private string FindSiblingLabel(UnityEngine.GameObject widgetObj)
        {
            if (widgetObj.transform.parent == null) return null;
            var parent = widgetObj.transform.parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.gameObject == widgetObj) continue;

                // Check direct component first, then search children of sibling
                var lt = child.GetComponent<LocText>();
                if (lt == null) lt = child.GetComponentInChildren<LocText>();
                if (lt != null)
                {
                    string cleaned = CleanLabel(lt.text);
                    if (cleaned != null) return cleaned;
                }
            }

            return null;
        }

        /// <summary>
        /// Search grandparent-level siblings for a label. Useful when a slider's label LocText
        /// is in a sibling container one level above (e.g., GameOptionsScreen camera speed).
        /// </summary>
        private string FindGrandparentLabel(UnityEngine.GameObject widgetObj)
        {
            var grandparent = widgetObj.transform.parent?.parent;
            if (grandparent == null) return null;

            // Search direct children of grandparent that are NOT the widget's parent
            var widgetParent = widgetObj.transform.parent;
            for (int i = 0; i < grandparent.childCount; i++)
            {
                var sibling = grandparent.GetChild(i);
                if (sibling == widgetParent) continue;

                var lt = sibling.GetComponent<LocText>();
                if (lt == null) lt = sibling.GetComponentInChildren<LocText>();
                if (lt != null)
                {
                    string cleaned = CleanLabel(lt.text);
                    if (cleaned != null && !_ambiguousLabels.Contains(cleaned))
                        return cleaned;
                }
            }

            return null;
        }

        /// <summary>
        /// Search for a label LocText that precedes the widget in the hierarchy.
        /// More targeted than FindSiblingLabel: searches backwards from the widget's position,
        /// so we find the closest section header rather than the first random text.
        /// Also checks the parent's own LocText and grandparent-level preceding siblings.
        /// Used for radio group labels where FindSiblingLabel would return a distant header.
        /// </summary>
        private string FindPrecedingLabel(UnityEngine.GameObject widgetObj)
        {
            if (widgetObj.transform.parent == null) return null;
            var parent = widgetObj.transform.parent;
            int widgetIndex = widgetObj.transform.GetSiblingIndex();

            // Search preceding siblings at parent level (closest label first)
            // Only check direct LocText on the sibling, NOT deep children —
            // GetComponentInChildren would dive into section containers and
            // return distant headers like "GENERAL" from unrelated sections.
            for (int i = widgetIndex - 1; i >= 0; i--)
            {
                var sibling = parent.GetChild(i);
                var lt = sibling.GetComponent<LocText>();
                if (lt != null)
                {
                    string cleaned = CleanLabel(lt.text);
                    if (cleaned != null && !_ambiguousLabels.Contains(cleaned))
                        return cleaned;
                }
            }

            // Check parent's own LocText (label might be on the container itself)
            var parentLt = parent.GetComponent<LocText>();
            if (parentLt != null)
            {
                string cleaned = CleanLabel(parentLt.text);
                if (cleaned != null && !_ambiguousLabels.Contains(cleaned))
                    return cleaned;
            }

            // Try grandparent level: search preceding siblings (direct LocText only)
            if (parent.parent != null)
            {
                int parentIndex = parent.GetSiblingIndex();
                var grandparent = parent.parent;
                for (int i = parentIndex - 1; i >= 0; i--)
                {
                    var sibling = grandparent.GetChild(i);
                    var lt = sibling.GetComponent<LocText>();
                    if (lt != null)
                    {
                        string cleaned = CleanLabel(lt.text);
                        if (cleaned != null && !_ambiguousLabels.Contains(cleaned))
                            return cleaned;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Like FindGrandparentLabel but returns the LocText component itself.
        /// Used for sliders to store a reference to game-managed value display text.
        /// Searches preceding siblings first for better label accuracy.
        /// </summary>
        private LocText FindGrandparentLocText(UnityEngine.GameObject widgetObj)
        {
            var grandparent = widgetObj.transform.parent?.parent;
            if (grandparent == null) return null;
            var widgetParent = widgetObj.transform.parent;
            int parentIndex = widgetParent.GetSiblingIndex();

            // Search preceding siblings first (closest label)
            for (int i = parentIndex - 1; i >= 0; i--)
            {
                var sibling = grandparent.GetChild(i);
                var lt = sibling.GetComponent<LocText>() ?? sibling.GetComponentInChildren<LocText>();
                if (lt != null && !string.IsNullOrEmpty(lt.text))
                {
                    string cleaned = CleanLabel(lt.text);
                    if (cleaned != null && !_ambiguousLabels.Contains(cleaned))
                        return lt;
                }
            }

            // Fallback: forward siblings
            for (int i = parentIndex + 1; i < grandparent.childCount; i++)
            {
                var sibling = grandparent.GetChild(i);
                if (sibling == widgetParent) continue;
                var lt = sibling.GetComponent<LocText>() ?? sibling.GetComponentInChildren<LocText>();
                if (lt != null && !string.IsNullOrEmpty(lt.text))
                {
                    string cleaned = CleanLabel(lt.text);
                    if (cleaned != null && !_ambiguousLabels.Contains(cleaned))
                        return lt;
                }
            }

            return null;
        }

        /// <summary>
        /// Strip a trailing value suffix from a label (e.g., "Camera Pan Speed: 100%" → "Camera Pan Speed").
        /// Returns the label portion if a "label: value" pattern is found, or null if no pattern detected.
        /// </summary>
        private string StripValueSuffix(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            int colonIdx = text.IndexOf(':');
            if (colonIdx <= 0 || colonIdx >= text.Length - 1) return null;

            string after = text.Substring(colonIdx + 1).Trim();
            if (after.Length > 0 && (char.IsDigit(after[0]) || after[0] == '-' || after[0] == '+'))
            {
                string before = text.Substring(0, colonIdx).Trim();
                return before.Length > 0 ? before : null;
            }
            return null;
        }

        /// <summary>
        /// Clean a label string. Returns the text as-is if valid, extracts a readable name
        /// from MISSING.STRINGS keys, or returns null if the label is empty.
        /// </summary>
        private string CleanLabel(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            if (!text.StartsWith("MISSING.STRINGS")) return text;

            // Extract the last key segment: MISSING.STRINGS.UI.FRONTEND.SCREEN.FULLSCREEN → FULLSCREEN
            int lastDot = text.LastIndexOf('.');
            if (lastDot < 0 || lastDot >= text.Length - 1) return null;
            string key = text.Substring(lastDot + 1);
            if (key.Length == 0) return null;

            // Title case: FULLSCREEN → Fullscreen
            return char.ToUpper(key[0]) + (key.Length > 1 ? key.Substring(1).ToLower() : "");
        }

        /// <summary>
        /// Extract a readable label from a GameObject name by splitting PascalCase
        /// and removing common suffixes (Button, Toggle, Slider, Dropdown).
        /// e.g. "alwaysPlayMusicButton" → "Always Play Music"
        /// </summary>
        private string LabelFromGameObjectName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            // Remove common suffixes
            foreach (var suffix in new[] { "Button", "Toggle", "Slider", "Dropdown" })
            {
                if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                    && name.Length > suffix.Length)
                {
                    name = name.Substring(0, name.Length - suffix.Length);
                    break;
                }
            }

            if (name.Length == 0) return null;

            // Split PascalCase/camelCase: alwaysPlayMusic → Always Play Music
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (i > 0 && char.IsUpper(c) && char.IsLower(name[i - 1]))
                    sb.Append(' ');
                sb.Append(i == 0 ? char.ToUpper(c) : c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get label text from a KButton's child LocText, using CleanLabel for MISSING.STRINGS.
        /// </summary>
        private string GetButtonLabel(KButton button)
        {
            var locText = button.GetComponentInChildren<LocText>();
            if (locText != null)
                return CleanLabel(locText.text);
            return null;
        }

        /// <summary>
        /// Filter out mouse-only UI controls that are irrelevant for keyboard navigation.
        /// Checks for drag handles, resize handles, and scrollbars.
        /// Close/done buttons are kept — they are valid keyboard targets.
        /// </summary>
        private bool IsMouseOnlyControl(UnityEngine.GameObject obj)
        {
            string name = obj.name.ToLowerInvariant();
            if (name.Contains("drag")) return true;
            if (name.Contains("resize")) return true;
            if (name.Contains("scrollbar")) return true;
            return false;
        }

        /// <summary>
        /// Determine display name from the screen type.
        /// </summary>
        private string GetDisplayNameForScreen(KScreen screen)
        {
            string typeName = screen.GetType().Name;
            switch (typeName)
            {
                case "AudioOptionsScreen":
                    return STRINGS.ONIACCESS.HANDLERS.AUDIO_OPTIONS;
                case "GraphicsOptionsScreen":
                    return STRINGS.ONIACCESS.HANDLERS.GRAPHICS_OPTIONS;
                case "GameOptionsScreen":
                    return STRINGS.ONIACCESS.HANDLERS.GAME_OPTIONS;
                case "MetricsOptionsScreen":
                    return STRINGS.ONIACCESS.HANDLERS.DATA_OPTIONS;
                case "FeedbackScreen":
                    return STRINGS.ONIACCESS.HANDLERS.FEEDBACK;
                case "CreditsScreen":
                    return STRINGS.ONIACCESS.HANDLERS.CREDITS;
                default:
                    return STRINGS.ONIACCESS.HANDLERS.OPTIONS;
            }
        }
    }
}
