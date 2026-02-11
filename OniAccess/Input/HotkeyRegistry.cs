using System;
using System.Collections.Generic;
using System.Text;
using OniAccess.Toggle;

namespace OniAccess.Input
{
    /// <summary>
    /// Modifier flags for hotkey bindings.
    /// Maps to Unity modifier key checks (Input.GetKey).
    /// </summary>
    [Flags]
    public enum HotkeyModifier
    {
        None = 0,
        Ctrl = 1,
        Shift = 2,
        Alt = 4,
        CtrlShift = Ctrl | Shift,
        CtrlAlt = Ctrl | Alt,
        ShiftAlt = Shift | Alt,
        CtrlShiftAlt = Ctrl | Shift | Alt,
    }

    /// <summary>
    /// A single hotkey binding with key, modifiers, context, description, and handler.
    /// </summary>
    public class HotkeyBinding
    {
        /// <summary>The Unity KeyCode for this binding.</summary>
        public UnityEngine.KeyCode Key { get; }

        /// <summary>Required modifier keys (Ctrl, Shift, Alt, or combinations).</summary>
        public HotkeyModifier Modifiers { get; }

        /// <summary>The game context where this binding is active.</summary>
        public AccessContext Context { get; }

        /// <summary>LocString description for help text generation.</summary>
        public string Description { get; }

        /// <summary>Action to execute when the hotkey is triggered.</summary>
        public System.Action Handler { get; }

        /// <summary>
        /// Per META-05: document what this key originally did in the game, or null if unbound.
        /// </summary>
        public string OriginalFunction { get; }

        public HotkeyBinding(UnityEngine.KeyCode key, HotkeyModifier modifiers,
            AccessContext context, string description, System.Action handler,
            string originalFunction = null)
        {
            Key = key;
            Modifiers = modifiers;
            Context = context;
            Description = description;
            Handler = handler;
            OriginalFunction = originalFunction;
        }

        /// <summary>
        /// Format the key combination as a human-readable string for help text.
        /// </summary>
        public string GetKeyDisplayName()
        {
            var parts = new List<string>();
            if ((Modifiers & HotkeyModifier.Ctrl) != 0) parts.Add("Ctrl");
            if ((Modifiers & HotkeyModifier.Shift) != 0) parts.Add("Shift");
            if ((Modifiers & HotkeyModifier.Alt) != 0) parts.Add("Alt");
            parts.Add(Key.ToString());
            return string.Join("+", parts.ToArray());
        }
    }

    /// <summary>
    /// Static registry for context-sensitive hotkey bindings with help text generation.
    ///
    /// Match logic: find binding where Key matches, Modifiers match, and Context is either
    /// Always or matches currentContext. When VanillaMode.IsEnabled is false, only Always
    /// context bindings fire.
    ///
    /// Frame dedup: tracks Time.frameCount per binding to prevent multiple fires per frame
    /// (per pitfall #3 from research).
    /// </summary>
    public static class HotkeyRegistry
    {
        private static readonly List<HotkeyBinding> _bindings = new List<HotkeyBinding>();

        /// <summary>
        /// Frame dedup: tracks the last frame a binding fired to prevent double-fire.
        /// Key is the binding index in _bindings.
        /// </summary>
        private static readonly Dictionary<int, int> _lastFireFrame = new Dictionary<int, int>();

        /// <summary>
        /// Register a hotkey binding. Duplicate key+modifier+context combinations
        /// are allowed (last registered wins in TryHandle).
        /// </summary>
        public static void Register(HotkeyBinding binding)
        {
            if (binding == null) return;
            _bindings.Add(binding);
        }

        /// <summary>
        /// Remove a binding matching the specified key, modifiers, and context.
        /// </summary>
        public static void Unregister(UnityEngine.KeyCode key, HotkeyModifier modifiers,
            AccessContext context)
        {
            _bindings.RemoveAll(b =>
                b.Key == key && b.Modifiers == modifiers && b.Context == context);
        }

