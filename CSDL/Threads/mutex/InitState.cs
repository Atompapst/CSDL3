// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Threads {
    public partial struct InitState {

        /// <summary>
        /// Return whether initialization should be done.
        /// </summary>
        public bool ShouldInit => SDL.ShouldInit(ref this);

        /// <summary>
        /// Return whether cleanup should be done.
        /// </summary>
        public bool ShouldQuit => SDL.ShouldQuit(ref this);

        /// <summary>
        /// Finish an initialization state transition.
        /// </summary>
        /// <param name="initialized"></param>
        public void Set(bool initialized) {
            SDL.SetInitialized(ref this, initialized);
        }

        /// <summary>
        /// Sets the initialization state to true.
        /// </summary>
        public void SetTrue() {
            SDL.SetInitialized(ref this, true);
        }

        /// <summary>
        /// Sets the initialization state to false.
        /// </summary>
        public void SetFalse() {
            SDL.SetInitialized(ref this, false);
        }
    }
}
