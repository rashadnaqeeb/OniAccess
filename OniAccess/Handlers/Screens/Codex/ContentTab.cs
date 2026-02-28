using System.Collections.Generic;

using HarmonyLib;

using OniAccess.Input;
using OniAccess.Speech;

namespace OniAccess.Handlers.Screens.Codex {
	/// <summary>
	/// Content tab: flat BaseMenuHandler that reads through article widgets
	/// sequentially. Rebuilt from CodexCache on each ChangeArticle call.
	/// Ctrl+Up/Down jumps between section headings (Title, Subtitle,
	/// CollapsibleHeader). Enter follows links.
	/// </summary>
	internal class ContentTab : BaseMenuHandler, ICodexTab {
		private readonly CodexScreenHandler _parent;

		/// <summary>
		/// Flat list of readable items in the current article.
		/// Rebuilt on every article change from CodexCache data.
		/// </summary>
		private readonly List<ContentItem> _items = new List<ContentItem>();

		internal ContentTab(CodexScreenHandler parent) : base(screen: null) {
			_parent = parent;
		}

		public string TabName => (string)STRINGS.ONIACCESS.CODEX.CONTENT_TAB;

		public override string DisplayName => TabName;

		private static readonly List<HelpEntry> _helpEntries = new List<HelpEntry> {
			new HelpEntry("Up/Down", STRINGS.ONIACCESS.HELP.NAVIGATE_ITEMS),
			new HelpEntry("Ctrl+Up/Down", STRINGS.ONIACCESS.HELP.JUMP_GROUP),
			new HelpEntry("Home/End", STRINGS.ONIACCESS.HELP.JUMP_FIRST_LAST),
			new HelpEntry("Enter", STRINGS.ONIACCESS.CODEX.FOLLOW_LINK_HELP),
			new HelpEntry("Alt+Left/Backspace", STRINGS.ONIACCESS.CODEX.HISTORY_BACK_HELP),
			new HelpEntry("Alt+Right", STRINGS.ONIACCESS.CODEX.HISTORY_FORWARD_HELP),
		};

		public override IReadOnlyList<HelpEntry> HelpEntries => _helpEntries;

		// ========================================
		// ICodexTab
		// ========================================

		public void OnTabActivated(bool announce) {
			_currentIndex = 0;
			_search.Clear();
			SuppressSearchThisFrame();
			if (announce)
				SpeechPipeline.SpeakInterrupt(TabName);
			RebuildWidgetList();
			SpeakFirstItem();
		}

		public void OnTabDeactivated() {
			_search.Clear();
		}

		public bool HandleInput() {
			if (TryHistoryNavigation())
				return true;
			return base.Tick();
		}

		public new bool HandleKeyDown(KButtonEvent e) {
			return base.HandleKeyDown(e);
		}

		// ========================================
		// Article change notification
		// ========================================

		/// <summary>
		/// Called by CodexScreenHandler when ChangeArticle fires.
		/// Rebuilds the widget list and resets cursor.
		/// </summary>
		internal void OnArticleChanged() {
			_currentIndex = 0;
			_search.Clear();
			SuppressSearchThisFrame();
			RebuildWidgetList();
			SpeakFirstItem();
		}

		// ========================================
		// BaseMenuHandler abstracts
		// ========================================

		public override int ItemCount => _items.Count;

		public override string GetItemLabel(int index) {
			if (index < 0 || index >= _items.Count) return null;
			return _items[index].text;
		}

		public override void SpeakCurrentItem(string parentContext = null) {
			if (_items.Count == 0) {
				SpeechPipeline.SpeakInterrupt(STRINGS.ONIACCESS.CODEX.NO_ARTICLE);
				return;
			}
			if (_currentIndex < 0 || _currentIndex >= _items.Count) return;
			string text = _items[_currentIndex].text;
			if (string.IsNullOrEmpty(text)) return;
			if (!string.IsNullOrEmpty(parentContext))
				text = parentContext + ", " + text;
			SpeechPipeline.SpeakInterrupt(text);
		}

		// ========================================
		// Section jumping (Ctrl+Up/Down)
		// ========================================

		protected override void JumpNextGroup() {
			for (int i = _currentIndex + 1; i < _items.Count; i++) {
				if (_items[i].isHeading) {
					_currentIndex = i;
					PlayHoverSound();
					SpeakCurrentItem();
					return;
				}
			}
			// Wrap to first heading
			for (int i = 0; i < _currentIndex; i++) {
				if (_items[i].isHeading) {
					_currentIndex = i;
					PlayWrapSound();
					SpeakCurrentItem();
					return;
				}
			}
		}

		protected override void JumpPrevGroup() {
			for (int i = _currentIndex - 1; i >= 0; i--) {
				if (_items[i].isHeading) {
					_currentIndex = i;
					PlayHoverSound();
					SpeakCurrentItem();
					return;
				}
			}
			// Wrap to last heading
			for (int i = _items.Count - 1; i > _currentIndex; i--) {
				if (_items[i].isHeading) {
					_currentIndex = i;
					PlayWrapSound();
					SpeakCurrentItem();
					return;
				}
			}
		}

