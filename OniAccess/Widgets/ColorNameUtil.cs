using System;
using System.Collections.Generic;
using UnityEngine;

namespace OniAccess.Widgets {
	public static class ColorNameUtil {
		private static Dictionary<Color, Func<string>> colorNames;

		public static string GetColorName(Color color) {
			if (colorNames == null) Initialize();
			Func<string> nameFunc;
			if (colorNames.TryGetValue(color, out nameFunc))
				return nameFunc();
			return null;
		}

		private static void Initialize() {
			colorNames = new Dictionary<Color, Func<string>> {
				// Row 1
				{ new Color(0.4862745f, 0.4862745f, 0.4862745f), () => (string)STRINGS.ONIACCESS.COLORS.DARK_GRAY },
				{ new Color(0f, 0f, 84f / 85f), () => (string)STRINGS.ONIACCESS.COLORS.BLUE },
				{ new Color(0f, 0f, 0.7372549f), () => (string)STRINGS.ONIACCESS.COLORS.DARK_BLUE },
				{ new Color(4f / 15f, 8f / 51f, 0.7372549f), () => (string)STRINGS.ONIACCESS.COLORS.INDIGO },
				{ new Color(0.5803922f, 0f, 44f / 85f), () => (string)STRINGS.ONIACCESS.COLORS.PURPLE },
				{ new Color(56f / 85f, 0f, 0.1254902f), () => (string)STRINGS.ONIACCESS.COLORS.MAROON },
				{ new Color(56f / 85f, 0.0627451f, 0f), () => (string)STRINGS.ONIACCESS.COLORS.DARK_RED },
				{ new Color(8f / 15f, 4f / 51f, 0f), () => (string)STRINGS.ONIACCESS.COLORS.BROWN },
				{ new Color(16f / 51f, 16f / 85f, 0f), () => (string)STRINGS.ONIACCESS.COLORS.DARK_OLIVE },
				{ new Color(0f, 0.47058824f, 0f), () => (string)STRINGS.ONIACCESS.COLORS.DARK_GREEN },
				{ new Color(0f, 0.40784314f, 0f), () => (string)STRINGS.ONIACCESS.COLORS.FOREST_GREEN },
				{ new Color(0f, 0.34509805f, 0f), () => (string)STRINGS.ONIACCESS.COLORS.DEEP_GREEN },
				{ new Color(0f, 0.2509804f, 0.34509805f), () => (string)STRINGS.ONIACCESS.COLORS.DARK_TEAL },
				{ new Color(0f, 0f, 0f), () => (string)STRINGS.ONIACCESS.COLORS.BLACK },
				// Row 2
				{ new Color(0.7372549f, 0.7372549f, 0.7372549f), () => (string)STRINGS.ONIACCESS.COLORS.GRAY },
				{ new Color(0f, 0.47058824f, 0.972549f), () => (string)STRINGS.ONIACCESS.COLORS.DODGER_BLUE },
				{ new Color(0f, 0.34509805f, 0.972549f), () => (string)STRINGS.ONIACCESS.COLORS.ROYAL_BLUE },
				{ new Color(0.40784314f, 4f / 15f, 84f / 85f), () => (string)STRINGS.ONIACCESS.COLORS.BLUE_VIOLET },
				{ new Color(72f / 85f, 0f, 0.8f), () => (string)STRINGS.ONIACCESS.COLORS.MAGENTA },
				{ new Color(76f / 85f, 0f, 0.34509805f), () => (string)STRINGS.ONIACCESS.COLORS.CRIMSON },
				{ new Color(0.972549f, 0.21960784f, 0f), () => (string)STRINGS.ONIACCESS.COLORS.RED_ORANGE },
				{ new Color(76f / 85f, 0.36078432f, 0.0627451f), () => (string)STRINGS.ONIACCESS.COLORS.ORANGE },
				{ new Color(0.6745098f, 0.4862745f, 0f), () => (string)STRINGS.ONIACCESS.COLORS.DARK_GOLD },
				{ new Color(0f, 0.72156864f, 0f), () => (string)STRINGS.ONIACCESS.COLORS.GREEN },
				{ new Color(0f, 56f / 85f, 0f), () => (string)STRINGS.ONIACCESS.COLORS.MEDIUM_GREEN },
				{ new Color(0f, 56f / 85f, 4f / 15f), () => (string)STRINGS.ONIACCESS.COLORS.SEA_GREEN },
				{ new Color(0f, 8f / 15f, 8f / 15f), () => (string)STRINGS.ONIACCESS.COLORS.TEAL },
				// index 27 is duplicate black — dictionary already has (0,0,0)
				// Row 3
				{ new Color(0.972549f, 0.972549f, 0.972549f), () => (string)STRINGS.ONIACCESS.COLORS.WHITE },
				{ new Color(0.23529412f, 0.7372549f, 84f / 85f), () => (string)STRINGS.ONIACCESS.COLORS.SKY_BLUE },
				{ new Color(0.40784314f, 8f / 15f, 84f / 85f), () => (string)STRINGS.ONIACCESS.COLORS.CORNFLOWER },
				{ new Color(0.59607846f, 0.47058824f, 0.972549f), () => (string)STRINGS.ONIACCESS.COLORS.VIOLET },
				{ new Color(0.972549f, 0.47058824f, 0.972549f), () => (string)STRINGS.ONIACCESS.COLORS.ORCHID },
				{ new Color(0.972549f, 0.34509805f, 0.59607846f), () => (string)STRINGS.ONIACCESS.COLORS.HOT_PINK },
				{ new Color(0.972549f, 0.47058824f, 0.34509805f), () => (string)STRINGS.ONIACCESS.COLORS.SALMON },
				{ new Color(84f / 85f, 32f / 51f, 4f / 15f), () => (string)STRINGS.ONIACCESS.COLORS.TANGERINE },
				{ new Color(0.972549f, 0.72156864f, 0f), () => (string)STRINGS.ONIACCESS.COLORS.GOLD },
				{ new Color(0.72156864f, 0.972549f, 8f / 85f), () => (string)STRINGS.ONIACCESS.COLORS.CHARTREUSE },
				{ new Color(0.34509805f, 72f / 85f, 28f / 85f), () => (string)STRINGS.ONIACCESS.COLORS.BRIGHT_GREEN },
				{ new Color(0.34509805f, 0.972549f, 0.59607846f), () => (string)STRINGS.ONIACCESS.COLORS.SPRING_GREEN },
				{ new Color(0f, 0.9098039f, 72f / 85f), () => (string)STRINGS.ONIACCESS.COLORS.CYAN },
				{ new Color(0.47058824f, 0.47058824f, 0.47058824f), () => (string)STRINGS.ONIACCESS.COLORS.MEDIUM_GRAY },
				// Row 4 (pastel)
				{ new Color(84f / 85f, 84f / 85f, 84f / 85f), () => (string)STRINGS.ONIACCESS.COLORS.OFF_WHITE },
				{ new Color(0.6431373f, 76f / 85f, 84f / 85f), () => (string)STRINGS.ONIACCESS.COLORS.LIGHT_BLUE },
				{ new Color(0.72156864f, 0.72156864f, 0.972549f), () => (string)STRINGS.ONIACCESS.COLORS.LAVENDER },
				{ new Color(72f / 85f, 0.72156864f, 0.972549f), () => (string)STRINGS.ONIACCESS.COLORS.LIGHT_PURPLE },
				{ new Color(0.972549f, 0.72156864f, 0.972549f), () => (string)STRINGS.ONIACCESS.COLORS.LIGHT_PINK },
				{ new Color(0.972549f, 0.72156864f, 64f / 85f), () => (string)STRINGS.ONIACCESS.COLORS.LIGHT_ROSE },
				{ new Color(0.9411765f, 0.8156863f, 0.6901961f), () => (string)STRINGS.ONIACCESS.COLORS.BEIGE },
				{ new Color(84f / 85f, 0.8784314f, 56f / 85f), () => (string)STRINGS.ONIACCESS.COLORS.CREAM },
				{ new Color(0.972549f, 72f / 85f, 0.47058824f), () => (string)STRINGS.ONIACCESS.COLORS.LIGHT_GOLD },
				{ new Color(72f / 85f, 0.972549f, 0.47058824f), () => (string)STRINGS.ONIACCESS.COLORS.LIGHT_LIME },
				{ new Color(0.72156864f, 0.972549f, 0.72156864f), () => (string)STRINGS.ONIACCESS.COLORS.PALE_GREEN },
			};
		}
	}
}
