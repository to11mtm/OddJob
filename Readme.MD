Oddjob is a Library meant to provide Lightweight Job Workers in a distributed environment.

The Overall goal is to have a modular architecture for both Job Storage and Execution.

The Basic providers given are SQLServer or SQLite for Storage, and Akka.NET for Execution.

Features:

  - Scheduling of Jobs to run on Same or Different process

  - Configurability of Jobs (all optional):
    - Set a Future time for jobs to execute, seconds or months into the future.
	- Set Retry Parameters for Jobs:
	  - Number of retries
	  - Time to wait for Retry (fixed interval, no backoff yet)
  
  - "Sure, Poke if you want" SQL Storage:
    - Data is stored in Human-Readable Formatting and schema.
	- Table Creation Script Helpers provided for Sql Providers.
	  - Indexes not included. Talk to your DBA and/or read notes below!

  - Low Friction API:
    - API is meant to be easy to stand up, but also easy to *expand into* more advanced use scenarios.

  - Graceful Failures:
    - Execution System is meant to fail gracefully; Log as much or as little as you want for your scenario.

Goals:

  - Have a cleaner API surface and easier stand-up than the 'enterprisey' Schedulers

  - Be more useful in Distributed scenarios than the 'blog-hotness' schedulers
  
  - Allow Scheduling of jobs in the future (i.e. days ahead)


Non-Goals:

  - Precision for Scheduling. IOW Scheduling a job at 5:00PM only guarantees it is not executed before.
  
  - Handling of Methods more complex than an explicit method call
    - The API doesn't go as far as forcing Interfaces on your methods, because it trusts you to follow this rule.

  - Perfect Work Distribution
    - Backpressure algorithm will stop when there are more `numWorkers*2` pending jobs in a queue. You could theoretically wind up with up to `(numWorkers*3)-1` queued up before it stops fetching.
	 - Please note that this does NOT mean you will have more than `numWorkers` jobs executing; the jobs will be 'enqueued' and executed when a given worker is done with the last job they were working on.
    - Jobs are grabbed in batches and distributed in a round-robin fashion. 
	 - This means that in some very unlucky circumstances, you *could* wind up with a single worker getting saturated with Long-running jobs. Easy Solution? Just put those jobs in their own Queue.
	  

  - Fast File System Queues
    - The File System Storage exists for Testing purposes. The way it adds/updates the Queue is absolutely terrifyingly bad in every sense of the word from a performance perspective.
	- SQLite is probably a safer option for most scenarios!
	

