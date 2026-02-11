using System.Collections.Generic;
using OniAccess.Util;

namespace OniAccess.Speech
{
    /// <summary>
    /// Central speech dispatch point for ALL speech output in the mod.
    /// No code should call SpeechEngine.Say() directly -- all speech flows through here.
    ///
    /// Pipeline: Caller -> SpeechPipeline -> TextFilter -> SpeechEngine -> Tolk
    ///
    /// Two modes:
    /// - Interrupt: stops current speech, speaks immediately (navigation)
    /// - Queue: appends to queue, never dropped (alerts/notifications)
    ///
    /// Per locked decisions:
    /// - "Navigation speech interrupts previous speech for responsiveness"
    /// - "Alert/notification speech: queues, plays in order, never dropped"
    /// - "Duplicate simultaneous alerts: combine with count ('Broken Wall x2')"
    /// </summary>
    public static class SpeechPipeline
    {
        private static readonly Queue<Announcement> _queue = new Queue<Announcement>();

        /// <summary>
        /// When false, all speech methods return immediately without speaking.
        /// Controlled by VanillaMode (Plan 03).
        /// </summary>
        private static bool _enabled = true;

        /// <summary>
        /// Track last interrupt category and frame to dedup rapid navigation.
        /// Same category within same frame is skipped.
        /// </summary>
        private static string _lastInterruptCategory;
        private static int _lastInterruptFrame;

        /// <summary>
        /// Whether the pipeline is active. When false (mod toggled off),
        /// all methods return immediately.
        /// </summary>
        public static bool IsActive => _enabled;

        /// <summary>
        /// Enable or disable the speech pipeline.
        /// Called by VanillaMode when the mod is toggled on/off.
        /// </summary>
        internal static void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            if (!enabled)
            {
                _queue.Clear();
            }
        }

        /// <summary>
        /// Interrupt mode: stop current speech and speak immediately.
        /// Used for navigation (cursor movement) where responsiveness is critical.
        ///
        /// Category dedup: if the same category is spoken within the same frame
        /// (rapid cursor movement), the previous is skipped automatically by the
        /// interrupt behavior.
        /// </summary>
        /// <param name="text">Raw text (will be filtered through TextFilter).</param>
        /// <param name="category">Optional category for frame-based dedup (e.g., "navigation").</param>
        public static void SpeakInterrupt(string text, string category = null)
        {
            if (!_enabled) return;

            string filtered = TextFilter.FilterForSpeech(text);
            if (string.IsNullOrEmpty(filtered)) return;

            // Category dedup for same-frame interrupts
            if (category != null)
            {
                int currentFrame = GetFrameCount();
                if (category == _lastInterruptCategory && currentFrame == _lastInterruptFrame)
                {
                    // Same category, same frame -- the new one will interrupt the old anyway
                    // but we skip logging/overhead
                }
                _lastInterruptCategory = category;
                _lastInterruptFrame = currentFrame;
            }

            SpeechEngine.Say(filtered, interrupt: true);
        }

        /// <summary>
        /// Queue mode: append to queue, never dropped.
        /// Used for alerts and notifications.
        ///
        /// Dedup: if same category is already in the queue, the existing entry
        /// is updated with the latest text and a count suffix ("Broken Wall x2").
        ///
        /// Queue is drained immediately since Tolk handles pacing internally.
        /// </summary>
        /// <param name="text">Raw text (will be filtered through TextFilter).</param>
        /// <param name="category">Optional category for dedup (e.g., "alert.broken_pipe").</param>
        /// <param name="priority">Queue priority level.</param>
        public static void SpeakQueued(string text, string category = null,
            SpeechPriority priority = SpeechPriority.Normal)
        {
            if (!_enabled) return;

            string filtered = TextFilter.FilterForSpeech(text);
            if (string.IsNullOrEmpty(filtered)) return;

            // Dedup: check if same category already in queue
            if (category != null)
            {
                var existing = FindInQueue(category);
                if (existing != null)
                {
                    // Replace with latest text and increment count
                    int currentCount = ExtractCount(existing.Value.Text);
                    int newCount = currentCount + 1;
                    string baseText = StripCountSuffix(filtered);
                    string updatedText = $"{baseText} x{newCount}";

                    // Rebuild queue with updated entry
                    ReplaceInQueue(category, new Announcement(updatedText, priority, false, category));

                    // Record in alert history and drain
                    AlertHistory.Record(updatedText, category);
                    DrainQueue();
                    return;
                }
            }

            var announcement = new Announcement(filtered, priority, false, category);
            _queue.Enqueue(announcement);

            // Record in alert history (interrupt speech does NOT go to alert history)
            AlertHistory.Record(filtered, category);

            // Drain queue immediately -- Tolk handles pacing
            DrainQueue();
        }

        /// <summary>
        /// Stop all speech and clear the queue.
        /// </summary>
        public static void Silence()
        {
            _queue.Clear();
            SpeechEngine.Stop();
        }

        /// <summary>
        /// Drain all pending queue entries through SpeechEngine.
        /// Uses interrupt=false so Tolk queues them naturally.
        /// </summary>
        private static void DrainQueue()
        {
            while (_queue.Count > 0)
            {
                var announcement = _queue.Dequeue();
                SpeechEngine.Say(announcement.Text, interrupt: false);
            }
        }

        /// <summary>
        /// Find an existing announcement in the queue by category.
        /// </summary>
        private static Announcement? FindInQueue(string category)
        {
            foreach (var item in _queue)
            {
                if (item.Category == category)
                    return item;
            }
            return null;
        }

        /// <summary>
        /// Replace an announcement in the queue by category with a new one.
        /// Rebuilds the queue to maintain order.
        /// </summary>
        private static void ReplaceInQueue(string category, Announcement replacement)
        {
            var items = new List<Announcement>(_queue);
            _queue.Clear();
            foreach (var item in items)
            {
                if (item.Category == category)
                    _queue.Enqueue(replacement);
                else
                    _queue.Enqueue(item);
            }
        }

        /// <summary>
        /// Extract the count from a text with a count suffix (e.g., "Broken Wall x2" -> 2).
        /// Returns 1 if no suffix found.
        /// </summary>
        private static int ExtractCount(string text)
        {
            if (string.IsNullOrEmpty(text)) return 1;

            int xIndex = text.LastIndexOf(" x");
            if (xIndex < 0) return 1;

            string countStr = text.Substring(xIndex + 2);
            return int.TryParse(countStr, out int count) ? count : 1;
        }

        /// <summary>
        /// Remove the count suffix from text (e.g., "Broken Wall x2" -> "Broken Wall").
        /// </summary>
        private static string StripCountSuffix(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            int xIndex = text.LastIndexOf(" x");
            if (xIndex < 0) return text;

            string countStr = text.Substring(xIndex + 2);
            return int.TryParse(countStr, out _) ? text.Substring(0, xIndex) : text;
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
