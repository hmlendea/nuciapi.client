# Security Policy

This policy defines the security maintenance scope, vulnerability reporting process, and disclosure expectations for the NuciAPI.Client library.

## 📑 Table of Contents

- [Supported Versions](#-supported-versions)
- [Reporting a Vulnerability](#-reporting-a-vulnerability)
- [Scope](#-scope)
- [Disclosure Policy](#-disclosure-policy)

## 🛡️ Supported Versions

Use this table to indicate which project versions currently receive security maintenance.

| Version | Distribution Channel | Supported |
|---------|--------------------|-----------|
| Latest version | NuGet | ✅ |
| Preceding versions | Any distribution channel | ❌ |

## 🚨 Reporting a Vulnerability

Please do not disclose suspected vulnerabilities publicly before maintainers have had an opportunity to validate and remediate them.

To report a vulnerability:
- [GitHub Security Advisories](https://github.com/hmlendea/nuciapi.client/security/advisories)
- Contact the maintainers directly

## 📌 Scope

The subsequent report categories are in scope for this repository:
- Authentication and authorisation header handling (Bearer token, HMAC)
- Request/response serialisation and deserialisation
- Query string generation and parameter encoding
- Client metadata headers (X-Client-ID, X-Request-ID, X-Timestamp)
- HMAC signature generation and validation

The subsequent categories are out of scope unless explicitly stated to the contrary:
- Server-side NuciAPI implementation
- Third-party HTTP client behaviour
- Transport-layer security (TLS) configuration
- Consumer application secret management

## 📢 Disclosure Policy

This project follows coordinated disclosure:
1. Vulnerabilities are investigated privately.
2. A remediation plan is prepared and validated.
3. Public disclosure is published after a fix, mitigation, or agreed risk decision is available.
4. Credit is attributed in accordance with reporter preference and project policy.