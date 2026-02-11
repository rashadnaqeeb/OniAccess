using System;
using System.Collections.Generic;

namespace OniAccess.Speech
{
    /// <summary>
    /// A single entry in the alert history buffer.
    /// Captures text, category, timing, location, and dedup count.
    /// </summary>
    public class AlertEntry
    {
        /// <summary>The filtered spoken text of the alert.</summary>
        public string Text { get; set; }

        /// <summary>Category for grouping (e.g., "alert.broken_pipe").</summary>
        public string Category { get; set; }

        /// <summary>Game time when the alert was recorded (for display).</summary>
        public float GameTime { get; set; }

        /// <summary>Grid cell for jump-to-location in Phase 6 navigation.</summary>
        public int Cell { get; set; }

        /// <summary>Dedup count -- incremented when same text appears within the dedup window.</summary>
        public int Count { get; set; }

        /// <summary>Real time (Time.time equivalent) for dedup window calculation.</summary>
        public float Timestamp { get; set; }
    }

    /// <summary>
    /// Ring buffer for alert history with deduplication.
    /// Phase 1 establishes the buffer infrastructure; Phase 6 adds navigation UI.
    ///
    /// Per locked decision: "Create a history buffer for alerts...
    /// Full implementation Phase 6; Phase 1 establishes buffer infrastructure."
    ///
    /// Dedup rule: if most recent entry has the same text and was recorded
    /// within 1.0 seconds, increment Count instead of creating a new entry.
    /// </summary>
    public static class AlertHistory
    {
        /// <summary>Fixed-size ring buffer capacity.</summary>
        private const int BufferSize = 100;

        /// <summary>Dedup window in seconds -- same text within this window increments count.</summary>
        private const float DedupWindowSeconds = 1.0f;

        private static readonly AlertEntry[] _buffer = new AlertEntry[BufferSize];
        private static int _writeIndex = 0;
        private static int _count = 0;

        /// <summary>
        /// Provides the current time for dedup window calculation.
        /// Defaults to a simple counter for environments where UnityEngine.Time is unavailable.
        /// Set to () => UnityEngine.Time.time during mod initialization.
        /// </summary>
        internal static Func<float> GetTime = () => 0f;

        /// <summary>
        /// Record an alert in the history buffer.
        /// If the most recent entry has the same text and was recorded within the
        /// dedup window (1 second), the count is incremented instead of creating a new entry.
        /// </summary>
        /// <param name="text">The filtered spoken text.</param>
        /// <param name="category">Category for grouping (nullable).</param>
        /// <param name="cell">Grid cell for jump-to-location (-1 if unknown).</param>
        public static void Record(string text, string category, int cell = -1)
        {
            if (string.IsNullOrEmpty(text)) return;

            float now = GetTime();

            // Check for dedup: if most recent entry has same text within the window
            if (_count > 0)
            {
                int lastIndex = (_writeIndex - 1 + BufferSize) % BufferSize;
                AlertEntry last = _buffer[lastIndex];
                if (last != null && last.Text == text && (now - last.Timestamp) < DedupWindowSeconds)
                {
                    last.Count++;
                    last.Timestamp = now;
                    return;
                }
            }

            // Create new entry
            var entry = new AlertEntry
            {
                Text = text,
                Category = category,
                GameTime = 0f, // Will be set from game time when available
                Cell = cell,
                Count = 1,
                Timestamp = now
            };

            _buffer[_writeIndex] = entry;
            _writeIndex = (_writeIndex + 1) % BufferSize;
            if (_count < BufferSize) _count++;
        }

        /// <summary>
        /// Return the most recent N entries in reverse chronological order (newest first).
        /// </summary>
        /// <param name="count">Maximum number of entries to return.</param>
        /// <returns>List of entries, newest first. May contain fewer than requested.</returns>
        public static List<AlertEntry> GetRecent(int count)
        {
            int toReturn = Math.Min(count, _count);
            var results = new List<AlertEntry>(toReturn);

            for (int i = 0; i < toReturn; i++)
            {
                int index = (_writeIndex - 1 - i + BufferSize) % BufferSize;
                if (_buffer[index] != null)
                {
                    results.Add(_buffer[index]);
                }
            }

            return results;
        }

        /// <summary>
        /// Reset the buffer, clearing all entries.
        /// </summary>
        public static void Clear()
        {
            for (int i = 0; i < BufferSize; i++)
            {
                _buffer[i] = null;
            }
            _writeIndex = 0;
            _count = 0;
        }
    }
}