Not-Initial-Goals (Will be done when there's a need):
 
 - Net Core Support
   - I don't think there's anything being done here that can't be done in core; But it's not at all set up for it, let alone validated.

 - Repeated Scheduling

 - Perfect API Surface
   - By this we are referring to the total API space. 
   - The public API is meant to be clean and lead towards the pit of success.
   - The Extensible parts of the API are supposed to be clean enough.
     - Shortcuts in cleanliness for the sake of extensibility are present. They will be removed when the need is there.
   - The Private API is subject to changes at any time, including undoing awkward surfaces.
   - The Public API is fairly stable at this point, although the Storage API **will** be cleaned up (see below.)
   -`DependencyInjectedJobExecutorShell` is messy
   	 - Please note that the `DependencyInjectedJobExecutorShell`, despite it's name, may not be the best choice for many scenarios.
	    - The way you register DI is clunky at this time
		- The Execution layer's registration doesn't play nicely with things like SimpleInjector's beautiful `Verify` method.
	 - Realistically, in most scenarios, The executor will be at or near-enough to the composition root that this is a moot point
	   - Unless you have a complex scenario, just use `HardInjectedJobExecutorShell` and get on with your life.


 - Robust Ability to run without DI
   - Jobs themselves essentially require DI to run.
     -To be clear; There is a `DefaultContainerFactory` that will work as long as your job type has a public, paramterless constructor. It will not work if your type has any constructor dependencies, or expects properties to be injected after the fact. This is used primarily for test scenarios as well as for if you are really DI averse.
	   - If you're in that camp, you'll want to use the `HardInjectedJobExecutorShell`. This shell will require you to provide creator functions for the `JobQueueLayerActor` and `JobWorkerActor`.

 - Minimal DB Round-Trips
   - Due to a semi-questionable design decision we have to:
     - Insert the Job Record in an 'Inserting' state (To prevent it being picked up before we are done)
	 - Insert each Parameter (And relevant Type Information)
	 - Insert each Generic Type Information parameter.
	 - Mark the Job record as 'New'
   - A different design option would be to Organize all parameter data (type included) into it's own serializable object.
     - This was avoided so that large parameter abuse would not cause too much overhead.
	 - Additionally, A large single Serialized object would be against the core API's 'human-readable' aspect.
 
 - Compact Generic Method Handling:
   - Generic method type info is stored in another table. We can't just guess based on parameter types, as there are no guarantees about generic/nongeneric parameter ordering.
   - This may change to a more compact type represenatation on the main table in the future.

 - Different Storage Options
   - I'd love to see this work with something like RabbitMQ.


Usage:

 - NetStandard/NetFramework Compatibility:
   - While Libraries are provided for both versions, compatibility between frameworks is not guaranteed in all cases. This is primarily due to the differences between the way types are serialized between versions.
   - We will try to write unit tests around compatibility if possible.

 - Queue Insertion:
   - Use an Implementation of `IJobQueueAdder`
    - If Using `SqlServerJobQueueAdder`, you'll have to register an `IJobQueueDbConnectionFactory` and use it alongside `SqlServerDataConnectionFactory`

 - Queue Processor:

   - The Queue Processor Consists of Four Components. 
        - `JobQueueCoordinator`
		  - A Coordinator that Handles the basic aspects of Fetching Jobs and sending them to the Workers.
		  - There are a variety of methods for handling results of Jobs and otehr Events, including:
		    - `OnJobQueueSaturated(DateTime saturationTime, int saturationMissedPulseCount, long queueLifeSaturationPulseCount)`
			  - This method will be called if the Queue is been at or above it's Saturation point (2*numberOfWorkers) on a given pulse
			- `OnJobRetry(JobFailed msg)`
			  - This method will be called when a job has been set to 'retried' based on the Queue's Logic. `JobFailed` will contain information about the Job's failure, i.e. parameters and the exception if you choose to log.
			- `OnJobFailed(JobFailed msg)`
			  - This method will be called when a job has been set to a 'failed' status. This status indicates the Queue processor will NOT pick it up again.
			- `OnJobSuccess(JobSuceeded msg)`
			  - This method will be called when a job has 'succeeded' (i.e. executed with no exceptions thrown.) The Queue processor will not pick up this job again.
			- `OnQueueTimeout(Exception ex)`
			  - Called when a Query to the Queue store has timed out. This will let you send alarms if needed, etc.
			- `OnJobTypeMissing(IOddJobWithMetadata job)`
			  - Called when a Job is missing type data. This may happen if you are reading from the incorrect queue. **If this is reached, The job has been marked as failed and will not be picked up for Queue processing.**
	    - `JobWorkerActor`
		  - A Job Worker. This Executes Jobs based on the metadata. If no execptions are thrown, the job is treated as a success.
		  - Currently this has no Extensino functionality, but will likely have such in the future.
		- `JobQueueLayerActor`
		  - A Semi-Asynchronous Handler for DB Connections.
		    - Queue Fetch is an Ask (effectively synchronous) operation
			- Queue Writes (failute, success, etc) is an asynchronus operation
			  - This is only a problem in a process crash. Normal Shutdown procedures will make a timeout-based best effort to let all queues (worker and otherwise) flush.
		  - This has a single overridable method:
		    - `OnQueueFailure(object failedCommand, Exception ex)`
			  - This will be called if the underlying Queue throws an exception.
		- `JobExecutorShell`
		  - A Container for handling the startup and shutdown of your Job Queues.
		    - This will likely be the composition root of a Queue Processing Application/service.
		  - `HardInjectedJobExecutorShell` and `DependencyInjectedJobExecutorShell` exist
		    - See below for notes.
        

   - If using 'HardInjectedJobExecutorShell' (Reccomended for most scenarios; lowest friction setup):
      - Implement an `IContainerFactory` if you have special initialization requirements or wish to use a DI container. Otherwise use `DefaultContainerFactory` which expectes a public parameterless constructor on any jobs.
	  - New-Up an instance of `HardInjectedJobExecutorShell`, providing functions to initialize the above interfaces as needed when newing up your components.
	    - If reaching into a DI container for these pieces, please note that these should be treated as non-singleton (i.e. transient) grabs from a container. This is the default for most DI Scenarios but is worth noting.
   
   - If using `DependencyInjectedJobExecutorShell` (Reccomended only for advanced scenarios):
      - Register the following types (Or a proper inheritance of) in your DI Container:
        - `JobWorkerActor`
        - `JobQueueCoordinator`
        - `JobQueueLayerActor`
      - Make sure you have registered Implementations of the following:
        - `IContainerFactory`
        - `IJobQueueManager`
	      - If Using `SqlServerJobQueueManager`, you'll also require registering `IJobQueueDbConnectionFactory` and use it alongside `SqlServerDataConnectionFactory`
		- 'IJobQueueExecutor'.
      - Register Dependencies for the jobs you expect the queue to process.
	  - Install the Akka.DI.%diFlavor% nuget package so that you can provide a Func<ActorSystem,IDependencyResolver> to the constructor
      - Create a JobExecutorShell
        - Make sure to hold on to a reference to this for the life of your application!
   - In Both Cases:
      - Start a queue with `StartJobQueue`
      - Stop queues with `StopJobQueue` when shutting down.

Missing Type handling:

  - The SQL Storage has to modes of behavior for Missing types:
	- First (If no `IStorageJobTypeResolver` is defined, or `NullOnMissingTypeJobTypeResolver` is used) will cause the job to have a null type; This will cause the Execution layer to mark the job in a failed state and fire the `OnJobTypeMissing` handler.
	- First (If `DefaltStorageJobTypeResolver` is used) will cause a failure on the storage layer.
	  - This can eventaully cause the queue to back up!

Extensibility:
  - Everything except the core Job Creator is an interface.
  - `JobQueueCoordinator` has Virtual methods for `OnJobSuccess`, `OnJobFailed`, `OnJobRetry`, and `OnQueueTimeout`
    - These methods are called after the Storage layer has been told to mark a given state change.
  - `JobQueueCoordinator` also has a virtual method for `OnQueueSaturation`
    - This method can be used to warn someone when a queue isn't working the way they expect.
  - `JobQueueLayerActor` has a Virtual Method for `OnQueueFailure`
    - This method can be used to warn about issues Reading from or Writing to the DB.


Unit Test Notes:
  - SqlServer Unit test uses LocalDB. 
    - Please be careful if using this against a non LocalDB instance, as the tests start by dropping and recreating the Queue schema under the default table names.
  - Shutdown Tests are touchy!
    - Because of the timers used for shutdown and the way it is tested, you may experience issues if running all of the unit tests in parallel.

SQL Common Notes:
  - The Common DB code is based on Linq2Db. This lets us support a variety of databases with minimal effort.
  - You will need to create your own Tables, using the Data types that 'make the most sense' for the data. 
    - The unit tests will be very handy here to make sure everything works as expected in your DB.
  - Please Note that Jobs are NOT automatically deleted.
	- `BaseSqlJobQueuePurger` exists for you to purge your stale jobs, if you choose to do so that way.

SQL Server Notes:
  - Table create scripts can be made by looking at `SqlServerDbJobTableHelper`.
    - Indexes are at the discretion of your DBA. I'd personally suggest columns `QueueName`, `JobGuid` (Unique on QueueTable, Non-Unique on QueueParamTable), `Status`, and `LockGuid` as minimums.
	  - Work with your DBA so they understand your load and can decide what indexes will be most useful to balance storage requirements and performance.

SQLite Notes:
  - Table create scripts can be made by looking at `SQLiteDbJobTableHelper`
  - All the normal SQLite rules (i.e. be careful with Network shared DB files) apply. 

Creating your own Modules:
  - Storage Modules benefit from the existence of `OddJob.Storage.BaseTests`
    - Your test harness can implement `StorageTests` and provide functions for the abstract properties `jobAddStoreFunc` and `jobMgrStoreFunc`
	  - Look at `OddJob.Storage.SqlServer.Tests` and `OddJob.Storage.FileSystem.Test` projects for example.
  - Adding support for any database supported by Linq2DB is trivial; look at the SQL Server and SQLite implementations to see how simple it can be!