// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.GPU {
    public partial struct GPUColorTargetBlendState {
        /// <summary>Enables or disables blending for the color target.</summary>
        public void SetEnableBlend(bool enable) {
            _enableBlend = enable;
        }

        /// <summary>Enables or disables the explicit color write mask.</summary>
        public void SetEnableColorWriteMask(bool enable) {
            _enableColorWriteMask = enable;
        }

        /// <summary>
        /// Builds a standard "premultiplied over" alpha blend state:
        /// result = source + destination * (1 - source.alpha).
        /// This matches shader outputs that already premultiply color by coverage/alpha
        /// (as the Slug text shaders do).
        /// </summary>
        public static GPUColorTargetBlendState PremultipliedAlpha() {
            GPUColorTargetBlendState state = new GPUColorTargetBlendState {
                SrcColorBlendfactor = GPUBlendFactor.One,
                DstColorBlendfactor = GPUBlendFactor.OneMinusSrcAlpha,
                ColorBlendOp = GPUBlendOp.Add,
                SrcAlphaBlendfactor = GPUBlendFactor.One,
                DstAlphaBlendfactor = GPUBlendFactor.OneMinusSrcAlpha,
                AlphaBlendOp = GPUBlendOp.Add,
            };
            state.SetEnableBlend(true);
            return state;
        }
    }
}
