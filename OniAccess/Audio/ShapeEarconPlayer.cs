using System.Collections;
using FMOD;
using FMODUnity;
using OniAccess.Handlers.Tiles.Sections;
using OniAccess.Util;
using UnityEngine;

namespace OniAccess.Audio {
	public class ShapeEarconPlayer: MonoBehaviour {
		public static ShapeEarconPlayer Instance { get; private set; }

		const float SegmentSeconds = 0.055f;
		const float GapSeconds = 0.01f;
		const float FadeSeconds = 0.005f;
		const float Volume = 0.15f;

		const float PanLeft = -0.79f;
		const float PanRight = 0.79f;
		const float PanCenter = 0f;

		// Harmonic profiles: [fundamental, 2nd, 3rd...]
		static readonly float[] WireHarmonics = { 1.0f };
		static readonly float[] PipeHarmonics = { 1.0f, 0.9f };

		// Each category has 3 tones: index 0 = up, 1 = down, 2 = horizontal
		const int ToneUp = 0;
		const int ToneDown = 1;
		const int ToneHorizontal = 2;
		const int TonesPerCategory = 3;

		enum ToneCategory { Wire, Pipe, Count }

		static readonly float[][] Frequencies = {
			// Wire: up, down, horizontal
			new[] { 709f, 297f, 457f },
			// Pipe (also used for rail): same pitch range, different timbre
			new[] { 709f, 297f, 457f },
		};

		readonly struct Segment {
			public readonly int ToneIndex;
			public readonly float Pan;
			public Segment(int toneIndex, float pan) {
				ToneIndex = toneIndex;
				Pan = pan;
			}
		}

		private Sound[] _tones;
		private Coroutine _activeSequence;
		private Channel _channel;

		private void Awake() {
			Instance = this;
			GenerateTones();
		}

		private void OnDestroy() {
			CancelAll();
			if (_tones != null) {
				foreach (var sound in _tones)
					if (sound.hasHandle())
						sound.release();
			}
			if (Instance == this)
				Instance = null;
		}

		private void GenerateTones() {
			int total = (int)ToneCategory.Count * TonesPerCategory;
			_tones = new Sound[total];

			var harmonicSets = new[] { WireHarmonics, PipeHarmonics };
			for (int cat = 0; cat < (int)ToneCategory.Count; cat++) {
				for (int tone = 0; tone < TonesPerCategory; tone++) {
					int idx = cat * TonesPerCategory + tone;
					_tones[idx] = ToneGenerator.CreateSegmentTone(
						Frequencies[cat][tone], SegmentSeconds,
						FadeSeconds, harmonicSets[cat]);
				}
			}
		}

		public void OnCursorMoved(int cell, HashedString overlayMode) {
			if (!ConfigManager.Config.PipeShapeEarcons
				|| !Grid.IsValidCell(cell)
				|| Game.Instance == null) {
				CancelAll();
				return;
			}

			var mapping = GetOverlayMapping(overlayMode);
			if (mapping == null) {
				CancelAll();
				return;
			}

			var (category, getManager, layers) = mapping.Value;
			if (Grid.Objects[cell, layers[0]] == null) {
				CancelAll();
				return;
			}

			var connections = getManager().GetConnections(cell, true)
				| GetBridgeConnections(cell, layers);
			var segments = GetSegments(connections, category);
			if (segments == null) {
				CancelAll();
				return;
			}

			CancelAll();
			_activeSequence = StartCoroutine(RunSequence(segments));
		}

		public void CancelAll() {
			if (_activeSequence != null) {
				StopCoroutine(_activeSequence);
				_activeSequence = null;
			}
			StopChannel();
		}

		private IEnumerator RunSequence(Segment[] segments) {
			for (int i = 0; i < segments.Length; i++) {
				PlaySegment(segments[i]);
				yield return new WaitForSecondsRealtime(SegmentSeconds);
				StopChannel();
				if (i < segments.Length - 1)
					yield return new WaitForSecondsRealtime(GapSeconds);
			}
			_activeSequence = null;
		}

		private void PlaySegment(Segment seg) {
			if (seg.ToneIndex < 0 || seg.ToneIndex >= _tones.Length
				|| !_tones[seg.ToneIndex].hasHandle()) {
				Log.Warn($"ShapeEarconPlayer: invalid tone index {seg.ToneIndex}");
				return;
			}
			var result = RuntimeManager.CoreSystem.playSound(
				_tones[seg.ToneIndex], default(ChannelGroup), false,
				out _channel);
			if (result != RESULT.OK) {
				Log.Warn($"ShapeEarconPlayer: playSound failed: {result}");
				return;
			}
			_channel.setVolume(Volume);
			_channel.setPan(seg.Pan);
		}

