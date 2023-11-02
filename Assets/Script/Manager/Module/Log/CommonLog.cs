using IFDEF = System.Diagnostics.ConditionalAttribute;

namespace Common
{
    public class CommonLog
    {
        static LogSystem SystemLog = new LogSystem("log_System.txt");


        [IFDEF("ENABLE_LOG_FILE")]
        public static void Log(string msg)
        {
            SystemLog.Log(msg);
        }


        [IFDEF("ENABLE_LOG_FILE")]
        public static void Log(string msg, object arg)
        {
            SystemLog.Log(msg, arg);
        }

        [IFDEF("ENABLE_LOG_FILE")]
        public static void Log(string msg, params object[] arg)
        {
            SystemLog.Log(msg, arg);
        }

        public static void Close()
        {
            SystemLog.End();
            LogSystem.Instance.End();
        }
    }
}