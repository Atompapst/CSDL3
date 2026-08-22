// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL3.Tests.TestSupport {
    /// <summary>
    /// Decodes the packed integer SDL and its satellite libraries return from their
    /// <c>*_Version()</c> entry points: <c>major * 1000000 + minor * 1000 + micro</c>.
    /// </summary>
    public readonly struct SdlVersionNumber {
        public SdlVersionNumber(int packed) {
            Packed = packed;
            Major = packed / 1000000;
            Minor = packed / 1000 % 1000;
            Micro = packed % 1000;
        }

        public int Packed { get; }
        public int Major { get; }
        public int Minor { get; }
        public int Micro { get; }

        public override string ToString() {
            return $"{Major}.{Minor}.{Micro} ({Packed})";
        }
    }
}
