# Kestrel Mock  
A .NET HTTP mock server for testing.

![badge](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/JasonRowe/fe23c15603763c6eb09eea7bd38ba23f/raw/code-coverage.json)
[![Unit Tests](https://github.com/JasonRowe/KestrelMock/actions/workflows/dotnetcore.yml/badge.svg)](https://github.com/JasonRowe/KestrelMock/actions/workflows/dotnetcore.yml)
[![docker](https://github.com/JasonRowe/KestrelMock/actions/workflows/docker.yml/badge.svg)](https://github.com/JasonRowe/KestrelMock/actions/workflows/docker.yml)
[![Nuget version](https://img.shields.io/nuget/v/kestrelmock)](https://www.nuget.org/packages/kestrelmock)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE.md)

## Overview

KestrelMock provides a simple way to mock HTTP APIs for testing purposes. It allows you to simulate server responses for various HTTP methods, paths, and headers. 

The library has evolved to prefer managing mocks via **Admin Endpoints** and **Observable Mocks** instead of relying on `appsettings.json` configuration, making it easier to manage and interact with mock settings dynamically.

---

## Install

To get started with KestrelMock, add the NuGet package to your project:

```
dotnet add package KestrelMock
```

---

## Getting Started

You can use KestrelMock in one of two primary ways:

1. **Using Admin Endpoints & Observable Mocks (Recommended)**
2. **Legacy Configuration via `appsettings.json`**

### 1. Using Admin Endpoints & Observable Mocks (Recommended)

The preferred way to use KestrelMock is via its Admin API for dynamic mock creation and observation. This allows for better flexibility, and the ability to monitor requests using the **observe endpoint**.

#### Starting KestrelMock

To start the KestrelMock server, the most flexible way is to use the following code. The configuration passed in is not validated and the web host is returned so you can controll the start and stop of the mock server.

```
KestrelMock.CreateWebHostBuilder(new string[] { 'http://localhost:60000' }, OptionalConfiguration);
```

This starts the server, which you can then interact with using HTTP requests. The server will be accessible on `http://localhost:60000`.

#### Create Mocks via Admin Endpoint

You can create, update, and delete mock settings via the Admin endpoint.

- **POST** `/kestrelmock/mocks` - Create a new mock
- **GET** `/kestrelmock/mocks` - List all mock settings
- **DELETE** `/kestrelmock/mocks/{id}` - Delete a mock by its ID

**Example POST request to create a mock:**

```
POST /kestrelmock/mocks
Content-Type: application/json

{
   "Request": {
      "Methods": [ "PUT" ],
      "PathStartsWith": "/api/supplier"
   },
   "Response": {
      "Status": 200,
      "Headers": [
         { "Content-Type": "application/json" }
      ],
      "Body": "{\"hello\":\"test\"}"
   },
   "Watch": {
      "RequestLogLimit": 10,
      "Id": "c3f57f2c-b989-46eb-93a3-247a6caebe6d"
   }
}
```

After creating a mock, the server will respond with a **watch ID** that can be used to track requests made to that mock.

---

#### Watch Requests using the Observe Endpoint

When you create a mock with a **Watch** object, an "observe" endpoint is automatically available to track requests made to that mock.

**Example of observing request data:**

- **GET** `/kestrelmock/observe/{watchId}` - Retrieves the request data for the given watch ID.

**Example request to view data from the observe endpoint:**

```
GET https://localhost:60000/kestrelmock/observe/c3f57f2c-b989-46eb-93a3-247a6caebe6d
```

This will return the details of requests (up to the configured log limit).

**Example response:**

```
[
   {
      "Path": "/api/supplier",
      "Body": "{\"Test\": \"foo\"}",
      "Method": "PUT"
   }
]
```

---

### 2. Legacy Configuration via `appsettings.json`

You can still use the legacy configuration method through `appsettings.json` if preferred, though it is no longer the recommended approach. This allows you to pre-define mock behavior before starting the server.

#### Example Setup in `appsettings.json`

```
{
   "MockSettings": [
      {
         "Request": {
            "Methods": [ "GET" ],
            "PathStartsWith": "/starts/with"
         },
         "Response": {
            "Status": 200,
            "Headers": [
               { "Content-Type": "application/json" }
            ],
            "Body": "{\"banana_x\": 8000}"
         }
      },
      {
         "Request": {
            "Methods": [ "POST" ],
            "Path": "/api/estimate",
            "BodyContains": "00000"
         },
         "Response": {
            "Status": 200,
            "Headers": [
               { "Content-Type": "application/json" }
            ],
            "Body": "BodyContains Works!"
         }
      }
   ]
}
```

#### Start the Mock Server

```
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

KestrelMock.RunAsync(config);
```

---

## Dynamic Mocking

KestrelMock supports advanced dynamic mocking, such as replacing parts of the response body based on request data or URI patterns.

### Example: Simple Body Replacement

```
{
   "Request": {
      "Methods": [ "GET" ],
      "PathStartsWith": "/api/persons/carl"
   },
   "Response": {
      "Status": 200,
      "Headers": [
         { "Content-Type": "application/json" }
      ],
      "Body": "./data/person.json",
      "Replace": {
         "BodyReplacements": {
            "year": "1987",
            "name": "carl"
         }
      }
   }
}
```

### Example: URI-based Replacement

```
{
   "Request": {
      "Methods": [ "GET" ],
      "PathStartsWith": "/api/cars"
   },
   "Response": {
      "Status": 200,
      "Headers": [
         { "Content-Type": "application/json" }
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
## Available Mock Properties

When defining mocks, you can specify various properties in the `Request` object to match specific HTTP request patterns. This allows for more flexibility in simulating different scenarios for testing. Below are the available properties you can use in the `Request` object and how they work:

### 1. `Path`

- **Description**: Matches the exact path of the request.
- **Use Case**: When you want to match a specific request path exactly.
- **Example**:

```
{
   "Request": {
      "Path": "/api/supplier"
   },
   "Response": {
      "Status": 200,
      "Body": "{\"message\": \"Success\"}"
   }
}
```

This will match any request made to `/api/supplier`.

---

### 2. `PathStartsWith`

- **Description**: Matches the path of the request if it starts with the specified string.
- **Use Case**: When you want to match paths that share a common prefix (e.g., `/api/products` and `/api/products/{id}`).
- **Example**:

```
{
   "Request": {
      "PathStartsWith": "/api/products"
   },
   "Response": {
      "Status": 200,
      "Body": "{\"message\": \"Product List\"}"
   }
}
```

This will match any path that starts with `/api/products`, such as `/api/products` or `/api/products/123`.

---

### 3. `BodyContains`

- **Description**: Matches requests where the body contains the specified substring.
- **Use Case**: When you want to mock a response for requests with specific data in the body.
- **Example**:

```
{
   "Request": {
      "BodyContains": "customer_id"
   },
   "Response": {
      "Status": 200,
      "Body": "{\"message\": \"Customer found\"}"
   }
}
```

This will match requests where the body contains `"customer_id"`.

---

### 4. `BodyContainsArray`

- **Description**: Matches requests where the body contains all of the substrings in the array.
- **Use Case**: When you want to match requests that contain several substrings in the body.
- **Example**:

```
{
   "Request": {
      "BodyContainsArray": ["order_id", "customer_id"]
   },
   "Response": {
      "Status": 200,
      "Body": "{\"message\": \"Order processed\"}"
   }
}
```

This will match requests where the body contains both `"order_id"` and `"customer_id"`.

---

### 5. `BodyDoesNotContain`

- **Description**: Matches requests where the body does **not** contain the specified substring.
- **Use Case**: When you want to exclude requests that contain certain data in the body.
- **Example**:

```
{
   "Request": {
      "BodyDoesNotContain": "error"
   },
   "Response": {
      "Status": 200,
      "Body": "{\"message\": \"Request processed successfully\"}"
   }
}
```

This will match requests where the body does **not** contain `"error"`.

---

## Docker Support

KestrelMock can be easily run via Docker. You can use the default template configuration or build your custom image.

### Run Default Configuration:

```
docker run -p 5006:5000 -e ASPNETCORE_URLS=http://*:5000 jasonrowe/kestrelmock
```

### Build and Run Custom Image:

```
docker build --no-cache -t kestrelmock:latest -f .\KestrelMockServerInstance\Dockerfile .
docker run -it --rm -p 5000:80 --name myapp kestrelmock:latest
```

---
