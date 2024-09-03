using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using log4net;
using log4net.Config;
using Modules.Utility;


namespace Modules.LogSystem
{
    //TODO:使用特性简化调用过程
    public static class LogManager
    {
        //Debug时的Logger，也就是编辑器模式下使用的Logger
        private static ILog _DebugLogger;

        private static StringBuilder _StringCache = new StringBuilder(1024);

        public static void Prepare()
        {
            //注册全局Log回调
            Application.logMessageReceivedThreaded += _LogMessageReceived_Callback;

#if UNITY_EDITOR
            /*GlobalContext.Properties["LogFolderPath"] = Application.dataPath + @"\Main\Data\Log\";
            XmlConfigurator.Configure(new FileInfo(Application.dataPath + "/Main/log4net.config"));

            _DebugLogger = log4net.LogManager.GetLogger("Debug");*/
#else
            //GlobalContext.Properties["LogFolderPath"] = Application.dataPath + @"\Main\Data\Log\";
            //XmlConfigurator.Configure(new FileInfo(Application.dataPath + "/Main/log4net.config"));

            //_DebugLogger = log4net.LogManager.GetLogger("Debug");
#endif
        }

        /// <summary>
        /// Unity消息回调
        /// </summary>
        /// <param name="log"></param>
        /// <param name="stack_trace"></param>
        /// <param name="type"></param>
        private static void _LogMessageReceived_Callback(string log, string stack_trace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                    //Error(log, stack_trace);
                    break;
                case LogType.Warning:
                    //Warn(log, stack_trace);
                    break;
                case LogType.Log:
                    break;
                case LogType.Assert:
                    break;
                case LogType.Exception:
                    break;
            }
        }

        /// <summary>
        /// 记录一般信息
        /// </summary>
        public static void Info(string src, string msg,
            [CallerMemberName] string member = "",
            params (string, object)[] args)
        {
            _StringCache.Clear();
            _StringCache.AppendLine($"[Log][{src}]@{member}: {msg}");

            foreach (var arg in args)
                _StringCache.AppendLine($"{arg.Item1}: {arg.Item2}\r\n");

#if UNITY_EDITOR || RELEASE_TEST
            UnityEngine.Debug.Log(_StringCache);
#endif

        }

        /// <summary>
        /// 记录一般信息（简单）
        /// </summary>
        public static void Info(string msg)
        {
#if UNITY_EDITOR || RELEASE_TEST
            UnityEngine.Debug.Log($"[Log]: {msg}");
#endif
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        [Conditional("DEBUG")]
        public static void Debug(string src, string msg,
            [CallerMemberName] string member = "",
            params (string, object)[] args)
        {
            _StringCache.Clear();
            _StringCache.AppendLine($"[<color=#1E90FF>Debug</color>][{Time.frameCount}][{src}]@{member}: {msg}");

            foreach (var arg in args)
                _StringCache.AppendLine($"{arg.Item1}: {arg.Item2}");

#if UNITY_EDITOR || RELEASE_TEST
            UnityEngine.Debug.Log(_StringCache);
#endif
        }

        /// <summary>
        /// 记录调试日志（简单）
        /// </summary>
        [Conditional("DEBUG")]
        public static void Debug(object msg)
        {
#if UNITY_EDITOR || RELEASE_TEST
            UnityEngine.Debug.Log($"[<color=#1E90FF>Debug</color>][{Time.frameCount}]: {msg}");
#endif
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        public static void Warn(string src, string msg,
            [CallerMemberName] string member = "",
            params (string, object)[] args)
        {
            _StringCache.Clear();
            _StringCache.AppendLine($"[<color=#FF7F24>Warn</color>][{src}]@{member}: {msg}");

            foreach (var arg in args)
                _StringCache.AppendLine($"{arg.Item1}: {arg.Item2}");

#if UNITY_EDITOR || RELEASE_TEST
            UnityEngine.Debug.LogWarning(_StringCache);
#endif
        }

        /// <summary>
        /// 记录警告日志（简单）
        /// </summary>
        public static void Warn(string msg)
        {
#if UNITY_EDITOR || RELEASE_TEST
            UnityEngine.Debug.LogWarning($"[<color=#FF7F24>Warn</color>]: {msg}");
#endif
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        public static void Error(string src, string msg,
            [CallerMemberName] string member = "",
            params (string, object)[] args)
        {
            _StringCache.Clear();
            _StringCache.AppendLine($"[<color=#FF3030>Error</color>][{src}]@{member}: {msg}");

            foreach (var arg in args)
                _StringCache.AppendLine($"{arg.Item1} : {arg.Item2}");

#if UNITY_EDITOR || RELEASE_TEST
            UnityEngine.Debug.LogError(_StringCache);
#endif
        }

        /// <summary>
        /// 记录错误日志（简单）
        /// </summary>
        public static void Error(string msg)
        {
#if UNITY_EDITOR || RELEASE_TEST
            UnityEngine.Debug.LogError($"[<color=#FF3030>Error</color>]: {msg}");
#endif
        }

        /// <summary>
        /// 断言（当cond==false时抛出异常）
        /// </summary>
        public static void Assert(bool cond, string src, string msg,
            [CallerMemberName] string member = "",
            params (string, object)[] args)
        {
            if (!cond)
                Throw($"断言失败：from {src} @{member} {msg}", args);
        }

        /// <summary>
        /// 断言（简单）（当cond==false时抛出异常）
        /// </summary>
        public static void Assert(bool cond, string msg)
        {
            if (!cond)
                Throw($"断言失败：{msg}");
        }

        /// <summary>
        /// 记录LogMessage形式的日志
        /// </summary>
        public static void LogMessage(LogMessage lmsg)
        {
            string str = $"from {lmsg.Source} @{lmsg.Position} : {lmsg.Message}\r\n";

            foreach (var arg in lmsg.Arguments)
                str += $"{arg.Key} : {arg.Value}\r\n";

#if UNITY_EDITOR || RELEASE_TEST
            switch (lmsg.Type)
            {
                case LogMessageType.Debug:
                    UnityEngine.Debug.Log($"[<color=#1E90FF>{lmsg.Type}</color>]{str}");
                    break;
                case LogMessageType.Warn:
                    UnityEngine.Debug.LogWarning($"[<color=#FF7F24>{lmsg.Type}</color>]{str}");
                    break;
                case LogMessageType.Exception or LogMessageType.Error:
                    UnityEngine.Debug.LogError(str);
                    break;
            }
#endif
        }

        /// <summary>
        /// 抛出一个GameException异常
        /// </summary>
        public static void Throw(string msg, params (string, object)[] data)
        {
            throw new GameException(msg, data);
        }
    }
}
