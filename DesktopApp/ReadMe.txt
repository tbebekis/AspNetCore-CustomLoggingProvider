Here is an example of a logging system. It contains the following parts:

A LogLevel enumration type with just three levels. It is easy to add more levels if needed.

A LogEntry class that serves as a container for a unit of log information.

An ILogListener interface with just a single method, ProcessLog(LogEntry Info). That method is called whenever a new LogEntry comes to existense.

A static Logger class which provides log methods, Info(), Warn() and Error(). 
Logger is also an ILogListener register. It calls all registered listeners, asynchronously (that is using a thread), 
whenever any of its log methods is called and a new LogEntry is generated.

There is also a fully functional LogFileListener class which saves log information to a text file, and also provides settings for log file retention policy.

The above logging system does not support Scopes, as Asp.Net Core does. It's easy to implement it though. 
All you need is another one Logger method, getting as parameter a string or whatever representing the Scope,  
and returning an interface with log methods, more or less, similar to the log methods of the static Logger class.