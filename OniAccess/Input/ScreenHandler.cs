using System.Collections.Generic;
using System.Linq;

namespace OniAccess.Input
{
    /// <summary>
    /// Abstract base for ALL screen handlers. Provides common infrastructure:
    /// - F12 help (pushes HelpHandler with composable entries)
    /// - Shift+I tooltip reading
    /// - A-Z type-ahead search (via ISearchable)
    /// - Screen entry speech (DisplayName interrupt, first widget queued)
    /// - Widget lifecycle (DiscoverWidgets, _widgets list, _currentIndex)
    ///
    /// BaseMenuHandler extends this with 1D list navigation (arrows, Home/End, Enter).
    /// Future 2D grid handlers (Phase 8) can extend ScreenHandler directly without
    /// inheriting irrelevant 1D navigation.
    ///
    /// Per locked decisions:
    /// - CapturesAllInput = true for all screen handlers (menus block input)
    /// - Name first, vary early: DisplayName is spoken on activation
    /// - Focus lands on first interactive widget, queued behind screen title
    /// </summary>
    public abstract class ScreenHandler : IAccessHandler, ISearchable
    {
        protected readonly List<WidgetInfo> _widgets = new List<WidgetInfo>();
        protected int _currentIndex;
        protected KScreen _screen;
        protected readonly TypeAheadSearch _search = new TypeAheadSearch();

        /// <summary>
        /// The KScreen this handler manages. Used by ContextDetector to match
        /// a deactivating screen to its handler for correct Pop behavior.
        /// </summary>
        public KScreen Screen => _screen;

        /// <summary>
        /// Display name spoken on activation (e.g., "Options", "Pause").
        /// Per locked decision: name first, vary early.
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// Help entries for F12 navigable help list. Subclasses compose from
        /// CommonHelpEntries + ListNavHelpEntries (or grid entries) + screen-specific.
        /// </summary>
        public abstract IReadOnlyList<HelpEntry> HelpEntries { get; }

        /// <summary>
        /// Populate _widgets from the screen's UI hierarchy.
        /// Each subclass implements to enumerate that screen's interactive elements.
        /// </summary>
        public abstract void DiscoverWidgets(KScreen screen);

        /// <summary>
        /// All screen handlers block input fallthrough to handlers below.
        /// Mouse and zoom actions pass through via ModInputRouter's IsPassThroughAction.
        /// </summary>
        public bool CapturesAllInput => true;

        // ========================================
        // COMPOSABLE HELP ENTRY LISTS
        // ========================================

        /// <summary>
        /// Help entries common to ALL screen handlers (F12, toggle, tooltip, search).
        /// </summary>
        protected static readonly List<HelpEntry> CommonHelpEntries = new List<HelpEntry>
        {
            new HelpEntry("F12", STRINGS.ONIACCESS.HOTKEYS.CONTEXT_HELP),
            new HelpEntry("Ctrl+Shift+F12", STRINGS.ONIACCESS.HOTKEYS.TOGGLE_MOD),
            new HelpEntry("Shift+I", STRINGS.ONIACCESS.HELP.READ_TOOLTIP),
            new HelpEntry("A-Z", STRINGS.ONIACCESS.HELP.TYPE_SEARCH),
        };

        /// <summary>
        /// Help entries for 1D list navigation. Used by BaseMenuHandler subclasses.
        /// Composed with CommonHelpEntries + screen-specific entries.
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
        // CONSTRUCTOR
        // ========================================

        protected ScreenHandler(KScreen screen)
        {
            _screen = screen;
        }

        // ========================================
        // IAccessHandler IMPLEMENTATION
        // ========================================

        /// <summary>
        /// Called when this handler becomes active on the stack.
        /// Discovers widgets, speaks screen name, queues first widget behind it.
        /// </summary>
        public virtual void OnActivate()
        {
            DiscoverWidgets(_screen);
            Speech.SpeechPipeline.SpeakInterrupt(DisplayName);
            _currentIndex = 0;

            if (_widgets.Count > 0)
            {
                Speech.SpeechPipeline.SpeakQueued(GetWidgetSpeechText(_widgets[0]));
            }

            _search.Clear();
        }

        /// <summary>
        /// Called when this handler is popped off the stack.
        /// </summary>
        public virtual void OnDeactivate()
        {
            _currentIndex = 0;
            _search.Clear();
        }

        /// <summary>
        /// Handle key down events from ONI's KButtonEvent system.
        /// Checks for Shift+I tooltip and A-Z search keys.
        /// </summary>
        public virtual bool HandleKeyDown(KButtonEvent e)
        {
            // Shift+I: read tooltip for current widget
            if (e.Controller.GetKeyDown(KKeyCode.I)
                && (UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftShift)
                    || UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightShift)))
            {
                SpeakTooltip();
                e.Consumed = true;
                return true;
            }

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

        /// <summary>
        /// Handle key up events. Default: no action.
        /// </summary>
        public virtual bool HandleKeyUp(KButtonEvent e)
        {
            return false;
        }

        /// <summary>
        /// Handle unbound keys (polled by KeyPoller). F12 pushes help.
        /// Subclasses override for additional unbound keys (arrows, Home/End, etc).
        /// </summary>
        public virtual bool HandleUnboundKey(UnityEngine.KeyCode keyCode)
        {
            if (keyCode == UnityEngine.KeyCode.F12)
            {
                HandlerStack.Push(new HelpHandler(HelpEntries));
                return true;
            }
            return false;
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

        // ========================================
        // UTILITY METHODS
        // ========================================

        /// <summary>
        /// Build the speech text for a widget. Default returns just the label.
        /// BaseMenuHandler overrides to add value for sliders/toggles/dropdowns.
        /// </summary>
        protected virtual string GetWidgetSpeechText(WidgetInfo widget)
        {
            return widget.Label;
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

        /// <summary>
        /// Play the wrap-around earcon sound when navigation wraps from last to first
        /// or first to last.
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
        /// Read the tooltip text for the currently focused widget and speak it.
        /// Triggered by Shift+I.
        /// </summary>
        protected void SpeakTooltip()
        {
            if (_currentIndex < 0 || _currentIndex >= _widgets.Count) return;
            var widget = _widgets[_currentIndex];
            if (widget.GameObject == null) return;

            var tooltip = widget.GameObject.GetComponent<ToolTip>();
            if (tooltip == null) return;

            string text = tooltip.GetMultiString(0);
            if (string.IsNullOrEmpty(text)) return;

            Speech.SpeechPipeline.SpeakInterrupt(text);
        }

        /// <summary>
        /// Format a slider value for speech. Uses integer format for wholeNumbers sliders,
        /// percent format for 0-100 range, and one-decimal format otherwise.
        /// </summary>
        protected string FormatSliderValue(KSlider slider)
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
    }
}
