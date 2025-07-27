using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;
using System.Diagnostics;

namespace Minimoo
{
    public enum LogLevel
    {
        Nothing = 0x0000,
        Error = 0x0001,
        Exception = 0x0011,
        Warning = 0x0111,
        Info = 0x1111,
    }

    public struct DebugLogItem
    {
        public object Message;
        public string Color;
        public int Framecount;
        public float Time;
        public int TimePrecision;
        public bool DisplayFrameCount;

        public DebugLogItem(object message, string color, int framecount, float time, int timePrecision, bool displayFrameCount)
        {
            Message = message;
            Color = color;
            Framecount = framecount;
            Time = time;
            TimePrecision = timePrecision;
            DisplayFrameCount = displayFrameCount;
        }
    }

    public static class D
    {
        private static bool s_enabled = false;
        public static bool Enabled { get { return s_enabled; } set { s_enabled = value; } }
        private static LogLevel s_logLevel = LogLevel.Info;
        private const int k_logValidDay = 2;
        private const int k_logHistoryMaxLength = 256;

        static D()
        {
#if D_LOG || UNITY_EDITOR
            s_enabled = true;
#else
            s_enabled = false;
#endif

#if D_LOG_INFO
            s_logLevel = LogLevel.Info;
#elif D_LOG_WARN
            s_logLevel = LogLevel.Warning;
#elif D_LOG_EXCEPT
            s_logLevel = LogLevel.Exception;
#elif D_LOG_ERROR
            s_logLevel = LogLevel.Error;
#endif
        }

        public static void Log(object message)
        {
            if (!ShouldLog(LogLevel.Info))
                return;

            string output = $"<color=#00FFFF>{message}</color>";

            UnityEngine.Debug.Log(output);
        }

        public static void LogFormat(string format, params object[] args)
        {
            if (!ShouldLog(LogLevel.Info))
                return;

            string message = string.Format(format, args);
            Log(message);
        }

        public static void LogWarning(object message)
        {
            Warn(message);
        }

        public static void Warn(object message)
        {
            if (!ShouldLog(LogLevel.Warning))
                return;

            string output = $"<color=#FFC400>{message}</color>";

            UnityEngine.Debug.LogWarning(output);
        }

        public static void WarnFormat(string format, params object[] args)
        {
            if (!ShouldLog(LogLevel.Warning))
                return;

            string message = string.Format(format, args);
            Warn(message);
        }

        public static void LogError(object message)
        {
            Error(message);
        }

        public static void Error(object message)
        {
            if (!ShouldLog(LogLevel.Error))
                return;

            string output = $"{message}";

            UnityEngine.Debug.LogError(output);
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            if (!ShouldLog(LogLevel.Error))
                return;

            string message = string.Format(format, args);
            Error(message);
        }

        public static void LogException(Exception exception)
        {
            Exception(exception, message);
        }

        public static void Exception(Exception exception)
        {
            if (!ShouldLog(LogLevel.Exception))
                return;

            var color = "#FF2A00";
            var stringBuilder = new StringBuilder();

            stringBuilder.Append($"<color={color}>Message: \n{exception.Message}</color>");

            if (exception.StackTrace != null)
            {
                stringBuilder.Append($"\n<color={color}>Stack Trace: \n{exception.StackTrace} </color>");
            }

            if (exception.InnerException != null)
            {
                stringBuilder.Append($"\n<color={color}>Inner Exception: \n{exception.InnerException}</color>");
            }

            UnityEngine.Debug.LogException(stringBuilder.ToString());
        }


        [System.Diagnostics.ConditionalAttribute("UNITY_EDITOR")]
        public static void Assert(bool condition)
        {
            Assert(condition, string.Empty, true);
        }

        [System.Diagnostics.ConditionalAttribute("UNITY_EDITOR")]
        public static void Assert(bool condition, string assertString)
        {
            Assert(condition, assertString, false);
        }

        [System.Diagnostics.ConditionalAttribute("UNITY_EDITOR")]
        public static void Assert(bool condition, string assertString, bool pauseOnFail)
        {
            if (!s_enabled)
            {
                return;
            }

            if (!condition)
            {
                UnityEngine.Debug.LogError("Assert Failed! " + assertString);
                if (pauseOnFail)
                {
                    UnityEngine.Debug.Break();
                }
            }
        }

        public static void SetLogLevel(LogLevel level)
        {
            s_logLevel = level;
            Log($"로그 레벨이 {level}로 설정되었습니다.");
        }

        public static LogLevel GetLogLevel()
        {
            return s_logLevel;
        }

        private static bool ShouldLog(LogLevel level)
        {
            return s_enabled && (s_logLevel & level) == level;
        }
    }
}