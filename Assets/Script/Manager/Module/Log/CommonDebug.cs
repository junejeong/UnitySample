using UnityEngine;
using System;
using IFDEF = System.Diagnostics.ConditionalAttribute;
using UnityEngine.Internal;
using System.Text;

namespace Common
{
	public class CommonDebug
    {
        static string _tag
        {
            get
            {
                return "[" + DateTime.Now + "]";
            }
        }
   
        public static void Close()
        {
            CommonLog.Close();
        }

        [IFDEF("ENABLE_LOG")]
        public static void Log(Type type, object text, string color = "black")
        {
            Debug.Log(string.Format(_tag + "<color={0}><b>{1}</b>  {2}</color>", color, type.Name, text));

#if ENABLE_LOG_FILE
            CommonLog.Log(_tag + text);
#endif
        }

        [IFDEF("ENABLE_LOG")]
        public static void Logf(string format, params object[] args)
        {
            Debug.Log(string.Format(_tag + format, args));

#if ENABLE_LOG_FILE
            CommonLog.Log(_tag + format, args);
#endif
        }

        [IFDEF("ENABLE_LOG")]
        public static void Log(string text)
        {
            Debug.Log(_tag + text);

#if ENABLE_LOG_FILE
            CommonLog.Log(_tag + text);
#endif
        }

        [IFDEF("ENABLE_LOG")]
        public static void Log(string text, UnityEngine.Object context)
        {
            Debug.Log(_tag + text, context);

#if ENABLE_LOG_FILE
            CommonLog.Log(_tag + text + context.name);
#endif
        }

        [IFDEF("ENABLE_LOG"), IFDEF("ENABLE_LOG_ERROR")]
        public static void LogError(string text)
        {
            Debug.LogError(_tag + text);

#if ENABLE_LOG_FILE
            CommonLog.Log(_tag + text);
#endif
        }

        [IFDEF("ENABLE_LOG"), IFDEF("ENABLE_LOG_ERROR")]
        public static void LogError(string text, UnityEngine.Object context)
        {
            Debug.LogError(_tag + text, context);

#if ENABLE_LOG_FILE
            CommonLog.Log(_tag + text + context.name);
#endif
        }

        [IFDEF("ENABLE_LOG"), IFDEF("ENABLE_LOG_WARNING")]
        public static void LogWarning(string text)
        {
            Debug.LogWarning(_tag + text);

#if ENABLE_LOG_FILE
            CommonLog.Log(_tag + text);
#endif
        }

        [IFDEF("ENABLE_LOG"), IFDEF("ENABLE_LOG_WARNING")]
        public static void LogWarning(string text, UnityEngine.Object context)
        {
            Debug.LogWarning(_tag + text, context);

#if ENABLE_LOG_FILE
            CommonLog.Log(_tag + text + context.name);
#endif
        }

        [IFDEF("ENABLE_LOG"), IFDEF("ENABLE_LOG_EXCEPTION")]
        public static void LogException(System.Exception exception)
        {
            Debug.LogException(/*_tag + */exception);

#if ENABLE_LOG_FILE
            CommonLog.Log(_tag + "Message : " + exception.Message + " //// Stacktrace : " + exception.StackTrace);
#endif
        }

        [IFDEF("ENABLE_LOG"), IFDEF("ENABLE_LOG_EXCEPTION")]
        public static void LogException(System.Exception exception, UnityEngine.Object context)
        {
            Debug.LogException(/*_tag + */exception, context);

#if ENABLE_LOG_FILE
            CommonLog.Log(_tag + context.name + "  Message : " + exception.Message + " //// Stacktrace : " + exception.StackTrace);
#endif
        }

        [IFDEF("ENABLE_LOG")]
        public static void Logf(object format, params object[] args)
        {
            Debug.Log(string.Format(_tag + format.ToString(), args));

#if ENABLE_LOG_FILE
            CommonLog.Log(_tag + format.ToString(), args);
#endif
        }

        [IFDEF("ENABLE_LOG")]
        public static void Log(object text)
        {
            Debug.Log(_tag + text.ToString());

#if ENABLE_LOG_FILE
            CommonLog.Log(_tag + text);
#endif
        }

        [IFDEF("ENABLE_LOG")]
        public static void Log(object text, UnityEngine.Object context)
        {
            Debug.Log(_tag + text.ToString(), context);

#if ENABLE_LOG_FILE
            CommonLog.Log(_tag + text.ToString() + context.name);
#endif
        }

        static StringBuilder _sb = new StringBuilder(200);

        [IFDEF("ENABLE_LOG")]
        public static void Logs(params object[] text)
        {
            if (text.Length == 1)
            {
                Debug.Log(_tag + text[0]);

#if ENABLE_LOG_FILE
                CommonLog.Log(_tag + text[0]);
#endif
            }
            else
            {
                _sb.Clear();
                for (int i = 0, len = text.Length; i < len;)
                {
                    _sb.Append(text[i]);
                    if (++i != len)
                    {
                        _sb.Append(' ');
                    }
                }
                Debug.Log(_tag + _sb.ToString());

#if ENABLE_LOG_FILE
                CommonLog.Log(_tag + _sb.ToString());
#endif
            }
        }

        [IFDEF("ENABLE_LOG"), IFDEF("ENABLE_LOG_ERROR")]
        public static void LogError(object text)
        {
            Debug.LogError(_tag + text.ToString());

#if ENABLE_LOG_FILE
            CommonLog.Log(_tag + text);
#endif
        }

        [IFDEF("ENABLE_LOG"), IFDEF("ENABLE_LOG_ERROR")]
        public static void LogError(object text, UnityEngine.Object context)
        {
            Debug.LogError(_tag + text.ToString(), context);

#if ENABLE_LOG_FILE
            CommonLog.Log(_tag + text.ToString() + context);
#endif
        }

        [IFDEF("ENABLE_LOG"), IFDEF("ENABLE_LOG_WARNING")]
        public static void LogWarning(object text)
        {
            Debug.LogWarning(_tag + text.ToString());

#if ENABLE_LOG_FILE
            CommonLog.Log(_tag + text);
#endif
        }

        [IFDEF("ENABLE_LOG"), IFDEF("ENABLE_LOG_WARNING")]
        public static void LogWarning(object text, UnityEngine.Object context)
        {
            Debug.LogWarning(_tag + text.ToString(), context);

#if ENABLE_LOG_FILE
            CommonLog.Log(_tag + text.ToString() + context);
#endif
        }

        public static implicit operator bool(CommonDebug exists)
        {
            return exists != null;
        }
    }
}