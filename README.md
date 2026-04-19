[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/fund.html) [![Latest Release](https://img.shields.io/github/v/release/hmlendea/nuciapi.client)](https://github.com/hmlendea/nuciapi.client/releases/latest) [![Build Status](https://github.com/hmlendea/nuciapi.client/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/nuciapi.client/actions/workflows/dotnet.yml)

# NuciAPI.Client

A lightweight .NET client library for calling NuciAPI-compatible REST endpoints.

## Features

- Strongly-typed request/response flow based on `NuciApiRequest` and `NuciApiResponse`
- Supports `GET`, `POST`, `PUT`, and `PATCH` style payload handling
- Automatic query-string generation for non-body requests
- Built-in request metadata headers:
  - `X-Client-ID`
  - `X-Request-ID`
  - `X-Timestamp`
- Optional authentication:
  - Bearer token (`Authorization: Bearer ...`)
  - HMAC header (`X-HMAC`)

## Installation

[![Get it from NuGet](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/nuget.png)](https://nuget.org/packages/NuciAPI.Client)

### .NET CLI

```bash
dotnet add package NuciAPI.Client
```

### Package Manager Console

```powershell
Install-Package NuciAPI.Client
```

## Requirements

- .NET (`net10.0`)

## Quick Start

```csharp
using System.Net.Http;
using NuciAPI.Client;
using NuciAPI.Requests;
using NuciAPI.Responses;

// Your request/response types should inherit from NuciApiRequest / NuciApiResponse
public class PingRequest : NuciApiRequest
{
	public string Message { get; set; }
}

public class PingResponse : NuciApiResponse
{
	public string Echo { get; set; }
}

INuciApiClient client = new NuciApiClient("https://api.example.com");

NuciApiResponse response = await client.SendRequestAsync<PingRequest, PingResponse>(
	HttpMethod.Post,
	new PingRequest { Message = "Hello" },
	"/ping");

if (response is PingResponse ok)
{
	// Handle success
	System.Console.WriteLine(ok.Echo);
}
else if (response is NuciApiErrorResponse err)
{
	// Handle API error
	System.Console.WriteLine(err.Message);
}
```

## Authentication

Use `NuciApiRequestAuthorisationInfo` to attach auth details:

```csharp
var auth = new NuciApiRequestAuthorisationInfo
{
	ClientId = "my-client",
	BearerToken = "<jwt-token>",
	HmacSharedSecretKey = "<shared-secret>"
};

NuciApiResponse response = await client.SendRequestAsync<PingRequest, PingResponse>(
	HttpMethod.Post,
	new PingRequest { Message = "Hello" },
	auth,
	"/ping");
```

### Header behavior

- `ClientId` defaults to machine name when not provided
- `X-Request-ID` is generated per request (GUID)
- `X-Timestamp` uses ISO 8601 format (`DateTimeOffset.Now.ToString("o")`)

## Request Serialization Rules

- For `POST`, `PUT`, `PATCH`: request object is serialized to JSON body
- For other methods (for example `GET`): request object is converted to query-string parameters

Query key resolution priority:

1. `[FromQuery(Name = "...")]`
2. `[JsonPropertyName("...")]`
3. camelCase property name fallback

Nested objects are flattened using dot notation.

## Error Handling

- On successful HTTP status: response body is deserialized to `TResponse`
- On non-success HTTP status:
  - If body is present: deserialized to `NuciApiErrorResponse`
  - If body is empty: a default `NuciApiErrorResponse` is generated from status code

## API Overview

`INuciApiClient` exposes:

- `BaseUrl`
- `SendRequestAsync<TRequest, TResponse>(HttpMethod, TRequest, string)`
- `SendRequestAsync<TRequest, TResponse>(HttpMethod, TRequest, NuciApiRequestAuthorisationInfo, string)`

## Development

### Build

```bash
dotnet restore
dotnet build
```

### Test

There is currently no test project in this repository.

If you add one later, run:

```bash
dotnet test
```

## Contributing

Contributions are welcome.

When contributing:

- keep the project cross-platform
- preserve the existing public API unless a breaking change is intentional
- keep changes focused and consistent with the current coding style
- update documentation when behavior changes
- include tests for new behavior when a test project is available

## License

Licensed under the GNU General Public License v3.0 or later.
See [LICENSE](./LICENSE) for details.