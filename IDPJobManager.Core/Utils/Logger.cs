﻿using System;
using System.Reflection;

namespace IDPJobManager.Core.Utils
{
    public sealed class Logger
    {
        #region 
        public static readonly Logger Instance = new Logger();
        private static readonly log4net.ILog _Logger4net = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
 
        private Logger()
        {
        }

        #endregion

        #region 
        public bool IsDebugEnabled
        {
            get { return _Logger4net.IsDebugEnabled; }
        }
        public bool IsInfoEnabled
        {
            get { return _Logger4net.IsInfoEnabled; }
        }
        public bool IsWarnEnabled
        {
            get { return _Logger4net.IsWarnEnabled; }
        }
        public bool IsErrorEnabled
        {
            get { return _Logger4net.IsErrorEnabled; }
        }
        public bool IsFatalEnabled
        {
            get { return _Logger4net.IsFatalEnabled; }
        }
        #endregion

        #region

        #region [ Debug ]  
        public void Debug(string message)
        {
            if (this.IsDebugEnabled)
            {
                this.Log(LogLevel.Debug, message);
            }
        }

        public void Debug(string message, Exception exception)
        {
            if (this.IsDebugEnabled)
            {
                this.Log(LogLevel.Debug, message, exception);
            }
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (this.IsDebugEnabled)
            {
                this.Log(LogLevel.Debug, format, args);
            }
        }

        public void DebugFormat(string format, Exception exception, params object[] args)
        {
            if (this.IsDebugEnabled)
            {
                this.Log(LogLevel.Debug, string.Format(format, args), exception);
            }
        }
        #endregion

        #region [ Info ]  
        public void Info(string message)
        {
            if (this.IsInfoEnabled)
            {
                this.Log(LogLevel.Info, message);
            }
        }

        public void Info(string message, Exception exception)
        {
            if (this.IsInfoEnabled)
            {
                this.Log(LogLevel.Info, message, exception);
            }
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (this.IsInfoEnabled)
            {
                this.Log(LogLevel.Info, format, args);
            }
        }

        public void InfoFormat(string format, Exception exception, params object[] args)
        {
            if (this.IsInfoEnabled)
            {
                this.Log(LogLevel.Info, string.Format(format, args), exception);
            }
        }
        #endregion

        #region  [ Warn ]  

        public void Warn(string message)
        {
            if (this.IsWarnEnabled)
            {
                this.Log(LogLevel.Warn, message);
            }
        }

        public void Warn(string message, Exception exception)
        {
            if (this.IsWarnEnabled)
            {
                this.Log(LogLevel.Warn, message, exception);
            }
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (this.IsWarnEnabled)
            {
                this.Log(LogLevel.Warn, format, args);
            }
        }

        public void WarnFormat(string format, Exception exception, params object[] args)
        {
            if (this.IsWarnEnabled)
            {
                this.Log(LogLevel.Warn, string.Format(format, args), exception);
            }
        }
        #endregion

        #region  [ Error ]  

        public void Error(string message)
        {
            if (this.IsErrorEnabled)
            {
                this.Log(LogLevel.Error, message);
            }
        }

        public void Error(string message, Exception exception)
        {
            if (this.IsErrorEnabled)
            {
                this.Log(LogLevel.Error, message, exception);
            }
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (this.IsErrorEnabled)
            {
                this.Log(LogLevel.Error, format, args);
            }
        }

        public void ErrorFormat(string format, Exception exception, params object[] args)
        {
            if (this.IsErrorEnabled)
            {
                this.Log(LogLevel.Error, string.Format(format, args), exception);
            }
        }
        #endregion

        #region  [ Fatal ]  

        public void Fatal(string message)
        {
            if (this.IsFatalEnabled)
            {
                this.Log(LogLevel.Fatal, message);
            }
        }

        public void Fatal(string message, Exception exception)
        {
            if (this.IsFatalEnabled)
            {
                this.Log(LogLevel.Fatal, message, exception);
            }
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (this.IsFatalEnabled)
            {
                this.Log(LogLevel.Fatal, format, args);
            }
        }

        public void FatalFormat(string format, Exception exception, params object[] args)
        {
            if (this.IsFatalEnabled)
            {
                this.Log(LogLevel.Fatal, string.Format(format, args), exception);
            }
        }
        #endregion
        #endregion

        #region
        /// <summary>  
        /// 输出普通日志  
        /// </summary>  
        /// <param name="level"></param>  
        /// <param name="format"></param>  
        /// <param name="args"></param>  
        private void Log(LogLevel level, string format, params object[] args)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    _Logger4net.DebugFormat(format, args);
                    break;
                case LogLevel.Info:
                    _Logger4net.InfoFormat(format, args);
                    break;
                case LogLevel.Warn:
                    _Logger4net.WarnFormat(format, args);
                    break;
                case LogLevel.Error:
                    _Logger4net.ErrorFormat(format, args);
                    break;
                case LogLevel.Fatal:
                    _Logger4net.FatalFormat(format, args);
                    break;
            }
        }

        /// <summary>  
        /// Prints the exceptional message   
        /// </summary>  
        /// <param name="level"></param>  
        /// <param name="message"></param>  
        /// <param name="exception"></param>  
        private void Log(LogLevel level, string message, Exception exception)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    _Logger4net.Debug(message, exception);
                    break;
                case LogLevel.Info:
                    _Logger4net.Info(message, exception);
                    break;
                case LogLevel.Warn:
                    _Logger4net.Warn(message, exception);
                    break;
                case LogLevel.Error:
                    _Logger4net.Error(message, exception);
                    break;
                case LogLevel.Fatal:
                    _Logger4net.Fatal(message, exception);
                    break;
            }
        }
        #endregion
    }//end of class  


    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }
}
