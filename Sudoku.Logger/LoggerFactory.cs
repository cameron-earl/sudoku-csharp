using System;
using System.Collections.Generic;
using System.Linq;
using FolderTransfer.Core;

namespace FolderTransfer.Logger
{
    public static class LoggerFactory
    {
        public static readonly IDictionary<LoggerType, Type> LoggerClasses = new Dictionary<LoggerType, Type>
        {
            {LoggerType.Console, typeof (ConsoleLogger)},
            {LoggerType.File, typeof (FileLogger)},
            {LoggerType.Sql, typeof (SqlLogger)}
        };

        public enum LoggerType
        {
            Console,
            File,
            Sql
        }

        public static LoggerType? GetLoggerType(ILogger logger)
        {
            if (logger == null) return null;
            var key = LoggerClasses.FirstOrDefault(x => x.Value == logger.GetType()).Key;
            return key;
        }

        public static ILogger CreateLogger(LoggerType type, string param)
        {
            var logger = (ILogger) Activator.CreateInstance(LoggerClasses[type], param);
            return logger;
        }
    }
}