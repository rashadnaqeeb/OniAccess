// Shared helpers for widget discovery across handlers.

namespace OniAccess.Widgets

static class WidgetDiscoveryUtil (line 8)
  static void TryAddButtonField(KScreen screen, string fieldName, string fallbackLabel, List<Widget> widgets) (line 14)
    // Reads a KButton from a named private/internal field on screen using Harmony Traverse.
    // Skips if the field doesn't exist or the button's GameObject is inactive in hierarchy.
    // Label resolution: reads child LocText via GetParsedText(), then .text, then falls back to fallbackLabel.
    // SpeechFunc closes over the captured button and calls WidgetOps.GetButtonLabel(captured, fallbackLabel) live.
    // Exceptions are caught and logged via Log.Error (never silent).
