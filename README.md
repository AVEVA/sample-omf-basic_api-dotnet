# Building a .NET sample to send OMF to PI or Cds

**Version**: 2.1.6

| Cds Test Status                                                                                                                                                                                                                                                                                                                                                    | EDS Test Status                                                                                                                                                                                                                                                                                                                                                    | PI Test Status                                                                                                                                                                                                                                                                                                                                                        |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [![Build Status](https://dev.azure.com/AVEVA-VSTS/Cloud%20Platform/_apis/build/status%2Fproduct-readiness%2FOMF%2FAVEVA.sample-omf-basic_api-dotnet?repoName=AVEVA%2Fsample-omf-basic_api-dotnet&branchName=main&jobName=Tests_ADH)](https://dev.azure.com/AVEVA-VSTS/Cloud%20Platform/_build/latest?definitionId=16159&repoName=AVEVA%2Fsample-omf-basic_api-dotnet&branchName=main) | [![Build Status](https://dev.azure.com/AVEVA-VSTS/Cloud%20Platform/_apis/build/status%2Fproduct-readiness%2FOMF%2FAVEVA.sample-omf-basic_api-dotnet?repoName=AVEVA%2Fsample-omf-basic_api-dotnet&branchName=main&jobName=Tests_EDS)](https://dev.azure.com/AVEVA-VSTS/Cloud%20Platform/_build/latest?definitionId=16159&repoName=AVEVA%2Fsample-omf-basic_api-dotnet&branchName=main) | [![Build Status](https://dev.azure.com/AVEVA-VSTS/Cloud%20Platform/_apis/build/status%2Fproduct-readiness%2FOMF%2FAVEVA.sample-omf-basic_api-dotnet?repoName=AVEVA%2Fsample-omf-basic_api-dotnet&branchName=main)](https://dev.azure.com/AVEVA-VSTS/Cloud%20Platform/_build/latest?definitionId=16159&repoName=AVEVA%2Fsample-omf-basic_api-dotnet&branchName=main&jobName=Tests_OnPrem) |

Developed against DotNet 8.0

## Building a sample with the rest calls directly

The sample does not makes use of the CONNECT data services client libraries.

The sample also does not use any libraries for connecting to PI. Generally a library will be easier to use.

This sample also doesn't use any help to build the JSON strings for the OMF messages. This works for simple examples, and for set demos, but if building something more it may be easier to not form the JSON messages by hand.

[OMF documentation](https://docs.aveva.com/bundle/omf/page/1283981.html)

## To run this sample in Visual Studio

1. Clone the GitHub repository
2. Open the solution file in Microsoft Visual Studio, [OMFAPI.sln](OMFAPI.sln)
3. Rename the file [appsettings.placeholder.json](OMFAPI/appsettings.placeholder.json) to appsettings.json
4. Update appsettings.json with the credentials for the enpoint(s) you want to send to. See [Configure endpoints and authentication](#configure-endpoints-and-authentication) below for additional details
5. Click **Debug** > **Start Debugging** (or F5)

## To test this sample in Visual Studio

1. Follow steps 1-4 from the section above
2. Click **Test** > **Run All Tests** (or Ctrl+R, A)

## Customizing the application

This application can be customized to send your own custom types, containers, and data by modifying the [OMF-Types.json](OMF-Types.json) [OMF-Containers.json](OMF-Containers.json), and [OMF-Data.json](OMF-Data.json) files respectively. Each one of these files contains an array of OMF json objects, which are created in the endpoints specified in [appsettings.json](appsettings.placeholder.json) when the application is run. For more information on forming OMF messages, please refer to our [OMF version 1.2 documentation](https://docs.aveva.com/bundle/omf/page/1283983.html).

In addition to modifying the json files mentioned above, the get_data function in [Program.cs](OMFAPI/Program.cs) should be updated to populate the OMF data messages specified in [OMF-Data.json](OMF-Data.json) with data from your data source. Finally, if there are any other activities that you would like to be running continuously, this logic can be added under the while loop in the RunMain() function of [Program.cs](OMFAPI/Program.cs).

## Configure endpoints and authentication

The sample is configured using the file [appsettings.placeholder.json](appsettings.placeholder.json). Before editing, rename this file to `appsettings.json`. This repository's `.gitignore` rules should prevent the file from ever being checked in to any fork or branch, to ensure credentials are not compromised.

The application can be configured to send to any number of endpoints specified in the endpoints array within appsettings.json. In addition, there are three types of endpoints: CONNECT data services[Cds](#cds-endpoint-configuration), [EDS](#eds-endpoint-configuration), and [PI](#pi-endpoint-configuration). Each of the 3 types of enpoints are configured differently and their configurations are explained in the sections below.

### Cds endpoint configuration

An OMF ingress client must be configured. On our [AVEVA Learning](https://www.youtube.com/channel/UC333r4jIeHaY-rGgMjON54g) Channel on YouTube we have a video on [Ceating an OMF Connection](https://www.youtube.com/watch?v=52lAnkGC1IM).

The format of the configuration for an Cds endpoint is shown below along with descriptions of each parameter. Replace all parameters with appropriate values.

```json
{
  "Selected": true,
  "EndpointType": "CDS",
  "Resource": "https://uswe.datahub.connect.aveva.com",
  "NamespaceId": "PLACEHOLDER_REPLACE_WITH_NAMESPACE_ID",
  "TenantId": "PLACEHOLDER_REPLACE_WITH__ID",
  "clientId": "PLACEHOLDER_REPLACE_WITH_CLIENT_ID",
  "ClientSecret": "PLACEHOLDER_REPLACE_WITH_CLIENT_SECRET",
  "ApiVersion": "v1",
  "VerifySSL": true,
  "UseCompression": false,
  "WebRequestTimeoutSeconds": 30
}
```

| Parameters               | Required | Type    | Description                                                                                                                                                      |
| ------------------------ | -------- | ------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Selected                 | required | boolean | Tells the application if the endpoint should be sent to                                                                                                          |
| EndpointType             | required | string  | The endpoint type. For Cds this will always be "CDS"                                                                                                             |
| Resource                 | required | string  | The endpoint for Cds if the namespace. If the /namespace is located in NA, it is https://uswe.datahub.connect.aveva.com and if in EMEA, it is https://euno.datahub.connect.aveva.com  |
| NamespaceId              | required | string  | The name of the Namespace in Cds that is being sent to                                                                                                           |
| TenantId                 | required | string  | The  ID of the  in Cds that is being sent to                                                                                                         |
| ClientId                 | required | string  | The client ID that is being used for authenticating to Cds                                                                                                       |
| ClientSecret             | required | string  | The client secret that is being used for authenticating to Cds                                                                                                   |
| ApiVersion               | required | string  | The API version of the Cds endpoint                                                                                                                              |
| VerifySSL                | optional | boolean | A feature flag for verifying SSL when connecting to the Cds endpoint. By default this is set to true as it is strongly recommended that SSL be checked           |
| UseCompression           | optional | boolean | A feature flag for enabling compression on messages sent to the Cds endpoint                                                                                     |
| WebRequestTimeoutSeconds | optional | integer | A feature flag for changing how long it takes for a request to time out                                                                                          |

### EDS endpoint configuration

The format of the configuration for an EDS endpoint is shown below along with descriptions of each parameter. Replace all parameters with appropriate values.

```json
{
  "Selected": true,
  "EndpointType": "EDS",
  "Resource": "http://localhost:5590",
  "ApiVersion": "v1",
  "UseCompression": false
}
```

| Parameters               | Required | Type    | Description                                                                                                                                       |
| ------------------------ | -------- | ------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| Selected                 | required | boolean | Tells the application if the endpoint should be sent to                                                                                           |
| EndpointType             | required | string  | The endpoint type. For EDS this will always be "EDS"                                                                                              |
| Resource                 | required | string  | The endpoint for EDS if the namespace. If EDS is being run on your local machine with the default configuration, it will be http://localhost:5590 |
| ApiVersion               | required | string  | The API version of the EDS endpoint                                                                                                               |
| UseCompression           | optional | boolean | A feature flag for enabling compression on messages sent to the Cds endpoint                                                                      |
| WebRequestTimeoutSeconds | optional | integer | A feature flag for changing how long it takes for a request to time out                                                                           |

### PI endpoint configuration

The format of the configuration for a PI endpoint is shown below along with descriptions of each parameter. Replace all parameters with appropriate values.

```json
{
  "Selected": true,
  "EndpointType": "PI",
  "Resource": "PLACEHOLDER_REPLACE_WITH_PI_WEB_API_URL",
  "DataArchiveName": "PLACEHOLDER_REPLACE_WITH_DATA_ARCHIVE_NAME",
  "Username": "PLACEHOLDER_REPLACE_WITH_USERNAME",
  "Password": "PLACEHOLDER_REPLACE_WITH_PASSWORD",
  "VerifySSL": true,
  "UseCompression": false
}
```

| Parameters               | Required | Type           | Description                                                                                                                                                                                                                                                                             |
| ------------------------ | -------- | -------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Selected                 | required | boolean        | Tells the application if the endpoint should be sent to                                                                                                                                                                                                                                 |
| EndpointType             | required | string         | The endpoint type. For PI this will always be "PI"                                                                                                                                                                                                                                      |
| Resource                 | required | string         | The URL of the PI Web API                                                                                                                                                                                                                                                               |
| DataArchiveName          | required | string         | The name of the PI Data Archive that is being sent to                                                                                                                                                                                                                                   |
| Username                 | required | string         | The username that is being used for authenticating to the PI Web API                                                                                                                                                                                                                    |
| Password                 | required | string         | The password that is being used for authenticating to the PI Web API                                                                                                                                                                                                                    |
| VerifySSL                | optional | boolean/string | A feature flag for verifying SSL when connecting to the PI Web API. Alternatively, this can specify the path to a .pem certificate file if a self-signed certificate is being used by the PI Web API. By defualt this is set to true as it is strongly recommended that SSL be checked. |
| UseCompression           | optional | boolean        | A feature flag for enabling compression on messages sent to the PI endpoint                                                                                                                                                                                                            |
| WebRequestTimeoutSeconds | optional | integer        | A feature flag for changing how long it takes for a request to time out                                                                                                                                                                                                                 |

---

For the general steps or switch languages see the Task [ReadMe](https://github.com/AVEVA/AVEVA-Samples-OMF/blob/main/docs/OMF_BASIC.md)  
For the main OMF page [ReadMe](https://docs.aveva.com/bundle/omf/page/1283981.html)  
For the main landing page [ReadMe](https://github.com/aveva)
