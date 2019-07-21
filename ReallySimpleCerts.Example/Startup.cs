using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Certes;
using Certes.Acme;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReallySimpleCerts.Core;

namespace ReallySimpleCerts.Example
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

            services.AddReallySimpleCerts(opts =>
            {
                opts.DefaultAzureServicePrincipal = new ServicePrincipalCredentials
                {
                    TenantId = "[tenant id]",
                    ClientId = "[client id]",
                    Secret = "[client secret]"
                };
                opts.CertificateInfo = new CsrInfo
                {
                    CommonName = " your.domain.com",
                    CountryName = "GB",
                    Locality = "[locality]",
                    Organization = "[organization]",
                    State = "[state]"
                };
                opts.Email = "your@email.com";
                opts.LetsEncryptTermsOfServiceAgreed = true;

                // LetsEncrypt rate-limits production certificate orders, so always use staging for local testing.
                // Switch this to the production root URI when deploying to an environment that needs a real cert.
                opts.IssuerRootUri = WellKnownServers.LetsEncryptStagingV2;
            })
            .WithAzureKeyVaultPersistence(opts =>
            {
                opts.KeyVaultRootUrl = "https://[your key vault].vault.azure.net/";
            })
            .WithAzureWebAppHandler(opts =>
            {
                opts.ResourceGroupName = "[resource group]";
                opts.WebAppName = "[webapp]";
                opts.SubscriptionId = "[subscription id]";
                opts.DnsRecordType = CustomHostNameDnsRecordType.A;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseReallySimpleCerts();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();
        }
    }
}
