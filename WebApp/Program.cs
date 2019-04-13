using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Tripous.AspNetCore.Logging;

namespace WebApp
{
    public class Program
    {


        static public void Main(string[] args)
        {
            CreateWebHostBuilder(args)
                .Build()
                .Run();
        }
        static public IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)

            // ConfigureAppConfiguration is used to specify additional IConfiguration for the app
            // SEE: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-2.2#set-up-a-host
            .ConfigureAppConfiguration((context, config) =>
            {
                // config.AddXmlFile("appsettings.xml", optional: true, reloadOnChange: true);
                // config.AddIniFile("config.ini", optional: true, reloadOnChange: true);
            })
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-2.2
            // https://stackoverflow.com/questions/46621788/how-to-use-https-ssl-with-kestrel-in-asp-net-core-2-x            
            .ConfigureKestrel((context, options) =>
            {
                options.AddServerHeader = false;
                options.AllowSynchronousIO = false;         // https://www.strathweb.com/2019/02/be-careful-when-manually-handling-json-requests-in-asp-net-core/

            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                //logging.AddConsole();
                //logging.AddDebug();
                logging.AddFileLogger();
                //logging.AddFileLogger(options => {  });
            })
            .UseStartup<Startup>()
            ;
        }


    }
}
