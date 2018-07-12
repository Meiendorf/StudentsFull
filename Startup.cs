using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Students.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Students.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using Hangfire;
using MassTransit;
using Students.Services;
using MassTransit.Util;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Students
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            try {
                var elasticUri = Configuration["ElasticConfiguration:Uri"];

                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
                    {
                        AutoRegisterTemplate = true,
                    })
                .CreateLogger();
            }
            catch
            {
                Console.WriteLine("Can't initialize connection with ElasticSearch!");
            }
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var bus = DefaultBus.CreateBus(Configuration.GetSection("RabbitMq"));
            services.AddSingleton<IBusControl>(bus);
            services.AddSingleton<IBus>(bus);
            services.AddSingleton<IPublishEndpoint>(bus);

            /*services.AddHangfire(x => x.UseSqlServerStorage(
                 Configuration.GetConnectionString("DefaultConnection")));*/

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            
            var connectionStr = Environment.GetEnvironmentVariable("CONSTR") ??
                                Configuration.GetConnectionString("DefaultConnection");

            var inMemory = Environment.GetEnvironmentVariable("INMEMORY");
            if (inMemory == null)
            {
                Console.WriteLine("Not in memory " + inMemory);
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionStr));
            }
            else
            {
                Console.WriteLine("InMemory " + inMemory);
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase());
            }

            services.AddIdentity<User, IdentityRole> (options =>
                {
                    options.Password.RequiredUniqueChars = 0;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireLowercase = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); 
            services
                .AddAuthentication(options =>
                {       
                  /*  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;*/
                })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Configuration["JwtIssuer"],
                        ValidAudience = Configuration["JwtIssuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtKey"])),
                        ClockSkew = TimeSpan.Zero // remove delay of token when expire
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("admin", x =>
                {
                    x.RequireRole("admin");
                });
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info() { Title = "Student API", Version = "v1" });
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login";
                options.LogoutPath = "/logoff";
                options.AccessDeniedPath = "/error";

                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = ctx =>
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api") &&
                            ctx.Response.StatusCode == (int)HttpStatusCode.OK)
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        }
                        else
                        {
                            ctx.Response.Redirect(ctx.RedirectUri);
                        }
                        return Task.FromResult(0);
                    }
                };

            });
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IBusControl bus, IApplicationLifetime lifetime, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Student API V1");
            });

            var busHandle = TaskUtil.Await(() => bus.StartAsync());
            lifetime.ApplicationStopping.Register(() => busHandle.Stop());

            try
            {
                loggerFactory.AddSerilog();
            }
            catch
            {

            }

            //app.UseHangfireServer();
            //UseHangfireDashboard();

            //app.UseMiddleware<ScheduleMiddleware>();

            app.UseCookiePolicy();
            app.UseAuthentication();

           
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
