﻿using AutoMapper;
using HAS.Registration.ApplicationServices.SendGrid;
using HAS.Registration.Data;
using HAS.Registration.Feature.GatedRegistration;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDb;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System;
using IdentityUser = Microsoft.AspNetCore.Identity.MongoDb.IdentityUser;

namespace HAS.Registration
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;

            var testConfig = Configuration["MPY:Other"];

            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));
            services.AddMediatR(typeof(Startup));
            services.AddScoped<GatedRegistrationContext>();

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
                var client = new MongoClient(Configuration["MongoDB:Identity:ConnectionString"]);
                var database = client.GetDatabase(Configuration["MongoDB:Identity:Database:Name"]);

                return UserStore<IdentityUser>.CreateAsync(database).GetAwaiter().GetResult();
            });

            services.AddTransient<IEmailSender, SendGridEmailSender>();
            
            services.AddMvc(options => { options.EnableEndpointRouting = false; }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
