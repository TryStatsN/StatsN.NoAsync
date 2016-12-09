﻿using StatsN.Exceptions;
using System;
using System.Text;

namespace StatsN
{
	public partial class Statsd : IStatsdSync
    {
        readonly StatsdOptions options;

        BaseCommunicationProvider _provider;

        /// <summary>
        /// Create a new Statsd client
        /// </summary>
        /// <typeparam name="T">Statsd client to use</typeparam>
        /// <param name="configure">configuration block</param>
        /// <returns></returns>
        public static Statsd New<T>(Action<StatsdOptions> configure) where T: BaseCommunicationProvider, new()
        {
            var options = new StatsdOptions();
            configure?.Invoke(options);
            return new Statsd(options, new T());
        }
        /// <summary>
        /// Create a new Statsd client
        /// </summary>
        /// <typeparam name="T">Statsd client to use</typeparam>
        /// <param name="options">client options</param>
        /// <returns></returns>
#pragma warning disable CC0022 // should dispose
        public static Statsd New<T>(StatsdOptions options) where T: BaseCommunicationProvider, new() => new Statsd(options, new T());
#pragma warning restore CC0022
        /// <summary>
        /// Create a new Statsd client. Defaults to Udp
        /// </summary>
        /// <param name="options">client options</param>
        /// <returns></returns>
        public static Statsd New(StatsdOptions options) => Statsd.New<Udp>(options);
        /// <summary>
        /// Create a new Statsd client. Defaults to Udp
        /// </summary>
        /// <param name="configure">configuration block</param>
        /// <returns></returns>
        public static Statsd New(Action<StatsdOptions> configure) => Statsd.New<Udp>(configure);
        /// <summary>
        /// Create a statsd client
        /// </summary>
        /// <param name="options">client options </param>
        /// <param name="provider">provider, defaults to udp if none is passed</param>
        public Statsd(StatsdOptions options, BaseCommunicationProvider provider = null)
        {
            if(options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if(provider == null)
            {
                provider = new Udp();
            }
            if (string.IsNullOrEmpty(options.HostOrIp))
            {
                options.LogEvent("No host or ip provided, failing back to null output", EventType.Error);
                _provider = new NullChannel();
            }
            if(options.Port < 0)
            {
                options.LogEvent("port provided, failing back to null output", EventType.Error);
                _provider = new NullChannel();
            }
            this.options = options;
            _provider = provider.Construct(options);

        }
        internal void LogMetric(string metricName, long value, string metricType, string postfix = "") => LogMetric(metricName, value.ToString(), metricType, postfix);
        
        internal void LogMetric(string metricName, string value, string metricType, string postfix = "")
        {
            if (!_provider.IsConnected && _provider.Connect())
            {
                options.LogEvent("unable to connect message transport", EventType.Error);
                return;
            }
            var calculateMetric = BuildMetric(metricName, value, metricType, options.Prefix, postfix);
            options.LogEvent(calculateMetric, EventType.Info);
            if (string.IsNullOrWhiteSpace(calculateMetric))
            {
                options.LogEvent($"Unable to generate metric for {metricType} value {value}", EventType.Error);
            }
            try
            {
                _provider.SendMetric(calculateMetric);
            }
            catch(Exception e)
            {
                options?.LogException(e);
            }
            
        }
        internal virtual string BuildMetric(string metricName, string value, string metricType, string prefix = "", string postfix = "")
        {
            if (string.IsNullOrWhiteSpace(metricName))
            {
                this.options.LogEvent("metric not passed to compile metric", EventType.Error);
                return string.Empty;
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                this.options.LogEvent("value not passed to compile metric", EventType.Error);
                return string.Empty;
            }
            if (string.IsNullOrWhiteSpace(metricType))
            {
                this.options.LogEvent("metric type not passed to compile metric. This really shouldnt happen", EventType.Error);
                return string.Empty;
            }
            StringBuilder builder;
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                builder = new StringBuilder(prefix, prefix.Length + metricName.Length + 4 + metricType.Length + value.Length + postfix.Length);
                builder.Append(Constants.dot);
                builder.Append(metricName);
            }
            else
            {
                builder = new StringBuilder(metricName, metricName.Length + 3 + metricType.Length + value.Length + postfix.Length);
            }
            builder.Append(Constants.colon);
            builder.Append(value);
            builder.Append(Constants.pipe);
            builder.Append(metricType);
            if (!string.IsNullOrWhiteSpace(postfix))
            {
                builder.Append(Constants.pipe);
                builder.Append(postfix);
            }
            return builder.ToString();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _provider?.Dispose();
                    _provider = null;
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
