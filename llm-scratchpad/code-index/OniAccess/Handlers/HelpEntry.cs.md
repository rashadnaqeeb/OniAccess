# HelpEntry.cs

## File-level comment
Simple data class for help list entries. Each handler provides its own list
via `IAccessHandler.HelpEntries`. Displayed in the F12 navigable help list.

---

```
sealed class HelpEntry (line 7)

  // Properties
  string KeyName { get; }       (line 8)
  string Description { get; }   (line 9)

  // Constructor
  HelpEntry(string keyName, string description) (line 11)

  // Methods
  override string ToString()    (line 16)
  // Returns "{KeyName}: {Description}".
```
