namespace OniAccess.Speech
{
    /// <summary>
    /// Priority levels for speech announcements.
    /// Higher priority announcements are processed first in the queue.
    /// </summary>
    public enum SpeechPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    /// <summary>
    /// Value type representing a speech announcement with priority, interrupt behavior,
    /// and category for deduplication.
    ///
    /// Category is used for dedup: if the same category appears multiple times in the
    /// queue, they are combined with a count suffix ("Broken Wall x2").
    /// </summary>
    public readonly struct Announcement
    {
        /// <summary>The filtered text to speak.</summary>
        public string Text { get; }

        /// <summary>Priority level for queue ordering.</summary>
        public SpeechPriority Priority { get; }

        /// <summary>If true, interrupts current speech immediately.</summary>
        public bool Interrupt { get; }

        /// <summary>
        /// Category for deduplication (e.g., "navigation", "alert.broken_pipe").
        /// Null means no dedup -- each announcement is unique.
        /// </summary>
        public string Category { get; }

        public Announcement(string text, SpeechPriority priority, bool interrupt, string category = null)
        {
            Text = text;
            Priority = priority;
            Interrupt = interrupt;
            Category = category;
        }
    }
}
