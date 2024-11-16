# Task Management System
A .NET Core 8 API for task management system. Using EFCore, ServiceBus queue, and ServiceBus Topic/Subscription.
## Setup
```
dotnet new webapi --name TaskManagementSystem --output .
```

```
dotnet tool install --global dotnet-ef
```

## Assumptions
I assumed that task status should not be updated directly from API controller. Instead an action/command should be placed on the Service Bus Queue and processed by a background service. Once status is updated successfully an event is sent to Service Bus Topic.

## Issues
1. Task class collides with .net Task. Consider renaming in order to avoid confusion with System.Threading.Task(used extensively).

## Comments
1. Used `JsonConverter` in order to return/send Enum string instead of int value.
2. Used Task<IActionResult> as return type for GET endpoint in order to have better control of HTTP response type.
3. Used `ProducesResponseType` in order to get better response description in Swagger UI.
4. Used dotnet new gitignore
5. Added `MaxLength` attributes for Task props in order to avoid VARCHAR(MAX) in generated tabele.
6. Used xUnit as testing framework. Used FakeItEasy as mocking library.
7. Introduced Repository, Service, and corresponding interfaces.
8. Retry logic for subscription handled by ServiceBus automatic mechanisms. Message will be retried a couple of times before it goes to Dead Letter Queue.
9. Retry logic for sending messages to Queue/Topic implemented using exponentional backoff.
