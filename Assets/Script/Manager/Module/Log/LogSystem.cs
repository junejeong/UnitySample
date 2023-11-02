using UnityEngine;
using System.IO;
using System.Text;
using System;
using IFDEF = System.Diagnostics.ConditionalAttribute;

namespace Common
{
    public class LogSystem
    {
        private static LogSystem _instance = null;

        public bool bDebugLog = false;
        private bool bCreateFile = false;
        private StreamWriter _output = null;
   
        private StringBuilder _msg = new StringBuilder(200);
        
        public static LogSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LogSystem();
                }
                return _instance;
            }
        }

        public LogSystem()
        {
        }

        public LogSystem(string fileName, string fullPath = "")
        {
            Begin(fileName, fullPath);
        }

        public void Begin(string fileName = "Log.txt", string fullPath = "")
        {
            End();
            bCreateFile = CreateFile(fileName, fullPath);
        }

        public void End()
        {
            bCreateFile = false;

            if (_output != null)
            {
                _output.Close();
                _output.Dispose();
                _output = null;
            }
        }

        private bool CreateFile(string fileName, string fullPath)
        {
            try
            {
                if (fullPath.Length <= 0)
                {

                    fullPath = Directory.GetCurrentDirectory() + "/Log/";
                }

                DirectoryInfo di = new DirectoryInfo(fullPath);
                if (di.Exists == false) di.Create();

                _output = new StreamWriter(fullPath + fileName, false, Encoding.Default);
                bCreateFile = true;

                return true;
            }
            catch (Exception e)
            {
                Debug.LogFormat("[Error] CreateFile Fail / Message : {0} / StackTrace : {1}", e.Message, e.StackTrace);

                return false;
            }
        }

        #region Log

        #region LogOverroading

        [IFDEF("ENABLE_LOG_FILE")]
        public void Log(object msg)
        {
            if (!bCreateFile)
            {
                Begin();
            }
            if (_output == null) return;

            DateSetting();
            _msg.Append(msg);
            FileWrite();
        }

        [IFDEF("ENABLE_LOG_FILE")]
        public void Log(string msg)
        {
            if (!bCreateFile)
            {
                Begin();
            }
            if (_output == null) return;

            DateSetting();
            _msg.Append(msg);
            FileWrite();
        }

        [IFDEF("ENABLE_LOG_FILE")]
        public void Log(string msg, object arg)
        {            
            if (!bCreateFile)
            {
                Begin();
            }
            if (_output == null) return;

            DateSetting();
            _msg.AppendFormat(msg, arg);
            FileWrite();
        }
        #endregion

        [IFDEF("ENABLE_LOG_FILE")]
        public void Log(string msg, params object[] arg)
        {
            if (!bCreateFile)
            {
                Begin();
            }
            if (_output == null) return;

            DateSetting();
            _msg.AppendFormat(msg, arg);
            FileWrite();
        }
        #endregion

        void DateSetting()
        {
            DateTime currentTime = DateTime.Now;
    
            _msg.Append("[");
            _msg.Append(currentTime.Day);
            _msg.Append("/");
            _msg.Append(currentTime.Month);
            _msg.Append("/");
            _msg.Append(currentTime.Year);
            _msg.Append(" ");

            _msg.Append(currentTime.Hour);
            _msg.Append(":");
            _msg.Append(currentTime.Minute);
            _msg.Append(":");
            _msg.Append(currentTime.Second);
            _msg.Append(".");
            _msg.Append(currentTime.Millisecond);
            _msg.Append("] ");
        }

        void FileWrite()
        {
            _output.WriteLine(_msg.ToString());

            _output.Flush();
            _msg.Length = 0;
        }

        [IFDEF("ENABLE_LOG")]
        private void DebugLog(string msg, params object[] arg)
        {
            Debug.LogFormat(msg, arg);
        }
    }
}