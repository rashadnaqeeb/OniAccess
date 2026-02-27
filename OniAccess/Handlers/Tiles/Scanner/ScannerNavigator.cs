using System.Collections.Generic;
using OniAccess.Handlers.Tiles.Scanner.Backends;
using OniAccess.Handlers.Tiles.Scanner.Routing;
using OniAccess.Speech;
using OniAccess.Util;

namespace OniAccess.Handlers.Tiles.Scanner {
	/// <summary>
	/// Owns scanner navigation state and orchestrates refresh, navigation,
	/// and teleport. Called from TileCursorHandler.Tick() for keybind dispatch.
	/// </summary>
	public class ScannerNavigator {
		private ScannerSnapshot _snapshot;
		private int _categoryIndex;
		private int _subcategoryIndex;
		private int _itemIndex;
		private int _instanceIndex;
		private int _lastWorldId = -1;

		// Backends
		private readonly GridScanner _gridScanner;
		private readonly ElementClusterBackend _elementBackend = new ElementClusterBackend();
		private readonly TileClusterBackend _tileBackend = new TileClusterBackend();
		private readonly NetworkSegmentBackend _networkBackend = new NetworkSegmentBackend();
		private readonly OrderBackend _orderBackend = new OrderBackend();
		private readonly BiomeBackend _biomeBackend = new BiomeBackend();
		private readonly EntityBackend _entityBackend;
		private readonly GeyserBackend _geyserBackend = new GeyserBackend();
		private readonly RoomBackend _roomBackend = new RoomBackend();

		// LocString lookup for spoken names — store LocString, cast at speech time
		private static readonly Dictionary<string, LocString> _categoryNames = BuildCategoryNames();
		private static readonly Dictionary<string, LocString> _subcategoryNames = BuildSubcategoryNames();

		public ScannerNavigator() {
			var biomeResolver = new BiomeNameResolver();
			_gridScanner = new GridScanner(biomeResolver);
			_entityBackend = new EntityBackend(new BuildingRouter());
		}

		/// <summary>
		/// Check for asteroid switch every tick. If the world changed,
		/// clear the snapshot so the next navigation key triggers a refresh.
		/// </summary>
		public void CheckWorldSwitch() {
			int worldId = ClusterManager.Instance.activeWorld.id;
			if (_lastWorldId >= 0 && worldId != _lastWorldId)
				_snapshot = null;
			_lastWorldId = worldId;
		}

