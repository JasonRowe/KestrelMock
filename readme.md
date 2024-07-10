# Kestrel Mock  
A .NET HTTP mock server.

![badge](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/JasonRowe/fe23c15603763c6eb09eea7bd38ba23f/raw/code-coverage.json)
[![Unit Tests](https://github.com/JasonRowe/KestrelMock/actions/workflows/dotnetcore.yml/badge.svg)](https://github.com/JasonRowe/KestrelMock/actions/workflows/dotnetcore.yml)
[![docker](https://github.com/JasonRowe/KestrelMock/actions/workflows/docker.yml/badge.svg)](https://github.com/JasonRowe/KestrelMock/actions/workflows/docker.yml)
[![Nuget version](https://img.shields.io/nuget/v/kestrelmock)](https://www.nuget.org/packages/kestrelmock)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE.md)


## Example Nuget Reference Usage (RunAsync)

For direct use in a test project you can add the KestralMock nuget package and RunAsync. This will startup the webserver and return so your tests can use the mock API's configured in appsettings.json. See example appsetting.json below. By default, the mock endpoints will use http://localhost:60000.

```csharp
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

KestrelMock.RunAsync(config);

```

## Example Nuget Reference Usage (CreateWebHostBuilder)

For direct use in a test project you can add the KestralMock nuget package and call CreateWebHostBuilder. CreateWebHostBuilder will return the web host so you can controll the start and stop of the mock server.

```csharp
webHost = KestrelMock.CreateWebHostBuilder(new string[] { YourUrl }, YourConfigurationRoot).Build();
webHost.Start();
```


## Example Server Usage (Run)
Server will run and not return until the process shuts down. See KestrelMockServer project as an example.

```csharp
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

KestrelMock.Run(config);

```


## Install

```cli
dotnet add package KestrelMock
```

## Example Mocks setup via appsettings.json

```json
{
   "MockSettings":[
      {
         "Request":{
            "Methods":[
               "GET"
            ],
            "PathStartsWith":"/starts/with"
         },
         "Response":{
            "Status":200,
            "Headers":[
               {
                  "Content-Type":"application/json"
               }
            ],
            "Body":"{\"banana_x\": 8000}"
         }
      },
      {
         "Request": {
            "Methods": [ "GET" ],
            "PathMatchesRegex": ".+\\d{4}.+"
         },
         "Response": {
            "Status": 200,
            "Headers": [
               {
                  "Content-Type": "application/json"
               }
            ],
            "Body": "{\"banana_x\": 8000}"
         }
      },
      {
         "Request":{
            "Methods":[
               "POST",
               "GET"
            ],
            "Path":"/hello/world"
         },
         "Response":{
            "Status":200,
            "Headers":[
               {
                  "Content-Type":"application/json"
               }
            ],
            "Body":"{\"hello\": \"world\"}"
         }
      },
      {
         "Request":{
            "Methods":[
               "POST"
            ],
            "Path":"/api/estimate",
            "BodyContains":"00000"
         },
         "Response":{
            "Status":200,
            "Headers":[
               {
                  "Content-Type":"application/json"
               }
            ],
            "Body":"BodyContains Works!"
         }
      },
      {
         "Request":{
            "Methods":[
               "POST"
            ],
            "Path":"/api/estimate",
            "BodyDoesNotContain":"00000"
         },
         "Response":{
            "Status":200,
            "Headers":[
               {
                  "Content-Type":"application/json"
               }
            ],
            "Body":"BodyDoesNotContain works!!"
         }
      },
      {
         "Request":{
            "Methods":[
               "POST",
               "GET"
            ],
            "Path":"/api/fromfile"
         },
         "Response":{
            "Status":200,
            "Headers":[
               {
                  "Content-Type":"application/json"
               }
            ],
            "BodyFromFilePath":"./TestData/body.txt"
         }
      }
   ]
}
```
## Example Mocks setup via admin endpoint

admin endpoint '/kestrelmock/mocks'

GET - returns list of mock settings objects.

POST - body is expected to be HttpMockSetting and adds new mock

DELETE - '/kestrelmock/mocks/YOURID' will delete by HttpMockSetting Id.

## Watch requests using observe endpoint

observe endpoint '/kestrelmock/observe/[Watch id]'

When adding the "Watch" object to your mock settings, an observe endpoint will be created to retrieve request data. The watch id can be added directly or retrieved via response of mock creation.

```
### Example post request to create mock with "Watch" object

POST https://localhost:44391/kestrelmock/mocks
Content-Type: application/json

{
   "Request": {
      "Methods": [ "PUT" ],
      "PathStartsWith": "/api/supplier"
   },
   "Response": {
      "Status": 200,
      "Headers": [
         {
         "Content-Type": "application/json"
         }
      ],
      "Body": "{\"hello\":\"test\"}",
      "BodyFromFilePath":null,
      "Replace": null
   },
   "Watch": {
      "RequestLogLimit": 10,
      "Id": "c3f57f2c-b989-46eb-93a3-247a6caebe6d"
   }
}
```

Example of mock creation response from request above
```
{"Message":"Dynamic mock added with observability, call /kestrelmock/observe/c3f57f2c-b989-46eb-93a3-247a6caebe6d","Watch":{"Id":"c3f57f2c-b989-46eb-93a3-247a6caebe6d","RequestLogLimit":10}}
```

If you send the following request to a mocked endpoint which has a watch.
```
### Send request to mocked endpoint with body data to test

PUT https://localhost:44391/api/supplier
Content-Type: application/json

{
   "Test" : "foo"
}
```

Then you can call the observe endpoint to see that request body from previous requests up to the limit. The request to the observe endpoint will clear all current data from the watch queue.
```
### See your requests using observe endpoint

GET https://localhost:44391/kestrelmock/observe/c3f57f2c-b989-46eb-93a3-247a6caebe6d
Content-Type: application/json
```

The observe endpoint will give you back the body. Example respons:

```
[{"Path":"/api/supplier","Body":"{\r\n   \"Test\" : \"foo\"\r\n}","Method":"PUT"}]
```


## Dynamic Mock

Some advanced dynamic mocking capabilities are provided for Json body data responses

### Simple body replace

```json
{
   "Request": {
      "Methods": [ "GET" ],
      "PathStartsWith": "/api/persons/carl"
   },
   "Response": {
      "Status": 200,
      "Headers": [
         {
         "Content-Type": "application/json"
         }
      ],
      "Body": "./data/person.json",
      "Replace": {
         "BodyReplacements": {
         "year": "1987",
         "name" : "carl"
         }
      }
   }
}
```

### From Uri : Regex

```json
{
   "Request": {
      "Methods": [ "GET" ],
      "PathStartsWith": "/api/cars"
   },
   "Response": {
      "Status": 200,
      "Headers": [
         {
         "Content-Type": "application/json"
         }
      ],
      "Body": "./my/generic/response.json",
      "Replace": {
          "RegexUriReplacements": {
            "car": "cars/([\\w\\d]+)/.+",
            "color": "/([\\w\\d]+)$"
          }
        }
   }
}
```

### From Uri: Uri template

UriPathReplacements is in the format bodyValue:uriValue
   
```json
{
      "Request": {
        "Methods": [ "GET" ],
        "PathStartsWith": "/api/wines"
      },
      "Response": {
        "Status": 200,
        "Headers": [
          {
            "Content-Type": "application/json"
          }
        ],
        "Body": "{\"wine\":\"W\",\"color\":\"C\",\"year\":\"Y\"}",
        "Replace": {
          "UriTemplate": "wines/{wine}/{color}?year={year}",
          "BodyReplacements": {
            "year": "1987"
          },
          "UriPathReplacements": {
            "wine": "{wine}",
            "color": "{color}",
            "year":"{year}"
          }
        }
      }
}
```

## DOCKER

you can just run kestrel mock default template configuration with

```bash
docker run -p 5006:5000 -e ASPNETCORE_URLS=http://*:5000 jasonrowe/kestrelmock
```

If you want you can create your own image, and then add a custom appsetting.json and responses folder

```bash
docker build --no-cache -t kestrelmock:latest -f .\KestrelMockServerInstance\Dockerfile .

docker run -it --rm -p 5000:80 --name myapp kestrelmock:latest
```

Keep in mind that within the container you can modify the behaviour changing: 
**/app/appsettings.json**
and 
**/app/responses** .  
Via docker cp command on a local container you can apply custom settings and mock response files  

```bash
cd mySettingsFolder

docker cp appsettings.json myapp:/app/appsettings.json

docker cp .\resp\ciao.json myapp:/app/responses/ciao.json
```

If you prefer build your custom image in CI/CD pipelines, you can append this to the docker file.
Else just push to a private image registry, and use that as a starting image

```dockerfile
FROM jasonrowe/kestrelmock as KestrelMockServerBase
WORKDIR /app
COPY ["responses","responses"]
COPY ["appsettings.json", "appsettings.json"]
ENTRYPOINT ["dotnet", "KestrelMockServerInstance.dll"]
```
