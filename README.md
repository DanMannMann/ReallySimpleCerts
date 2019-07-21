# ReallySimpleCerts
[![Build Status](https://dev.azure.com/danmann/ReallySimpleCerts/_apis/build/status/ReallySimpleCerts.Core?branchName=master)](https://dev.azure.com/danmann/ReallySimpleCerts/_build/latest?definitionId=2&branchName=master)
[![Test Status](https://img.shields.io/azure-devops/tests/danmann/ReallySimpleCerts/2.svg)](https://dev.azure.com/danmann/ReallySimpleCerts/_build/latest?definitionId=2&branchName=master)
![Nuget](https://img.shields.io/nuget/v/Marsman.ReallySimpleCerts.svg)

#### Really simple certificates for ASP.NET Core &amp; Azure web apps, using Lets Encrypt via Certes.

### Installation

Find the package on [nuget](https://www.nuget.org/packages/Marsman.ReallySimpleCerts "Marsman.ReallySimpleCerts") or install using `Install-Package Marsman.ReallySimpleCerts` in VS Package Manager or `dotnet add package Marsman.ReallySimpleCerts` in your shell.


### Usage

There are two distinct situations that ReallySimpleCerts aims to support. The first - and simplest - use case is to leverage Let's Encrypt certificates in an ASP.NET Core app running on Kestrel with no reverse proxy, as is typical when running in a container or during development. The second use case is to automate the creation of hostname bindings & SSL SNI bindings for an Azure Website (App Service).

#### No reverse proxy
The following configuration supports the first use case, using Azure Key Vault to pesist the certificate and related information. When ReallySimpleCerts is used (in any configuration), _all_ requests which arrive in the Kestrel pipeline using https will use the created certificate. 
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
#### Azure App Service Binding
Requests arriving at Kestrel when it is hosted behind a reverse proxy, such as the IIS in-process hosting used in an Azure Website, generally arrive using http. The standard configuration above is useless in this situation - we need to register the hostname & associated certificate with the host in order to use it. The `AzureRmThirdPartyDomainCertificateHandler` can be used to automate the creation & maintainance of these bindings from within the app itself. Insert the following after the `.WithAzureKeyVaultPersistence` call to add & configure the handler.
```
.WithAzureWebAppHandler(opts =>
{
	opts.ResourceGroupName = "[resource group]";
	opts.WebAppName = "[webapp]";
	opts.SubscriptionId = "[subscription id]";
	opts.DnsRecordType = CustomHostNameDnsRecordType.A;
})
```
By default the `AzureRmThirdPartyDomainCertificateHandler` uses the service principal specified in `ReallySimpleCertOptions`, but it can also be given its own specific principal by setting `opts.ServicePrincipalCredentials`. The principal used requires "website contributor" access to the **resource group** containing the target web app, in order to create the hostname & SSL SNI bindings.

#### Certificate Storage
Storage of the certificate and related information can be configured to use Azure Key Vault (recommended) or Azure Blob Storage. To use Azure Blob Storage, replace the call to `WithAzureKeyVaultPersistence` with the following:
```
.WithAzureBlobStorePersistence(opts =>
{
	opts.StorageConnectionString = "[connection string]";
	opts.ContainerName = "[container name]";
})
```
The core package also includes in-memory & file-based persistence implementations, which can be useful for testing/experimentation but should **never** be used in production environments or with any certificate that needs to remain secure.

#### Supporting Other Hosts
The `AzureRmThirdPartyDomainCertificateHandler` implements `IHostNameHandler` & `ICertificateHandler`, which are called when the certificate is created or updated, as well as when the certificate is initially loaded on app start. These implementations create & maintain the hostname binding & the SNI SSL binding, respectively. By implementing these interfaces & registering the implementation as a transient service, it is possible to support deploying the Lets Encrypt certificate to any host which allows bindings to be created via an API.
