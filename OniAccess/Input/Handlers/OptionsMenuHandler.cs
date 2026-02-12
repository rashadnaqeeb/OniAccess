using System.Collections.Generic;
using HarmonyLib;

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

        public override void OnActivate()
        {
            // Determine display name from screen type
            _displayName = GetDisplayNameForScreen(_screen);
            base.OnActivate();
        }

        public override void DiscoverWidgets(KScreen screen)
        {
            _widgets.Clear();

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
        /// Finds sliders, toggles, and buttons with associated labels.
        /// Filters out mouse-only controls (drag handles, resize handles, close buttons).
        /// </summary>
        private void DiscoverOptionWidgets(KScreen screen)
        {
            // Discover sliders (volume controls, etc.)
            var sliders = screen.GetComponentsInChildren<KSlider>(true);
            foreach (var slider in sliders)
            {
                if (slider == null || !slider.gameObject.activeInHierarchy) continue;
                if (IsMouseOnlyControl(slider.gameObject)) continue;

                string label = FindWidgetLabel(slider.gameObject);
                if (string.IsNullOrEmpty(label)) continue;

                _widgets.Add(new WidgetInfo
                {
                    Label = label,
                    Component = slider,
                    Type = WidgetType.Slider,
                    GameObject = slider.gameObject
                });
            }

            // Discover toggles (checkboxes, on/off settings)
            var toggles = screen.GetComponentsInChildren<KToggle>(true);
            foreach (var toggle in toggles)
            {
                if (toggle == null || !toggle.gameObject.activeInHierarchy) continue;
                if (IsMouseOnlyControl(toggle.gameObject)) continue;

                string label = FindWidgetLabel(toggle.gameObject);
                if (string.IsNullOrEmpty(label)) continue;

                _widgets.Add(new WidgetInfo
                {
                    Label = label,
                    Component = toggle,
                    Type = WidgetType.Toggle,
                    GameObject = toggle.gameObject
                });
            }

            // Discover buttons (apply, close, done, etc.)
            var kbuttons = screen.GetComponentsInChildren<KButton>(true);
            foreach (var kb in kbuttons)
            {
                if (kb == null || !kb.gameObject.activeInHierarchy) continue;
                if (!kb.isInteractable) continue;
                if (IsMouseOnlyControl(kb.gameObject)) continue;

                // Skip buttons that are children of sliders or toggles (already captured)
                if (kb.GetComponentInParent<KSlider>() != null) continue;
                if (kb.GetComponentInParent<KToggle>() != null) continue;

                string label = GetButtonLabel(kb);
                if (string.IsNullOrEmpty(label)) continue;

                _widgets.Add(new WidgetInfo
                {
                    Label = label,
                    Component = kb,
                    Type = WidgetType.Button,
                    GameObject = kb.gameObject
                });
            }
        }

        /// <summary>
        /// Find a speakable label for a widget by checking sibling/parent LocText components.
        /// Options screens typically have a label LocText near each interactive widget.
        /// </summary>
        private string FindWidgetLabel(UnityEngine.GameObject widgetObj)
        {
            // Check for a LocText on the widget itself
            var locText = widgetObj.GetComponentInChildren<LocText>();
            if (locText != null && !string.IsNullOrEmpty(locText.text))
                return locText.text;

            // Check parent for a sibling LocText (common pattern: label + control side by side)
            if (widgetObj.transform.parent != null)
            {
                var parentTexts = widgetObj.transform.parent.GetComponentsInChildren<LocText>();
                foreach (var lt in parentTexts)
                {
                    if (lt != null && !string.IsNullOrEmpty(lt.text)
                        && lt.gameObject != widgetObj)
                    {
                        return lt.text;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get label text from a KButton's child LocText.
        /// </summary>
        private string GetButtonLabel(KButton button)
        {
            var locText = button.GetComponentInChildren<LocText>();
            if (locText != null && !string.IsNullOrEmpty(locText.text))
                return locText.text;
            return null;
        }

        /// <summary>
        /// Filter out mouse-only UI controls that are irrelevant for keyboard navigation.
        /// Checks for drag handles, resize handles, close buttons, and controls without labels.
        /// </summary>
        private bool IsMouseOnlyControl(UnityEngine.GameObject obj)
        {
            string name = obj.name.ToLowerInvariant();
            if (name.Contains("drag")) return true;
            if (name.Contains("resize")) return true;
            if (name.Contains("close")) return true;
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
                default:
                    return STRINGS.ONIACCESS.HANDLERS.OPTIONS;
            }
        }
    }
}
