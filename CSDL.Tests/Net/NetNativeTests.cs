// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using SdlAddress = CSDL.Net.Address;
using SdlNet = CSDL.Net.Net;
using SdlNetStatus = CSDL.Net.Status;
using CSDL3.Tests.TestSupport;

namespace CSDL3.Tests.Net {
    [Collection(SdlCollection.Name)]
    public class NetNativeTests {
        [Fact]
        public void Version_WithNativeRuntime_LoadsSdlNet() {
            Assert.True(SdlNet.Version > 0);
        }

        [Fact]
        public void GetLocalAddresses_WithNativeRuntime_ReturnsUsableAddresses() {
            SdlAddress[] addresses = SdlNet.GetLocalAddresses();
            try {
                Assert.NotEmpty(addresses);
                Assert.All(addresses, address => Assert.NotEmpty(address.GetBytes()));
            } finally {
                foreach (SdlAddress address in addresses) {
                    address.Dispose();
                }
            }
        }

        [Fact]
        public void Resolve_Localhost_WithNativeRuntime_CompletesSuccessfully() {
            using SdlAddress address = SdlAddress.Resolve("localhost");

            Assert.True(address.WaitUntilResolved(5000));
            Assert.Equal(SdlNetStatus.Success, address.Status);
            Assert.NotEmpty(address.GetBytes());
        }
    }
}
