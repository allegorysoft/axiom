---
title: Installation
description: How to install Axiom packages into your .NET project.
---

# Installation

Axiom is distributed as NuGet packages. Install only what you need.

## Packages

The main packages to build an Axiom application are:

| Package | Description |
|---|---|
| `Allegory.Axiom.DependencyInjection.Abstractions` | DI abstractions and service registration |
| `Allegory.Axiom.Hosting.Abstractions` | Core hosting and module lifecycle |

## Creating a console application

This guide walks you through building a minimal Axiom console application step by step. You will create a simple service that prints a message every second, wired together using Axiom's hosting and dependency injection infrastructure.

### 1. Create a new project

Requires .NET 10.0+.
```bash
dotnet new console -n MyAxiomApp
cd MyAxiomApp
```

### 2. Install packages

```bash
dotnet add package Allegory.Axiom.Hosting.Abstractions
dotnet add package Microsoft.Extensions.Hosting
```

### 3. Create your service

Services implementing `ISingletonService` are automatically registered no manual wiring needed. Create `Implementation.cs`:
```csharp
using Allegory.Axiom.DependencyInjection;

namespace MyAxiomApp;

public class Implementation : ISingletonService
{
    public async Task DoAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            Console.WriteLine("Hello, Axiom!");
            await Task.Delay(1000, cancellationToken);
        }
    }
}
```

### 4. Create a background worker

Create `WorkerService.cs`:

```csharp
using Microsoft.Extensions.Hosting;

namespace MyAxiomApp;

public class WorkerService(Implementation implementation) : BackgroundService
{
    public Implementation Implementation { get; } = implementation;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Implementation.DoAsync(stoppingToken);
    }
}
```

### 5. Create an application package

An application package is the entry point for registering your services. Create `MyAxiomAppPackage.cs`:

```csharp
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MyAxiomApp;

public class MyAxiomAppPackage : IConfigureApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddHostedService<WorkerService>();
        return ValueTask.CompletedTask;
    }
}
```

### 6. Configure the host

Update `Program.cs`:

```csharp
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
await builder.ConfigureApplicationAsync();
var host = builder.Build();
await host.InitializeApplicationAsync();
host.Run();
```

### 7. Run

```bash
dotnet run
```

You should see `Hello, Axiom!` printed every second.