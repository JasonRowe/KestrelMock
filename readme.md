# Kestrel Mock

A .Net Core HTTP mock server.


## Example Use
```
var config = new ConfigurationBuilde().AddJsonFile("appsettings.json", optional: false).Build();

KestrelMock.Run(config);

```
## Install
```
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
            "BodyFromFilePath":".\\TestData\\body.txt"
         }
      }
   ]
}
```