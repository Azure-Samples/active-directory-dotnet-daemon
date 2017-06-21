---
services: active-directory
platforms: dotnet
author: jmprieur
---

# Calling web APIs in a daemon or long-running process

In this sample a Windows console application calls a web API using its app identity.  This scenario is useful for situations where headless or unattended job or process needs to run as an application identity, instead of as a user's identity.  The application uses the Active Directory Authentication Library (ADAL) to get a token from Azure AD using the OAuth 2.0 client credential flow, where the client credential is a password.

For more information about how the protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](http://go.microsoft.com/fwlink/?LinkId=394414).

> Looking for previous versions of this code sample? Check out the tags on the [releases](../../releases) GitHub page.

## How To Run This Sample

To run this sample you will need:
- Visual Studio 2013
- An Internet connection
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, please see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/) 

### Step 1:  Clone or download this repository

From your shell or command line:

`git clone https://github.com/Azure-Samples/active-directory-dotnet-daemon.git`

### Step 2:  Register the sample with your Azure Active Directory tenant

There are two projects in this sample.  Each needs to be separately registered in your Azure AD tenant.

#### Register the TodoListService web API

1. Sign in to the [Azure portal](https://portal.azure.com).
2. On the top bar, click on your account and under the **Directory** list, choose the Active Directory tenant where you wish to register your application.
3. Click on **More Services** in the left hand nav, and choose **Azure Active Directory**.
4. Click on **App registrations** and choose **Add**.
5. Enter a friendly name for the application, for example 'TodoListService' and select 'Web Application and/or Web API' as the Application Type. For the sign-on URL, enter the base URL for the sample, which is by default `https://localhost:44321`. Click on **Create** to create the application.
6. While still in the Azure portal, choose your application, click on **Settings** and choose **Properties**.
7. Find the Application ID value and copy it to the clipboard.
8. For the App ID URI, enter https://\<your_tenant_name\>/TodoListService, replacing \<your_tenant_name\> with the name of your Azure AD tenant. 

#### Register the TodoListDaemon app

1. Sign in to the [Azure portal](https://portal.azure.com).
2. On the top bar, click on your account and under the **Directory** list, choose the Active Directory tenant where you wish to register your application.
3. Click on **More Services** in the left hand nav, and choose **Azure Active Directory**, then click on **App registrations** and choose **Add**.
4. Enter a friendly name for the application, for example 'TodoListDaemon' and select 'Web Application and/or Web API' as the Application Type. Since this application is a daemon and not a web application, it doesn't have a sign-in URL, so for this field, just enter "http://TodoListDaemon".
5. While still in the Azure portal, choose your application, click on **Settings** and choose **Properties**.
6. Find the Application ID value and copy it to the clipboard.
7. From the Settings menu, choose **Keys** and add a key - select a key duration of either 1 year or 2 years. When you save this page, the key value will be displayed, copy and save the value in a safe location - you will need this key later to configure the project in Visual Studio - this key value will not be displayed again, nor retrievable by any other means, so please record it as soon as it is visible from the Azure Portal.
8. Configure Permissions for your application - in the Settings menu, choose the 'Required permissions' section, click on **Add**, then **Select an API**, and type 'TodoListService' in the textbox. Then, click on  **Select Permissions** and select 'Access TodoListService'.

### Step 3:  Configure the sample to use your Azure AD tenant

#### Configure the TodoListService project

1. Open the solution in Visual Studio 2013.
2. Open the `web.config` file.
3. Find the app key `ida:Tenant` and replace the value with your AAD tenant name.
4. Find the app key `ida:Audience` and replace the value with the App ID URI you registered earlier, for example `https://<your_tenant_name>/TodoListService`.

#### Configure the TodoListDaemon project

1. Open `app.config'.
2. Find the app key `ida:Tenant` and replace the value with your AAD tenant name.
3. Find the app key `ida:ClientId` and replace the value with the Application ID for the TodoListDaemon from the Azure portal.
4. Find the app key `ida:AppKey` and replace the value with the key for the TodoListDaemon from the Azure portal.
5. Find the app key `todo:TodoListResourceId` and replace the value with the  App ID URI of the TodoListService.
6. Find the app key `todo:TodoListBaseAddress` and replace the value with the base address of the TodoListService project.

**NOTE:** The TodoListService's `ida:Audience` and TodoListDaemon's `todo:TodoListResourceId` app key values must not only match the App ID URI you configured, but they must also match each other exactly, including case. Otherwise calls to the TodoListService /api/todolist endpoint will fail with "Error: unauthorized".

### Step 4:  Trust the IIS Express SSL certificate

Since the web API is SSL protected, the client of the API (the web app) will refuse the SSL connection to the web API unless it trusts the API's SSL certificate.  Use the following steps in Windows Powershell to trust the IIS Express SSL certificate.  You only need to do this once.  If you fail to do this step, calls to the TodoListService will always throw an unhandled exception where the inner exception message is:

"The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel."

To configure your computer to trust the IIS Express SSL certificate, begin by opening a Windows Powershell command window as Administrator.

Query your personal certificate store to find the thumbprint of the certificate for `CN=localhost`:

```
PS C:\windows\system32> dir Cert:\LocalMachine\My


    Directory: Microsoft.PowerShell.Security\Certificate::LocalMachine\My


Thumbprint                                Subject
----------                                -------
C24798908DA71693C1053F42A462327543B38042  CN=localhost
```

Next, add the certificate to the Trusted Root store:

```
PS C:\windows\system32> $cert = (get-item cert:\LocalMachine\My\C24798908DA71693C1053F42A462327543B38042)
PS C:\windows\system32> $store = (get-item cert:\Localmachine\Root)
PS C:\windows\system32> $store.Open("ReadWrite")
PS C:\windows\system32> $store.Add($cert)
PS C:\windows\system32> $store.Close()
```

You can verify the certificate is in the Trusted Root store by running this command:

`PS C:\windows\system32> dir Cert:\LocalMachine\Root`

### Step 5:  Run the sample

Clean the solution, rebuild the solution, and run it.  You might want to go into the solution properties and set both projects as startup projects, with the service project starting first.

The daemon will add items to its To Do list and then read them back.

## How To Deploy This Sample to Azure

Coming soon.

## About The Code

Coming soon.

## How To Recreate This Sample

First, in Visual Studio 2013 create an empty solution to host the  projects.  Then, follow these steps to create each project.

### Creating the TodoListService Project

1. In the solution, create a new ASP.Net MVC web API project called TodoListService and while creating the project, click the Change Authentication button, select Organizational Accounts, Cloud - Single Organization, enter the name of your Azure AD tenant, and set the Access Level to Single Sign On.  You will be prompted to sign-in to your Azure AD tenant.  NOTE:  You must sign-in with a user that is in the tenant; you cannot, during this step, sign-in with a Microsoft account.
2. In the `Models` folder add a new class called `TodoItem.cs`.  Copy the implementation of TodoItem from this sample into the class.
3. Add a new, empty, Web API 2 controller called `TodoListController`.
4. Copy the implementation of the TodoListController from this sample into the controller.  Don't forget to add the `[Authorize]` attribute to the class.
5. In `TodoListController` resolving missing references by adding `using` statements for `System.Collections.Concurrent`, `TodoListService.Models`, `System.Security.Claims`.

### Creating the TodoListDaemon Project

1. In the solution, create a new Windows --> Console Application called TodoListDaemon.
2. Add the (stable) Active Directory Authentication Library (ADAL) NuGet, Microsoft.IdentityModel.Clients.ActiveDirectory, version 1.0.3 (or higher) to the project.
3. Add  assembly references to `System.Net.Http`, `System.Web.Extensions`, and `System.Configuration`.
4. Add a new class to the project called `TodoItem.cs`.  Copy the code from the sample project file of same name into this class, completely replacing the code in the file in the new project.
5. Copy the code from `Program.cs` in the sample project into the file of same name in the new project, completely replacing the code in the file in the new project.
6. In `app.config` create keys for `ida:AADInstance`, `ida:Tenant`, `ida:ClientId`, `ida:AppKey`, `todo:TodoListResourceId`, and `todo:TodoListBaseAddress` and set them accordingly.  For the public Azure cloud, the value of `ida:AADInstance` is `https://login.windows.net/{0}`.

Finally, in the properties of the solution itself, set both projects as startup projects.
