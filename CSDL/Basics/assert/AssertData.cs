// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

namespace CSDL {
    public partial struct AssertData {
        public AssertData(bool alwaysIgnore, uint triggerCount, nint condition, nint filename, int lineNum, nint function, nint next = 0) {
            _alwaysIgnore = alwaysIgnore;
            TriggerCount = triggerCount;
            _condition = condition;
            _filename = filename;
            Linenum = lineNum;
            _function = function;
            Next = next == 0 ? NativePtr<AssertData>.Zero : new NativePtr<AssertData>(next);
        }
    }

    /// <summary>
    /// Represents an item in an assertion report. Each instance contains information
    /// about an assertion that was triggered during the execution of the program.
    /// </summary>
    /// <seealso cref="CSDL.Assert.GetReport"/>
    public sealed record AssertReportItem(
        bool AlwaysIgnore,
        uint TriggerCount,
        string Condition,
        string Filename,
        int LineNumber,
        string Function
    );
}
