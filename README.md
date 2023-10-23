# SignalR.HubExtensions

An extension for ASP.NET Core SignalR that facilitates the automatic creation of a JavaScript (js) file, which implements the functions of a SignalR hub.

## Features
- Automatically generates a JavaScript (js) file containing connection and hub methods based on your defined SignalR hubs.
- Provides a simple method to connect and reconnect to hubs.
- Allows easy calling of server-side hub methods directly from JavaScript.
- Supports event handlers for hub callbacks using the `on_methodName` syntax.

## Installation

```bash
dotnet add package SignalR.HubExtensions
```

## Usage

To generate the JavaScript file for your hubs:

```csharp
// In your Startup.cs or where you configure SignalR
services.AddSignalR()
    .CreateJs("myGeneratedHubFile.js");
```

### Example

If you have a SignalR hub:

```csharp
public class ChatHub : Hub
{
    public Task SendMessage(string user, string message)
    {
        return Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
```

The generated JavaScript file will include:
- A method to start the hub connection.
- A method called `SendMessage` to call the server-side method.
- An event handler `on_ReceiveMessage` which you can implement to handle the callback.

## Limitations
- The auto-generated JavaScript file does not include the SignalR client library. Ensure that you've included the necessary SignalR JavaScript client library in your project.
