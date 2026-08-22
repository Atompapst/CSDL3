// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System.Runtime.InteropServices;
using CSDL.Extensions;

namespace CSDL {
    /// <summary>
    /// SDL times are signed, 64-bit integers (<see cref="long"/>) representing nanoseconds since the Unix epoch (Jan 1, 1970).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct Time(long Ticks) {
        public static implicit operator long(Time time) {
            return time.Ticks;
        }
        public static implicit operator Time(long ticks) {
            return new Time(ticks);
        }
        public override string ToString() {
            return Ticks.ToString();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Time.GetCurrentTime"/>
        public static Time Now {
            get {
                Time ticks = default;
                SDL.GetCurrentTime(ref ticks).LogIfFalse();
                return ticks;
            }
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Time.GetDateTimeLocalePreferences"/>
        public static bool GetLocalePreferences(out DateFormat dateFormat, out TimeFormat timeFormat) {
            DateFormat df = default;
            TimeFormat tf = default;
            bool result = SDL.GetDateTimeLocalePreferences(NativePtr<DateFormat>.FromRef(ref df), NativePtr<TimeFormat>.FromRef(ref tf)).LogIfFalse();
            dateFormat = df;
            timeFormat = tf;
            return result;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Time.DateTimeToTime"/>
        public static bool FromDateTime(DateTime dateTime, out Time ticks) {
            ticks = default;
            return SDL.DateTimeToTime(in dateTime, ref ticks).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Time.TimeToDateTime"/>
        public bool ToDateTime(out DateTime dateTime, bool localTime = true) {
            dateTime = default;
            return SDL.TimeToDateTime(this, ref dateTime, localTime).LogIfFalse();
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Time.TimeToWindows"/>
        public void ToWindows(out uint dwLowDateTime, out uint dwHighDateTime) {
            SDL.TimeToWindows(this, out dwLowDateTime, out dwHighDateTime);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Time.TimeFromWindows"/>
        public static Time FromWindows(uint dwLowDateTime, uint dwHighDateTime) {
            return SDL.TimeFromWindows(dwLowDateTime, dwHighDateTime);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Time.GetDaysInMonth"/>
        public static int GetDaysInMonth(int year, int month) {
            return SDL.GetDaysInMonth(year, month);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Time.GetDayOfYear"/>
        public static int GetDayOfYear(int year, int month, int day) {
            return SDL.GetDayOfYear(year, month, day);
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Time.GetDayOfWeek"/>
        public static int GetDayOfWeek(int year, int month, int day) {
            return SDL.GetDayOfWeek(year, month, day);
        }
    }
}
