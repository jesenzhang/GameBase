using System;
using System.Collections.Generic;

namespace GameBase
{
    public static class DateTimer
    {
        private static DateTime m_baseTime = new DateTime(1970, 1, 1);
        public static long TotalDays
        {
            get
            {
                return (long)DateTime.Now.Subtract(m_baseTime).TotalDays;
            }
        }
        public static long TotalHours
        {
            get
            {
                return (long)DateTime.Now.Subtract(m_baseTime).TotalHours;
            }
        }
        public static long TotalMilliseconds
        {
            get
            {
                return (long)DateTime.Now.Subtract(m_baseTime).TotalMilliseconds;
            }
        }
        public static long TotalMinutes
        {
            get
            {
                return (long)DateTime.Now.Subtract(m_baseTime).TotalMinutes;
            }
        }
        public static long TotalSeconds
        {
            get
            {
                return (long)DateTime.Now.Subtract(m_baseTime).TotalSeconds;
            }
        }
        public static long TotalDaysForTime(DateTime time)
        {
            return (long)time.Subtract(m_baseTime).TotalDays;
        }
        public static long TotalHoursForTime(DateTime time)
        {
            return (long)time.Subtract(m_baseTime).TotalHours;
        }
        public static long TotalMillisecondsForTime(DateTime time)
        {
            return (long)time.Subtract(m_baseTime).TotalMilliseconds;
        }
        public static long TotalMinutesForTime(DateTime time)
        {
            return (long)time.Subtract(m_baseTime).TotalMinutes;
        }
        public static long TotalSecondsForTime(DateTime time)
        {
            return (long)time.Subtract(m_baseTime).TotalSeconds;
        }
    }
}
