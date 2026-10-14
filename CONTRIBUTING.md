# Contributing to NuciAPI.Client

This document covers the guidelines and processes for contributing to this project, including how to report issues, suggest enhancements, submit code changes, and follow project standards.

## 📑 Table of Contents

- [How to Contribute](#-how-to-contribute)
  - [Reporting Issues](#reporting-issues)
  - [Suggesting Enhancements](#suggesting-enhancements)
  - [Code Contributions](#code-contributions)
    - [Prerequisites](#prerequisites)
    - [Development Setup](#development-setup)
    - [Making Changes](#making-changes)
    - [Pull Request Guidelines](#pull-request-guidelines)
  - [Code Style](#code-style)
  - [Testing](#testing)
  - [Documentation](#documentation)
- [Code of Conduct](#-code-of-conduct)
- [Security](#-security)
- [License](#-license)

## 🤝 How to Contribute

### Reporting Issues

- Search existing issues first.
- Use the issue templates if available.
- Provide clear reproduction steps.
- Include environment details.

### Suggesting Enhancements

- Check the roadmap and existing discussions.
- Explain the use case and expected behavior.
- Consider implementation complexity.

### Code Contributions

#### Prerequisites

- .NET SDK 10.0 or newer

#### Development Setup

```bash
# Clone the repository
git clone https://github.com/hmlendea/nuciapi.client.git
cd nuciapi.client

# Install dependencies
dotnet restore
```

#### Making Changes

1. Fork the repository.
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Make your changes.
4. Run tests: `dotnet test --no-build --verbosity normal`
5. Run linters: `dotnet build --no-restore`
6. Commit with clear and descriptive messages.
7. Push to your fork.
8. Open a Pull Request.

#### Pull Request Guidelines

- Target the `master` branch.
- Keep PRs focused and atomic.
- Update documentation if applicable.
- Add tests for any new functionality.
- Ensure the CI checks pass.

### Code Style

Follow the project's coding standards:
- C# coding conventions as defined in the repository instructions
- Run `dotnet build --no-restore` before committing to verify style compliance

### Testing

```bash
# Run all tests
dotnet test --no-build --verbosity normal

# Run specific test suite
dotnet test NuciAPI.Client.UnitTests/NuciAPI.Client.UnitTests.csproj --no-build --verbosity normal
```

### Documentation

- Update relevant docs for changes.
- Follow the documentation style guide.
- Preview changes locally if possible.

## 📋 Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](https://www.contributor-covenant.org/version/2/1/code_of_conduct/). By participating, you are expected to uphold this code.

## 🔒 Security

For security issues, please refer to the [Security Policy](SECURITY.md).

## 📄 License

This project is licensed under the [GPL-3.0-or-later](LICENSE) license. By contributing, you agree that your contributions will be licensed under the same terms.