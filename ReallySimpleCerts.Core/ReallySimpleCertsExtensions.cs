using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ReallySimpleCerts.Core
{
    public static class ReallySimpleCertsExtensions
    {
        public interface IReallySimpleCertsPersistenceChooser
        {
            ReallySimpleCertsBuilder WithAzureBlobStorePersistence(Action<BlobStorePersistenceOptions> configure);
            ReallySimpleCertsBuilder WithAzureKeyVaultPersistence(Action<KeyVaultPersistenceOptions> configure);
            ReallySimpleCertsBuilder WithUnsafeDiskPersistence(Action<UnsafeDiskPersistenceOptions> configure);
            ReallySimpleCertsBuilder WithInMemoryPersistence();
        }

        public class ReallySimpleCertsPersistenceChooser : IReallySimpleCertsPersistenceChooser
        {
            private readonly IServiceCollection services;

            public ReallySimpleCertsPersistenceChooser(IServiceCollection services)
            {
                this.services = services;
            }
            public ReallySimpleCertsBuilder WithAzureBlobStorePersistence(Action<BlobStorePersistenceOptions> configure)
            {
                if (configure != null) services.Configure(configure);
                services.AddTransient<IReallySimpleCertPersistence, BlobStorePersistence>();
                if (!services.Any(x => x.ServiceType == typeof(IBlobContainerFactory)))
                {
                    services.AddSingleton<IBlobContainerFactory, DefaultBlobContainerFactory>();
                }
                return new ReallySimpleCertsBuilder(services);
            }

            public ReallySimpleCertsBuilder WithAzureKeyVaultPersistence(Action<KeyVaultPersistenceOptions> configure)
            {
                if (configure != null) services.Configure(configure);
                services.AddTransient<IReallySimpleCertPersistence, KeyVaultPersistence>();
                if (!services.Any(x => x.ServiceType == typeof(IKeyVaultClientFactory)))
                {
                    services.AddSingleton<IKeyVaultClientFactory, DefaultKeyVaultClientFactory>();
                }
                return new ReallySimpleCertsBuilder(services);
            }

            public ReallySimpleCertsBuilder WithInMemoryPersistence()
            {
                services.AddSingleton<IReallySimpleCertPersistence, InMemoryPersistence>();
                return new ReallySimpleCertsBuilder(services);
            }

            public ReallySimpleCertsBuilder WithPersistence<Tpersistence>()
                where Tpersistence : class, IReallySimpleCertPersistence
            {
                services.AddTransient<IReallySimpleCertPersistence, Tpersistence>();
                return new ReallySimpleCertsBuilder(services);
            }

            public ReallySimpleCertsBuilder WithPersistence<Tpersistence, TpersistenceOpts>(Action<TpersistenceOpts> configure)
                where Tpersistence : class, IReallySimpleCertPersistence
                where TpersistenceOpts : class
            {
                if (configure != null) services.Configure(configure);
                services.AddTransient<IReallySimpleCertPersistence, Tpersistence>();
                return new ReallySimpleCertsBuilder(services);
            }

            public ReallySimpleCertsBuilder WithUnsafeDiskPersistence(Action<UnsafeDiskPersistenceOptions> configure)
            {
                if (configure != null) services.Configure(configure);
                services.AddTransient<IReallySimpleCertPersistence, UnsafeDiskPersistence>();
                return new ReallySimpleCertsBuilder(services);
            }
        }

        public class ReallySimpleCertsBuilder : IServiceCollection
        {
            private readonly IServiceCollection services;

            public ReallySimpleCertsBuilder(IServiceCollection services)
            {
                this.services = services;
            }

            public ReallySimpleCertsBuilder WithAzureWebAppHandler(Action<AzureRmOptions> configure)
            {
                if (configure != null) services.Configure(configure);
                services.AddTransient<ICertificateHandler, AzureRmThirdPartyDomainCertificateHandler>();
                services.AddTransient<IHostNameHandler, AzureRmThirdPartyDomainCertificateHandler>();
                services.AddSingleton<IAzureConfigurationFactory, DefaultAzureConfigurationFactory>();
                return this;
            }

            public ReallySimpleCertsBuilder WithAzureBlobStorePersistence(Action<BlobStorePersistenceOptions> configure)
            {
                if (configure != null) services.Configure(configure);
                services.AddTransient<IReallySimpleCertPersistence, BlobStorePersistence>();
                return this;
            }

            public ServiceDescriptor this[int index] { get => services[index]; set => services[index] = value; }

            public int Count { get => services.Count; }
            public bool IsReadOnly { get => services.IsReadOnly; }

            public void Add(ServiceDescriptor item)
            {
                services.Add(item);
            }

            public void Clear()
            {
                services.Clear();
            }

            public bool Contains(ServiceDescriptor item)
            {
                return services.Contains(item);
            }

            public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
            {
                services.CopyTo(array, arrayIndex);
            }

            public IEnumerator<ServiceDescriptor> GetEnumerator()
            {
                return services.GetEnumerator();
            }

            public int IndexOf(ServiceDescriptor item)
            {
                return services.IndexOf(item);
            }

            public void Insert(int index, ServiceDescriptor item)
            {
                services.Insert(index, item);
            }

            public bool Remove(ServiceDescriptor item)
            {
                return services.Remove(item);
            }

            public void RemoveAt(int index)
            {
                services.RemoveAt(index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return services.GetEnumerator();
            }
        }

        public static IReallySimpleCertsPersistenceChooser AddReallySimpleCerts(this IServiceCollection services, Action<ReallySimpleCertOptions> configure = null)
        {
            if (configure != null) services.Configure(configure);
            services.AddTransient<IConfigureOptions<KestrelServerOptions>, KestrelOptionsSetup>();
            services.AddSingleton<ReallySimpleCertProvider>();
            services.AddSingleton<IHostedService, ReallySimpleCertProvider>(x => x.GetRequiredService<ReallySimpleCertProvider>());
            if (!services.Any(x => x.ServiceType == typeof(IAcmeContextFactory)))
            {
                services.AddSingleton<IAcmeContextFactory, DefaultAcmeContextFactory>();
            }
            return new ReallySimpleCertsPersistenceChooser(services); 
        }

        public static IApplicationBuilder UseReallySimpleCerts(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value.StartsWith("/.well-known/acme-challenge/"))
                {
                    var persistence = app.ApplicationServices.GetRequiredService<IReallySimpleCertPersistence>();
                    var opts = app.ApplicationServices.GetRequiredService<IOptions<ReallySimpleCertOptions>>();
                    var provider = app.ApplicationServices.GetRequiredService<ReallySimpleCertProvider>();

                    var token = context.Request.Path.Value.Split('/').Last();
                    var authz = await persistence.GetAuthz(token);

                    if (authz == default)
                    {
                        app.ApplicationServices.GetRequiredService<ILogger<ReallySimpleCertProvider>>()
                            .LogWarning($"Received a valid acme-challenge request but the authz could not be found by token {token}.");
                        await next();
                        return;
                    }

                    var bytes = Encoding.UTF8.GetBytes(authz.authz);
                    await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);

                    _ = Task.Run(async () => { await Task.Delay(1000); _ = provider.Continue(token); });
                }
                else
                {
                    await next();
                }
            });
            return app;
        }
    }
}
