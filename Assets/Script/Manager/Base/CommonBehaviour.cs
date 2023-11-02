using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace Common
{    public class CommonBehaviour : MonoBehaviour
    {
        public void SetEnabled(bool enabled)
        {
            this.enabled = enabled;
        }

        public bool IsEnabled()
        {
            return this.enabled;
        }

        public void StopAllCoroutine()
        {
            base.StopAllCoroutines();
        }

        #region LOG
        [System.Diagnostics.Conditional("ENABLE_LOG")]
        public void Logf(string format, params object[] args)
        {
            CommonDebug.Log(string.Format(format, args), gameObject);
        }

        [System.Diagnostics.Conditional("ENABLE_LOG")]
        public void Log(string text)
        {
            CommonDebug.Log(text, gameObject);
        }

        [System.Diagnostics.Conditional("ENABLE_LOG")]
        public void Log(string text, UnityEngine.Object context)
        {
            CommonDebug.Log(text, context);
        }

        public void LogError(string text)
        {
            CommonDebug.LogError(text, gameObject);
        }

        public void LogError(string text, UnityEngine.Object context)
        {
            CommonDebug.LogError(text, context);
        }

        public void LogWarning(string text)
        {
            CommonDebug.LogWarning(text, gameObject);
        }

        public void LogWarning(string text, UnityEngine.Object context)
        {
            CommonDebug.LogWarning(text, context);
        }

        public void LogException(System.Exception exception)
        {
            CommonDebug.LogException(exception, gameObject);
        }

        public void LogException(System.Exception exception, UnityEngine.Object context)
        {
            CommonDebug.LogException(exception, context);
        }


        [System.Diagnostics.Conditional("ENABLE_LOG")]
        public void Logf(object format, params object[] args)
        {
            CommonDebug.Logf(format, args);
        }

        [System.Diagnostics.Conditional("ENABLE_LOG")]
        public void Log(object text)
        {
            CommonDebug.Log(text, gameObject);
        }

        [System.Diagnostics.Conditional("ENABLE_LOG")]
        public void Log(object text, UnityEngine.Object context)
        {
            CommonDebug.Log(text, context);
        }

        public void LogError(object text)
        {
            CommonDebug.LogError(text, gameObject);
        }

        public void LogError(object text, UnityEngine.Object context)
        {
            CommonDebug.LogError(text, context);
        }

        public void LogWarning(object text)
        {
            CommonDebug.LogWarning(text, gameObject);
        }

        public void LogWarning(object text, UnityEngine.Object context)
        {
            CommonDebug.LogWarning(text, context);
        }

        [System.Diagnostics.Conditional("ENABLE_LOG")]
        public void Logs(params object[] text)
        {
            CommonDebug.Logs(text);
        }


        public void PrintStack()
        {
            var fullStack = new StackTrace();
            var stack = fullStack.GetFrame(1);
            Log(string.Format("{0} :{1}", stack.GetMethod().Name, stack.GetFileLineNumber()));
        }

        public void PrintStack(string txt)
        {
            var fullStack = new StackTrace();
            var stack = fullStack.GetFrame(1);
            Log(string.Format("{0} :{1}", stack.GetMethod().Name, stack.GetFileLineNumber()) + txt);
        }
        #endregion
    }
}