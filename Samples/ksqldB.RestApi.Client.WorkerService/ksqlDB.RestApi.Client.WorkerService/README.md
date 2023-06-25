### Hosted services in .NET

In .NET, the `IHostedService` interface is a part of the `Microsoft.Extensions.Hosting` namespace and is used to define a **background task** or a **long-running service** that runs within the application's host.
It provides a standardized way to implement and manage background tasks in a .NET application.

By implementing the `IHostedService` interface, a class can be registered as a hosted service within the application's dependency injection container.
The host will manage the lifecycle of the hosted service and ensure that the `StartAsync` method is called when the host starts and the `StopAsync` method is called when the host is shutting down.

### Containerization

**Containerization** refers to the process of encapsulating an application, along with its dependencies and configuration, into a self-contained unit called a **container**.
Containers provide a lightweight and isolated environment that allows the application to run consistently across different computing environments.

In containerization, the application and its dependencies are bundled together in a **container image**, which contains the necessary files, libraries, and configurations.
This image can be easily distributed, deployed, and run on any system that supports the containerization platform, such as [Docker](https://www.docker.com/).

Containerizing the app using the specified **Dockerfile** with the .NET runtime and SDK images offers benefits such as isolation, portability, simplified deployment, scalability, version control, and consistent development environments.

