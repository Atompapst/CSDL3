// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
namespace CSDL.File {
    /// <summary>
    /// Represents the available modes for opening or interacting with input/output streams.
    /// </summary>
    [Flags]
    public enum IOStreamMode {
        /// <summary>
        /// Represents a mode that allows reading operations on a stream. <c>r</c>
        /// </summary>
        /// <remarks>
        /// When the <c>Read</c> mode is used, the stream is opened for reading data from the file.
        /// This mode is typically used in conjunction with other flags for more specific behavior,
        /// such as combining it with <c>Plus</c> to allow both reading and writing.
        /// </remarks>
        Read = 1 << 0,
        /// <summary>
        /// Represents a mode that allows writing operations on a stream. <c>w</c>
        /// </summary>
        /// <remarks>
        /// When the <c>Write</c> mode is used, the stream is opened for writing data to the file.
        /// This mode typically truncates the existing contents of the file unless combined with
        /// other modifiers such as <c>Plus</c>, which allows reading and writing simultaneously.
        /// Use this mode to create or overwrite the contents of a stream.
        /// </remarks>
        Write = 1 << 1,
        /// <summary>
        /// Represents a mode that allows appending operations to a stream. <c>a</c>
        /// </summary>
        /// <remarks>
        /// When the <c>Append</c> mode is used, the stream is opened in a way that allows data to be written
        /// to the end of the file, preserving any existing content. This mode is commonly used for scenarios
        /// where additional data needs to be appended without modifying the existing contents of the stream.
        /// </remarks>
        Append = 1 << 2,
        /// <summary>
        /// Represents a mode that allows both reading and writing operations on a stream. <c>+</c>
        /// </summary>
        /// <remarks>
        /// The <c>Plus</c> mode is a modifier that can be used in combination with a base mode,
        /// such as <c>Read</c>, <c>Write</c>, or <c>Append</c>, to enable bidirectional access
        /// to the stream. This mode is particularly useful for applications that need to both
        /// read from and write to the same stream without reopening it.
        /// </remarks>
        Plus = 1 << 4,
        /// <summary>
        /// Represents a mode that signifies binary data operations on a stream. <c>b</c>
        /// </summary>
        /// <remarks>
        /// When the <c>Binary</c> mode is used, the stream operates in binary mode,
        /// as opposed to text mode. This mode is typically utilized to ensure that
        /// data is read or written exactly as it exists, without any encoding or
        /// translation for text formats. <c>Binary</c> mode is often combined with
        /// base modes such as <c>Read</c>, <c>Write</c>, or <c>Append</c>.
        /// </remarks>
        Binary = 1 << 5,
        /// <summary>
        /// Represents a mode that ensures exclusive access to a stream. <c>x</c>
        /// </summary>
        /// <remarks>
        /// When the <c>Exclusive</c> mode is used, the stream is locked to prevent
        /// access from other processes or threads. This mode is typically useful in
        /// scenarios where data integrity and isolation are required during stream operations.
        /// </remarks>
        Exclusive = 1 << 6,
    }
}
