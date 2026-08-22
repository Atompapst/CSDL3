// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;

namespace CSDL.EventHandlers.Interfaces {
    public interface IRenderEvents {
        event Action<RenderEvent>? Any;
    }
}

namespace CSDL.EventHandlers {
    internal sealed class Render : Interfaces.IRenderEvents {
        public event Action<RenderEvent>? Any;

        internal void Handle(RenderEvent renderEvent) {
            Any?.Invoke(renderEvent);
        }
    }
}
