using System;
using System.IO;
using UnityEngine;

namespace BasyaFramework.Logger
{
    public class BxDebug
    {
        private static BxDebug _instance;
        private static readonly object _lock = new object();
        private readonly string _logFilePath;
        private bool _enableFileLogging = true;
        private bool _enableConsoleLogging = true;
        private LogLevel _minimumLogLevel = LogLevel.Debug;

        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3
        }

        private BxDebug()
        {
            try
            {
                _logFilePath = Path.Combine(Application.persistentDataPath, "game.log");
                Debug.Log($"日志文件路径: {_logFilePath}");
                InitializeLogFile();
            }
            catch (Exception ex)
            {
                Debug.LogError($"日志系统初始化异常: {ex.Message}");
                _enableFileLogging = false;
            }
        }

        public static BxDebug Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new BxDebug();
                        }
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// 初始化日志文件
        /// </summary>
        private void InitializeLogFile()
        {
            try
            {
                // 确保日志目录存在
                string logDirectory = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                if (!File.Exists(_logFilePath))
                {
                    File.Create(_logFilePath).Dispose();
                }

                Debug.Log("======= 日志系统初始化 =======");
            }
            catch (Exception ex)
            {
                Debug.LogError($"无法创建日志文件: {ex.Message}");
                _enableFileLogging = false;
            }
        }

        /// <summary>
        /// 设置最小日志级别
        /// </summary>
        /// <param name="level">日志级别</param>
        public void SetMinimumLogLevel(LogLevel level)
        {
            _minimumLogLevel = level;
        }

        /// <summary>
        /// 启用或禁用文件日志记录
        /// </summary>
        /// <param name="enable">是否启用</param>
        public void EnableFileLogging(bool enable)
        {
            _enableFileLogging = enable;
        }

        /// <summary>
        /// 启用或禁用控制台日志记录
        /// </summary>
        /// <param name="enable">是否启用</param>
        public void EnableConsoleLogging(bool enable)
        {
            _enableConsoleLogging = enable;
        }

        /// <summary>
        /// 记录Debug级别的日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public static void Log(object message)
        {
            Instance.LogMessage(message.ToString(), LogLevel.Debug);
        }

        /// <summary>
        /// 记录Info级别的日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public static void LogInfo(object message)
        {
            Instance.LogMessage(message.ToString(), LogLevel.Info);
        }

        /// <summary>
        /// 记录Warning级别的日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public static void LogWarning(object message)
        {
            Instance.LogMessage(message.ToString(), LogLevel.Warning);
        }

        /// <summary>
        /// 记录Error级别的日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public static void LogError(object message)
        {
            Instance.LogMessage(message.ToString(), LogLevel.Error);
        }

        /// <summary>
        /// 记录异常日志
        /// </summary>
        /// <param name="exception">异常对象</param>
        /// <param name="additionalMessage">附加信息</param>
        public static void LogException(Exception exception, string additionalMessage = "")
        {
            string message = $"异常: {exception.Message}\n堆栈跟踪: {exception.StackTrace}";
            if (!string.IsNullOrEmpty(additionalMessage))
            {
                message = $"{additionalMessage}\n{message}";
            }

            Instance.LogMessage(message, LogLevel.Error);
        }

        /// <summary>
        /// 写入日志消息的核心方法
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="level">日志级别</param>
        private void LogMessage(string message, LogLevel level)
        {
            // 检查是否应该记录此级别的日志
            if (level < _minimumLogLevel) return;
            
            string levelString = level.ToString().ToUpper();
            string formattedMessage = $"[{levelString}] {message}";

            // 控制台输出
            if (_enableConsoleLogging)
            {
                switch (level)
                {
                    case LogLevel.Debug:
                        Debug.Log(formattedMessage);
                        break;
                    case LogLevel.Info:
                        Debug.Log(formattedMessage);
                        break;
                    case LogLevel.Warning:
                        Debug.LogWarning(formattedMessage);
                        break;
                    case LogLevel.Error:
                        Debug.LogError(formattedMessage);
                        break;
                }
            }

            // 文件输出
            if (_enableFileLogging)
            {
                WriteToFile(formattedMessage);
            }
        }

        /// <summary>
        /// 将日志写入文件
        /// </summary>
        /// <param name="message">日志消息</param>
        private void WriteToFile(string message)
        {
            try
            {
                using (StreamWriter writer = File.AppendText(_logFilePath))
                {
                    writer.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"写入日志文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清除日志文件
        /// </summary>
        public void ClearLogFile()
        {
            try
            {
                if (File.Exists(_logFilePath))
                {
                    File.WriteAllText(_logFilePath, string.Empty);
                    LogInfo("日志文件已清空");
                }
            }
            catch (Exception ex)
            {
                LogError($"清空日志文件失败: {ex.Message}");
            }
        }
    }
}