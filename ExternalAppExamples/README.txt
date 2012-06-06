1. Introduction
===============

These examples demonstrate how to build ExternalApps for MXit's External Application API.

The examples were created with Microsoft Visual C# 2010 Express, which you can download and use free of
charge from: http://www.microsoft.com/express/downloads/





2. How do I run an example?
===========================

To run an example you need to do a few things (in the correct order):

2.1 Register with MXit as an ExternalApp developer.
2.2 Configure the example to use your Test API Key.
2.3 Add a reference to .NET's System.Drawing DLL for each project.
2.4 Add a reference to MXit's ExternalAppSDK DLL for each project.
2.5 Download the ExternalAppAPI's service reference.
2.6 Set the example's console application project as the solution's startup project.
2.7 Compile and run the example's console application.



2.1. Register with MXit as an ExternalApp developer
---------------------------------------------------

To connect any ExternalApp to MXit's platform, you'll need a MXit Developer account.

To create an account, point your browser to http://code.mxit.com and follow the steps to register
as a MXit developer.

After registering as a MXit Developer, you'll receive a "Test API Key". This is basically a
connection name and password for the ExternalAppAPI, that you can use to develop and test your
ExternalApps on MXit's platform.



2.2. Configure the example to use your Test API Key
---------------------------------------------------

Once you've registered an ExternalApp developer account with MXit, you can use the credentials from
your Test API Key to connect and run an example ExternalApp on MXit's platform.

To configure an example to use the credentials from your Test API Key:

- Double-click the "MXitExternalAppExamples.sln" file to open the solution with Microsoft Visual Studio.
- Open the example's application configuration file (e.g. to run the TicTacToe example, you need to
  update "MXit.ExternalApp.Examples.TicTacToe\app.config").
- Replace the 'ExternalApp.Name' and 'ExternalApp.Password' application setting values with the
  ExternalApp name and password from your Test API Key.



2.3 Add a reference to .NET's System.Drawing DLL
------------------------------------------------

To compile the solution, you'll need to include .NET's 'System.Drawing' library as a reference in each
of the solution's projects. This library is distributed with the .NET framework.

To add a reference to the library:

- Double-click the "MXitExternalAppExamples.sln" file to open the solution with Microsoft Visual Studio.
- For each project in the solution:
  - Expand the project in the solution explorer.
  - Right-click on the project's 'References' and select 'Add Reference'.
  - In the '.NET' tab, select 'System.Drawing' from the list of libraries.



2.4 Add a reference to MXit's ExternalAppSDK DLL
------------------------------------------------

To compile the example, you'll need to include MXit's External Application SDK library as a
reference in each of the solution's projects. The SDK library is distributed as a DLL file in the
installation that these examples form a part of.

To find the location of the library on your harddrive, click on
  Start => All Programs => MXit Lifestyle => MXit External Application SDK X.X.X => Library

- Double-click the "MXitExternalAppExamples.sln" file to open the solution with Microsoft Visual Studio.
- For each project in the solution:
  - Expand the project in the solution explorer.
  - Right-click on the project's 'References' and select 'Add Reference'.
  - Browse to the location where you installed MXit's External Application SDK, and from the "lib"
    directory select "MXitExternalAppSDK.dll".



2.5. Download the ExternalAppAPI's service reference
----------------------------------------------------

Now we are ready to download the External Application API's service reference.

NB: Do not download the service reference before you've added .NET's System.Drawing and MXit's
ExternalAppSDK DLLs as references to the projects (see previous steps for instructions on how to
do this).

To make this step simpler we've already added the service reference in the project, all you need to
do is update it. 

- Expand the 'MXit.ExternalApp' project's 'Service References' in the solution explorer.
- Right-click on the 'ExternalAppAPI' service reference and select 'Update Service Reference' from
  the popup menu. 



2.6. Compile and run the example's console application
------------------------------------------------------

In Microsoft Visual Studio's solution explorer:

- Right-click on the example project you wish to run and select 'Set as StartUp Project' from the
  popup menu.

From Microsoft Visual Studio's main menu:

- Click on "Debug => Build Solution" to compile the application.
- Click on "Debug => Start Debugging" to run the application. 





3. OK, I'm up and running. How do I access the example from MXit?
=================================================================

To access the example, login to your MXit account and add the ExternalApp's service to your
contacts.

The name of the service to add is the name of the ExternalApp that you specified in the example's
configuration file.

Note: If this is the first time you're using the ExternalApp's connection credentials to connect
the ExternalApp to MXit's platform, it can take up to an hour for the ExternalApp to become
available on MXit.

To see live statistics about the example ExternalApp while it is running in MXit, add a service
called "externalappapi" to your MXit contacts and follow the menu items to browse to the
ExternalApp that the example is connected as.





4. Right, I got it working. So how do I get started with my own ExternalApp?
============================================================================

To make it easier for you guys to create new ExternalApps, we've included a generic ExternalApp
framework in the solution.

The framework handles all the nitty gritty technical stuff to make ExternalApps more optimized
and robust (e.g. auto-reconnects to the ExternalAppAPI, using a request queue, etc.).

If you use the framework to write ExternalApps, all you need to worry about is the application's
business logic and data storage. 

All the ExternalApp examples are built on this framework so you shouldn't have too much difficulty
figuring out how to use it.

The framework is included in the 'MXit.ExternalApp' project.



4.1. The 'MXit.ExternalApp' framework and .NET 3.5/4.0+
-------------------------------------------------------

We built the framework for .NET 4.0, but then added backwards compatibility code for .NET 3.5.

If you're using .NET 4.0 or higher, you probably want to remove the backwards compatibility code.

To convert the 'MXit.ExternalApp' project to .NET 4.0:
- In the project's properties, change the 'Target framework' to .NET 4.0.
- Uncomment the 'DotNet4' #define in the "MXit.ExternalApp\ExternalAppServiceBase.cs" file.
- Uncomment the 'DotNet4' #define in the "MXit.ExternalApp\ExternalAppService.cs" file.
- Rebuild the project.
