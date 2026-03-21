using System.Reflection;
using HarmonyLib;
using OniAccess.Util;
using UnityEngine;

namespace OniAccess.Audio {
	public class FootstepPlayer {
		public static FootstepPlayer Instance { get; private set; }

		const float Volume = 1.15f;

		private static readonly MethodInfo _getFloorCategory =
			AccessTools.Method(typeof(Substance), "GetFloorEventAudioCategory");

		static FootstepPlayer() {
			if (_getFloorCategory == null)
				Log.Warn("FootstepPlayer: Substance.GetFloorEventAudioCategory not found");
		}

		public FootstepPlayer() {
			Instance = this;
		}

		public static void Destroy() {
			Instance = null;
		}

		public void Play(int cell) {
			if (!ConfigManager.Config.FootstepEarcons) return;
			if (!Grid.IsValidCell(cell)) return;

			if (Grid.Foundation[cell]) {
				PlayEvent(GetFoundationCategory(cell) + "_land", cell);
				return;
			}

			Element element = Grid.Element[cell];
			if (element.IsSolid) {
				PlayEvent(GetSolidCategory(element) + "_land", cell);
				return;
			}

			if (element.IsLiquid) {
				PlayEvent("Liquid_footstep", cell);
				return;
			}

			// Gas/vacuum: only ladders get footstep sounds
			var building = GetBuilding(cell);
			if (building != null && building.GetComponent<Ladder>() != null) {
				string name = building.Def.PrefabID == "LadderFast"
					? "Ladder_footstep_Plastic"
					: "Ladder_footstep";
				PlayEvent(name, cell);
				return;
			}

			// Open space: silence
		}

		private void PlayEvent(string name, int cell) {
			string sound = GlobalAssets.GetSound(name, true);
			if (sound == null)
				sound = GlobalAssets.GetSound("Rock_land", true);
			if (sound == null) return;

			Vector3 pos = SoundListenerController.Instance != null
				? SoundListenerController.Instance.transform.GetPosition()
				: Vector3.zero;
			var instance = KFMOD.BeginOneShot(sound, pos);
			if (instance.isValid()) {
				instance.setVolume(Volume);
				float depth = SoundUtil.GetLiquidDepth(cell);
				if (depth > 0f)
					instance.setParameterByName("liquidDepth", depth);
				KFMOD.EndOneShot(instance);
			}
		}

		private static string GetFoundationCategory(int cell) {
			GameObject go = Grid.Objects[cell, (int)ObjectLayer.Building];
			if (go != null) {
				Building building = go.GetComponent<BuildingComplete>();
				if (building != null) {
					switch (building.Def.PrefabID) {
						case "PlasticTile": return "TilePlastic";
						case "GlassTile": return "TileGlass";
						case "BunkerTile": return "TileBunker";
						case "MetalTile": return "TileMetal";
						case "CarpetTile": return "Carpet";
						case "SnowTile": return "TileSnow";
						case "WoodTile": return "TileWood";
						default: return "Tile";
					}
				}
			}
			return "Tile";
		}

		private static Building GetBuilding(int cell) {
			GameObject go = Grid.Objects[cell, (int)ObjectLayer.Building];
			if (go != null)
				return go.GetComponent<BuildingComplete>();
			return null;
		}

		private static string GetSolidCategory(Element element) {
			if (_getFloorCategory != null) {
				string category = (string)_getFloorCategory.Invoke(element.substance, null);
				if (!string.IsNullOrEmpty(category)) return category;
			}
			if (element.HasTag(GameTags.RefinedMetal)) return "RefinedMetal";
			if (element.HasTag(GameTags.Metal)) return "RawMetal";
			return "Rock";
		}
	}
}
