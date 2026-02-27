# ConsumedKey.cs

## Overview
Simple value type representing a key+modifier combination that a handler
pre-consumes at the KButtonEvent level (so the game never sees it).
Used via `IAccessHandler.ConsumedKeys`.

---

```
struct ConsumedKey (line 2)

  // Fields
  KKeyCode KeyCode    (line 3)
  Modifier Modifier   (line 4)

  // Constructor
  ConsumedKey(KKeyCode keyCode, Modifier modifier = Modifier.None) (line 6)
```
