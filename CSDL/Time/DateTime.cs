// SPDX-FileCopyrightText: 2026 Christof Ignacy
// SPDX-License-Identifier: Zlib

using System;
using CSDL.Extensions;

namespace CSDL {
    public partial struct DateTime {
        /// <summary>
        /// Gets the date component of this instance.
        /// </summary>
        public DateTime Date => new DateTime(Year, Month, Day, 0, 0, 0, 0, UtcOffset);

        /// <summary>
        /// Gets the time of Day for this instance.
        /// </summary>
        public TimeSpan TimeOfDay => new TimeSpan(0, Hour, Minute, Second, Nanosecond / 1_000_000, Nanosecond / 1_000);

        public DateTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, int nanosecond = 0, int utcOffset = 0) {
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = minute;
            Second = second;
            Nanosecond = nanosecond;
            DayOfWeek = SDL.GetDayOfWeek(year, month, day);
            UtcOffset = utcOffset;
        }

        internal DateTime(DateTime th) {
            this = th;
        }

        /// <inheritdoc cref="CSDL.Internal.Docs.Time.GetDayOfYear"/>
        public int DayOfYear => SDL.GetDayOfYear(Year, Month, Day);

        /// <summary>
        /// Gets a DateTime object that is set to the current date and time on this computer, expressed as local time.
        /// </summary>
        public static DateTime Now {
            get {
                DateTime dt = new DateTime();
                Time ticks = default;
                if (SDL.GetCurrentTime(ref ticks).LogIfFalse()) {
                    SDL.TimeToDateTime(ticks, ref dt, true).LogIfFalse();
                }
                return new DateTime(dt);
            }
        }

        /// <summary>
        /// Gets a DateTime object that is set to the current date and time on this computer, expressed as UTC.
        /// </summary>
        public static DateTime UtcNow {
            get {
                DateTime dt = new DateTime();
                Time ticks = default;
                if (SDL.GetCurrentTime(ref ticks).LogIfFalse()) {
                    SDL.TimeToDateTime(ticks, ref dt, false).LogIfFalse();
                }
                return new DateTime(dt);
            }
        }

        /// <summary>
        /// Gets the current date with time set to 00:00:00.
        /// </summary>
        public static DateTime Today => Now.Date;

        /// <inheritdoc cref="CSDL.Internal.Docs.Time.DateTimeToTime"/>
        public Time ToTime() {
            DateTime dt = this;
            Time ticks = default;
            SDL.DateTimeToTime(in dt, ref ticks).LogIfFalse();
            return ticks;
        }

        /// <summary>
        /// Adds the specified TimeSpan to this DateTime.
        /// </summary>
        public DateTime Add(TimeSpan timeSpan) {
            long ticks = ToTime().Ticks;
            ticks += timeSpan.Ticks * 100; // Convert 100ns ticks to Nanoseconds

            DateTime dt = default;
            if (!SDL.TimeToDateTime(new Time(ticks), ref dt, false).LogIfFalse()) {
                return this;
            }
            return new DateTime(dt);
        }

        /// <summary>
        /// Adds the specified number of Years to this DateTime.
        /// </summary>
        public DateTime AddYears(int Years) {
            return new DateTime(Year + Years, Month, Day, Hour, Minute, Second, Nanosecond, UtcOffset);
        }

        /// <summary>
        /// Adds the specified number of Months to this DateTime.
        /// </summary>
        public DateTime AddMonths(int Months) {
            int newMonth = Month + Months;
            int newYear = Year;

            while (newMonth > 12) {
                newMonth -= 12;
                newYear++;
            }
            while (newMonth < 1) {
                newMonth += 12;
                newYear--;
            }

            int maxDay = SDL.GetDaysInMonth(newYear, newMonth);
            int newDay = Day > maxDay ? maxDay : Day;

            return new DateTime(newYear, newMonth, newDay, Hour, Minute, Second, Nanosecond, UtcOffset);
        }

        /// <summary>
        /// Adds the specified number of Days to this DateTime.
        /// </summary>
        public DateTime AddDays(double Days) {
            return Add(TimeSpan.FromDays(Days));
        }

        /// <summary>
        /// Adds the specified number of Hours to this DateTime.
        /// </summary>
        public DateTime AddHours(double Hours) {
            return Add(TimeSpan.FromHours(Hours));
        }

        /// <summary>
        /// Adds the specified number of Minutes to this DateTime.
        /// </summary>
        public DateTime AddMinutes(double Minutes) {
            return Add(TimeSpan.FromMinutes(Minutes));
        }

        /// <summary>
        /// Adds the specified number of Seconds to this DateTime.
        /// </summary>
        public DateTime AddSeconds(double Seconds) {
            return Add(TimeSpan.FromSeconds(Seconds));
        }

