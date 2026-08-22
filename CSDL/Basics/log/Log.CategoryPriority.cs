// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL {
    public static partial class Log {

        /// <summary>
        /// Provides access to the active log priority for each SDL log category.
        /// </summary>
        public static readonly ILogPriorityMap CategoryPriority = new CategoryPriorityMap();

        public interface ILogPriorityMap {
            /// <inheritdoc cref="CategoryPriorityMap.this"/>
            LogPriority this[LogCategory category] { get; set; }
            
            /// <inheritdoc cref="CategoryPriorityMap.SetForAll"/>
            void SetForAll(LogPriority priority);

            /// <inheritdoc cref="CategoryPriorityMap.Reset"/>
            void Reset();
        }

        private sealed class CategoryPriorityMap : ILogPriorityMap {
            internal CategoryPriorityMap() { }

            /// <summary>
            /// Get or Set the active log priority for a given category.
            /// </summary>
            /// <seealso cref="CSDL.Internal.Docs.Log.GetLogPriority">GetLogPriority</seealso>
            /// <seealso cref="CSDL.Internal.Docs.Log.SetLogPriority">SetLogPriority</seealso>
            public LogPriority this[LogCategory category] {
                get => SDL.GetLogPriority((int)category);
                set => SDL.SetLogPriority((int)category, value);
            }

            /// <inheritdoc cref="CSDL.Internal.Docs.Log.SetLogPriorities"/>
            public void SetForAll(LogPriority priority) {
                SDL.SetLogPriorities(priority);
            }

            /// <inheritdoc cref="CSDL.Internal.Docs.Log.Reset"/>
            public void Reset() {
                SDL.ResetLogPriorities();
            }
        }
    }
}
