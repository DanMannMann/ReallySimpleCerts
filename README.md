# ReallySimpleCerts
[![Build Status](https://dev.azure.com/danmann/ReallySimpleCerts/_apis/build/status/ReallySimpleCerts.Core?branchName=master)](https://dev.azure.com/danmann/ReallySimpleCerts/_build/latest?definitionId=2&branchName=master)
[![Test Status](https://img.shields.io/azure-devops/tests/danmann/ReallySimpleCerts/2.svg)](https://dev.azure.com/danmann/ReallySimpleCerts/_build/latest?definitionId=2&branchName=master)
![Nuget](https://img.shields.io/nuget/v/Marsman.ReallySimpleCerts.svg)

#### Really simple certificates for ASP.NET Core &amp; Azure web apps, using Lets Encrypt via Certes.

### Installation

Find the package on [nuget](https://www.nuget.org/packages/Marsman.ReallySimpleCerts "Marsman.ReallySimpleCerts") or install using `Install-Package Marsman.ReallySimpleCerts` in VS Package Manager or `dotnet add package Marsman.ReallySimpleCerts` in your shell.


### Usage

There are two distinct situations that ReallySimpleCerts aims to support. The first - and simplest - is to leverage Let's Encrypt certificates in an ASP.NET Core app running on Kestrel with no reverse proxy, as is typical when deploying to a container. The following configuration supports this scenario.
```
using Certes;
using Certes.Acme;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using ReallySimpleCerts.Core;
...
public void ConfigureServices(IServiceCollection services)
{
	...
	services.AddReallySimpleCerts(opts =>
	{
		// The service principal is used by AzureKeyVaultPersistence
		// to authenticate to the configured key store.
		// The principal used for the key vault needs an access 
		// policy granting at least get, list and set permissions over secrets.
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

		// LetsEncrypt rate-limits production certificate orders, 
		// so always use staging for local testing. Switch this to 
		// the production root URI when deploying to an environment 
		// that needs a real cert.
		opts.IssuerRootUri = WellKnownServers.LetsEncryptStagingV2;
	})
	.WithAzureKeyVaultPersistence(opts =>
	{
		opts.KeyVaultRootUrl = "https://[your key vault].vault.azure.net/";
    
		// Set this to use the app's managed identity to 
		// connect to key vault (only works while running in Azure).
		// opts.UseManagedIdentity = true;
	});
  ...
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{	
	...
	app.UseReallySimpleCerts();
	...
}
```


When ReallySimpleCerts is used _all_ requests which arrive in the Kestrel pipeline using https will use the created certificate. Storage of the certificate and related information can be configured to use Azure Key Vault (recommended)