        /// <summary>
        /// Adds the specified number of milliSeconds to this DateTime.
        /// </summary>
        public DateTime AddMilliSeconds(double milliSeconds) {
            return Add(TimeSpan.FromMilliseconds(milliSeconds));
        }

        /// <summary>
        /// Compares this instance to another DateTime and returns an indication of their relative values.
        /// </summary>
        private int CompareTo(DateTime other) {
            long thisTicks = ToTime().Ticks;
            long otherTicks = other.ToTime().Ticks;
            return thisTicks.CompareTo(otherTicks);
        }

        /// <summary>
        /// Compares this instance to another object and returns an indication of their relative values.
        /// </summary>
        public int CompareTo(object obj) {
            if (obj == null) return 1;
            if (obj is not DateTime other)
                throw new SDLException("Object must be of type DateTime");
            return CompareTo(other);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to another DateTime.
        /// </summary>
        public bool Equals(DateTime other) {
            return Year == other.Year &&
                   Month == other.Month &&
                   Day == other.Day &&
                   Hour == other.Hour &&
                   Minute == other.Minute &&
                   Second == other.Second &&
                   Nanosecond == other.Nanosecond &&
                   UtcOffset == other.UtcOffset;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        public override bool Equals(object? obj) {
            return obj is DateTime other && Equals(other);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() {
            return HashCode.Combine(Year, Month, Day, Hour, Minute, Second, Nanosecond, UtcOffset);
        }

        /// <summary>
        /// Subtracts the specified DateTime from this instance and returns a TimeSpan.
        /// </summary>
        public TimeSpan Subtract(DateTime value) {
            long thisTicks = ToTime().Ticks;
            long otherTicks = value.ToTime().Ticks;
            long diffNanoseconds = thisTicks - otherTicks;
            // Convert Nanoseconds to 100-Nanosecond ticks for TimeSpan
            return new TimeSpan(diffNanoseconds / 100);
        }

        /// <summary>
        /// Subtracts the specified TimeSpan from this instance.
        /// </summary>
        public DateTime Subtract(TimeSpan value) {
            return Add(-value);
        }

        // Operators
        public static bool operator ==(DateTime left, DateTime right) {
            return left.Equals(right);
        }

        public static bool operator !=(DateTime left, DateTime right) {
            return !left.Equals(right);
        }

        public static bool operator <(DateTime left, DateTime right) {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(DateTime left, DateTime right) {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(DateTime left, DateTime right) {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(DateTime left, DateTime right) {
            return left.CompareTo(right) >= 0;
        }

        public static DateTime operator +(DateTime dateTime, TimeSpan timeSpan) {
            return dateTime.Add(timeSpan);
        }

        public static DateTime operator -(DateTime dateTime, TimeSpan timeSpan) {
            return dateTime.Subtract(timeSpan);
        }

        public static TimeSpan operator -(DateTime left, DateTime right) {
            return left.Subtract(right);
        }

        public override string ToString() {
            return $"{Year:D4}-{Month:D2}-{Day:D2} {Hour:D2}:{Minute:D2}:{Second:D2}.{Nanosecond / 1_000_000:D3}";
        }

        /// <summary>
        /// Converts the value of the current DateTime object to its equivalent long date string representation.
        /// </summary>
        public string ToLongDateString() {
            string[] DayNames = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            return $"{DayNames[DayOfWeek]}, {Year:D4}-{Month:D2}-{Day:D2}";
        }

        /// <summary>
        /// Converts the value of the current DateTime object to its equivalent short date string representation.
        /// </summary>
        public string ToShortDateString() {
            return $"{Month:D2}/{Day:D2}/{Year:D4}";
        }

        /// <summary>
        /// Converts the value of the current DateTime object to its equivalent long time string representation.
        /// </summary>
        public string ToLongTimeString() {
            return $"{Hour:D2}:{Minute:D2}:{Second:D2}";
        }

        /// <summary>
        /// Converts the value of the current DateTime object to its equivalent short time string representation.
        /// </summary>
        public string ToShortTimeString() {
            return $"{Hour:D2}:{Minute:D2}";
        }

        public string ToLongString() {
            string[] DayNames = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            return $"{DayNames[DayOfWeek]}, {Year:D4}-{Month:D2}-{Day:D2} {Hour:D2}:{Minute:D2}:{Second:D2}.{Nanosecond:D9} (UTC{(UtcOffset >= 0 ? "+" : "")}{UtcOffset / 3600:D2}:{Math.Abs(UtcOffset % 3600) / 60:D2})";
        }
    }
}
