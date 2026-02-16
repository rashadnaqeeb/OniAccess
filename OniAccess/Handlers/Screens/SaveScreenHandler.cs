using System.Collections.Generic;
using HarmonyLib;

using OniAccess.Widgets;

namespace OniAccess.Handlers.Screens {
	/// <summary>
	/// Handler for SaveScreen (save game dialog from Pause menu).
	///
	/// Flat list: "New Save" button, existing save entries (overwrite targets),
	/// and close button. Each save entry shows filename + date from
	/// HierarchyReferences "Title" and "Date" refs (RectTransform -> LocText).
	/// </summary>
	public class SaveScreenHandler: BaseMenuHandler {
		public override string DisplayName => (string)STRINGS.UI.FRONTEND.SAVESCREEN.TITLE;

		public override IReadOnlyList<HelpEntry> HelpEntries { get; }

		public SaveScreenHandler(KScreen screen) : base(screen) {
			HelpEntries = BuildHelpEntries();
		}

		public override bool DiscoverWidgets(KScreen screen) {
			_widgets.Clear();

			WidgetDiscoveryUtil.TryAddButtonField(screen, "newSaveButton", null, _widgets);

			try {
				var oldSavesRoot = Traverse.Create(screen).Field("oldSavesRoot")
					.GetValue<UnityEngine.Transform>();
				if (oldSavesRoot != null) {
					for (int i = 0; i < oldSavesRoot.childCount; i++) {
						var child = oldSavesRoot.GetChild(i);
						if (child == null || !child.gameObject.activeInHierarchy) continue;

						var refs = child.GetComponent<HierarchyReferences>();
						if (refs == null) continue;

						string label = BuildSaveEntryLabel(refs);
						if (string.IsNullOrEmpty(label)) continue;

						var kbutton = child.GetComponent<KButton>();
						_widgets.Add(new WidgetInfo {
							Label = label,
							Component = kbutton,
							Type = kbutton != null ? WidgetType.Button : WidgetType.Label,
							GameObject = child.gameObject
						});
					}
				}
			} catch (System.Exception ex) {
				Util.Log.Error($"SaveScreenHandler.DiscoverWidgets(oldSavesRoot): {ex.Message}");
			}

			WidgetDiscoveryUtil.TryAddButtonField(screen, "closeButton", null, _widgets);

			Util.Log.Debug($"SaveScreenHandler.DiscoverWidgets: {_widgets.Count} widgets");
			return true;
		}

		/// <summary>
		/// Build label from HierarchyReferences "Title" and "Date" refs.
		/// Same RectTransform -> LocText pattern as SaveLoadHandler.
		/// </summary>
		private string BuildSaveEntryLabel(HierarchyReferences refs) {
			var parts = new List<string>();

			string title = GetReferenceText(refs, "Title");
			if (!string.IsNullOrEmpty(title)) parts.Add(title);

			string date = GetReferenceText(refs, "Date");
			if (!string.IsNullOrEmpty(date)) parts.Add(date);

			return parts.Count > 0 ? string.Join(", ", parts) : null;
		}

		private string GetReferenceText(HierarchyReferences refs, string refName) {
			if (!refs.HasReference(refName)) return null;
			try {
				var component = refs.GetReference(refName);
				if (component == null) return null;
				var locText = component as LocText
					?? component.gameObject.GetComponent<LocText>();
				if (locText != null && !string.IsNullOrEmpty(locText.text))
					return locText.text.Trim();
			} catch (System.Exception ex) {
				Util.Log.Error($"SaveScreenHandler.GetReferenceText({refName}): {ex.Message}");
			}
			return null;
		}
	}
}
