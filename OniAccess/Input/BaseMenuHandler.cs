using System.Collections.Generic;

namespace OniAccess.Input
{
    /// <summary>
    /// 1D list navigation layer extending ScreenHandler.
    /// Owns the widget list, cursor index, and all menu-specific behavior:
    /// - Widget discovery and lifecycle
    /// - Arrow navigation with wrap-around
    /// - Home/End, Enter activation, Left/Right adjustment
    /// - Tab stubs for tabbed screens
    /// - Shift+I tooltip reading
    /// - A-Z type-ahead search
    /// - Widget validity checking
    ///
    /// Concrete list-based handlers extend this and implement only:
    /// - DiscoverWidgets (populate _widgets)
    /// - DisplayName (screen title for speech)
    /// - HelpEntries (composing from CommonHelpEntries + MenuHelpEntries
    ///   + ListNavHelpEntries + screen-specific)
    ///
    /// Future 2D grid handlers extend ScreenHandler directly with their own
    /// state (cursor position, tile data) without inheriting any of this.
    ///
    /// Per locked decisions:
    /// - Arrow keys navigate Up/Down between items with wrap-around
    /// - Home/End jump to first/last
    /// - Enter activates (KButton.SignalClick, KToggle.Click)
    /// - Left/Right adjust sliders and cycle dropdowns
    /// - Shift+Left/Right for large step adjustment
    /// - Tab/Shift+Tab for tabbed screens (virtual stubs)
    /// - Widget readout: label and value only, no type announcement
    /// </summary>
    public abstract class BaseMenuHandler : ScreenHandler, ISearchable
    {
        protected readonly List<WidgetInfo> _widgets = new List<WidgetInfo>();
        protected int _currentIndex;
        protected readonly TypeAheadSearch _search = new TypeAheadSearch();

        protected BaseMenuHandler(KScreen screen) : base(screen) { }

        /// <summary>
        /// Menus are modal: block all input from reaching handlers below.
        /// </summary>
        public override bool CapturesAllInput => true;

        // ========================================
        // COMPOSABLE HELP ENTRY LISTS (menu-specific)
        // ========================================

        /// <summary>
        /// Help entries for menu-specific features (tooltip, search).
        /// </summary>
        protected static readonly List<HelpEntry> MenuHelpEntries = new List<HelpEntry>
        {
            new HelpEntry("Shift+I", STRINGS.ONIACCESS.HELP.READ_TOOLTIP),
            new HelpEntry("A-Z", STRINGS.ONIACCESS.HELP.TYPE_SEARCH),
        };

        /// <summary>
        /// Help entries for 1D list navigation.
        /// </summary>
        protected static readonly List<HelpEntry> ListNavHelpEntries = new List<HelpEntry>
        {
            new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
            new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
            new HelpEntry("Enter", STRINGS.ONIACCESS.HELP.SELECT_ITEM),
            new HelpEntry("Left/Right", STRINGS.ONIACCESS.HELP.ADJUST_VALUE),
            new HelpEntry("Shift+Left/Right", STRINGS.ONIACCESS.HELP.ADJUST_VALUE_LARGE),
        };

        // ========================================
        // ABSTRACT: WIDGET DISCOVERY
        // ========================================

        /// <summary>
        /// Populate _widgets from the screen's UI hierarchy.
        /// Each subclass implements to enumerate that screen's interactive elements.
        /// </summary>
        public abstract void DiscoverWidgets(KScreen screen);

        // ========================================
        // LIFECYCLE
        // ========================================

        /// <summary>
        /// Called when this handler becomes active on the stack.
        /// Speaks screen name, discovers widgets, queues first widget.
        /// </summary>
        public override void OnActivate()
        {
            base.OnActivate();
            DiscoverWidgets(_screen);
            _currentIndex = 0;
            _search.Clear();

            if (_widgets.Count > 0)
            {
                Speech.SpeechPipeline.SpeakQueued(GetWidgetSpeechText(_widgets[0]));
            }
        }

        /// <summary>
        /// Called when this handler is popped off the stack.
        /// </summary>
        public override void OnDeactivate()
        {
            base.OnDeactivate();
            _currentIndex = 0;
            _search.Clear();
        }

        // ========================================
        // KEY DOWN HANDLING (Shift+I tooltip, A-Z search)
        // ========================================

