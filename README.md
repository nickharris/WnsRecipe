WnsRecipe
=========

The Windows Push Notification Service Recipe provides an object model to easily construct and send Toast, Tile, Badge and Raw Notifications using the Windows Push Notification Services (WNS).

<a name="using-the-wns-recipe" />
# Using the WNS Recipe #

After installing this NuGet, you will have a **WnsRecipe** assembly added to your project references. 

![Image 1](images/image-1.png?raw=true)

To send notifications using WNS, you must first register your application at the Windows Push Notifications & Live Connect portal to obtain Package Security Identifier (SID) and a secret key that your cloud service uses to authenticate with WNS. 

An application receives push notifications by requesting a notification channel from WNS, which returns a channel URI that the application then registers with your cloud service. 

In your cloud service, the WnsAccessTokenProvider authenticates with WNS by providing its credentials, the package SID and secret key, and receives in return an access token that the provider caches and can reuse for multiple notification requests. 

![Image 2](images/image-2.png?raw=true)

The cloud service constructs a notification request by filling out a template class that contains the information that will be sent with the notification, including text and image references.

![Image 3](images/image-3.png?raw=true)

Using the channel URI of a registered client, the cloud service can then send a notification whenever it has an update for the user. 

![Image 4](images/image-4.png?raw=true)

Note that to have the Send method available, you need to add a reference to the **NotificationsExtensions** namespace. 

![Image 5](images/image-5.png?raw=true)

<a name="diagnostics" />
# Diagnostics #

To ease troubleshooting, the WNS Recipe has been instrumented to write trace information by taking advantage of the diagnostics support provided by the .NET Framework.

By default, tracing is disabled. To enable it, you first need to configure the trace source used by the recipe by enabling one or more trace listeners and then set the level of information included in the trace by choosing from the levels: error, warning, information, or verbose. 

For example, at the error level, the component logs any failures in sending notifications, including status information, request and response sent to WNS, as well as any HTTP headers. Increasing the level to Information also records any notifications that are sent successfully. 

Your choice of listener will vary based on the environment where the application runs. For example, not all listeners can be used in the Windows Azure environment or some require special considerations. 

You can configure the trace source programmatically, by initializing the TraceSource property of the WnsDiagnostics class.

![Image 6](images/image-6.png?raw=true)

Otherwise, the trace source can be configured using the application configuration file. The WNS Recipe NuGet adds a new WNSRecipe source in the system.diagnostics section of the configuration file of your project. To complete the configuration, set the switchValue to choose the trace level and add one or more trace listeners, for example, by uncommenting one of the entries that have already been provided. 

![Image 7](images/image-7.png?raw=true)

Note that some trace listeners in this section may require additional configuration steps such as configuring output file location, setting file permissions, creating event sources, etc. For additional information on trace listener configuration, please refer to [Configuring Trace Listeners](http://msdn.microsoft.com/en-us/library/ff664708.aspx) and [Trace Listeners](http://msdn.microsoft.com/en-us/library/4y5y10s7.aspx). 

To complete the configuration steps, you must enable tracing by calling the Enable method of the WnsDiagnostics class. 

![Image 8](images/image-8.png?raw=true)

By default, the Enable method sets up a default trace source named WNSRecipe. Optionally, the Enable method allows you to choose a different name for the trace source used by the WNS Recipe, for example, to share the same trace source used by other components in your application. For example, the following code enables the WNS Recipe to use the WNSDiagnostics trace source. 

![Image 9](images/image-9.png?raw=true)

<a name="configuring-a-windows-azure-application" />
## Configuring a Windows Azure application ##

In order to read the trace level information from the Service Configuration and change it without redeploying or stopping the service, complete de following steps: 

- Add a setting named **WnsRecipeDiagnosticsTraceLevel** in the Service Configuration. In the value field enter the desired trace level:
- Add the following "using" code in the Global.asax.cs file:
````C#
using System.Diagnostics;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
````                   
- Add the following code snippet in the Application_Start method:

	````C#
	string traceLevelConfigurationSettingName = "WnsRecipeDiagnosticsTraceLevel";
	SourceLevels traceLevel = SourceLevels.Error;
	
	
	if (RoleEnvironment.IsAvailable)
	{
    	var traceLevelValue = CloudConfigurationManager.GetSetting(traceLevelConfigurationSettingName);
    	Enum.TryParse(traceLevelValue, true, out traceLevel);
	
    	RoleEnvironment.Changed += (sender, e) =>
    	{
        	if (e.Changes.OfType()
                            	.Where(change => change.ConfigurationSettingName == traceLevelConfigurationSettingName)
                            	.Any())
        	{
            	traceLevelValue = CloudConfigurationManager.GetSetting(traceLevelConfigurationSettingName);
            	Enum.TryParse(traceLevelValue, true, out traceLevel);
        	}
    	};
	}

	NotificationsExtensions.WnsDiagnostics.TraceSource.Switch.Level = traceLevel;
	````





                    