		private void StopChannel() {
			if (_channel.hasHandle())
				_channel.stop();
			_channel = default;
		}

		private static Segment[] GetSegments(
				UtilityConnections connections, ToneCategory category) {
			bool up = (connections & UtilityConnections.Up) != 0;
			bool down = (connections & UtilityConnections.Down) != 0;
			bool left = (connections & UtilityConnections.Left) != 0;
			bool right = (connections & UtilityConnections.Right) != 0;
			int count = (up ? 1 : 0) + (down ? 1 : 0)
				+ (left ? 1 : 0) + (right ? 1 : 0);

			int b = (int)category * TonesPerCategory;
			int u = b + ToneUp;
			int d = b + ToneDown;
			int h = b + ToneHorizontal;

			switch (count) {
				case 0:
					return null;
				case 1:
					if (up) return new[] { new Segment(u, PanCenter) };
					if (down) return new[] { new Segment(d, PanCenter) };
					if (left) return new[] { new Segment(h, PanLeft) };
					return new[] { new Segment(h, PanRight) };
				case 2:
					if (up && down)
						return new[] {
							new Segment(u, PanCenter),
							new Segment(d, PanCenter)
						};
					if (left && right)
						return new[] {
							new Segment(h, PanLeft),
							new Segment(h, PanRight)
						};
					// Corner — up corners lead with up; down corners
					// lead with horizontal to match T-form ordering.
					if (up)
						return new[] {
							new Segment(u, PanCenter),
							new Segment(h, left ? PanLeft : PanRight)
						};
					return new[] {
						new Segment(h, left ? PanLeft : PanRight),
						new Segment(d, PanCenter)
					};
				case 3:
					if (!up)
						return new[] {
							new Segment(h, PanLeft),
							new Segment(d, PanCenter),
							new Segment(h, PanRight)
						};
					if (!down)
						return new[] {
							new Segment(h, PanLeft),
							new Segment(u, PanCenter),
							new Segment(h, PanRight)
						};
					if (!left)
						return new[] {
							new Segment(u, PanCenter),
							new Segment(h, PanRight),
							new Segment(d, PanCenter)
						};
					return new[] {
						new Segment(u, PanCenter),
						new Segment(h, PanLeft),
						new Segment(d, PanCenter)
					};
				default:
					return new[] {
						new Segment(u, PanCenter),
						new Segment(h, PanRight),
						new Segment(d, PanCenter),
						new Segment(h, PanLeft)
					};
			}
		}

		private static UtilityConnections GetBridgeConnections(
				int cell, int[] layers) {
			var result = (UtilityConnections)0;
			foreach (int layer in layers) {
				var go = Grid.Objects[cell, layer];
				if (go == null) continue;
				if (ConduitSection.IsBridgeEndpoint(go))
					result |= ConduitSection.GetBridgeDirection(go, cell);
			}
			result |= ConduitSection.FindJointPlateConnections(cell);
			return result;
		}

		private static (ToneCategory, System.Func<IUtilityNetworkMgr>, int[])?
				GetOverlayMapping(HashedString overlayMode) {
			if (overlayMode == OverlayModes.Power.ID)
				return (ToneCategory.Wire,
					() => Game.Instance.electricalConduitSystem,
					new[] { (int)ObjectLayer.Wire,
						(int)ObjectLayer.WireConnectors });
			if (overlayMode == OverlayModes.LiquidConduits.ID)
				return (ToneCategory.Pipe,
					() => Game.Instance.liquidConduitSystem,
					new[] { (int)ObjectLayer.LiquidConduit,
						(int)ObjectLayer.LiquidConduitConnection });
			if (overlayMode == OverlayModes.GasConduits.ID)
				return (ToneCategory.Pipe,
					() => Game.Instance.gasConduitSystem,
					new[] { (int)ObjectLayer.GasConduit,
						(int)ObjectLayer.GasConduitConnection });
			if (overlayMode == OverlayModes.SolidConveyor.ID)
				return (ToneCategory.Pipe,
					() => Game.Instance.solidConduitSystem,
					new[] { (int)ObjectLayer.SolidConduit,
						(int)ObjectLayer.SolidConduitConnection });
			if (overlayMode == OverlayModes.Logic.ID)
				return (ToneCategory.Wire,
					() => Game.Instance.logicCircuitSystem,
					new[] { (int)ObjectLayer.LogicWire });
			return null;
		}
	}
}
