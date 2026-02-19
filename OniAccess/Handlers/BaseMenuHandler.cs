using System.Collections.Generic;

using OniAccess.Input;

namespace OniAccess.Handlers {
	/// <summary>
	/// Reusable 1D list navigation base extending BaseScreenHandler.
	/// Provides arrow-key navigation with wrap-around, Home/End, Enter activation,
	/// Left/Right adjustment, Tab stubs, and A-Z type-ahead search.
	///
	/// Accepts a null KScreen because it serves both screen-bound widget handlers
	/// (via BaseWidgetHandler) and lightweight overlay browsers like TooltipBrowserHandler
	/// and HelpHandler that have no KScreen.
	///
	/// Subclasses implement ItemCount, GetItemLabel, and SpeakCurrentItem to describe
	/// their list. Override ActivateCurrentItem, AdjustCurrentItem, and NavigateTab*
	/// for interaction behavior.
	/// </summary>
	public abstract class BaseMenuHandler: BaseScreenHandler, ISearchable {
		protected int _currentIndex;
		protected readonly TypeAheadSearch _search = new TypeAheadSearch();

		protected BaseMenuHandler(KScreen screen = null) : base(screen) { }

		/// <summary>
		/// Menus are modal: block all input from reaching handlers below.
		/// </summary>
		public override bool CapturesAllInput => true;

		// ========================================
		// ABSTRACT: LIST DESCRIPTION
		// ========================================

		/// <summary>
		/// Number of items in the navigable list.
		/// </summary>
		public abstract int ItemCount { get; }

		/// <summary>
		/// Searchable/speakable label for the item at the given index.
		/// </summary>
		public abstract string GetItemLabel(int index);

		/// <summary>
		/// Speak the currently focused item via SpeakInterrupt.
		/// </summary>
		public abstract void SpeakCurrentItem();

		// ========================================
		// VIRTUAL HOOKS
		// ========================================

		/// <summary>
		/// Whether the item at the given index is valid for navigation.
		/// Default returns true. Widget handlers override to check component state.
		/// </summary>
		protected virtual bool IsItemValid(int index) => true;

		/// <summary>
		/// Activate the currently focused item (Enter key). No-op default.
		/// </summary>
		protected virtual void ActivateCurrentItem() { }

		/// <summary>
		/// Adjust the currently focused item's value (Left/Right). No-op default.
		/// </summary>
		protected virtual void AdjustCurrentItem(int direction, bool isLargeStep) { }

		/// <summary>
		/// Navigate to next tab section. No-op default for non-tabbed screens.
		/// </summary>
		protected virtual void NavigateTabForward() { }

		/// <summary>
		/// Navigate to previous tab section. No-op default.
		/// </summary>
		protected virtual void NavigateTabBackward() { }

		// ========================================
		// COMPOSABLE HELP ENTRY LISTS
		// ========================================

		/// <summary>
		/// Help entries for menu-specific features (search).
		/// </summary>
		protected static readonly List<HelpEntry> MenuHelpEntries = new List<HelpEntry> {
			new HelpEntry("A-Z", STRINGS.ONIACCESS.HELP.TYPE_SEARCH),
		};

		/// <summary>
		/// Help entries for 1D list navigation.
		/// </summary>
		protected static readonly List<HelpEntry> ListNavHelpEntries = new List<HelpEntry> {
			new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
			new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
			new HelpEntry("Enter", STRINGS.ONIACCESS.HELP.SELECT_ITEM),
			new HelpEntry("Left/Right", STRINGS.ONIACCESS.HELP.ADJUST_VALUE),
			new HelpEntry("Shift+Left/Right", STRINGS.ONIACCESS.HELP.ADJUST_VALUE_LARGE),
		};

		/// <summary>
		/// Build a help entry list combining menu + list-nav entries with optional extras.
		/// </summary>
		protected IReadOnlyList<HelpEntry> BuildHelpEntries(params HelpEntry[] extra) {
			var list = new List<HelpEntry>();
			list.AddRange(MenuHelpEntries);
			list.AddRange(ListNavHelpEntries);
			list.AddRange(extra);
			return list;
		}

		// ========================================
		// NAVIGATION METHODS
		// ========================================

		/// <summary>
		/// Move to next item with wrap-around. Skips invalid items.
		/// </summary>
		protected void NavigateNext() {
			if (ItemCount == 0) return;
			int start = _currentIndex;
			for (int i = 0; i < ItemCount; i++) {
				int candidate = (start + 1 + i) % ItemCount;
				if (IsItemValid(candidate)) {
					bool wrapped = candidate <= _currentIndex;
					_currentIndex = candidate;
					if (wrapped) PlayWrapSound();
					else PlayHoverSound();
					SpeakCurrentItem();
					return;
				}
			}
		}

		/// <summary>
		/// Move to previous item with wrap-around. Skips invalid items.
		/// </summary>
		protected void NavigatePrev() {
			if (ItemCount == 0) return;
			int start = _currentIndex;
			for (int i = 0; i < ItemCount; i++) {
				int candidate = (start - 1 - i + ItemCount) % ItemCount;
				if (IsItemValid(candidate)) {
					bool wrapped = candidate >= _currentIndex;
					_currentIndex = candidate;
					if (wrapped) PlayWrapSound();
					else PlayHoverSound();
					SpeakCurrentItem();
					return;
				}
			}
		}

