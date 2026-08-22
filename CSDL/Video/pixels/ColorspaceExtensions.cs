// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL.Video {
    public static class ColorspaceExtensions {
        /// <inheritdoc cref="CSDL.Video.Macros.Colorspacematrix"/>
        public static MatrixCoefficients GetMatrixCoefficients(this Colorspace colorspace) {
            return Macros.Colorspacematrix((uint)colorspace);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.Colorspacerange"/>
        public static ColorRange GetRange(this Colorspace colorspace) {
            return Macros.Colorspacerange((uint)colorspace);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.DefineColorspace"/>
        public static Colorspace Define(ColorType type, ColorRange range, ColorPrimaries primaries, TransferCharacteristics transfer, MatrixCoefficients matrix, ChromaLocation chroma) {
            return (Colorspace)Macros.DefineColorspace((uint)type, (uint)range, (uint)primaries, (uint)transfer, (uint)matrix, (uint)chroma);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.IscolorspaceFullRange"/>
        public static bool IsFullRange(this Colorspace colorspace) {
            return Macros.IscolorspaceFullRange((uint)colorspace);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.IscolorspaceLimitedRange"/>
        public static bool IsLimitedRange(this Colorspace colorspace) {
            return Macros.IscolorspaceLimitedRange((uint)colorspace);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.IscolorspaceMatrixBT2020Ncl"/>
        public static bool UsesBT2020NclMatrix(this Colorspace colorspace) {
            return Macros.IscolorspaceMatrixBT2020Ncl((uint)colorspace);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.IscolorspaceMatrixBT601"/>
        public static bool UsesBT601Matrix(this Colorspace colorspace) {
            return Macros.IscolorspaceMatrixBT601((uint)colorspace);
        }

        /// <inheritdoc cref="CSDL.Video.Macros.IscolorspaceMatrixBT709"/>
        public static bool UsesBT709Matrix(this Colorspace colorspace) {
            return Macros.IscolorspaceMatrixBT709((uint)colorspace);
        }
    }
}
