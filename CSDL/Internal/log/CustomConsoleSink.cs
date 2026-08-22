// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
using static CSDL.Log;
namespace CSDL.Internal.Logging {
    internal sealed class CustomConsoleSink : ILogSink {
        public readonly Dictionary<LogPriority, ConsoleColor> CustomLogColors =
            new Dictionary<LogPriority, ConsoleColor> {
                [LogPriority.Count] = ConsoleColor.White,
                [LogPriority.Trace] = ConsoleColor.DarkGray,
                [LogPriority.Verbose] = ConsoleColor.Gray,
                [LogPriority.Debug] = ConsoleColor.Gray,
                [LogPriority.Info] = ConsoleColor.Green,
                [LogPriority.Warn] = ConsoleColor.Yellow,
                [LogPriority.Error] = ConsoleColor.Red,
                [LogPriority.Critical] = ConsoleColor.Magenta,
            };

        private readonly LogFormatter _formatter;

        public CustomConsoleSink(LogFormatter formatter) {
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        public void Write(in LogEntry entry) {
            string text = _formatter.Format(in entry);

            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = CustomLogColors.GetValueOrDefault(entry.Priority, ConsoleColor.White);

            Console.WriteLine(text);
            Console.ForegroundColor = oldColor;
        }

        public void Flush() { }
    }
}
