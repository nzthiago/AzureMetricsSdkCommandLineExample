Azure Metrics SDK Command Line Example
=================================

This sample includes .NET code that makes use of the Azure Monitoring SDK, which can be obtained via [NuGet](http://www.nuget.org/packages/Microsoft.WindowsAzure.Management.Monitoring/). This SDK allows for the retrieval of Metrics from Windows Azure assets. This example uses [the Web Sites Management Library](http://www.nuget.org/packages/Microsoft.WindowsAzure.Management.WebSites/), which is also available via NuGet as part of the Windows Azure Management Libraries, to pull down a list of the Web Sites running in an Azure subscription. Then, the metrics for each of the Web Sites is retrieved and spilled out to the console window. 

## Setup ##
This example authenticates against the Azure REST API using a certificate. To enable this, create a self-signed certificate and then upload the certificate using the Windows Azure portal. Then, create a PFX for the CER file and include the PFX in this Visual Studio project. 

The Program.CS file will need to be changed to point to your own PFX file and to include the proper password for your PFX. 

    var cred = CertificateCredentialHelper.CreateCredential(_subId, "YOUR PFX FILENAME HERE", "YOUR PFX PASSWORD HERE");

Additionally, you'll need to set your own Azure subscription ID in the Program.cs file as well. 

    static string _subId = "YOUR SUBSCRIPTION ID HERE";

Once these changes are made, the code should work. 