		/// <summary>
		/// Jump to first valid item.
		/// </summary>
		protected void NavigateFirst() {
			if (ItemCount == 0) return;
			for (int i = 0; i < ItemCount; i++) {
				if (IsItemValid(i)) {
					_currentIndex = i;
					PlayHoverSound();
					SpeakCurrentItem();
					return;
				}
			}
		}

		/// <summary>
		/// Jump to last valid item.
		/// </summary>
		protected void NavigateLast() {
			if (ItemCount == 0) return;
			for (int i = ItemCount - 1; i >= 0; i--) {
				if (IsItemValid(i)) {
					_currentIndex = i;
					PlayHoverSound();
					SpeakCurrentItem();
					return;
				}
			}
		}

		// ========================================
		// SOUNDS
		// ========================================

		protected void PlayWrapSound() {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound("Negative"));
			} catch (System.Exception ex) {
				Util.Log.Error($"PlayWrapSound failed: {ex.Message}");
			}
		}

		protected void PlayHoverSound() {
			try {
				KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Mouseover"));
			} catch (System.Exception ex) {
				Util.Log.Error($"PlayHoverSound failed: {ex.Message}");
			}
		}

		// ========================================
		// SEARCH
		// ========================================

		private static readonly UnityEngine.KeyCode[] _searchNavKeys = {
			UnityEngine.KeyCode.UpArrow, UnityEngine.KeyCode.DownArrow,
			UnityEngine.KeyCode.Home, UnityEngine.KeyCode.End,
			UnityEngine.KeyCode.Backspace,
		};

		/// <summary>
		/// Route keys through _search.HandleKey before standard navigation.
		/// Returns true if the search consumed the key.
		/// </summary>
		protected bool TryRouteToSearch(bool ctrlHeld, bool altHeld) {
			// A-Z (no modifiers) — start or continue search
			if (!ctrlHeld && !altHeld) {
				for (var k = UnityEngine.KeyCode.A; k <= UnityEngine.KeyCode.Z; k++) {
					if (UnityEngine.Input.GetKeyDown(k))
						return _search.HandleKey(k, ctrlHeld, altHeld, this);
				}
			}

			// Navigation keys captured by search when active
			for (int i = 0; i < _searchNavKeys.Length; i++) {
				if (UnityEngine.Input.GetKeyDown(_searchNavKeys[i]))
					return _search.HandleKey(_searchNavKeys[i], ctrlHeld, altHeld, this);
			}

			return false;
		}

		// ========================================
		// LIFECYCLE
		// ========================================

		/// <summary>
		/// Speaks DisplayName (via BaseScreenHandler), resets cursor and search.
		/// </summary>
		public override void OnActivate() {
			base.OnActivate();
			_currentIndex = 0;
			_search.Clear();
		}

		/// <summary>
		/// Resets cursor and search.
		/// </summary>
		public override void OnDeactivate() {
			base.OnDeactivate();
			_currentIndex = 0;
			_search.Clear();
		}

		// ========================================
		// TICK: KEY DETECTION
		// ========================================

		/// <summary>
		/// Per-frame key detection for navigation and type-ahead search.
		/// Subclasses should call base.Tick() to get navigation handling.
		/// </summary>
		public override void Tick() {
			base.Tick();

			bool ctrlHeld = InputUtil.CtrlHeld();
			bool altHeld = InputUtil.AltHeld();

			if (TryRouteToSearch(ctrlHeld, altHeld))
				return;

			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.DownArrow)) {
				NavigateNext();
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.UpArrow)) {
				NavigatePrev();
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Home)) {
				NavigateFirst();
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.End)) {
				NavigateLast();
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return)) {
				ActivateCurrentItem();
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Tab)) {
				if (InputUtil.ShiftHeld())
					NavigateTabBackward();
				else
					NavigateTabForward();
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.LeftArrow)) {
				AdjustCurrentItem(-1, InputUtil.ShiftHeld());
				return;
			}
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.RightArrow)) {
				AdjustCurrentItem(1, InputUtil.ShiftHeld());
				return;
			}
		}

		/// <summary>
		/// Intercept Escape via KButtonEvent when search is active.
		/// Clears search instead of letting the game close the screen.
		/// </summary>
		public override bool HandleKeyDown(KButtonEvent e) {
			if (_search.IsSearchActive && e.TryConsume(Action.Escape)) {
				_search.Clear();
				Speech.SpeechPipeline.SpeakInterrupt(STRINGS.ONIACCESS.SEARCH.CLEARED);
				return true;
			}

			return false;
		}

		// ========================================
		// ISearchable IMPLEMENTATION
		// ========================================

		public int SearchItemCount => ItemCount;

		public int SearchCurrentIndex => _currentIndex;

		public string GetSearchLabel(int index) {
			if (index < 0 || index >= ItemCount) return null;
			return Speech.TextFilter.FilterForSpeech(GetItemLabel(index));
		}

		public void SearchMoveTo(int index) {
			if (index < 0 || index >= ItemCount) return;
			_currentIndex = index;
			SpeakCurrentItem();
		}
	}
}
