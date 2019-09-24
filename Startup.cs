using HAS.Registration.ApplicationServices.SendGrid;
using HAS.Registration.Configuration;
using HAS.Registration.Feature.GatedRegistration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDb;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                   .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                   .AddEnvironmentVariables();

            if(env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new CloudSettings
            {
                DBConnectionString_MongoDB = Configuration.GetSection("MongoDB:Identity:ConnectionString").Value,
                DBConnectionString_MongoDB_DatabaseName = Configuration.GetSection("MongoDB:Identity:DatabaseName").Value,
                Azure_Queue_ConnectionString = Configuration.GetSection("Azure:Storage:ConnectionString").Value,
                Azure_Queue_Name_ReservationCompletedEvent = Configuration.GetSection("Azure:Storage:Queue:Name:RegistrationEvent").Value
            });

            services.AddSingleton(new AuthMessageSenderOptions
            {
                SendGridKey = Configuration.GetSection("SendGrid:Key").Value,
                SendGridUser = Configuration.GetSection("SendGrid:User").Value
            });

            services.AddIdentity<IdentityUser>(config =>
            {
                config.Lockout.MaxFailedAccessAttempts = 5;
                config.Lockout.AllowedForNewUsers = true;
                config.Lockout.DefaultLockoutTimeSpan = new TimeSpan(0, 5, 0);

                config.SignIn.RequireConfirmedEmail = true;

                config.User.RequireUniqueEmail = true;

                config.Password.RequiredLength = 6;
                config.Password.RequireDigit = true;
                config.Password.RequireUppercase = true;
                config.Password.RequireNonAlphanumeric = true;
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
