using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;

using System.Collections.Concurrent;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Tripous.AspNetCore.Logging
{


    /// <summary>
    /// A base logger provider class. 
    /// <para>A logger provider essentialy represents the medium where log information is saved.</para>
    /// <para>This class may serve as base class in writing a file or database logger provider.</para>
    /// </summary>
    public abstract class LoggerProvider : IDisposable, ILoggerProvider, ISupportExternalScope
    {
        ConcurrentDictionary<string, Logger> loggers = new ConcurrentDictionary<string, Logger>();
        IExternalScopeProvider fScopeProvider;
        protected IDisposable SettingsChangeToken;

        /* interface implementations */
        /// <summary>
        /// Called by the logging framework in order to set external scope information source for the logger provider. 
        /// <para>ISupportExternalScope implementation</para>
        /// </summary>
        void ISupportExternalScope.SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            fScopeProvider = scopeProvider;
        }
        /// <summary>
        /// Returns an ILogger instance to serve a specified category.
        /// <para>The category is usually the fully qualified class name of a class asking for a logger, e.g. MyNamespace.MyClass </para>
        /// </summary>
        ILogger ILoggerProvider.CreateLogger(string Category)
        {
            return loggers.GetOrAdd(Category,
            (category) => {
                return new Logger(this, category);
            });
        }
        /// <summary>
        /// Disposes this instance
        /// </summary>
        void IDisposable.Dispose()
        {
            if (!this.IsDisposed)
            {
                try
                {
                    Dispose(true);
                }
                catch
                {
                }

                this.IsDisposed = true;
                GC.SuppressFinalize(this);  // instructs GC not bother to call the destructor                
            }
        }


        /* protected */
        /// <summary>
        /// Disposes the options change toker. IDisposable pattern implementation.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (SettingsChangeToken != null)
            {
                SettingsChangeToken.Dispose();
                SettingsChangeToken = null;
            }
        }

        /* construction */
        /// <summary>
        /// Constructor
        /// </summary>
        public LoggerProvider()
        {
        }
        /// <summary>
        /// Destructor.
        /// </summary>
        ~LoggerProvider()
        {
            if (!this.IsDisposed)
            {
                Dispose(false);
            }
        }

        /* public */
        /// <summary>
        /// Returns true if a specified log level is enabled.
        /// <para>Called by logger instances created by this provider.</para>
        /// </summary>
        public abstract bool IsEnabled(LogLevel logLevel);
        /// <summary>
        /// The loggers do not actually log the information in any medium.
        /// Instead the call their provider WriteLog() method, passing the gathered log information.
        /// </summary>
        public abstract void WriteLog(LogEntry Info);
        
        /* properties */
        /// <summary>
        /// Returns the scope provider. 
        /// <para>Called by logger instances created by this provider.</para>
        /// </summary>
        internal IExternalScopeProvider ScopeProvider
        {
            get
            {
                if (fScopeProvider == null)
                    fScopeProvider = new LoggerExternalScopeProvider();
                return fScopeProvider;
            }
        }
        /// <summary>
        /// Returns true when this instance is disposed.
        /// </summary> 
        public bool IsDisposed { get; protected set; }
    }

 
}
