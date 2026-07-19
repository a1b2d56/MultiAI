# MultiAI — Multi-LLM Desktop Client

A sleek, native Windows desktop application built with **WinUI 3** and **.NET 8** that provides a unified interface for interacting with **Google Gemini**, **OpenAI**, and **Anthropic Claude**.

---

## Features

- **Multi-Provider Support**: Seamlessly switch between Google Gemini, OpenAI (GPT-4o, o1), and Anthropic Claude (Claude 3.7, 3.5 Sonnet & Haiku).
- **Real-Time Token Streaming**: Token-by-token streaming response rendering powered by asynchronous enumerables (`IAsyncEnumerable<string>`).
- **Clean MVVM Architecture**: Built with `CommunityToolkit.Mvvm` separating UI views from business logic and LLM provider implementations.
- **Local SQLite History**: Chat sessions and messages are stored locally at `%LOCALAPPDATA%\MultiAI\history.db`.
- **Automatic Smart Titling**: Automatically summarizes new conversations into clean 3-5 word titles using fast background requests.
- **Secure Key Vault**: API keys are securely stored in the Windows Credential Manager (`PasswordVault`).
- **Markdown & Code Highlighting**: Rich markdown text rendering with full code block support.
- **Automated Unit Testing & CI**: Includes xUnit test suite (`MultiAI.Tests`) and GitHub Actions build verification.

---

## Tech Stack

- **UI Framework**: WinUI 3 (Windows App SDK)
- **Runtime**: .NET 8.0 (`net8.0-windows10.0.19041.0`)
- **MVVM**: `CommunityToolkit.Mvvm` 8.2
- **Database**: SQLite (`sqlite-net-pcl`)
- **OpenAI SDK**: `OpenAI` 2.1
- **Testing**: xUnit 2.7

---

## Getting Started

### Prerequisites
- Windows 10 (version 1809 or higher) or Windows 11
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (with *.NET Desktop Development* and *Windows App SDK* workloads)

### Build & Run

```powershell
# Restore dependencies
dotnet restore MultiAI.slnx

# Build the solution
dotnet build MultiAI.slnx

# Run unit tests
dotnet test MultiAI.slnx
```

---

## License
MIT License
