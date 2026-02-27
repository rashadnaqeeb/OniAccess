# TypeAheadSearch.cs

## File-level comment
Reusable type-ahead search helper for keyboard navigation. Builds a filtered
results list (tiered matching) that can be navigated with Up/Down. Match
priority (tier 0 = best):
- 0: start-of-string whole word
- 1: start-of-string prefix
- 2: mid-string whole word
- 3: mid-string prefix
- 4: substring anywhere

Use `HandleKey()` with an `ISearchable` for centralized search behavior, or
the lower-level API (`AddChar`/`Search`/`NavigateResults`) for custom handling.

Assembly attribute: `[InternalsVisibleTo("OniAccess.Tests")]` (line 6)

---

```
class TypeAheadSearch (line 17)

  // Private fields
  private StringBuilder _buffer                           (line 18)
  // Current search buffer text.

  private float _lastTime                                 (line 19)
  // Time of last key input; used for timeout detection.

  private bool _isSearchActive                            (line 22)
  private List<int> _resultIndices                        (line 23)
  // Original-list indices of current filtered results.

  private List<string> _resultNames                       (line 24)
  private int _resultCursor                               (line 25)
  // Current position within _resultIndices.

  private const int TierCount = 5                         (line 28)
  private List<int>[] _tierIndices                        (line 29)
  private List<string>[] _tierNames                       (line 30)
  // Per-tier working lists for search; pre-allocated to avoid per-search allocation.

  private List<int> _workIndices                          (line 31)
  private List<string> _workNames                         (line 32)
  // Swap-buffer working lists merged from tier lists before replacing _resultIndices.

  private System.Action<int> _announceResult              (line 35)
  // Optional callback set by Search(); called with original index on each result navigation.

  private readonly System.Func<int, string> _getLabelCached    (line 38)
  private readonly System.Action<int> _moveToIndexCached       (line 39)
  // Cached delegates for RunSearch to avoid per-call allocation.

  private ISearchable _searchable                         (line 42)
  // Current searchable context, updated each HandleKey call.

  private readonly System.Func<float> _getTime            (line 45)
  // Injectable time source; defaults to Time.realtimeSinceStartup.

  // Constructors
  TypeAheadSearch()                                       (line 47)
  internal TypeAheadSearch(System.Func<float> timeSource) (line 49)
  // Internal constructor accepts a custom time source for offline tests.

  private static float DefaultGetTime()                   (line 61)

  // Properties
  float Timeout { get; set; }                             (line 66)
  // Seconds before buffer resets on new input. Default 1.5f.

  string Buffer { get; }                                  (line 71)
  bool HasBuffer { get; }                                 (line 76)
  bool IsSearchActive { get; }                            (line 82)
  int ResultCount { get; }                                (line 87)

  int SelectedOriginalIndex { get; }                      (line 92)
  // The original-list index of the currently selected result, or -1 if none.

  // Public API
  string AddChar(char c)                                  (line 101)
  // Appends c to buffer. Resets buffer first if timeout has elapsed.
  // Returns updated buffer string.

  bool RemoveChar()                                       (line 113)
  // Removes last character from buffer (backspace behavior).
  // Returns false if buffer was already empty.

  void Clear()                                            (line 125)
  // Clears buffer, results, and search-active flag.

  // Centralized search behavior
  bool HandleKey(KeyCode keyCode, bool ctrlHeld, bool altHeld, ISearchable searchable) (line 147)
  // Main entry point for search-aware key routing. Call this after modifier
  // shortcuts but before standard navigation.
  //
  // When search is active:
  //   Up/Down: NavigateResults. Home/End: JumpToFirst/Last.
  //   Escape: Clear + announce cleared. Backspace: RemoveChar + re-search or Clear.
  //   A-Z (no mods): AddChar + re-search.
  //   Any other key: Clear search (cursor stays at result), return false so handler
  //     processes the key normally.
  //
  // When search is inactive:
  //   A-Z (no mods): start search if SearchItemCount > 0.
  //   Backspace with leftover buffer: continue editing.
  //
  // Returns true if the key was consumed by search.

  private void RunSearch()                                (line 221)
  // Calls Search using _searchable's item count and cached delegates.

  void Search(int itemCount, Func<int, string> nameByIndex, Action<int> announceResult = null) (line 233)
  // Performs tiered search. Handles the "repeat single-letter to cycle" pattern:
  // typing the same letter again when results exist cycles through them rather than
  // extending the buffer. Announces no-match or the first result. Swaps working
  // lists into result lists to avoid re-allocation.

  void NavigateResults(int direction)                     (line 305)
  // Moves _resultCursor by direction (wrapping), announces result.

  void JumpToFirstResult()                                (line 316)
  void JumpToLastResult()                                 (line 325)

  private void AnnounceCurrentResult()                    (line 333)
  // Calls _announceResult if set, otherwise SpeakInterrupt on the result name.

  private static bool IsAllSameChar(string s)             (line 342)
  // Returns true if every character in s is the same (e.g., "bbb").

  internal static int MatchTier(string lowerName, string lowerPrefix) (line 358)
  // Returns the match tier (0-4) or -1 for no match. Internal for test access.
  // Tier 0: start of string + whole word (boundary = space/comma or end).
  // Tier 1: start of string + prefix.
  // Tier 2: mid-string word start + whole word.
  // Tier 3: mid-string word start + prefix.
  // Tier 4: substring anywhere.
```
