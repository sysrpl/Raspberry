﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static Codebot.Raspberry.Libc;

namespace Codebot.Raspberry
{
    /// <summary>
    /// The precise timer class is used to measure time intervals in with the
    /// greatest possible accuracy.
    /// </summary>
    public class PreciseTimer : IDisposable
    {
        const double EPSILON = 0.000_000_1d;
        static readonly double frequency;

        static PreciseTimer()
        {
            frequency = Stopwatch.Frequency;
            var t = new timespec()
            {
                tv_sec = IntPtr.Zero,
                tv_nsec = (IntPtr)100_000
            };
            nanosleep(ref t, IntPtr.Zero);
            t.tv_nsec = (IntPtr)1_000;
            for (var i = 0; i < 10; i++)
                nanosleep(ref t, IntPtr.Zero);
            timespec n;
            double a, b, c;
            for (var i = 0; i < 10; i++)
            {
                clock_gettime(CLOCK_MONOTONIC_RAW, out n);
                a = (int)n.tv_sec * 1000d + (int)n.tv_nsec / 1_000_000d;
                nanosleep(ref t, IntPtr.Zero);
                clock_gettime(CLOCK_MONOTONIC_RAW, out n);
                b = (int)n.tv_sec * 1000d + (int)n.tv_nsec / 1_000_000d;
                c = b - a;
                if (c > WaitResolution)
                    WaitResolution = c;
            }
            WaitResolution *= 1.5d;
        }

        /// <summary>
        /// Now is the current precise time in milliseconds.
        /// </summary>
        public static double Now => Stopwatch.GetTimestamp() / frequency * 1000d;

        /// <summary>
        /// Wait a precise number of specified milliseconds.
        /// </summary>
        public static void Wait(double milliseconds)
        {
            double start = Stopwatch.GetTimestamp() / frequency * 1000d;
            if (milliseconds < WaitResolution)
            {
                while (milliseconds - Stopwatch.GetTimestamp() / frequency * 1000d + start > 0d)
                  { }
                return;
            }
            timespec t;
            t.tv_sec = IntPtr.Zero;
            t.tv_nsec = (IntPtr)500_000_000;
            while (milliseconds - Stopwatch.GetTimestamp() / frequency * 1000d + start > 1_000_000_000)
                clock_nanosleep(CLOCK_MONOTONIC_RAW, 0, ref t, IntPtr.Zero);
            t.tv_nsec = (IntPtr)50_000_000;
            while (milliseconds - Stopwatch.GetTimestamp() / frequency * 1000d + start > 100_000_000)
                clock_nanosleep(CLOCK_MONOTONIC_RAW, 0, ref t, IntPtr.Zero);
            t.tv_nsec = (IntPtr)5_000_000;
            while (milliseconds - Stopwatch.GetTimestamp() / frequency * 1000d + start > 10_000_000)
                clock_nanosleep(CLOCK_MONOTONIC_RAW, 0, ref t, IntPtr.Zero);
            t.tv_nsec = (IntPtr)500_000;
            while (milliseconds - Stopwatch.GetTimestamp() / frequency * 1000d + start > 1_000_000)
                clock_nanosleep(CLOCK_MONOTONIC_RAW, 0, ref t, IntPtr.Zero);
            t.tv_nsec = (IntPtr)1_000;
            while (milliseconds - Stopwatch.GetTimestamp() / frequency * 1000d + start > WaitResolution)
                clock_nanosleep(CLOCK_MONOTONIC_RAW, 0, ref t, IntPtr.Zero);
            while (milliseconds - Stopwatch.GetTimestamp() / frequency * 1000d + start > 0d)
                { }
        }

        /// <summary>
        /// Invoke an async callback once after n milliseconds.
        /// </summary>
        /// <param name="milliseconds">The number of milliseconds to wait before the
        /// callback is invoked.</param>
        /// <param name="callback">The callback to invoke after n milliseconds.</param>
        public static async Task Once(double milliseconds, Action callback)
        {
            var timer = new PreciseTimer();
            await Task.Run(() =>
            {
                if (milliseconds < EPSILON)
                {
                    callback();
                    return;
                }
                Wait(milliseconds - timer.ElapsedMilliseconds);
                callback();
            });
        }

        /// <summary>
        /// Invoke an async callback every time n milliseconds have passed.
        /// </summary>
        /// <param name="milliseconds">The number of milliseconds to wait before the
        /// callback is invoked.</param>
        /// <param name="callback">The callback to invoke every n milliseconds.
        /// When callback returns false the task is cancelled.</param>
        public static async Task Every(double milliseconds, Func<bool> callback)
        {
            var timer = new PreciseTimer();
            await Task.Run(() =>
            {
                if (milliseconds < EPSILON)
                    return;
                while (true)
                {
                    var w = timer.ElapsedMilliseconds % milliseconds;
                    Wait(milliseconds - w);
                    if (!callback())
                        break;
                }
            });
        }

        /// <summary>
        /// The minimum sleep time in milliseconds supported by your hardware.
        /// </summary>
        public static double WaitResolution { get; private set; }

        double start;

        /// <summary>
        /// Initializes a new precise timer.
        /// </summary>
        public PreciseTimer()
        {
            Reset();
        }

        /// <summary>
        /// Reset the timer to zero.
        /// </summary>
        public void Reset()
        {
            if (enabled)
            {
                Enabled = false;
                Enabled = true;
            }
            else
                start = Now;
        }

        /// <summary>
        /// The number of seconds since the last reset.
        /// </summary>
        public double ElapsedSeconds { get => Stopwatch.GetTimestamp() / frequency - start; }

        /// <summary>
        /// The number of milliseconds since the last reset.
        /// </summary>
        public double ElapsedMilliseconds { get => (Stopwatch.GetTimestamp() / frequency - start) * 1_000d; }

        /// <summary>
        /// The number of microseconds since the last reset.
        /// </summary>
        public double ElapsedMicroseconds { get => (Stopwatch.GetTimestamp() / frequency - start) * 1_000_000d; }

        /// <summary>
        /// The number of nanoseconds since the last reset.
        /// </summary>
        public double ElapsedNanoseconds { get => (Stopwatch.GetTimestamp() / frequency - start) * 1_000_000_000d; }

        void ElapseTask(long id, double mark, double interval)
        {
            while (true)
            {
                var w = (ElapsedMilliseconds - mark) % interval;
                Wait(interval - w);
                if (id == taskId)
                    OnElapsed(this, EventArgs.Empty);
                else
                    break;
            }
        }

        /// <summary>
        /// The number of milliseconds to ellaspe before OnElapsed is invoked.
        /// </summary>
        public double Interval { get; set;  }

        Task task;
        bool enabled;
        long taskId;

        /// <summary>
        /// When enabled is set to true, OnElapsed will be invoked every interval
        /// number of milliseconds.
        /// </summary>
        public bool Enabled
        {
            get => enabled;
            set
            {
                var e = value && Interval > EPSILON;
                if (e == enabled)
                    return;
                enabled = e;
                taskId++;
                if (enabled)
                   task = Task.Run(() => ElapseTask(taskId, ElapsedMilliseconds, Interval));
                else
                {
                    task?.Wait();
                    task = null;
                }
            }
        }

        /// <summary>
        /// OnElapsed is invoked every interval number of milliseconds when enabled
        /// is set to true.
        /// </summary>
        public event EventHandler OnElapsed;

        /// <summary>
        /// Dispose releases all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Enabled = false;
        }
    }
}
