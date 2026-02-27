// File-level comment: Base class for all navigable UI widgets. Subclasses define
// type-specific behavior (speech, validity, activation, adjustment)
// so dispatch happens via virtual calls instead of switch statements.

namespace OniAccess.Widgets

class Widget (line 9)
  string Label { get; set; } (line 10)
  UnityEngine.Component Component { get; set; } (line 11)
  UnityEngine.GameObject GameObject { get; set; } (line 12)
  object Tag { get; set; } (line 13)
  System.Func<string> SpeechFunc { get; set; } (line 14)
    // When set, short-circuits GetSpeechText() before subclass logic runs.
  bool SuppressTooltip { get; set; } (line 15)
  List<Widget> Children { get; set; } (line 16)
    // Populated for drillable parent widgets (groups, categories). Used by navigation to enter a sub-list.

  virtual string GetSpeechText() (line 22)
    // SpeechFunc short-circuits if set and returns non-empty text; otherwise delegates to Label.
  virtual bool IsValid() (line 33)
    // Returns false if GameObject is inactive in hierarchy; true if Component or GameObject is non-null.
  virtual bool Activate() (line 41)
    // Activate the widget (click, toggle, begin editing). Returns true if handled. Base returns false.
  virtual bool Adjust(int direction, int stepLevel) (line 47)
    // Adjust the widget's value (slider step, dropdown cycle). Returns true if value changed. Base returns false.
  virtual bool IsAdjustable (line 52)
    // Whether this widget supports Left/Right adjustment. Base returns false.
