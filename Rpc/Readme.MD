# RPC Plugins

RPC Plugins are Currently based on the amazing [MagicOnion](https://github.com/cysharp/magiconion). This gives us:
 - GRPC as a Transport
 - MessagePack as a format
 - Simple, Easy to read bindings
 - Minimal additional code/dependencies for load balanced (Multi-Rpc-Server) scenarios.
   - Multiple Backend processes on a single RPC server are supported OOTB.
   - Load balancing simply require a redis instance supporting pub-sub alongside whatever load balancer is used for the connections.
 - Built-in HTTP Gateway with Swagger.
 
Please look at the Integration tests and the Sample folder for usage; these samples should be all of the code you require to get up and running. More Full documentation will be added later.

 
Akka Remoting and SignalR were Considered, but due to the additional challenges of each (be it less natural C# bindings, forcing ASPNETCORE as a full requirement, or lack of built in connection recovery logic...) they were not chosen at this time.