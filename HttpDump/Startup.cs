using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HttpDump.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HttpDump
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSignalR();
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseSignalR(routes =>
            {
                routes.MapHub<DumpHub>("/dumphub");
            });

            app.Use(async (context, next) =>
            {
                string bodyString;
                using (var sw = new StreamReader(context.Request.Body))
                {
                    bodyString = sw.ReadToEnd();
                }

                var hubContext = context.RequestServices.GetService<IHubContext<DumpHub>>();
                await hubContext.Clients.All.SendAsync("ReceiveMessage",
                    new
                    {
                        Url = context.Request.GetDisplayUrl(),
                        Method = context.Request.Method,
                        Body = bodyString
                    });

                await next.Invoke();

                if (context.Response.StatusCode != (int)HttpStatusCode.OK)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.OK;

                    //context.Response.Body = new MemoryStream();
                    //context.Response.Body.Seek(0, SeekOrigin.Begin);
                    //await context.Response.WriteAsync($"FAKE HTTP SUCCESS FROM HTTP DUMP");

                }
            });

            app.UseMvc();
        }
    }
}