        /// <summary>
        /// Find and execute a matching binding for the given key, modifiers, and context.
        /// Returns true if a binding was found and executed (event should be consumed).
        ///
        /// When VanillaMode.IsEnabled is false, only Always-context bindings fire.
        /// More specific modifiers are checked first (Ctrl+Shift before plain key).
        /// Frame dedup prevents multiple fires per frame per binding.
        /// </summary>
        public static bool TryHandle(UnityEngine.KeyCode key, HotkeyModifier activeModifiers,
            AccessContext currentContext)
        {
            int currentFrame = GetFrameCount();

            // Iterate bindings -- check most specific modifier matches first
            // by iterating all bindings and finding the best match
            HotkeyBinding bestMatch = null;
            int bestIndex = -1;
            int bestModifierCount = -1;

            for (int i = 0; i < _bindings.Count; i++)
            {
                var binding = _bindings[i];

                // Key must match
                if (binding.Key != key) continue;

                // Modifiers must match exactly
                if (binding.Modifiers != activeModifiers) continue;

                // Context check: when mod is off, only Always fires
                if (!VanillaMode.IsEnabled)
                {
                    if (binding.Context != AccessContext.Always) continue;
                }
                else
                {
                    // When mod is on: Always and Global fire in any context;
                    // other contexts must match exactly
                    if (binding.Context != AccessContext.Always &&
                        binding.Context != AccessContext.Global &&
                        binding.Context != currentContext)
                    {
                        continue;
                    }
                }

                // Prefer the binding with more specific modifiers (higher flag count)
                int modCount = CountFlags((int)binding.Modifiers);
                if (modCount > bestModifierCount)
                {
                    bestMatch = binding;
                    bestIndex = i;
                    bestModifierCount = modCount;
                }
            }

            if (bestMatch == null) return false;

            // Frame dedup: don't fire same binding twice in one frame
            if (_lastFireFrame.TryGetValue(bestIndex, out int lastFrame) && lastFrame == currentFrame)
            {
                return true; // Already fired this frame, but still consume the event
            }

            _lastFireFrame[bestIndex] = currentFrame;
            if (bestMatch.Handler != null) bestMatch.Handler();
            return true;
        }

        /// <summary>
        /// Generate help text listing all bindings active in the given context.
        /// Includes Always and Global bindings plus context-specific ones.
        /// Format: one binding per line, "Key: Description".
        /// </summary>
        public static string GetHelpText(AccessContext context)
        {
            var sb = new StringBuilder();
            sb.AppendLine(STRINGS.ONIACCESS.SPEECH.HELP_HEADER);

            bool anyFound = false;

            foreach (var binding in _bindings)
            {
                // Include Always and Global bindings, plus ones matching the specified context
                if (binding.Context == AccessContext.Always ||
                    binding.Context == AccessContext.Global ||
                    binding.Context == context)
                {
                    sb.AppendLine($"{binding.GetKeyDisplayName()}: {binding.Description}");
                    anyFound = true;
                }
            }

            if (!anyFound)
            {
                sb.AppendLine(STRINGS.ONIACCESS.SPEECH.NO_COMMANDS);
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Clear all bindings. Used for testing or mod shutdown.
        /// </summary>
        internal static void Clear()
        {
            _bindings.Clear();
            _lastFireFrame.Clear();
        }

        /// <summary>
        /// Get the number of registered bindings. Used for testing.
        /// </summary>
        internal static int Count => _bindings.Count;

        /// <summary>
        /// Count the number of set bits in a flag value.
        /// </summary>
        private static int CountFlags(int value)
        {
            int count = 0;
            while (value != 0)
            {
                count += value & 1;
                value >>= 1;
            }
            return count;
        }

        /// <summary>
        /// Get the current frame count for dedup.
        /// Wraps UnityEngine.Time.frameCount with a fallback for non-Unity contexts.
        /// </summary>
        private static int GetFrameCount()
        {
            try
            {
                return UnityEngine.Time.frameCount;
            }
            catch
            {
                return 0;
            }
        }
    }
}
