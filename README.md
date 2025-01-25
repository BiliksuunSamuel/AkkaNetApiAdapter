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
    // You can provide the number of instances and upper bound for your actor if you want to register the actor with routers.
     services.AddActorSystem(c => builder.Configuration.GetSection(nameof(ActorConfig)).Bind(c),
      actorTypes: new[] { (typeof(CustomerActor)),(typeof(OrderActor),3,6) },
      subscriptions: new[] { (typeof(CustomerActor), typeof(Customer)) });
     
     /*
     For the OrderActor, we have specified the number of instances as 3 and the upper bound as 6.
     This means that the actor system will create 3 instances of the OrderActor and will not exceed 6 instances.
     
     The upper parameter in the DefaultResizer class represents
     the maximum number of routees (actor instances) that can be created.
     This value sets an upper limit on the number of actors that can be spawned to handle the workload.
     The resizer will not create more than this number of routees,
     even if the system is under high load
     */
    
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

OR 

### Using the actor system in your .Net Service Project, Eg. ***`Worker`*** `service`
```csharp
 
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddActorSystem(c => hostContext.Configuration.GetSection(nameof(ActorConfig)).Bind(c),
                actorTypes: new[] { typeof(CustomerActor) },
                subscriptions: new[] { (typeof(CustomerActor), typeof(Customer)) });
        })
        .Build();
    
    ResolveActorSystem(host);
    
    await host.RunAsync();
return;
 
static IHost ResolveActorSystem(IHost host)
{
    var actorSystem = host.Services.GetRequiredService<ActorSystem>();
    _ = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));
    return host;
}


```
<hr>

## Use Cases

### Define your actor class
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

### Broadcasting Events

The `BroadcastEventAsync` method allows you to broadcast a message to all subscribers of the event in the actor system. This is useful for scenarios where you need to notify multiple actors about an event.


```csharp
public class MyService
{
    private readonly IAkkaActorEventService _eventService;

    public MyService(IAkkaActorEventService eventService)
    {
        _eventService = eventService;
    }

    public async Task NotifyAllActorsAsync(MyEvent message)
    {
        bool success = await _eventService.BroadcastEventAsync(message);
        if (success)
        {
            // Event broadcasted successfully
        }
        else
        {
            // Handle the failure
        }
    }
}
```

### Sending Events to Specific Actors
The SendEventAsync method allows you to send a message to a specific actor. This is useful for scenarios where you need to communicate directly with a particular actor.
 ```csharp
public class MyService
{
    private readonly IAkkaActorEventService _eventService;

    public MyService(IAkkaActorEventService eventService)
    {
        _eventService = eventService;
    }

    public async Task SendMessageToActorAsync(MyMessage message)
    {
        bool success = await _eventService.SendEventAsync<MyActor, MyMessage>(message);
        if (success)
        {
            // Message sent successfully
        }
        else
        {
            // Handle the failure
        }
    }
}
```

### Getting a Response from an Actor

The `ActorAskAsync` method allows you to send a message to a specific actor and get a response. This is useful for scenarios where you need to query an actor for some information.

```csharp
public class MyService
{
    private readonly IAkkaActorEventService _eventService;

    public MyService(IAkkaActorEventService eventService)
    {
        _eventService = eventService;
    }

    public async Task<MyResponse?> QueryActorAsync(MyQuery message)
    {
        var response = await _eventService.ActorAskAsync<MyActor, MyQuery, MyResponse>(message);
        if (response != null)
        {
            // Process the response
        }
        else
        {
            // Handle the failure
        }
        return response;
    }
}
```


### Mocking and Testing
The abstraction provided by IAkkaActorEventService makes it easy to mock and test your business logic. By injecting this service into your classes, you can easily replace it with a mock implementation during unit testing, allowing you to verify that your business logic interacts with the actor system as expected without needing to spin up actual actors.

```csharp

public class MyServiceTests
{
    [Fact]
    public async Task SendMessageToActorAsync_ShouldSendMessage()
    {
        // Arrange
        var mockEventService = new Mock<IAkkaActorEventService>();
        mockEventService
            .Setup(service => service.SendEventAsync<MyActor, MyMessage>(It.IsAny<MyMessage>()))
            .ReturnsAsync(true);

        var service = new MyService(mockEventService.Object);
        var message = new MyMessage();

        // Act
        var result = await service.SendMessageToActorAsync(message);

        // Assert
        Assert.True(result);
        mockEventService.Verify(service => service.SendEventAsync<MyActor, MyMessage>(message), Times.Once);
    }
    
    [Fact]
    public async Task QueryActorAsync_ShouldReturnResponse()
    {
        // Arrange
        var mockEventService = new Mock<IAkkaActorEventService>();
        var expectedResponse = new MyResponse();
        mockEventService
            .Setup(service => service.ActorAskAsync<MyActor, MyQuery, MyResponse>(It.IsAny<MyQuery>()))
            .ReturnsAsync(expectedResponse);

        var service = new MyService(mockEventService.Object);
        var query = new MyQuery();

        // Act
        var result = await service.QueryActorAsync(query);

        // Assert
        Assert.Equal(expectedResponse, result);
        mockEventService.Verify(service => service.ActorAskAsync<MyActor, MyQuery, MyResponse>(query), Times.Once);
    }
}
```