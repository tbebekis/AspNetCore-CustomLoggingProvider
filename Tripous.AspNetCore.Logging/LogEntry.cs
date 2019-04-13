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
    /// The information of a log entry.
    /// <para>The logger creates an instance of this class when its Log() method is called, fills the properties and then passes the info to the provider calling WriteLog(). </para>
    /// </summary>
    public class LogEntry
    {
        


        /* construction */
        /// <summary>
        /// Constructor
        /// </summary>
        public LogEntry()
        {
            TimeStampUtc = DateTime.UtcNow;
            UserName = Environment.UserName;
        }

        /* properties */
        /// <summary>
        /// Returns the host name of the local computer
        /// </summary>
        static public readonly string StaticHostName = System.Net.Dns.GetHostName();

        /// <summary>
        /// User name of the person who is currently logged on to operating system.
        /// </summary>
        public string UserName { get; private set; }
        /// <summary>
        /// Host name of local computer
        /// </summary>
        public string HostName { get { return StaticHostName; } }
        /// <summary>
        /// Date and time, in UTC, of the creation time of this instance
        /// </summary>
        public DateTime TimeStampUtc { get; private set; }
        /// <summary>
        /// Category this instance belongs to.
        /// <para>The category is usually the fully qualified class name of a class asking for a logger, e.g. MyNamespace.MyClass </para>
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// The log level of this information.
        /// </summary>
        public LogLevel Level { get; set; }
        /// <summary>
        /// The message of this information
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// The exception this information represents, if any, else null.
        /// </summary>
        public Exception Exception { get; set; }
        /// <summary>
        /// The EventId of this information. 
        /// <para>An EventId with Id set to zero, usually means no EventId.</para>
        /// </summary>
        public EventId EventId { get; set; }
        /// <summary>
        /// The state object. Contains information regarding the text message.
        /// <para>It looks like its type is always Microsoft.Extensions.Logging.Internal.FormattedLogValues </para>
        /// </summary>
        public object State { get; set; }
        /// <summary>
        /// Used when State is just a string type. So far null.
        /// </summary>
        public string StateText { get; set; }
        /// <summary>
        /// A dictionary with State properties.
        /// <para>When the log message is a message template with format values, e.g. <code>Logger.LogInformation("Customer {CustomerId} order {OrderId} is completed", CustomerId, OrderId)</code>  </para>
        /// this dictionary contains entries gathered from the message in order to ease any Structured Logging providers.
        /// </summary>
        public Dictionary<string, object> StateProperties { get; set; }
        /// <summary>
        /// The scopes currently in use, if any. The last scope is
        /// </summary>
        public List<LogScopeInfo> Scopes { get; set; }
    }


}
