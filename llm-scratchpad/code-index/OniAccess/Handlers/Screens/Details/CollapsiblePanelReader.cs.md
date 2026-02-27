namespace OniAccess.Handlers.Screens.Details

// Reads a CollapsibleDetailContentPanel into a DetailSection.
// Shared by PropertiesTab and PersonalityTab — both panels use the same
// SetLabel/Commit pattern with DetailLabel children under Content.

static class CollapsiblePanelReader (line 12)
  // Walk active DetailLabel children in the panel's Content transform.
  // Entries whose text starts with a space are folded into the preceding
  // non-indented entry's SpeechFunc (spoken as one item, joined with spaces).
  public static DetailSection BuildSection(CollapsibleDetailContentPanel gameSection) (line 18)
