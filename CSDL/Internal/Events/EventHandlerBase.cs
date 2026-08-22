// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.EventHandlers {
    /// <summary>
    /// Base class for event handlers that provides common lifecycle management.
    /// Tracks whether the handler has been modified this frame and only resets when necessary.
    /// </summary>
    internal abstract class EventHandlerBase : IEventCycle {
        private bool _isDirty;

        /// <summary>
        /// Marks this handler as dirty, indicating it has processed events this frame
        /// and needs to be reset at the start of the next frame.
        /// </summary>
        protected void MarkDirty() {
            _isDirty = true;
        }

        /// <summary>
        /// Called at the beginning of each frame. Only performs a reset if the handler
        /// has been marked dirty (i.e., it processed events last frame).
        /// </summary>
        public void BeginCycle() {
            if (!_isDirty) return;
            ResetState();
            _isDirty = false;
        }

        /// <summary>
        /// Resets all handler-specific state (counters, flags, collections).
        /// Only called when the handler has been marked dirty.
        /// </summary>
        protected abstract void ResetState();

        /// <summary>
        /// Convenience method to increment an event counter and mark the handler as dirty.
        /// </summary>
        protected void IncrementCounter(ref Counter counter) {
            counter.Increment();
            MarkDirty();
        }
    }
}
