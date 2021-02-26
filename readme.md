# Kestrel Mock  

![Build status](https://github.com/JasonRowe/KestrelMock/workflows/.NET%20Core/badge.svg?branch=master)
[![Nuget version](https://img.shields.io/nuget/v/kestrelmock)](https://www.nuget.org/packages/kestrelmock)


A .Net Core HTTP mock server.


## Example Nuget Reference Usage (RunAsync)

For direct use in a test project you can add the KestralMock nuget package and RunAsync. This will startup the webserver and return so your tests can use the mock API's configured in appsettings.json. See example appsetting.json below. By default, the mock endpoints will use http://localhost:60000.

```csharp
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

KestrelMock.RunAsync(config);

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

## Example appsettings.json

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

## DOCKER usage

you can just run kestrel mock default template configuration with

```bash
docker run -p 5006:5000 -e ASPNETCORE_URLS=http://*:5000 jasonrowe/kestrelmock
```

If you want you can create your own image, and then add a custom appsetting.json and responses folder

```bash
docker build --no-cache -t kestrelmock:latest -f .\KestrelMockServer\Dockerfile .

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
ENTRYPOINT ["dotnet", "KestrelMockServer.dll"]
```
