using System;
using System.IO;
using Klei;
using OniAccess.Util;
using YamlDotNet.Serialization;

namespace OniAccess {
	public static class ConfigManager {
		private const string FileName = "oni-access-config.yml";
		private static string _path;

		public static ModConfig Config { get; private set; }

		public static void Load(string dataDir) {
			Directory.CreateDirectory(dataDir);
			_path = Path.Combine(dataDir, FileName);
			if (!File.Exists(_path)) {
				Config = new ModConfig();
				Log.Info($"No config file found at {_path}, using defaults");
				return;
			}
			try {
				Config = YamlIO.LoadFile<ModConfig>(_path, OnYamlError);
				if (Config == null) {
					Log.Warn($"Config file at {_path} parsed as null, using defaults");
					Config = new ModConfig();
				} else {
					Log.Info($"Loaded config from {_path}");
				}
			} catch (Exception ex) {
				Log.Warn($"Failed to load config from {_path}: {ex.Message}");
				Config = new ModConfig();
			}
		}

		public static void Save() {
			try {
				// Serialize directly instead of using YamlIO.Save because ONI's
				// bundled YamlDotNet omits properties equal to the type default
				// (e.g. false for bool). Properties like FootstepEarcons that
				// default to true in the initializer would lose a user's "false"
				// choice on reload since the deserializer never sees the key.
				using (var writer = new StreamWriter(_path))
					new SerializerBuilder().EmitDefaults().Build()
						.Serialize(writer, Config);
			} catch (Exception ex) {
				Log.Warn($"Failed to save config to {_path}: {ex.Message}");
			}
		}

		private static void OnYamlError(YamlIO.Error error, bool forceLogAsWarning) {
			if (forceLogAsWarning || error.severity == YamlIO.Error.Severity.Recoverable)
				Log.Warn($"Config YAML issue in {error.file.full_path}: {error.message}");
			else
				Log.Error($"Config YAML error in {error.file.full_path}: {error.message}");
		}
	}
}