		/// <summary>
		/// Refresh the scan. Runs all backends, builds a new snapshot.
		/// Preserves categoryIndex if the same category still exists.
		/// </summary>
		public void Refresh() {
			int worldId = ClusterManager.Instance.activeWorld.id;
			int cursorCell = TileCursor.Instance.Cell;
			_lastWorldId = worldId;

			string previousCategory = _snapshot != null && _categoryIndex < _snapshot.CategoryCount
				? _snapshot.GetCategory(_categoryIndex).Name
				: null;

			var allEntries = new List<ScanEntry>();
			bool success = false;

			try {
				var gridResult = _gridScanner.Scan(worldId);

				_elementBackend.SetGridData(gridResult.Elements);
				_tileBackend.SetGridData(gridResult.Tiles);
				_networkBackend.SetGridData(gridResult.NetworkSegments, gridResult.Bridges);
				_orderBackend.SetGridData(gridResult.OrderClusters, gridResult.IndividualOrders);
				_biomeBackend.SetGridData(gridResult.Biomes);

				allEntries.AddRange(_elementBackend.Scan(worldId));
				allEntries.AddRange(_tileBackend.Scan(worldId));
				allEntries.AddRange(_networkBackend.Scan(worldId));
				allEntries.AddRange(_orderBackend.Scan(worldId));
				allEntries.AddRange(_biomeBackend.Scan(worldId));
				allEntries.AddRange(_entityBackend.Scan(worldId));
				allEntries.AddRange(_geyserBackend.Scan(worldId));
				allEntries.AddRange(_roomBackend.Scan(worldId));
				success = true;
			} catch (System.Exception ex) {
				Log.Error($"ScannerNavigator.Refresh: {ex}");
				allEntries.Clear();
			}

			_snapshot = new ScannerSnapshot(allEntries, cursorCell);

			// Preserve category position if possible
			_categoryIndex = 0;
			if (previousCategory != null) {
				for (int i = 0; i < _snapshot.CategoryCount; i++) {
					if (_snapshot.GetCategory(i).Name == previousCategory) {
						_categoryIndex = i;
						break;
					}
				}
			}
			_subcategoryIndex = 0;
			_itemIndex = 0;
			_instanceIndex = 0;

			if (success)
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.SCANNER.REFRESHED);
			else
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.SCANNER.EMPTY);
		}

		public void CycleCategory(int direction) {
			if (EnsureSnapshot()) return;
			if (_snapshot.CategoryCount == 0) {
				SpeakEmpty();
				return;
			}

			int prev = _categoryIndex;
			_categoryIndex = Wrap(_categoryIndex, direction, _snapshot.CategoryCount);
			if (_categoryIndex != prev || _snapshot.CategoryCount == 1)
				PlayWrapCheck(prev, _categoryIndex, direction, _snapshot.CategoryCount);

			_subcategoryIndex = 0;
			_itemIndex = 0;
			_instanceIndex = 0;

			var cat = _snapshot.GetCategory(_categoryIndex);
			string catName = GetCategoryName(cat.Name);
			SpeechPipeline.SpeakInterrupt(catName);

			// Queue first item announcement
			string itemAnnouncement = ValidateAndAnnounce(speakOnEmpty: false);
			if (itemAnnouncement != null)
				SpeechPipeline.SpeakQueued(itemAnnouncement);
		}

		public void CycleSubcategory(int direction) {
			if (EnsureSnapshot()) return;
			if (_snapshot.CategoryCount == 0) {
				SpeakEmpty();
				return;
			}

			var cat = _snapshot.GetCategory(_categoryIndex);
			int prev = _subcategoryIndex;
			_subcategoryIndex = WrapSkipEmpty(
				_subcategoryIndex, direction, cat.Subcategories,
				sub => sub.Items.Count > 0);
			PlayWrapCheck(prev, _subcategoryIndex, direction, cat.Subcategories.Count);

			_itemIndex = 0;
			_instanceIndex = 0;

			var sub = cat.Subcategories[_subcategoryIndex];
			string subName = GetSubcategoryName(sub.Name);
			SpeechPipeline.SpeakInterrupt(subName);

			string itemAnnouncement = ValidateAndAnnounce(speakOnEmpty: false);
			if (itemAnnouncement != null)
				SpeechPipeline.SpeakQueued(itemAnnouncement);
		}

		public void CycleItem(int direction) {
			if (EnsureSnapshot()) return;
			if (_snapshot.CategoryCount == 0) {
				SpeakEmpty();
				return;
			}

			var sub = CurrentSubcategory();
			if (sub == null || sub.Items.Count == 0) {
				SpeakEmpty();
				return;
			}

			int prev = _itemIndex;
			_itemIndex = Wrap(_itemIndex, direction, sub.Items.Count);
			PlayWrapCheck(prev, _itemIndex, direction, sub.Items.Count);

			_instanceIndex = 0;

			string announcement = ValidateAndAnnounce();
			if (announcement != null)
				SpeechPipeline.SpeakInterrupt(announcement);
		}

		public void CycleInstance(int direction) {
			if (EnsureSnapshot()) return;
			if (_snapshot.CategoryCount == 0) {
				SpeakEmpty();
				return;
			}

			var item = CurrentItem();
			if (item == null || item.Instances.Count == 0) {
				SpeakEmpty();
				return;
			}

			int prev = _instanceIndex;
			_instanceIndex = Wrap(_instanceIndex, direction, item.Instances.Count);
			PlayWrapCheck(prev, _instanceIndex, direction, item.Instances.Count);

			string announcement = ValidateAndAnnounce();
			if (announcement != null)
				SpeechPipeline.SpeakInterrupt(announcement);
		}

		public void Teleport() {
			if (_snapshot == null || _snapshot.CategoryCount == 0) return;

			var entry = CurrentEntry();
			if (entry == null) return;

			int cursorCell = TileCursor.Instance.Cell;
			if (!entry.Backend.ValidateEntry(entry, cursorCell)) {
				RemoveCurrentAndAdvance();
				SpeechPipeline.SpeakInterrupt(
					(string)STRINGS.ONIACCESS.SCANNER.INVALID);
				return;
			}

			string speech = TileCursor.Instance.JumpTo(entry.Cell);
			if (speech != null)
				SpeechPipeline.SpeakInterrupt(speech);
		}

		// -------------------------------------------------------------------
		// Private helpers
		// -------------------------------------------------------------------

		/// <summary>
		/// Auto-refresh if no snapshot. Returns true if a refresh was performed
		/// (caller should return since Refresh speaks its own announcement).
		/// </summary>
		private bool EnsureSnapshot() {
			if (_snapshot != null) return false;
			Refresh();
			return true;
		}

		private ScannerSubcategory CurrentSubcategory() {
			if (_snapshot == null || _categoryIndex >= _snapshot.CategoryCount)
				return null;
			var cat = _snapshot.GetCategory(_categoryIndex);
			if (_subcategoryIndex >= cat.Subcategories.Count) return null;
			return cat.Subcategories[_subcategoryIndex];
		}

		private ScannerItem CurrentItem() {
			var sub = CurrentSubcategory();
			if (sub == null || _itemIndex >= sub.Items.Count) return null;
			return sub.Items[_itemIndex];
		}

		private ScanEntry CurrentEntry() {
			var item = CurrentItem();
			if (item == null || _instanceIndex >= item.Instances.Count) return null;
			return item.Instances[_instanceIndex];
		}

		private string ValidateAndAnnounce(bool speakOnEmpty = true) {
			while (true) {
				var item = CurrentItem();
				if (item == null || item.Instances.Count == 0) {
					if (speakOnEmpty) SpeakEmpty();
					return null;
				}

				ClampInstanceIndex(item);
				var entry = item.Instances[_instanceIndex];
				int cursorCell = TileCursor.Instance.Cell;

				if (entry.Backend.ValidateEntry(entry, cursorCell))
					return FormatAnnouncement(entry, item);

				RemoveCurrentAndAdvance();
				if (CurrentEntry() == null) {
					if (speakOnEmpty) SpeakEmpty();
					return null;
				}
			}
		}

		private string FormatAnnouncement(ScanEntry entry, ScannerItem item) {
			int cursorCell = TileCursor.Instance.Cell;
			string name = entry.Backend.FormatName(entry);
			return AnnouncementFormatter.FormatEntityInstance(
				name, cursorCell, entry.Cell,
				_instanceIndex + 1, item.Instances.Count);
		}

		private void RemoveCurrentAndAdvance() {
			var item = CurrentItem();
			if (item == null) return;

			if (_instanceIndex >= item.Instances.Count) return;
			var entry = item.Instances[_instanceIndex];

			// Save current positions by name so we can re-find after pruning
			string catName = _snapshot.GetCategory(_categoryIndex).Name;
			var sub = CurrentSubcategory();
			string subName = sub?.Name;

			_snapshot.RemoveInstance(item, entry);

			// Re-find indices after structural changes
			if (_snapshot.CategoryCount == 0) return;
			_categoryIndex = FindIndexByName(
				_snapshot.Categories, catName, c => c.Name);
			var cat = _snapshot.GetCategory(_categoryIndex);
			if (subName != null)
				_subcategoryIndex = FindIndexByName(
					cat.Subcategories, subName, s => s.Name);
			if (_subcategoryIndex >= cat.Subcategories.Count)
				_subcategoryIndex = cat.Subcategories.Count - 1;

			sub = CurrentSubcategory();
			if (sub == null || sub.Items.Count == 0) return;
			if (_itemIndex >= sub.Items.Count)
				_itemIndex = sub.Items.Count - 1;
			var currentItem = CurrentItem();
			if (currentItem == null || currentItem.Instances.Count == 0) return;
			ClampInstanceIndex(currentItem);
		}

		private static int FindIndexByName<T>(
				List<T> list, string name, System.Func<T, string> getName) {
			for (int i = 0; i < list.Count; i++) {
				if (getName(list[i]) == name) return i;
			}
			return list.Count > 0 ? list.Count - 1 : 0;
		}

		private void ClampInstanceIndex(ScannerItem item) {
			if (_instanceIndex >= item.Instances.Count)
				_instanceIndex = item.Instances.Count - 1;
		}

		private static int Wrap(int current, int direction, int count) {
			if (count == 0) return 0;
			return (current + direction + count) % count;
		}

		private static int WrapSkipEmpty<T>(
				int current, int direction, List<T> list,
				System.Func<T, bool> isNonEmpty) {
			int count = list.Count;
			if (count == 0) return 0;
			int next = current;
			for (int i = 0; i < count; i++) {
				next = (next + direction + count) % count;
				if (isNonEmpty(list[next])) return next;
			}
			return current;
		}

		private static void PlayWrapCheck(
				int prev, int next, int direction, int count) {
			if (count <= 1) return;
			bool wrapped = (direction > 0 && next <= prev)
				|| (direction < 0 && next >= prev);
			if (wrapped) BaseScreenHandler.PlayWrapSound();
		}

		private static void SpeakEmpty() {
			SpeechPipeline.SpeakInterrupt(
				(string)STRINGS.ONIACCESS.SCANNER.EMPTY);
		}

		// -------------------------------------------------------------------
		// LocString name lookups
		// -------------------------------------------------------------------

		private static string GetCategoryName(string taxonomyName) {
			return _categoryNames.TryGetValue(taxonomyName, out LocString loc)
				? (string)loc : taxonomyName;
		}

		private static string GetSubcategoryName(string taxonomyName) {
			return _subcategoryNames.TryGetValue(taxonomyName, out LocString loc)
				? (string)loc : taxonomyName;
		}

		private static Dictionary<string, LocString> BuildCategoryNames() {
			return new Dictionary<string, LocString> {
				{ ScannerTaxonomy.Categories.Solids, STRINGS.ONIACCESS.SCANNER.CATEGORIES.SOLIDS },
				{ ScannerTaxonomy.Categories.Liquids, STRINGS.ONIACCESS.SCANNER.CATEGORIES.LIQUIDS },
				{ ScannerTaxonomy.Categories.Gases, STRINGS.ONIACCESS.SCANNER.CATEGORIES.GASES },
				{ ScannerTaxonomy.Categories.Buildings, STRINGS.ONIACCESS.SCANNER.CATEGORIES.BUILDINGS },
				{ ScannerTaxonomy.Categories.Networks, STRINGS.ONIACCESS.SCANNER.CATEGORIES.NETWORKS },
				{ ScannerTaxonomy.Categories.Automation, STRINGS.ONIACCESS.SCANNER.CATEGORIES.AUTOMATION },
				{ ScannerTaxonomy.Categories.Debris, STRINGS.ONIACCESS.SCANNER.CATEGORIES.DEBRIS },
				{ ScannerTaxonomy.Categories.Zones, STRINGS.ONIACCESS.SCANNER.CATEGORIES.ZONES },
				{ ScannerTaxonomy.Categories.Life, STRINGS.ONIACCESS.SCANNER.CATEGORIES.LIFE },
			};
		}

		private static Dictionary<string, LocString> BuildSubcategoryNames() {
			return new Dictionary<string, LocString> {
				{ ScannerTaxonomy.Subcategories.All, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.ALL },
				{ ScannerTaxonomy.Subcategories.Ores, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.ORES },
				{ ScannerTaxonomy.Subcategories.Stone, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.STONE },
				{ ScannerTaxonomy.Subcategories.Consumables, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.CONSUMABLES },
				{ ScannerTaxonomy.Subcategories.Organics, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.ORGANICS },
				{ ScannerTaxonomy.Subcategories.Ice, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.ICE },
				{ ScannerTaxonomy.Subcategories.Refined, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.REFINED },
				{ ScannerTaxonomy.Subcategories.Tiles, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.TILES },
				{ ScannerTaxonomy.Subcategories.Waters, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.WATERS },
				{ ScannerTaxonomy.Subcategories.Fuels, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.FUELS },
				{ ScannerTaxonomy.Subcategories.Molten, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.MOLTEN },
				{ ScannerTaxonomy.Subcategories.Misc, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.MISC },
				{ ScannerTaxonomy.Subcategories.Safe, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.SAFE },
				{ ScannerTaxonomy.Subcategories.Unsafe, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.UNSAFE },
				{ ScannerTaxonomy.Subcategories.Oxygen, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.OXYGEN },
				{ ScannerTaxonomy.Subcategories.Generators, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.GENERATORS },
				{ ScannerTaxonomy.Subcategories.Farming, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.FARMING },
				{ ScannerTaxonomy.Subcategories.Production, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.PRODUCTION },
				{ ScannerTaxonomy.Subcategories.Storage, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.STORAGE },
				{ ScannerTaxonomy.Subcategories.Refining, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.REFINING },
				{ ScannerTaxonomy.Subcategories.Temperature, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.TEMPERATURE },
				{ ScannerTaxonomy.Subcategories.Wellness, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.WELLNESS },
				{ ScannerTaxonomy.Subcategories.Morale, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.MORALE },
				{ ScannerTaxonomy.Subcategories.Infrastructure, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.INFRASTRUCTURE },
				{ ScannerTaxonomy.Subcategories.Rocketry, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.ROCKETRY },
				{ ScannerTaxonomy.Subcategories.Geysers, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.GEYSERS },
				{ ScannerTaxonomy.Subcategories.Power, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.POWER },
				{ ScannerTaxonomy.Subcategories.Liquid, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.LIQUID },
				{ ScannerTaxonomy.Subcategories.Gas, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.GAS },
				{ ScannerTaxonomy.Subcategories.Conveyor, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.CONVEYOR },
				{ ScannerTaxonomy.Subcategories.Transport, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.TRANSPORT },
				{ ScannerTaxonomy.Subcategories.Sensors, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.SENSORS },
				{ ScannerTaxonomy.Subcategories.Gates, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.GATES },
				{ ScannerTaxonomy.Subcategories.Controls, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.CONTROLS },
				{ ScannerTaxonomy.Subcategories.Wires, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.WIRES },
				{ ScannerTaxonomy.Subcategories.Materials, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.MATERIALS },
				{ ScannerTaxonomy.Subcategories.Food, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.FOOD },
				{ ScannerTaxonomy.Subcategories.Items, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.ITEMS },
				{ ScannerTaxonomy.Subcategories.Bottles, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.BOTTLES },
				{ ScannerTaxonomy.Subcategories.Orders, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.ORDERS },
				{ ScannerTaxonomy.Subcategories.Rooms, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.ROOMS },
				{ ScannerTaxonomy.Subcategories.Biomes, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.BIOMES },
				{ ScannerTaxonomy.Subcategories.Duplicants, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.DUPLICANTS },
				{ ScannerTaxonomy.Subcategories.Robots, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.ROBOTS },
				{ ScannerTaxonomy.Subcategories.TameCritters, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.TAME_CRITTERS },
				{ ScannerTaxonomy.Subcategories.WildCritters, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.WILD_CRITTERS },
				{ ScannerTaxonomy.Subcategories.WildPlants, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.WILD_PLANTS },
				{ ScannerTaxonomy.Subcategories.FarmPlants, STRINGS.ONIACCESS.SCANNER.SUBCATEGORIES.FARM_PLANTS },
			};
		}
	}
}
