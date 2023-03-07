# Conf24: Azure Container Apps – serverless container apps in action

Demos for cloud native
conference [Conf24 on March 23rd 2023](https://www.conf42.com/Cloud_Native_2023_Bojan_Vrhovnik_azure_serverless_container_apps?eventId=Conf42Conference_vkcrMclJysKT&ocid=aid3057829).

Kubernetes is becoming de facto community standard for running container workloads at scale. Setting up and maintaining
Kubernetes is not an easy task to do, let alone focus on the efficient workloads by following all the best practices in
cloud native area. Azure Container App brings the best of Kubernetes with cloud best practices applied without handling
the Kubernetes beast behind the scenes. It gives developers and IT pros the time to focus on the workload, by giving
them tools to focus on the fully managed experience for them and their customers. In this session we will explore how to
deploy and configure Kubernetes-style applications, focusing on application lifecycle, autoscaling, ingress without
managing infrastructure, enriching the functionality with distributed application runtime (Dapr), securely manage all
the secrets, and enabling hybrid scenarios with running apps on Kubernetes on-prem as managed offerings.

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

TBD

# Additional information

You can read about different techniques and options here:

1. [What-The-Hack initiative](https://aka.ms/wth)
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
2. [MetroApps](https://mahapps.com/) - MahApps.Metro is a framework that allows developers to cobble together a Metro or
   Modern UI for their own WPF applications with minimal effort.
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