using System;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using DatingApp.API.Models;

namespace DatingApp.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            using(var scope = host.Services.CreateScope())
            {
                var Services = scope.ServiceProvider;

                try 
                {
                    var context = Services.GetRequiredService<DataContext>();                
                    var userManager = Services.GetRequiredService<UserManager<User>>();
                    var roleManager = Services.GetRequiredService<RoleManager<Role>>();

                    context.Database.Migrate();
                    Seed.SeedUsers(userManager, roleManager);
                } 
                catch(Exception ex) 
                {
                    var logger = Services.GetRequiredService<ILogger<Program>>();

                    logger.LogError(ex, "An error ocurred during migration");
                }
            }
            
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
