using System;

using Board.Enums;

using log4net;

namespace Board.Common
{
    public static class LogHelper
    {
        //private static ILog _log;

        //public static ILog Log
        //{
        //    get { return _log ?? (_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)); }
        //}

        public static ILog Log { get; set; }

        public static LogLevelEnum LogLevel { get; set; }

        public static void LogMessage(Type type, LogLevelEnum logLevel, string message, Exception ex)
        {
            Log = LogManager.GetLogger(type);
            switch(logLevel)
            {
                case LogLevelEnum.Debug:
#if DEBUG
                    Log.Debug(message, ex);
#endif
                    break;
                case LogLevelEnum.Error:
                    Log.Error(message, ex);
                    break;
                case LogLevelEnum.Fatal:
                    Log.Fatal(message, ex);
                    break;
                case LogLevelEnum.Info:
                    Log.Info(message, ex);
                    break;
                case LogLevelEnum.Warn:
                    Log.Warn(message, ex);
                    break;
            }
        }
    }
}