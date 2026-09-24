// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Collections.Generic;

namespace CSDL {
    internal static partial class HandleTable {
        /// <summary>
        ///     Logs every resource that was never disposed and returns how many there were.
        /// </summary>
        internal static int ReportLeaks() {
            int total = CountLeaks(out Dictionary<string, int> leaks);

            foreach (KeyValuePair<string, int> leak in leaks) {
                Log.Error(
                    $"[CSDL LEAK DETECTOR]: {leak.Value} '{leak.Key}' handle(s) were never disposed. " +
                    $"Their native resources are being leaked rather than freed from an unknown thread at an " +
                    $"unknown point in SDL's teardown. Wrap them in a 'using' block. FIX YOUR CODE.");
            }

            return total;
        }

        /// <summary>The same count as <see cref="ReportLeaks" />, without saying anything about it.</summary>
        internal static int CountLeaks() {
            return CountLeaks(out _);
        }

        private static int CountLeaks(out Dictionary<string, int> leaks) {
            leaks = new Dictionary<string, int>();
            int total = 0;

            lock (Gate) {
                for (int slot = 0; slot < _slotsUsed; slot++) {
                    ref Slot entry = ref SegmentOf(slot)[slot & SegmentMask];
                    if (entry.Pointer == 0 || entry.Kind != HandleKind.Owned) continue;
                    if (!IsOwnerChainAliveLocked(entry.Owner) && !entry.SurvivesOwner) continue;
                    string name = TypeNames[entry.TypeTag];
                    leaks[name] = leaks.TryGetValue(name, out int seen) ? seen + 1 : 1;
                    total++;
                }
            }

            return total;
        }

        /// <summary>The number of slots ever handed out, for tests that assert the table is not growing.</summary>
        internal static int SlotsUsed {
            get {
                lock (Gate) {
                    return _slotsUsed;
                }
            }
        }

        internal static string Describe(long id) {
            int slot = (int)((uint)id & SlotMask);
            uint generation = (uint)(id >> 32);
            bool view = ((uint)id & ViewFlag) != 0;
            return view ? $"#{slot}.{generation}&" : $"#{slot}.{generation}";
        }
    }
}
