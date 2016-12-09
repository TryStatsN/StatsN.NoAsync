﻿using StatsN.Exceptions;
using System;
using System.Diagnostics;

namespace StatsN
{
	public class StatsdOptions
    {
        public Action<System.Exception> OnExceptionGenerated;
        public Action<StatsdLogMessage> OnLogEventGenerated;
        public TraceSource TraceSource { get; private set; } = new TraceSource(Constants.StatsN, SourceLevels.Warning);
        public bool BufferMetrics { get; set; } = false;
        public int BufferSize { get; set; } = 512;
        public string HostOrIp { get; set; } = Constants.Localhost;
        public int Port { get; set; } = 8125;
        string prefix = string.Empty;
        public string Prefix
        {
            get
            {
                return prefix;
            }

            set
            {
                prefix = value.TrimEnd(Constants.dot);
            }
        }

        internal void LogException(Exception exception)
        {
            if(OnExceptionGenerated == null)
            {
                LogEvent(new StatsdLogMessage(exception.Message, EventType.Error));
                return;
            }
            OnExceptionGenerated.Invoke(exception);

        }
        internal void LogEvent(string message, EventType weight) => this.LogEvent(new StatsdLogMessage(message, weight));
        
        internal void LogEvent(StatsdLogMessage logMessage)
        {
            if (OnLogEventGenerated == null)
            {
                switch (logMessage.Weight)
                {
                    case EventType.Info:
                        TraceSource.TraceEvent(TraceEventType.Information, 1, logMessage.Message);
                        break;
                    case EventType.Warning:
                        TraceSource.TraceEvent(TraceEventType.Warning, 2, logMessage.Message);
                        break;
                    case EventType.Error:
                        TraceSource.TraceEvent(TraceEventType.Error, 1, logMessage.Message);
                        break;
                }
            }
            else
            {
                OnLogEventGenerated?.Invoke(logMessage);
            }
        }
    }
}
