using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ContosoUniversity.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ContosoUniversity {

    public class Program {

        public static void Main(string[] args) {
            //CreateWebHostBuilder(args).Build().Run();  // initial template code

            var host = CreateWebHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope()) {

                // create an instance of service provider that our School Context can be attached to
                var services = scope.ServiceProvider;

                try {
                    // attempt to make an instance of our DB context
                    var context = services.GetRequiredService<SchoolContext>();

                    // populate (seed) the database according to our DB initializer class
                    DbInitializer.Initialize(context);
                } catch (Exception ex) {
                    // grab any relevant exceptions so that an error message can be generated and reviewed later
                    var logger = services.GetRequiredService<ILogger<Program>>();

                    logger.LogError(ex, "An error occurred during an attempt to seed the database...");
                }
            }

            // run the .NET core Web App
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
