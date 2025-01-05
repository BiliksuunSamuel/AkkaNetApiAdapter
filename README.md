# AkkaNetApiAdapter

**AkkaNetApiAdapter** is a lightweight and efficient SDK designed to simplify the integration of **Akka.NET** actor systems into .NET API applications. It provides a seamless way to incorporate the actor model, enabling the creation of scalable, fault-tolerant, and distributed systems with ease.

---

## Table of Contents
- [Features](#features)
- [Installation](#installation)
- [Getting Started](#getting-started)
    - [1. Register the Actor System](#1-register-the-actor-system)
    - [2. Create an Actor](#2-create-an-actor)
    - [3. Use the Actor in Your API Project](#3-use-the-actor-in-your-api)

---

## Features

- **Seamless Integration**: Provides tools for easily integrating Akka.NET with .NET APIs.
- **Dependency Injection Support**: Integrates with .NET Core's DI system for actor registration and management.
- **Scalability and Resilience**: Leverages Akka.NET's actor model for distributed and fault-tolerant systems.
- **Customizable Lifecycle Management**: Manage actor lifecycles, supervision strategies, and routing configurations.
- **Comprehensive Documentation and Examples**: Includes ready-to-use examples for faster onboarding.

---

## Installation

Install the NuGet package via the .NET CLI:
```bash
dotnet add package AkkaNetApiAdapter
```

<hr>

## Getting Started
In your Program.cs or StartUp.cs file, register the AkkaNetApiAdapter within the service container:
    
### 1. Register the Actor System
```csharp
  var builder = WebApplication.CreateBuilder(args);
{
    var services = builder.Services;
    var config = builder.Configuration;
    
    // Register the Actor System
    //To add your actors to the actor system, pass them as props to the AddActorSystem method
     services.AddActorSystem(c => config.GetSection(nameof(ActorConfig)).Bind(c), typeof(MyActor));
    
}
```
### 2. Create extension method for adding actor system to WebApplicationBuilder
```csharp
  public static class WebApplicationExtensions{
    public static void UseActorSystem(this WebApplication app)
    {
        var actorSys = app.Services.GetRequiredService<ActorSystem>();

        _ = actorSys ?? throw new ArgumentNullException(nameof(actorSys));
    }
  }
```
### 3. Register the Actor System in WebApplicationBuilder
```csharp
  var app = builder.Build();
{
    // Register the Actor System
    app.UseActorSystem();
}
```
<hr>

## Create an Actor
Define an actor class inheriting from BaseActor
 ```csharp
 public class MyActor:BaseActor{
     
      public MyActor(){
            ReceiveAsync<ProcessMessage>(DoProcessMessage);
        }
     //method to handle messages
     private async Task DoProcessMessage(ProcessMessage message)
     {
         //do something with the message
     }
 }
 ```

## Use the Actor in Your API Project
```csharp
   public async Task MyMethod(){
       var myMessage=new {};
       
       //Get the Actor by name and send the message
       TopLevelActors.GetActor<MyActor>(nameof(MyActor)).Tell(myMessage);
    }
```