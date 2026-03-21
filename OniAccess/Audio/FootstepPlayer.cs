using System.Reflection;
using HarmonyLib;
using OniAccess.Util;
using UnityEngine;

namespace OniAccess.Audio {
	public class FootstepPlayer {
		public static FootstepPlayer Instance { get; private set; }

		static float Volume => ConfigManager.Config.FootstepVolume;

		private static readonly MethodInfo _getOreBumpSound =
			AccessTools.Method(typeof(Substance), "GetOreBumpSound");

		static FootstepPlayer() {
			if (_getOreBumpSound == null)
				Log.Warn("FootstepPlayer: Substance.GetOreBumpSound not found");
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
				PlayEvent(GetFoundationCategory(cell) + "_footstep", cell);
				return;
			}

			Element element = Grid.Element[cell];
			if (element.IsSolid) {
				PlayEvent("Ore_bump_" + GetOreBumpCategory(element), cell);
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
				sound = GlobalAssets.GetSound("Ore_bump_rock", true);
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

		private static string GetOreBumpCategory(Element element) {
			if (_getOreBumpSound != null) {
				string category = (string)_getOreBumpSound.Invoke(element.substance, null);
				if (!string.IsNullOrEmpty(category)) return category;
			}
			if (element.HasTag(GameTags.RefinedMetal)) return "RefinedMetal";
			if (element.HasTag(GameTags.Metal)) return "RawMetal";
			return "rock";
		}
	}
}
