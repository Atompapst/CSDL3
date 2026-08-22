// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using CSDL.GPU;

namespace CSDL3.Tests.GPU {
    public class GPUConfigurationTests {
        [Fact]
        public void GPUBooleanConfigurationFieldsAreSettable() {
            GPUDepthStencilState depthStencil = new() {
                EnableDepthTest = true,
                EnableDepthWrite = true,
                EnableStencilTest = true,
            };
            GPURasterizerState rasterizer = new() {
                EnableDepthBias = true,
                EnableDepthClip = true,
            };
            GPUSamplerCreateInfo sampler = new() {
                EnableAnisotropy = true,
                EnableCompare = true,
            };
            GPUColorTargetInfo colorTarget = new() {
                Cycle = true,
                CycleResolveTexture = true,
            };
            GPUStorageBufferReadWriteBinding storageBuffer = new() { Cycle = true };
            GPUStorageTextureReadWriteBinding storageTexture = new() { Cycle = true };
            GPUBlitInfo blit = new() { Cycle = true };

            Assert.True(depthStencil.EnableDepthTest);
            Assert.True(depthStencil.EnableDepthWrite);
            Assert.True(depthStencil.EnableStencilTest);
            Assert.True(rasterizer.EnableDepthBias);
            Assert.True(rasterizer.EnableDepthClip);
            Assert.True(sampler.EnableAnisotropy);
            Assert.True(sampler.EnableCompare);
            Assert.True(colorTarget.Cycle);
            Assert.True(colorTarget.CycleResolveTexture);
            Assert.True(storageBuffer.Cycle);
            Assert.True(storageTexture.Cycle);
            Assert.True(blit.Cycle);
        }
    }
}
