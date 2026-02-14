#The Job Command Aggregation Pattern

This project is a sample of a Command Aggregation Pattern.

The Problem: A common challenge in distributed applications (and microservices) is the idea of a transaction.

As a contrived example, consider when one completes checkout at an online retailer. One may expect something like the following to happen:

 - The customer's order history is updated
 - A notification is sent to the warehouse
 - An email is sent to the customer

Oftentimes (in my experience, in any case) this is handled by a single, tightly coupled method at the end of an operation, that has to either:

 - Have awareness of each service (To call them) or,
 - Use a message bus, sometimes working with a very raw json/xml format.

Oddjob, used in this example pattern, provides an in-between.

 - Commands are created as standard .NET objects, with any 'boundary unsafe' information contained in JSON format.
   - This means that they can pass service boundaries, with intermediates being completely unaware of the interfaces/types involved in executing a job.
 - Commands can be collected (aggregated) as a coordinator/aggregator queries multiple services involved in a request.
 - Each service may decide what side effects are relevant to a given command.
   - As noted above, the caller need not know anything about what command is added, although it has the capability to inspect the created .NET objects if desired.
 - Once all of the resulting commands are collected, they can be atomically written to the database.


 Example Notes:

  - This example uses multiple Job Table sets; 
    - You can use a single table set if you'd like. 
	- That said, Some DBs may perform better if you use separate tables for queues.
  - Folders:
    - The Actors folder is where most of the pattern magic happens.
      - Note that the Decorators are only aware of the interface they are requesting; implementation is hidden.
    - The Configuration folder has mappings and a Table configuration type.
      - You can do all sorts of interesting things here, if you're daring.
    - Helpers is a set of helper classes for Table creation and SQLite in-memory management.
      - ArrayHelper is also there, but you'll probably find a better pattern for what is done there.
    - JobDI has a SimpleInjector Implementation for `IContainerFactory`
    - Messages is a series of domain commands.
    - Service1 and Service2 are the service workers.
     - The Contract folders contain the interfaces.
     - SimpleInjector is used for the Job DI, because it's great.
  - Service1 and Service2 would normally each be a separate process.
  - While the Decorator Services are part of the same ecosystem as the Aggregator and Result writer, there is nothing stopping these from being parts of different subsystems, in different .NET processes.
