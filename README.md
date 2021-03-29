# Building a .NET sample to send OMF to PI or OCS

**Version**: 2.0.0

| OCS Test Status                                                                                                                                                                                                                                                                                                                                                    | EDS Test Status                                                                                                                                                                                                                                                                                                                                                        | PI Test Status                                                                                                                                                                                                                                                                                                                                                        |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [![Build Status](https://dev.azure.com/osieng/engineering/_apis/build/status/product-readiness/OMF/osisoft.sample-omf-basic_api-dotnet?repoName=osisoft%2Fsample-omf-basic_api-dotnet&branchName=main&jobName=Tests_OCS)](https://dev.azure.com/osieng/engineering/_build/latest?definitionId=2634&repoName=osisoft%2Fsample-omf-basic_api-dotnet&branchName=main) | [![Build Status](https://dev.azure.com/osieng/engineering/_apis/build/status/product-readiness/OMF/osisoft.sample-omf-basic_api-dotnet?repoName=osisoft%2Fsample-omf-basic_api-dotnet&branchName=main&jobName=Tests_EDS)](https://dev.azure.com/osieng/engineering/_build/latest?definitionId=2634&repoName=osisoft%2Fsample-omf-basic_api-dotnet&branchName=main) | [![Build Status](https://dev.azure.com/osieng/engineering/_apis/build/status/product-readiness/OMF/osisoft.sample-omf-basic_api-dotnet?repoName=osisoft%2Fsample-omf-basic_api-dotnet&branchName=main&jobName=Tests_OnPrem)](https://dev.azure.com/osieng/engineering/_build/latest?definitionId=2634&repoName=osisoft%2Fsample-omf-basic_api-dotnet&branchName=main) |

Developed against DotNet 5.0

## Building a sample with the rest calls directly

The sample does not makes use of the OSIsoft Cloud Services Client Libraries.

The sample also does not use any libraries for connecting to PI. Generally a library will be easier to use.

This sample also doesn't use any help to build the JSON strings for the OMF messages. This works for simple examples, and for set demos, but if building something more it may be easier to not form the JSON messages by hand.

[OMF documentation](https://omf-docs.osisoft.com/)

## To Run this Sample in Visual Studio:

1. Clone the GitHub repository
2. Open the solution file in Microsoft Visual Studio, [OMF_API.sln](OMF_API.sln)
3. Rename the file [appsettings.placeholder.json](OMF_API/appsettings.placeholder.json) to appsettings.json
4. Update appsettings.json with the credentials for the enpoint(s) you want to send to. See [Configure endpoints and authentication](#configure-endpoints-and-authentication) below for additional details
5. Click **Debug** > **Start Debugging** (or F5)

## To Test this Sample in Visual Studio:

1. Follow steps 1-4 from the section above
2. Click **Test** > **Run All Tests** (or Ctrl+R, A)

## Customizing the Application

This application can be customized to send your own custom types, containers, and data by modifying the [OMF-Types.json](OMF_API/OMF-Types.json), 
[OMF-Containers.json](OMF_API/OMF-Containers.json), and [OMF-Data.json](OMF_API/OMF-Data.json) files respectively. Each one of these files contains an array of OMF json objects, which are
created in the endpoints specified in [config.json](OMF_API/config-placeholder.json) when the application is run. For more information on forming OMF messages, please refer to our 
[OMF version 1.1 documentation](https://omf-docs.osisoft.com/documentation_v11/Whats_New.html).  
  
In addition to modifying the json files mentioned above, the get_data function in [program.py](OMF_API/program.py) should be updated to populate the OMF data messages specified in 
[OMF-Data.json](OMF_API/OMF-Data.json) with data from your data source.  
  
Finally, if there are any other activities that you would like to be running continuously, this logic can be added under the while loop in the main() function of 
[program.py](OMF_API/program.py).

## Configure Endpoints and Authentication

The sample is configured using the file [config-placeholder.json](OMF_API/config-placeholder.json). Before editing, rename this file to `config.json`. This repository's `.gitignore` rules should prevent the file from ever being checked in to any fork or branch, to ensure credentials are not compromised.

The application can be configured to send to any number of endpoints specified in the endpoints array within config.json. In addition, there are three types of endpoints: [OCS](#ocs-endpoint-configuration), [EDS](#eds-endpoint-configuration), and [PI](#pi-endpoint-configuration). Each of the 3 types of enpoints are configured differently and their configurations are explained in the sections below.

### OCS Endpoint Configuration
The format of the configuration for an OCS endpoint is shown below along with descriptions of each parameter. Replace all parameters with appropriate values.

```json
{
  "endpoint_type": "OCS",
  "resource": "https://dat-b.osisoft.com",
  "namespace_name": "REPLACE_WITH_NAMESPACE_NAME",
  "tenant": "RPLACE_WITH_TENANT_ID",
  "client_id": "REPLACE_WITH_APPLICATION_IDENTIFIER",
  "client_secret": "REPLACE_WITH_APPLICATION_SECRET",
  "api_version": "v1",
  "verify_ssl": true,
  "use_compression": false
}
```

| Parameters                  | Required | Type    | Description                                                                                                                                                      |
| --------------------------- | -------- | ------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| endpoint_type               | required | string  | The endpoint type. For OCS this will always be "OCS"                                                                                                             |
| resource                    | required | string  | The endpoint for OCS if the namespace. If the tenant/namespace is located in NA, it is https://dat-b.osisoft.com and if in EMEA, it is https://dat-d.osisoft.com |
| namespace_name              | required | string  | The name of the Namespace in OCS that is being sent to                                                                                                           |
| tenant                      | required | string  | The Tenant ID of the Tenant in OCS that is being sent to                                                                                                         |
| client_id                   | required | string  | The client ID that is being used for authenticating to OCS                                                                                                       |
| client_secret               | required | string  | The client secret that is being used for authenticating to OCS                                                                                                   |
| api_version                 | required | string  | The API version of the OCS endpoint                                                                                                                              |
| verify_ssl                  | optional | boolean | A feature flag for verifying SSL when connecting to the OCS endpoint. By defualt this is set to true as it is strongly recommended that SSL be checked           |
| use_compression             | optional | boolean | A feature flag for enabling compression on messages sent to the OCS endpoint                                                                                     |

### EDS Endpoint Configurations
The format of the configuration for an EDS endpoint is shown below along with descriptions of each parameter. Replace all parameters with appropriate values.

```json
{
  "endpoint_type": "EDS",
  "resource": "http://localhost:5590",
  "api_version": "v1",
  "use_compression": false
}
```

| Parameters                  | Required | Type    | Description                                                                                                                                       |
| --------------------------- | -------- | ------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| endpoint_type               | required | string  | The endpoint type. For EDS this will always be "EDS"                                                                                              |
| resource                    | required | string  | The endpoint for EDS if the namespace. If EDS is being run on your local machine with the default configuration, it will be http://localhost:5590 |
| api_version                 | required | string  | The API version of the EDS endpoint                                                                                                               |
| use_compression             | optional | boolean | A feature flag for enabling compression on messages sent to the OCS endpoint                                                                      |

### PI Endpoint Configuration
The format of the configuration for a PI endpoint is shown below along with descriptions of each parameter. Replace all parameters with appropriate values.

```json
{
  "endpoint_type": "PI",
  "resource": "REPLACE_WITH_PI_WEB_API_URL",
  "data_server_name": "REPLACE_WITH_DATA_ARCHIVE_NAME",
  "username": "REPLACE_WITH_USERNAME",
  "password": "REPLACE_WITH_PASSWORD",
  "verify_ssl": "REPLACE",
  "use_compression": false
}
```

| Parameters                  | Required | Type           | Description                                                                                                                                                                                                                                                                             |
| --------------------------- | -------- | -------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| endpoint_type               | required | string         | The endpoint type. For PI this will always be "PI"                                                                                                                                                                                                                                      |
| resource                    | required | string         | The URL of the PI Web API                                                                                                                                                                                                                                                               |
| data_server_name            | required | string         | The name of the PI Data Archive that is being sent to                                                                                                                                                                                                                                   |
| username                    | required | string         | The username that is being used for authenticating to the PI Web API                                                                                                                                                                                                                    |
| password                    | required | string         | The password that is being used for authenticating to the PI Web API                                                                                                                                                                                                                    |
| verify_ssl                  | optional | boolean        | A feature flag for verifying SSL when connecting to the PI Web API. Alternatively, this can specify the path to a .pem certificate file if a self-signed certificate is being used by the PI Web API. By defualt this is set to true as it is strongly recommended that SSL be checked. |
| use_compression             | optional | boolean/string | A feature flag for enabling compression on messages sent to the OCS endpoint                                                                                                                                                                                                            |

---

For the general steps or switch languages see the Task [ReadMe](https://github.com/osisoft/OSI-Samples-OMF/blob/main/docs/OMF_BASIC_README.md)  
For the main OMF page [ReadMe](https://github.com/osisoft/OSI-Samples-OMF)  
For the main landing page [ReadMe](https://github.com/osisoft/OSI-Samples)