		// ========================================
		// Activation (Enter): links and videos
		// ========================================

		protected override void ActivateCurrentItem() {
			if (_currentIndex < 0 || _currentIndex >= _items.Count) return;

			var item = _items[_currentIndex];

			if (item.video != null) {
				PlayVideo(item.video);
				return;
			}

			var links = item.links;
			if (links == null || links.Count == 0) return;

			if (links.Count == 1) {
				FollowLink(links[0].id);
				return;
			}

			// Multiple links: open popup sub-menu
			var linkMenu = new LinkMenuHandler(_parent, links);
			HandlerStack.Push(linkMenu);
		}

		private static void PlayVideo(CodexVideo video) {
			var clip = Assets.GetVideo(video.name);
			if (clip == null) return;
			VideoScreen.Instance.PlayVideo(clip);
			if (!string.IsNullOrEmpty(video.overlayName))
				VideoScreen.Instance.SetOverlayText(video.overlayName, video.overlayTexts);
		}

		internal void FollowLink(string entryId) {
			var codexScreen = _parent.CodexScreen;
			if (codexScreen == null) return;
			PlayOpenSound();
			codexScreen.ChangeArticle(entryId);
		}

		// ========================================
		// History navigation (Alt+Left / Backspace / Alt+Right)
		// ========================================

		private bool TryHistoryNavigation() {
			bool altHeld = InputUtil.AltHeld();

			bool back = (altHeld && UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.LeftArrow))
				|| UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Backspace);
			bool forward = altHeld && UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.RightArrow);

			if (!back && !forward) return false;

			var codexScreen = _parent.CodexScreen;
			if (codexScreen == null) return false;

			var t = Traverse.Create(codexScreen);
			int idx = t.Field<int>("currentHistoryIdx").Value;
			var history = t.Field<List<CodexScreen.HistoryEntry>>("history").Value;

			if (back) {
				if (idx <= 0) {
					SpeechPipeline.SpeakInterrupt(STRINGS.ONIACCESS.CODEX.NO_BACK);
					return true;
				}
				t.Method("HistoryStepBack").GetValue();
			} else {
				if (idx >= history.Count - 1) {
					SpeechPipeline.SpeakInterrupt(STRINGS.ONIACCESS.CODEX.NO_FORWARD);
					return true;
				}
				t.Method("HistoryStepForward").GetValue();
			}

			return true;
		}

		// ========================================
		// Widget list building
		// ========================================

		private void RebuildWidgetList() {
			_items.Clear();

			var codexScreen = _parent.CodexScreen;
			if (codexScreen == null) return;

			string entryId = codexScreen.activeEntryID;
			if (string.IsNullOrEmpty(entryId)) return;
			if (!CodexCache.entries.TryGetValue(entryId, out var entry)) return;
			if (entry.contentContainers == null) return;

			string lastLockedId = null;
			foreach (var cc in entry.contentContainers) {
				if (!CodexHelper.IsContainerVisible(cc)) continue;

				if (CodexHelper.IsContainerLocked(cc)) {
					// Only announce locked content once per lockID
					if (cc.lockID != lastLockedId) {
						lastLockedId = cc.lockID;
						_items.Add(new ContentItem {
							text = (string)STRINGS.ONIACCESS.CODEX.LOCKED_CONTENT,
							isHeading = false,
							links = null,
						});
					}
					continue;
				}
				lastLockedId = null;

				if (cc.content == null) continue;
				foreach (var widget in cc.content) {
					if (!CodexHelper.IsWidgetVisible(widget)) continue;

					// CodexElementCategoryList expands to header + individual elements
					if (widget is CodexElementCategoryList ecl) {
						AddElementCategoryItems(ecl);
						continue;
					}

					string text = WidgetTextExtractor.GetText(widget, entryId);
					if (text == null) continue;

					_items.Add(new ContentItem {
						text = text,
						isHeading = WidgetTextExtractor.IsSectionHeading(widget),
						links = WidgetTextExtractor.GetLinks(widget),
						video = widget as CodexVideo,
					});
				}
			}
		}

		private void AddElementCategoryItems(CodexElementCategoryList widget) {
			var items = WidgetTextExtractor.GetElementCategoryItems(widget);
			foreach (var (text, isHeader) in items) {
				_items.Add(new ContentItem {
					text = text,
					isHeading = isHeader,
					links = null,
				});
			}
		}

		private void SpeakFirstItem() {
			if (_items.Count == 0) {
				SpeechPipeline.SpeakQueued(STRINGS.ONIACCESS.CODEX.NO_ARTICLE);
				return;
			}
			string text = _items[0].text;
			if (!string.IsNullOrEmpty(text))
				SpeechPipeline.SpeakQueued(text);
		}

		// ========================================
		// Content item
		// ========================================

		private struct ContentItem {
			internal string text;
			internal bool isHeading;
			internal List<(string id, string text)> links;
			internal CodexVideo video;
		}
	}
}
