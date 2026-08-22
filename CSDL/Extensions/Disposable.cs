// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using System.Collections.Generic;
namespace CSDL.Extensions {
    public static class Disposable {
        public static void Dispose<T>(this T[]? items) where T : IDisposable {
            if (items == null) return;
            foreach (T item in items) {
                item.Dispose();
            }
        }

        public static void Dispose<T>(this T[,]? items) where T : IDisposable {
            if (items == null) return;
            int rows = items.GetLength(0);
            int cols = items.GetLength(1);
            for (int row = 0; row < rows; row++) {
                for (int col = 0; col < cols; col++) {
                    items[row, col].Dispose();
                }
            }
        }

        public static void Dispose<T>(this IEnumerable<T> items) where T : IDisposable {
            foreach (T item in items) {
                item.Dispose();
            }
        }

        public static void ForEach<T>(this T[] array, Action<T> action) {
            foreach (T item in array) {
                action(item);
            }
        }
    }
}
