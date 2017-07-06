# ServiceFabricVotingApp
A sample Service Fabric voting app with one stateless service and a user interface for data entry and one stateful service for keeping vote counting results.
The code is based on two Service Fabric lab materials below. Part I was created by the Microsoft Service Fabric team. Part II, based on Part I, was created by my colleague Max Knor. 
* [Service Fabric Lab Part I](https://blogs.msdn.microsoft.com/azureservicefabric/2016/07/06/introduction-to-service-fabric-lab-part-1/) - The app has a stateless service containing a single page application 
* [Service Fabric Lab Part II](http://blog.knor.net/index.php/2016/12/14/fan-sequel-introduction-to-service-fabric-lab-part-2/) - The app adds a stateful service, in addition to a stateful service.


## Getting Started

You can download the sample app and test it locally. If you have an Azure account, you can publish it to the Service Fabric cluster in Azure. More detail at 
[Developing and Deploying a Service Fabric Voting App to Azure](https://blogs.msdn.microsoft.com/zxue/2017/02/25/developing-and-deploying-a-service-fabric-voting-app-to-azure/)

### Prerequisites

The app is created and tested in Visual Studio 2015, Update 3. It requires Service Fabric SDK 2.0 and Azure SDK for .NET 2.9. 

* [Service Fabric SDK 2.0](http://www.microsoft.com/web/handlers/webpi.ashx?command=getinstallerredirect&appid=MicrosoftAzure-ServiceFabric-VS2015/) 
* [Azure SDK for .NET 2.9](https://azure.microsoft.com/downloads/) 
