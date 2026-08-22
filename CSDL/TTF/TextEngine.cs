// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.TTF {
    /// <summary>
    /// Base class for the backends that can lay out and draw a <see cref="TextObject"/>
    /// </summary>
    /// <seealso cref="GLTextEngine"/>
    /// <seealso cref="GPUTextEngine"/>
    /// <seealso cref="RendererTextEngine"/>
    /// <seealso cref="SurfaceTextEngine"/>
    public abstract class TextEngine : NativeHandle<Opaque.SdlTextEngine> { }
}
