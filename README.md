# Conf24: Azure Container Apps – serverless container apps in action

Demos for cloud native
conference [Conf24 on March 30rd 2023](https://www.conf42.com/Cloud_Native_2023_Bojan_Vrhovnik_azure_serverless_container_apps?eventId=Conf42Conference_vkcrMclJysKT&ocid=aid3057829).

Kubernetes is becoming de facto community standard for running container workloads at scale. Setting up and maintaining
Kubernetes is not an easy task to do, let alone focus on the efficient workloads by following all the best practices in
cloud native area. Azure Container App brings the best of Kubernetes with cloud best practices applied without handling
the Kubernetes beast behind the scenes. It gives developers and IT pros the time to focus on the workload, by giving
them tools to focus on the fully managed experience for them and their customers. In this session we will explore how to
deploy and configure Kubernetes-style applications, focusing on application lifecycle, autoscaling, ingress without
managing infrastructure, enriching the functionality with distributed application runtime (Dapr), securely manage all
the secrets, and enabling hybrid scenarios with running apps on Kubernetes on-prem as managed offerings.

<!-- TOC -->
* [Conf24: Azure Container Apps – serverless container apps in action](#conf24--azure-container-apps--serverless-container-apps-in-action)
  * [Prerequisites](#prerequisites)
  * [Demos](#demos)
    * [Data folder](#data-folder)
    * [Generators folder](#generators-folder)
    * [UI folder](#ui-folder)
      * [ITS.Web - web application to work with the tickets](#itsweb---web-application-to-work-with-the-tickets)
      * [ITS.Web.ReportApi - API to provide REST services to get back the result](#itswebreportapi---api-to-provide-rest-services-to-get-back-the-result)
  * [Containers](#containers)
  * [Create and load environment variables](#create-and-load-environment-variables)
* [Additional information](#additional-information)
* [Credits](#credits)
* [Contributing](#contributing)
<!-- TOC -->

## Prerequisites

1. an active [Azure](https://www.azure.com) subscription - [MSDN](https://my.visualstudio.com) or trial
   or [Azure Pass](https://microsoftazurepass.com) is fine - you can also do all of the work
   in [Azure Shell](https://shell.azure.com) (all tools installed) and by
   using [Github Codespaces](https://docs.github.com/en/codespaces/developing-in-codespaces/creating-a-codespace)
2. [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/) installed to work with Azure or Azure PowerShell
   module installed
3. [PowerShell](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.2)
   installed - we do recommend an editor like [Visual Studio Code](https://code.visualstudio.com) to be able to write
   scripts, YAML pipelines and connect to repos to submit changes.
4. [OPTIONAL] [Windows Terminal](https://learn.microsoft.com/en-us/windows/terminal/install)

If you will be working on your local machines, you will need to have:

1. [Powershell](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.2)
   installed
2. git installed - instructions step by step [here](https://docs.github.com/en/get-started/quickstart/set-up-git)
3. [.NET](https://dot.net) installed to run the application if you want to run it locally
4. [SQL server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) to install the database and to populate
   it with data
5. an editor (besides notepad) to see and work with code, yaml, scripts and more (for
   example [Visual Studio Code](https://code.visualstudio.com))

## Demos

Demo is about simple issue tracking system (ITS) for managing work tasks. Stats about the system are on front page and
are refreshed daily.

Demo site enables you to add your work tasks and track their progress - you can specify online resources which are
associated with links. You can add register into the system, add tasks, comment on tasks, collaborate and more. You can
download list of tasks in PDF format and can associate online resources with tasks,which will get downloaded and stored
in PDF for offline viewing and will generate reports with attached resources.

![Demo solution structure](https://webeudatastorage.blob.core.windows.net/web/conf-24-solution-demo.png)

### Data folder

Data folder contains libraries to work with the database and external services like Azure Storage.

![Structure of libraries to work with data](https://webeudatastorage.blob.core.windows.net/web/conf-24-solution-data.png)

1. **ITS.Interfaces** - contains interfaces for data access and external services
2. **ITS.Models** - contains POCO objects to map the data from database
3. **ITS.SQL** - interface implementation for SQL database
4. **ITS.Storage** - interface implementation for local file storage - to save data to local file
5. **ITS.Storage.Azure** - interface implementation for Azure Storage - to save data to Azure Storage
6. **ITS.Storage.Dapr** - interface implementation for [Dapr](https://dapr.io) - to save data to Dapr components

You can replace the implementation of the interface (to use for example Azure Storage instead of local file system) in
**Program.cs** file inside projects.

### Generators folder

Generators folder contains command line tools to generate data for the system.

1. **ITS.File.Stats.Generator** - generates stats for the system and stores it to the database
2. **ITS.SQL.Generator** - generates database for the system and populates it with bogus data
   with [Bogus library](https://github.com/bchavez/Bogus).

![SQL data generator](https://webeudatastorage.blob.core.windows.net/web/conf24-sql-generator.gif)

You will need to provide env variables or follow procedure on the screen for ITS.File.Stats.Generator to work:

1. **SQL_CONNECTION_STRING** - connection string to the SQL database
2. **FILEPATH** - path to the file where to store the data

For the ITS.SQL.Generator you will need to provide env variables or follow procedure on the screen:

1. **FOLDER_ROOT** - path to the folder where to store the data - if you don't provide it, it will download from github
   and then extract it, run the code and do the job
2. **DROP_DATABASE** - true/false - if you want to drop the database before generating new one
3. **CREATE_TABLES** - true/false - if you want to create tables before generating new data
4. **DEFAULT_PASSWORD** - password for the users
5. **RECORD_NUMBER** - number of records to generate

### UI folder

UI folder contains web application to work with the tickets, API to provide REST services to get back the result and
background service to calculate statistics for the tasks on a daily basis.

#### ITS.Web - web application to work with the tickets

![web app](https://webeudatastorage.blob.core.windows.net/web/conf-24-solution-web.png)

In program.cs file you can change the implementation of the interface to use different storage for the data and
configure the app settings.

Those settings are stored in **appsettings.json** file and you can use env to override them:

1. **SqlOptions__ConnectionString** - connection string to the database
2. **AuthOptions__ApiKey** - API key which will match when doing an api call from client to server. Check
   out [ApiKeyAuthFilter.cs](\src\ITS\ITS.Web.ReportApi\Authentication\ApiKeyAuthFilter.cs) for more details.
3. **AuthOptions__HashSalt** - salt for hashing the ID to have youtube like routes (instead of ID). Check out hashids
   library [here](https://hashids.org/).
4. **AzureStorageOptions__ConnectionString** - connection string to the Azure Storage account
5. **AzureStorageOptions__ContainerName** - name of the container in the Azure Storage account
6. **AzureStorageOptions__BlobName** - name of the blob in the Azure Storage account (which will holds the file with
   stats information)
7. **ApiOptions__ReportApiUrl** - URL to the ReportApi (for example https://localhost:5001/)

To run it, navigate to the [folder](\src\ITS\ITS.Web\) and run (example below with one environment variable):

```powershell
Set-Item Env:AuthOptions__ApiKey "12345678"
dotnet run 
```

It uses forms authentication which can be replaced with something else (for
example [Azure AAD Auth](https://learn.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal)
or B2C) respectively.

#### ITS.Web.ReportApi - API to provide REST services to get back the result

It has only one [endpoint](\src\ITS\ITS.Web.ReportApi\Controllers\TaskApiReportsController.cs) to get back the stats
information from the database and generate PDF files with the data.

![API rest](https://webeudatastorage.blob.core.windows.net/web/conf-24-solution-api.png)

Those settings are stored in **appsettings.json** file and you can use env to override them:

1. **SqlOptions__ConnectionString** - connection string to the database
2. **AuthOptions__ApiKey** - API key which will match when doing an api call from client to server. Check
   out [ApiKeyAuthFilter.cs](\src\ITS\ITS.Web.ReportApi\Authentication\ApiKeyAuthFilter.cs) for more details.
3. **AuthOptions__HashSalt** - salt for hashing the ID to have youtube like routes (instead of ID). Check out hashids
   library [here](https://hashids.org/).
4. **AzureStorageOptions__ConnectionString** - connection string to the Azure Storage account
5. **AzureStorageOptions__ContainerName** - name of the container in the Azure Storage account
6. **AzureStorageOptions__BlobName** - name of the blob in the Azure Storage account (which will holds the file with
   stats information)

To run it, navigate to the [folder](\src\ITS\ITS.Web.ReportApi\) and run (example below with one environment variable):

```powershell
Set-Item Env:AuthOptions__ApiKey "12345678"
dotnet run 
```

## Containers

To use Azure Container Apps you will need to store containers to registry to be pulled from.

In our example we are using Azure Container Registry and also their build task options.

Dockerfiles for containers are located in [containers folder](containers).

To compile containers in **one go**, navigate to [scripts folder](scripts) and run Compile-Container script:

```powershell
.\Compile-Containers.ps1
```

To modify the depoyment, you can configure the following parameters:

1. **ResourceGroupName** - name of the resource group where to store the container
2. **RegistryName** - name of the container registry
3. **FolderName**  - name of the folder where you can find Dockerfiles (by default folder containers)
4. **TagName** - tag name for the container (by default latest)
5. **SourceFolder** - folder where to look for the source code (by default src)
6. **InstallCli** - if you want to install Azure CLI (by default false)

For example to build containers with tag 1.0.0:

```powershell
.\Compile-Containers.ps1 -TagName 1.0.0
```

## Create and load environment variables

The easiest way to create environment variables is to store it to file, exclude that file from getting into source
control and load variables via PowerShell (as demonstrated below).

Example of env variable inside env file:
AuthOptions__ApiKey=1234567890

You can check out [docs for dotnet run](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-run) for more options
how to run the app with parameters.

If you want to automate the process, you can use PowerShell script to prepare the environment for you.

```powershell
Get-Content $PathToENVFile | ForEach-Object {
    $name, $value = $_.split('=')
    Set-Content env:\$name $value
}
```

Prepare file (example [here](./scripts/env-file-example.changetoenv) - rename to .env) and exclude *.env files from
putting it to the repo. More [here](https://docs.github.com/en/get-started/getting-started-with-git/ignoring-files).
Open PowerShell and run the upper command by replacing PathToENVFile the path to your file in double quotes.

For ITS.Web you will need additionally:

1. **ApiOptions__ReportApiUrl** - URL to the ReportApi (for example https://localhost:5001/)

# Additional information

You can read about different techniques and options here:

1. [Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/)
2. [What-The-Hack initiative](https://aka.ms/wth)
2. [Azure Samples](https://github.com/Azure-Samples)
   or [use code browser](https://docs.microsoft.com/en-us/samples/browse/?products=azure)
3. [Azure Architecture Center](https://docs.microsoft.com/en-us/azure/architecture/)
4. [Application Architecture Guide](https://docs.microsoft.com/en-us/azure/architecture/guide/)
5. [Cloud Adoption Framework](https://docs.microsoft.com/en-us/azure/cloud-adoption-framework/)
6. [Well-Architected Framework](https://docs.microsoft.com/en-us/azure/architecture/framework/)
7. [Microsoft Learn](https://docs.microsoft.com/en-us/learn/roles/solutions-architect)

# Credits

1. [Spectre.Console](https://spectreconsole.net/) - Spectre.Console is a .NET Standard 2.0 library that makes it easier
   to create beautiful console applications.
2. [Bogus library](https://github.com/bchavez/Bogus) - Bogus is a .NET library for generating fake data.
3. [Polly.net](https://www.thepollyproject.org/) - Polly is a .NET resilience and transient-fault-handling library that
   allows developers to express policies such as Retry, Circuit Breaker, Timeout, Bulkhead Isolation, and Fallback in a
   fluent and thread-safe manner.
3. [HTMX](https://htmx.org) - htmx gives you access to AJAX, CSS Transitions, WebSockets and Server Sent Events directly
   in HTML, using attributes, so you can build modern user interfaces with the simplicity and power of hypertext.
4. [QuestPDF](https://github.com/QuestPDF/QuestPDF) - QuestPDF is an open-source .NET library for PDF documents
   generation.
5. [Dapr](https://dapr.io/) - Dapr is a portable, event-driven, runtime for building distributed applications.

# Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.