// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib


namespace CSDL.File {
    public partial class IOStream {
        private static string ModeToString(IOStreamMode mode) {
            string baseMode;
            if (mode.HasFlag(IOStreamMode.Read))
                baseMode = "r";
            else if (mode.HasFlag(IOStreamMode.Write))
                baseMode = "w";
            else if (mode.HasFlag(IOStreamMode.Append))
                baseMode = "a";
            else
                throw new SDLException("Must specify Read, Write, or Append as base mode");

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(baseMode);

            // "x" (exclusive) is always after base
            if (mode.HasFlag(IOStreamMode.Exclusive))
                sb.Append('x');

            // "b" (binary) can go after base+exclusive or after '+'
            // SDL accepts both locations
            if (mode.HasFlag(IOStreamMode.Binary))
                sb.Append('b');

            // '+' (update)
            if (mode.HasFlag(IOStreamMode.Plus))
                sb.Append('+');

            return sb.ToString();
        }
    }

}
