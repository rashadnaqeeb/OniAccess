// Stateless utility methods for widget speech, tooltip reading, validity
// checking, and programmatic interaction. Extracted from BaseWidgetHandler
// so that any handler (including NestedMenuHandler-based ones like
// DetailsScreenHandler) can reuse them without inheritance.

namespace OniAccess.Widgets

static class WidgetOps (line 8)

  // ---- SPEECH ----

  static string GetSpeechText(Widget widget) (line 16)
    // Delegates to widget.GetSpeechText() then passes result through CleanTooltipEntry().

  // ---- TOOLTIP ----

  static string GetTooltipText(Widget widget) (line 28)
    // Returns null if SuppressTooltip or GameObject is null.
    // Searches for ToolTip component on GameObject, then children, then parents.
    // Returns ReadAllTooltipText() result or null.

  static string AppendTooltip(string speech, string tooltip) (line 46)
    // Appends tooltip to speech with ", " separator. Skips if tooltip duplicates
    // an existing comma-separated segment already present in speech.

  static string ReadAllTooltipText(ToolTip tooltip) (line 59)
    // Calls tooltip.RebuildDynamicTooltip(). Collects all multiString entries,
    // passes each through CleanTooltipEntry, and joins with ". " sentence boundaries.
    // Returns null if all entries are empty.

  private static string CleanTooltipEntry(string text) (line 88)
    // Replaces bullet characters (U+2022) with ". " and newlines with ". ".
    // Trims leading whitespace, collapses doubled periods.
    // Makes tooltip text pause naturally between fields for a screen reader.

  // ---- VALIDITY ----

  static bool IsValid(Widget widget) (line 117)
    // Returns false if widget is null; otherwise delegates to widget.IsValid().

  // ---- MULTI-TOGGLE STATE ----

  static string GetMultiToggleState(MultiToggle mt) (line 132)
    // Maps mt.CurrentState to a speech string. Two modes based on state count:
    //   4-state (ReceptacleSideScreen, mutation panel):
    //     state 0 = off, 1 = selected, 2 = disabled, 3 = selected+disabled
    //   2/3-state:
    //     0 = off, last = on, middle = mixed

  // ---- SLIDER FORMATTING ----

  static string FormatSliderValue(KSlider slider) (line 157)
    // For whole-number sliders: returns int string.
    // For sliders with range [0..1] or [0..100]: returns GetFormattedPercent().
    // Otherwise: returns value.ToString("F1").

  // ---- INTERACTION ----

  static void ClickButton(KButton button) (line 174)
    // Plays pointer-down sound then signals a left-click via SignalClick(KKeyCode.Mouse0).

  static void ClickMultiToggle(MultiToggle toggle) (line 179)
    // Synthesizes a PointerEventData with left button / clickCount=1 and
    // calls toggle.OnPointerDown then toggle.OnPointerClick.

  static string GetButtonLabel(KButton button, string fallback = null) (line 192)
    // Returns GetParsedText() from child LocText, then .text, then fallback.
