# Kestrel Mock  

![Build status](https://github.com/JasonRowe/KestrelMock/workflows/.NET%20Core/badge.svg?branch=master)
[![Nuget version](https://img.shields.io/nuget/v/kestrelmock)](https://www.nuget.org/packages/kestrelmock)


A .Net Core HTTP mock server.

## Example Use

Create a simple dotnetcore aspnet project and in program.cs write just the following lines (Startup.cs is not required, but can be used to provide custom configuration, types are public)

```csharp
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();

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

### From Uri: Uri template (path only)

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
          "UriTemplate": "wines/{wine}/{color}",
          "BodyReplacements": {
            "year": "1987"
          },
          "UriPathReplacements": {
            "wine": "{wine}",
            "color": "{color}"
          }
        }
      }
}
```
