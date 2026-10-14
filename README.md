[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding) [![Latest Release](https://img.shields.io/github/v/release/hmlendea/nuciapi.client)](https://github.com/hmlendea/nuciapi.client/releases/latest) [![Build Status](https://github.com/hmlendea/nuciapi.client/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/nuciapi.client/actions/workflows/dotnet.yml) [![NuGet](https://img.shields.io/nuget/v/NuciAPI.Client)](https://nuget.org/packages/NuciAPI.Client) [![License](https://img.shields.io/github/license/hmlendea/nuciapi.client)](https://github.com/hmlendea/nuciapi.client/blob/master/LICENSE)

# NuciAPI.Client

A lightweight .NET client library for calling NuciAPI-compatible REST endpoints.

## 📑 Table of Contents

- [Capabilities](#-capabilities)
- [Usage](#-usage)
- [Installation](#-installation)
- [Development](#️-development)
- [API Reference](#-api-reference)
- [Versioning](#-versioning)
- [Data Formats and Protocols](#-data-formats-and-protocols)
- [Contributing](#-contributing)
- [Security](#-security)
- [Project Engagement](#-project-engagement)
- [License](#-license)

## ✨ Capabilities

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

## 🚀 Usage

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

if (response is PingResponse pingResponse)
{
    // Handle success
    System.Console.WriteLine(pingResponse.Echo);
}
else if (response is NuciApiErrorResponse errorResponse)
{
    // Handle API error
    System.Console.WriteLine(errorResponse.Message);
}
```

### Authentication

Use `NuciApiRequestAuthorisationInfo` to attach auth details:

```csharp
NuciApiRequestAuthorisationInfo authorisation = new()
{
    ClientId = "my-client",
    BearerToken = "<jwt-token>",
    HmacSharedSecretKey = "<shared-secret>"
};

NuciApiResponse response = await client.SendRequestAsync<PingRequest, PingResponse>(
    HttpMethod.Post,
    new PingRequest { Message = "Hello" },
    authorisation,
    "/ping");
```

### Header behavior

- `ClientId` defaults to machine name when not provided
- `X-Request-ID` is generated per request (GUID)
- `X-Timestamp` uses ISO 8601 format (`DateTimeOffset.Now.ToString("o")`)

## 📦 Installation

[![Obtain it from NuGet](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/nuget.png)](https://nuget.org/packages/NuciAPI.Client)

### Package Manager Installation

```bash
dotnet add package NuciAPI.Client
```

Or, via the `Package Manager Console`:
```powershell
Install-Package NuciAPI.Client
```

## 🛠️ Development

### Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Setup

```bash
git clone https://github.com/hmlendea/nuciapi.client.git
cd nuciapi.client
dotnet restore
```

### Build

```bash
dotnet build
```

### Test

```bash
dotnet test --no-build --verbosity normal
```

## 🛡️ Privacy and Data

NuciAPI.Client does not collect, process, transmit, or persist user data, analytics, diagnostics, or telemetry.

For the detailed description of how the application handles privacy and personal data, see [PRIVACY.md](./PRIVACY.md).

## ⚙️ GitHub Actions

| Workflow | Purpose | What it does |
|----------|---------|--------------|
| [ dotnet.yml ](https://github.com/hmlendea/nuciapi.client/blob/master/.github/workflows/dotnet.yml) | Continuous integration | Restores dependencies, builds the solution, and runs tests on every push and pull request to `master` |

## 🗂️ Project Structure

### Projects and Packages

| Project | Type | Purpose |
|---------|------|---------|
| `NuciAPI.Client` | Library | Core NuciAPI client implementation |
| `NuciAPI.Client.UnitTests` | Test project | Unit tests for the client library |

### Dependencies

| Package | Version | Scope | Purpose |
|---------|---------|-------|---------|
| `NuciAPI` | 3.5.1 | Runtime | Core NuciAPI request/response types |
| `NuciExtensions` | 5.3.1 | Runtime | JSON serialization extensions |
| `NuciSecurity.HMAC` | 4.1.3 | Runtime | HMAC signature generation |
| `Microsoft.AspNetCore.WebUtilities` | 10.0.6 | Runtime | Query string helpers |

## 📚 API Reference

This library exposes the following public types:

| Type | Kind | Description |
|------|------|-------------|
| `INuciApiClient` | Interface | Core client contract |
| `NuciApiClient` | Class | Default implementation |
| `NuciApiRequestAuthorisationInfo` | Class | Authentication configuration |
| `NuciApiRequest` | Class | Base request type (from NuciAPI) |
| `NuciApiResponse` | Class | Base response type (from NuciAPI) |
| `NuciApiErrorResponse` | Class | Error response type (from NuciAPI) |

## 🔢 Versioning

This project follows [Semantic Versioning](https://semver.org/). The version is defined in `NuciAPI.Client.csproj` and published to NuGet.

## 📐 Data Formats and Protocols

- **Request serialization**: JSON for `POST`, `PUT`, `PATCH`; query string for `GET`
- **Response deserialization**: JSON with camelCase property mapping
- **Property naming**: `JsonPropertyName` attributes on request/response DTOs
- **Query parameters**: `FromQuery` attributes for GET request properties
- **Date/time**: ISO 8601 (`DateTimeOffset.ToString("o")`) for timestamps
- **Authentication**: Bearer token (`Authorization: Bearer`) and HMAC (`X-HMAC`)

## 🤝 Contributing

You are welcome to submit any suggestion, feedback, or modification to this project.

When doing so, please:
- Maintain cross-platform compatibility
- Preserve the existing public contract unless a breaking change is intentional
- Submit focused pull requests that conform to the existing code style
- Maintain your branch synchronised with `master`
- Revise the documentation when functionality changes
- Properly test all modifications, including edge cases and error conditions
- Add tests for additional or modified functionality
- Raise a new [issue](https://github.com/hmlendea/nuciapi.client/issues) for problems or suggestions

See the [contributing guidelines](./CONTRIBUTING.md) for details on how to report issues, suggest enhancements, and submit changes.

## 🔒 Security

NuciAPI.Client follows a coordinated disclosure policy for security vulnerabilities.

For information on reporting security vulnerabilities, see [SECURITY.md](./SECURITY.md).

## 💝 Project Engagement

Discovered a problem or have a suggestion? [Open an issue](https://github.com/hmlendea/nuciapi.client/issues)!

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding) or starring ⭐️ it on GitHub!

[![Donate](https://raw.githubusercontent.com/hmlendea/readme-assets/master/donate_generic.png)](https://hmlendea.go.ro/funding)

## 📄 License

This project is being distributed under the `GNU General Public License v3.0 or later`.
See [LICENSE](./LICENSE) for further information.