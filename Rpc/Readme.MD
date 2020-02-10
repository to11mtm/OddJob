# RPC Plugins

RPC Plugins are Currently based on the amazing [MagicOnion](https://github.com/cysharp/magiconion). This gives us:
 - GRPC as a Transport
 - MessagePack as a format
 - Simple, Easy to read bindings
 - Minimal additional code/dependencies for load balanced scenarios.
   - Load balancing simply require a redis instance supporting pub-sub alongside whatever load balancer is used for the connections.
 - Built-in HTTP Gateway with Swagger.
 
Please look at the Integration tests and the Sample folder for usage.

 
Akka Remoting and SignalR were Considered, but due to the additional challenges of each (be it less natural C# bindings, forcing ASPNETCORE as a full requirement, or lack of built in connection recovery logic...) they were not chosen at this time.