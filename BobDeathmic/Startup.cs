using BobDeathmic.Data;
using BobDeathmic.Eventbus;
using BobDeathmic.Models;
using BobDeathmic.Services;
using BobDeathmic.Services.Helper;
using BobDeathmic.Services.Streams.Checker.Twitch;
using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using JavaScriptEngineSwitcher.Jurassic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using React.AspNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.Cron.Setup;
using BobDeathmic.Cron;
using BobDeathmic.Data.DBModels.User;
using BobDeathmic.Services.Discords;
using BobDeathmic.Services.Streams.Relay.Twitch;
using BobDeathmic.Services.Commands;

namespace BobDeathmic
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"), mysqlOptions =>
                {
                    mysqlOptions.ServerVersion(new Version(10, 3, 8), ServerType.MariaDb);
                })
                );
            services.AddIdentity<ChatUserModel, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(2);
                options.Lockout.MaxFailedAccessAttempts = 10;
                //options.User.RequireUniqueEmail = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 2;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.LoginPath = "/Main/Login";
            });

            // Add application services.
            services.AddMemoryCache();
            services.AddSingleton<IEventBus, EventBusLocal>();
            
            //services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, TwitchChecker>();
            //services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, DLiveChecker>();
            //services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, Services.MixerChecker>();

            services.AddSingleton<ICommandService, CommandService>();
            //services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, DiscordService>();
            //services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, TwitchRelayCenter>();
            //services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, TwitchAPICalls>();
            //services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, StrawPollService>();
            //services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, SchedulerHostService>();
            

            //services.AddSingleton<IScheduledTask, EventCalendarTask>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddReact();
            services.AddJsEngineSwitcher(options => options.DefaultEngineName = ChakraCoreJsEngine.EngineName).AddChakraCore();
            //services.AddJsEngineSwitcher().AddJurassic();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Main/Error");
            }
            app.UseReact(config =>
            {
                config.AllowJavaScriptPrecompilation = true;
                config.SetLoadBabel(true);
                config.SetLoadReact(true);
                config.UseServerSideRendering = false;


                // If you want to use server-side rendering of React components,
                // add all the necessary JavaScript files here. This includes
                // your components as well as all of their dependencies.
                // See http://reactjs.net/ for more information. Example:
                //config
                //    .AddScript("~/Scripts/First.jsx")
                //    .AddScript("~/Scripts/Second.jsx");

                // If you use an external build too (for example, Babel, Webpack,
                // Browserify or Gulp), you can improve performance by disabling
                // ReactJS.NET's version of Babel and loading the pre-transpiled
                // scripts. Example:
                //config
                //    .SetLoadBabel(false)
                //    .AddScriptWithoutTransform("~/Scripts/bundle.server.js");
            });
            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Main}/{action=Index}/{id?}");
            });
        }

    }
}
