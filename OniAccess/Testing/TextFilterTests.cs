using System.Collections.Generic;
using OniAccess.Speech;

namespace OniAccess.Testing
{
    /// <summary>
    /// In-game test class for TextFilter. Uses a simple static runner pattern
    /// because ONI mods run inside Unity/Mono where external test frameworks
    /// are impractical.
    /// </summary>
    public static class TextFilterTests
    {
        public static List<(string name, bool passed, string detail)> RunAll()
        {
            var results = new List<(string name, bool passed, string detail)>();

            // Ensure default sprites are registered before tests
            TextFilter.RegisterSprite("warning", "warning:");
            TextFilter.RegisterSprite("logic_signal_green", "green signal");
            TextFilter.RegisterSprite("logic_signal_red", "red signal");

            results.Add(StripsBoldTags());
            results.Add(StripsColorTags());
            results.Add(StripsNestedTags());
            results.Add(ConvertsSpriteToText());
            results.Add(StripsUnregisteredSprite());
            results.Add(StripsTmpBracketSprites());
            results.Add(HandlesLinkTags());
            results.Add(NormalizesWhitespace());
            results.Add(HandlesEmptyAndNull());
            results.Add(PreservesPlainText());
            results.Add(StripsHotkeyPlaceholders());
            results.Add(StripsSizeStyle());

            return results;
        }

        private static (string, bool, string) AssertEqual(string testName, string expected, string actual)
        {
            bool passed = expected == actual;
            string detail = passed
                ? $"OK: \"{actual}\""
                : $"FAIL: expected \"{expected}\" but got \"{actual}\"";
            return (testName, passed, detail);
        }

        private static (string, bool, string) StripsBoldTags()
        {
            string result = TextFilter.FilterForSpeech("<b>Warning</b>");
            return AssertEqual("StripsBoldTags", "Warning", result);
        }

        private static (string, bool, string) StripsColorTags()
        {
            string result = TextFilter.FilterForSpeech("<color=#FF0000>Hot</color>");
            return AssertEqual("StripsColorTags", "Hot", result);
        }

        private static (string, bool, string) StripsNestedTags()
        {
            string result = TextFilter.FilterForSpeech("<b><color=red>Alert</color></b>");
            return AssertEqual("StripsNestedTags", "Alert", result);
        }

        private static (string, bool, string) ConvertsSpriteToText()
        {
            string result = TextFilter.FilterForSpeech("<sprite name=warning>Pipe broken");
            return AssertEqual("ConvertsSpriteToText", "warning: Pipe broken", result);
        }

        private static (string, bool, string) StripsUnregisteredSprite()
        {
            string result = TextFilter.FilterForSpeech("<sprite name=decorative_icon>text");
            return AssertEqual("StripsUnregisteredSprite", "text", result);
        }

        private static (string, bool, string) StripsTmpBracketSprites()
        {
            string result = TextFilter.FilterForSpeech("[icon_name] some text");
            return AssertEqual("StripsTmpBracketSprites", "some text", result);
        }

        private static (string, bool, string) HandlesLinkTags()
        {
            string result = TextFilter.FilterForSpeech("<link=\"LINK_ID\">Click here</link>");
            return AssertEqual("HandlesLinkTags", "Click here", result);
        }

        private static (string, bool, string) NormalizesWhitespace()
        {
            string result = TextFilter.FilterForSpeech("word1   word2\n\nword3");
            return AssertEqual("NormalizesWhitespace", "word1 word2 word3", result);
        }

        private static (string, bool, string) HandlesEmptyAndNull()
        {
            string nullResult = TextFilter.FilterForSpeech(null);
            string emptyResult = TextFilter.FilterForSpeech("");

            if (nullResult != "")
                return ("HandlesEmptyAndNull", false, $"FAIL: null -> \"{nullResult}\" (expected \"\")");
            if (emptyResult != "")
                return ("HandlesEmptyAndNull", false, $"FAIL: empty -> \"{emptyResult}\" (expected \"\")");

            return ("HandlesEmptyAndNull", true, "OK: null -> \"\", empty -> \"\"");
        }

        private static (string, bool, string) PreservesPlainText()
        {
            string input = "Copper Ore, 200 kg, 25\u00B0C";
            string result = TextFilter.FilterForSpeech(input);
            return AssertEqual("PreservesPlainText", input, result);
        }

        private static (string, bool, string) StripsHotkeyPlaceholders()
        {
            string result = TextFilter.FilterForSpeech("Build a Ladder {Hotkey}");
            return AssertEqual("StripsHotkeyPlaceholders", "Build a Ladder", result);
        }

        private static (string, bool, string) StripsSizeStyle()
        {
            string result = TextFilter.FilterForSpeech("<size=10><style=\"KKeyword\">text</style></size>");
            return AssertEqual("StripsSizeStyle", "text", result);
        }
    }
}