        /// <summary>
        /// Handle key down events for menu-specific features:
        /// Shift+I tooltip reading and A-Z type-ahead search.
        /// </summary>
        public override bool HandleKeyDown(KButtonEvent e)
        {
            // A-Z search: iterate KKeyCode.A through KKeyCode.Z
            bool ctrlHeld = UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftControl)
                         || UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightControl);
            bool altHeld = UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftAlt)
                        || UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightAlt);

            for (KKeyCode kk = KKeyCode.A; kk <= KKeyCode.Z; kk++)
            {
                if (e.Controller.GetKeyDown(kk))
                {
                    UnityEngine.KeyCode unityKey = UnityEngine.KeyCode.A + (kk - KKeyCode.A);
                    if (_search.HandleKey(unityKey, ctrlHeld, altHeld, this))
                    {
                        e.Consumed = true;
                        return true;
                    }
                }
            }

            return false;
        }

        // ========================================
        // UNBOUND KEY HANDLING (arrows, Home/End, Enter, Tab)
        // ========================================

        /// <summary>
        /// Handle unbound keys for 1D list navigation.
        /// Falls through to ScreenHandler for F12 help.
        /// </summary>
        public override bool HandleUnboundKey(UnityEngine.KeyCode keyCode)
        {
            switch (keyCode)
            {
                case UnityEngine.KeyCode.DownArrow:
                    NavigateNext();
                    return true;
                case UnityEngine.KeyCode.UpArrow:
                    NavigatePrev();
                    return true;
                case UnityEngine.KeyCode.Home:
                    NavigateFirst();
                    return true;
                case UnityEngine.KeyCode.End:
                    NavigateLast();
                    return true;
                case UnityEngine.KeyCode.Return:
                    ActivateCurrentWidget();
                    return true;
                case UnityEngine.KeyCode.Tab:
                    if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftShift)
                        || UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightShift))
                    {
                        NavigateTabBackward();
                    }
                    else
                    {
                        NavigateTabForward();
                    }
                    return true;
                case UnityEngine.KeyCode.I:
                    // Shift+I: read tooltip (only dispatched when Shift is held)
                    SpeakTooltip();
                    return true;
                case UnityEngine.KeyCode.LeftArrow:
                case UnityEngine.KeyCode.RightArrow:
                {
                    int direction = keyCode == UnityEngine.KeyCode.RightArrow ? 1 : -1;
                    bool isLargeStep = UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftShift)
                                    || UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightShift);
                    AdjustCurrentWidget(direction, isLargeStep);
                    return true;
                }
                default:
                    return base.HandleUnboundKey(keyCode);
            }
        }

        // ========================================
        // WIDGET VALIDITY
        // ========================================

        /// <summary>
        /// Check whether a widget is still valid (not destroyed, active in hierarchy,
        /// and interactable where applicable). Guards against stale references when
        /// game UI changes after DiscoverWidgets.
        /// </summary>
        protected virtual bool IsWidgetValid(WidgetInfo widget)
        {
            if (widget == null || widget.GameObject == null) return false;
            if (!widget.GameObject.activeInHierarchy) return false;

            switch (widget.Type)
            {
                case WidgetType.Button:
                {
                    var btn = widget.Component as KButton;
                    return btn != null && btn.isInteractable;
                }
                case WidgetType.Toggle:
                {
                    var toggle = widget.Component as KToggle;
                    return toggle != null && toggle.IsInteractable();
                }
                case WidgetType.Slider:
                {
                    var slider = widget.Component as KSlider;
                    return slider != null && slider.interactable;
                }
                default:
                    return widget.Component != null;
            }
        }

        // ========================================
        // NAVIGATION METHODS
        // ========================================

        /// <summary>
        /// Move to next widget with wrap-around. Skips invalid widgets.
        /// Plays wrap sound when wrapping to first.
        /// </summary>
        protected void NavigateNext()
        {
            if (_widgets.Count == 0) return;
            int start = _currentIndex;
            for (int i = 0; i < _widgets.Count; i++)
            {
                int candidate = (start + 1 + i) % _widgets.Count;
                if (IsWidgetValid(_widgets[candidate]))
                {
                    bool wrapped = candidate <= _currentIndex;
                    _currentIndex = candidate;
                    if (wrapped) PlayWrapSound();
                    SpeakCurrentWidget();
                    return;
                }
            }
        }

        /// <summary>
        /// Move to previous widget with wrap-around. Skips invalid widgets.
        /// Plays wrap sound when wrapping to last.
        /// </summary>
        protected void NavigatePrev()
        {
            if (_widgets.Count == 0) return;
            int start = _currentIndex;
            for (int i = 0; i < _widgets.Count; i++)
            {
                int candidate = (start - 1 - i + _widgets.Count) % _widgets.Count;
                if (IsWidgetValid(_widgets[candidate]))
                {
                    bool wrapped = candidate >= _currentIndex;
                    _currentIndex = candidate;
                    if (wrapped) PlayWrapSound();
                    SpeakCurrentWidget();
                    return;
                }
            }
        }

        /// <summary>
        /// Jump to first widget.
        /// </summary>
        protected void NavigateFirst()
        {
            if (_widgets.Count == 0) return;
            _currentIndex = 0;
            SpeakCurrentWidget();
        }

        /// <summary>
        /// Jump to last widget.
        /// </summary>
        protected void NavigateLast()
        {
            if (_widgets.Count == 0) return;
            _currentIndex = _widgets.Count - 1;
            SpeakCurrentWidget();
        }

        /// <summary>
        /// Navigate to next tab section. No-op default for non-tabbed screens.
        /// Subclasses override for tabbed screens (e.g., colony setup panels).
        /// </summary>
        protected virtual void NavigateTabForward() { }

        /// <summary>
        /// Navigate to previous tab section. No-op default.
        /// </summary>
        protected virtual void NavigateTabBackward() { }

        // ========================================
        // WIDGET SPEECH
        // ========================================

        /// <summary>
        /// Build speech text for a widget: "label, value" for sliders/toggles/dropdowns,
        /// just "label" for buttons/labels. No type announcement per locked decision.
        /// </summary>
        protected virtual string GetWidgetSpeechText(WidgetInfo widget)
        {
            switch (widget.Type)
            {
                case WidgetType.Toggle:
                {
                    var toggle = widget.Component as KToggle;
                    if (toggle != null)
                    {
                        string state = toggle.isOn ? "on" : "off";
                        return $"{widget.Label}, {state}";
                    }
                    return widget.Label;
                }
                case WidgetType.Slider:
                {
                    var slider = widget.Component as KSlider;
                    if (slider != null)
                    {
                        return $"{widget.Label}, {FormatSliderValue(slider)}";
                    }
                    return widget.Label;
                }
                case WidgetType.Dropdown:
                    // Dropdown value reading is screen-specific.
                    // Subclasses override GetWidgetSpeechText for dropdowns
                    // or provide the value in the Label itself.
                    return widget.Label;
                default:
                    return widget.Label;
            }
        }

        /// <summary>
        /// Speak the currently focused widget via SpeakInterrupt.
        /// </summary>
        protected void SpeakCurrentWidget()
        {
            if (_currentIndex >= 0 && _currentIndex < _widgets.Count)
            {
                Speech.SpeechPipeline.SpeakInterrupt(GetWidgetSpeechText(_widgets[_currentIndex]));
            }
        }

        // ========================================
        // WIDGET INTERACTION
        // ========================================

        /// <summary>
        /// Activate the currently focused widget. Dispatches by WidgetType:
        /// - Button: SignalClick (triggers onClick + plays button sound)
        /// - Toggle: Click() then speak new state
        /// - TextInput: no-op (subclasses handle)
        /// </summary>
        protected virtual void ActivateCurrentWidget()
        {
            if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
            var widget = _widgets[_currentIndex];
            if (!IsWidgetValid(widget)) return;

            switch (widget.Type)
            {
                case WidgetType.Button:
                {
                    var kbutton = widget.Component as KButton;
                    kbutton?.SignalClick(KKeyCode.Mouse0);
                    break;
                }
                case WidgetType.Toggle:
                {
                    var toggle = widget.Component as KToggle;
                    if (toggle != null)
                    {
                        toggle.Click();
                        string state = toggle.isOn ? "on" : "off";
                        Speech.SpeechPipeline.SpeakInterrupt($"{widget.Label}, {state}");
                    }
                    break;
                }
                case WidgetType.TextInput:
                    // No-op default. Subclasses handle text input activation.
                    break;
            }
        }

        /// <summary>
        /// Adjust the currently focused widget's value. Dispatches by WidgetType:
        /// - Slider: step by wholeNumbers-aware increment, speak new value
        /// - Dropdown: delegate to CycleDropdown virtual method
        /// </summary>
        protected virtual void AdjustCurrentWidget(int direction, bool isLargeStep)
        {
            if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
            var widget = _widgets[_currentIndex];
            if (!IsWidgetValid(widget)) return;

            switch (widget.Type)
            {
                case WidgetType.Slider:
                {
                    var slider = widget.Component as KSlider;
                    if (slider == null) return;

                    float step;
                    if (slider.wholeNumbers)
                    {
                        step = isLargeStep ? 10f : 1f;
                    }
                    else
                    {
                        float range = slider.maxValue - slider.minValue;
                        step = isLargeStep ? range * 0.1f : range * 0.01f;
                    }

                    slider.value = UnityEngine.Mathf.Clamp(
                        slider.value + step * direction,
                        slider.minValue, slider.maxValue);

                    // KSlider.onValueChanged fires automatically from setting .value
                    Speech.SpeechPipeline.SpeakInterrupt(
                        $"{widget.Label}, {FormatSliderValue(slider)}");
                    break;
                }
                case WidgetType.Dropdown:
                    CycleDropdown(widget, direction);
                    break;
            }
        }

        /// <summary>
        /// Cycle a dropdown widget's value. No-op default.
        /// Subclasses override for screen-specific dropdown cycling logic.
        /// </summary>
        protected virtual void CycleDropdown(WidgetInfo widget, int direction) { }

        // ========================================
        // TOOLTIP READING
        // ========================================

        /// <summary>
        /// Read the tooltip text for the currently focused widget and speak it.
        /// Triggered by Shift+I.
        /// </summary>
        protected virtual void SpeakTooltip()
        {
            if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
            var widget = _widgets[_currentIndex];
            if (widget.GameObject == null) return;

            Util.Log.Debug($"SpeakTooltip: widget='{widget.Label}', go='{widget.GameObject.name}'");
            var tooltip = widget.GameObject.GetComponent<ToolTip>();
            if (tooltip == null)
                tooltip = widget.GameObject.GetComponentInChildren<ToolTip>();
            if (tooltip == null)
            {
                Util.Log.Debug("SpeakTooltip: no ToolTip component found");
                return;
            }

            Util.Log.Debug($"SpeakTooltip: found ToolTip, multiStringCount={tooltip.multiStringCount}");
            string text = null;
            if (tooltip.multiStringCount > 0)
                text = tooltip.GetMultiString(0);
            if (string.IsNullOrEmpty(text) && tooltip.OnToolTip != null)
                text = tooltip.OnToolTip();
            if (string.IsNullOrEmpty(text))
            {
                Util.Log.Debug("SpeakTooltip: tooltip text is empty");
                return;
            }

            Util.Log.Debug($"SpeakTooltip: speaking '{text}'");
            Speech.SpeechPipeline.SpeakInterrupt(text);
        }

        // ========================================
        // UTILITY METHODS
        // ========================================

        /// <summary>
        /// Play the wrap-around earcon sound when navigation wraps.
        /// </summary>
        protected void PlayWrapSound()
        {
            try
            {
                KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Click_Close"));
            }
            catch (System.Exception ex)
            {
                Util.Log.Debug($"PlayWrapSound failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Format a slider value for speech. Uses integer format for wholeNumbers sliders,
        /// percent format for 0-100 range, and one-decimal format otherwise.
        /// </summary>
        protected virtual string FormatSliderValue(KSlider slider)
        {
            if (slider.wholeNumbers)
            {
                return ((int)slider.value).ToString();
            }

            if (slider.minValue >= 0f && slider.maxValue <= 100f)
            {
                return GameUtil.GetFormattedPercent(slider.value);
            }

            return slider.value.ToString("F1");
        }

        // ========================================
        // ISearchable IMPLEMENTATION
        // ========================================

        public int SearchItemCount => _widgets.Count;

        public int SearchCurrentIndex => _currentIndex;

        public string GetSearchLabel(int index)
        {
            if (index < 0 || index >= _widgets.Count) return null;
            return _widgets[index].Label;
        }

        public void SearchMoveTo(int index)
        {
            if (index < 0 || index >= _widgets.Count) return;
            _currentIndex = index;
            SpeakCurrentWidget();
        }
    }
}
