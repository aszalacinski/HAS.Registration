using HAS.Registration.Configuration;
using HAS.Registration.Feature.GatedRegistration;
using HAS.Registration.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDb;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using IdentityUser = Microsoft.AspNetCore.Identity.MongoDb.IdentityUser;

namespace HAS.Registration
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(env.ContentRootPath)
                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                   .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var dbconn_myp = Configuration.GetSection("DBCONNECTIONSTRING_MONGODB_MPY").Value;

            services.AddSingleton(new CloudSettings
            {
                DBConnectionString_MongoDB = Configuration.GetSection("DBCONNECTIONSTRING_MONGODB_IDENTITY").Value,
                DBConnectionString_MongoDB_DatabaseName = Configuration["CloudSettings:DBConnectionString_MongoDB_DatabaseName"]
            });

            services.AddSingleton(new AuthMessageSenderOptions
            {
                SendGridKey = Configuration.GetSection("SENDGRID_KEY").Value,
                SendGridUser = Configuration.GetSection("SENDGRID_USER").Value
            });

            services.AddIdentity<IdentityUser>(config =>
            {
                config.Lockout.MaxFailedAccessAttempts = 5;
                config.Lockout.AllowedForNewUsers = true;
                config.Lockout.DefaultLockoutTimeSpan = new TimeSpan(0, 5, 0);

                config.SignIn.RequireConfirmedEmail = true;
            })
                .AddDefaultTokenProviders();


            services.AddSingleton<IUserStore<IdentityUser>>(provider =>
            {
                var options = provider.GetService<CloudSettings>();
                var client = new MongoClient(options.DBConnectionString_MongoDB);
                var database = client.GetDatabase(options.DBConnectionString_MongoDB_DatabaseName);

                return UserStore<IdentityUser>.CreateAsync(database).GetAwaiter().GetResult();
            });

            services.AddTransient<IEmailSender, SendGridEmailSender>();

            services.AddGatedRegstration();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
