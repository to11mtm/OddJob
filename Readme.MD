Oddjob is a Library meant to provide Lightweight Job Workers in a distributed environment.

The Overall goal is to have a modular architecture for both Job Storage and Execution.

The Basic providers given are SQLServer for Storage, and Akka.NET for Execution.

Goals:

  - Have a cleaner API surface and easier stand-up than the 'enterprisey' Schedulers

  - Be more useful in Distributed scenarios than the 'blog-hotness' schedulers
  
  - Allow Scheduling of jobs in the future (i.e. days ahead)


Non-Goals:

  - Precision for Scheduling. IOW Scheduling a job at 5:00PM only guarantees it is not executed before.
  
  - Handling of Methods more complex than an explicit method call
    - The API doesn't go as far as forcing Interfaces on your methods, because it trusts you to follow this rule.

  - Perfect Work Distribution
    - Backpressure algorithm will stop when there are more `numWorkers*2` pending jobs in a queue. You could theoretically wind up with up to `(numWorkers*3)-1)` queued up before it stops fetching.
    - Jobs are grabbed in batches and distributed in a round-robin fashion. 
	- However, Coordinators do not keep track of individual worker queue sizes.

  - Fast File System Queues
    - The File System Storage exists for Testing purposes. The way it adds/updates the Queue is absolutely terrifyingly bad in every sense of the word.
	

Not-Initial-Goals (Will be done when there's a need):

 - Repeated Scheduling.

 - Perfect API Surface

 - Ability to run without DI
   - Jobs themselves essentially require DI to run.

 - Minimal DB Round-Trips
   - Due to a semi-questionable design decision we have to:
     - Insert the Job Record in an 'Inserting' state (To prevent it being picked up before we are done)
	 - Insert each Parameter (And relevant Type Information)
	 - Mark the Job record as 'New'
   - A different design option would be to Organize all parameter data (type included) into it's own serializable object.

 - Different Storage Options
   - I'd love to see this work with something like RabbitMQ.


Usage:

 - Queue Insertion:
   - Use an Implementation of `IJobQueueAdder`
    - If Using `SqlServerJobQueueAdder`, you'll also require an implementation of `IJobQueueDbConnectionFactory`

 - Queue Processor:
   - Register the following types in your DI Container:
     - `JobWorkerActor`
     - `JobQueueCoordinator`
     - `JobQueueLayerActor`
   - Make sure you have registered Implementations of the following:
     - `IContainerFactory`
     - `IJobQueueManager`
	   - If Using `SqlServerJobQueueManager`, you'll also require registering an implementation of `IJobQueueDbConnectionFactory`
   - Register Dependencies for the jobs you expect the queue to process.
   - Create a JobExecutorShell
     - Make sure to hold on to a reference to this for the life of your application!
   - Start a queue with `JobExecutorShell.StartJobQueue`
   - Stop queues with `JobExecutorShell.StopJobQueue` when shutting down.

Extensibility:
  - Everything except the core Job Creator is an interface.
  - `JobQueueCoordinator` has Virtual methods for `OnJobSuccess`, `OnJobFailed` and `OnJobRetry`
    - These methods are called after the Storage layer has been told to mark a given state change.
