namespace OniAccess.Input
{
    /// <summary>
    /// 1D list navigation layer extending ScreenHandler.
    /// Provides arrow navigation with wrap-around, Home/End, Enter activation,
    /// Left/Right value adjustment, and Tab stubs for tabbed screens.
    ///
    /// Concrete list-based handlers extend this and implement only:
    /// - DiscoverWidgets (populate _widgets)
    /// - DisplayName (screen title for speech)
    /// - HelpEntries (composing from CommonHelpEntries + ListNavHelpEntries + screen-specific)
    ///
    /// Future 2D grid handlers (Phase 8) extend ScreenHandler directly,
    /// sharing infrastructure without inheriting irrelevant 1D navigation.
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
    public abstract class BaseMenuHandler : ScreenHandler
    {
        protected BaseMenuHandler(KScreen screen) : base(screen) { }

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
        // NAVIGATION METHODS
        // ========================================

        /// <summary>
        /// Move to next widget with wrap-around. Plays wrap sound when wrapping to first.
        /// </summary>
        protected void NavigateNext()
        {
            if (_widgets.Count == 0) return;
            _currentIndex = (_currentIndex + 1) % _widgets.Count;
            if (_currentIndex == 0) PlayWrapSound();
            SpeakCurrentWidget();
        }

        /// <summary>
        /// Move to previous widget with wrap-around. Plays wrap sound when wrapping to last.
        /// </summary>
        protected void NavigatePrev()
        {
            if (_widgets.Count == 0) return;
            int prev = _currentIndex;
            _currentIndex = (_currentIndex - 1 + _widgets.Count) % _widgets.Count;
            if (_currentIndex == _widgets.Count - 1 && prev == 0) PlayWrapSound();
            SpeakCurrentWidget();
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
        // WIDGET SPEECH TEXT
        // ========================================

        /// <summary>
        /// Build speech text for a widget: "label, value" for sliders/toggles/dropdowns,
        /// just "label" for buttons/labels. No type announcement per locked decision.
        /// </summary>
        protected override string GetWidgetSpeechText(WidgetInfo widget)
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
    }
}
