namespace CSDL.Video.blendmode {
    public static class BlendModeExtension {
        
        /// <inheritdoc cref="CSDL.Internal.Docs.Blendmode.ComposeCustomBlendMode"/>
        public static BlendMode ComposeCustom(this BlendFactor srcColorFactor, BlendFactor dstColorFactor, BlendOperation colorOperation, BlendFactor srcAlphaFactor, BlendFactor dstAlphaFactor, BlendOperation alphaOperation) {
            return SDL.ComposeCustomBlendMode(srcColorFactor, dstColorFactor, colorOperation, srcAlphaFactor, dstAlphaFactor, alphaOperation);
        }
    }
}
