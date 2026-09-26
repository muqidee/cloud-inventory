# Cloud Inventory

A cross-platform desktop browser for inspecting AWS infrastructure across multiple profiles, accounts and regions.

## Requirements

- .NET SDK 10
- Windows, macOS or Linux

## Build

```powershell
dotnet restore CloudInventory.slnx
dotnet build CloudInventory.slnx
```

## Run

```powershell
dotnet run --project src/CloudInventory.Desktop/CloudInventory.Desktop.csproj
```

## Test

```powershell
dotnet test CloudInventory.slnx
```

## Project structure

- `CloudInventory.Domain` contains infrastructure domain concepts.
- `CloudInventory.Application` contains use cases and provider abstractions.
- `CloudInventory.Infrastructure.Aws` contains AWS SDK integrations.
- `CloudInventory.Desktop` contains the Avalonia composition root and user interface.
