# Freqently asked Questions

## How Fast is it?
  - Benchmark:
    - Hardware:
      - i7 8750H patched to 2020/02/29 for all microcode/spectre exploits
      - 32GB RAM
      - SQL Server LocalDB running against RAM Drive
    - Components:
      - One RPC Streaming Hub
      - 1 or 3 'Server(s)' with 100 JobWorkers each
      - 100,005 jobs
      - 5 'Clients' Submitting a simple Job
      - Simple Job writes Time in a 'Hello World' fashion
    - Results:
      - 1 JobServer, using TimedCache:
        - Consistently > 1000 Jobs/Sec, as high as >1300 jobs/sec
      - 3 JobServers, using LRUTimedCache: 
        - Consistently > 900 Jobs/Sec, typically 1000 Jobs/Sec

Multi-server deployments will be slower because each server 'competes' for the lock. There is code in place to minimize 'hard' contentions but the JobServer will still make the attempt. 

## Where do I get started? How do I use this?

 - A number of samples exist in the Repository.
   - GlutenFree.OddJob.SampleApp.Console
     - This is a very basic, minimal implementation that creates an in-memory jobstore and does a 'hello world' style job every few seconds until you quit.
   - GlutenFree.OddJob.Samples.Serialized.CommandAggregator
     - This project showcases the 'Command Aggreagator' pattern.
       - In short, ensuring that a set of commands is persisted as a group.
   - GlutenFree.OddJob.Rpc.AkkaSample
     - This shows off the cool toys. :) A Distributed Hub and Spoke pattern for your consumers if you want to use the system in a responsive fashion.


## Design Decisions
   
  - Why Newtonsoft?
    - It's a familiar API for most people, and can handle complex scenarios with minimal developer intervention.
    - Serialization is usually -not- the hangup in benchmarks.
    
  - Why Linq2Db?
    - EF and it's ilk don't let you do the kind of single pass updates we use for locking unless you add so much code you're barely using EF...
    - Dapper doesn't let us easily support a range of databases
  
  - Why Akka?
    - Easy management of thread concerns.
    - Reasoning about recovery is simple.
    - State management 
  
  - Why MagicOnion?
    - I mean, have you checked out how slick the bindings are?
    - Akka Remoting is a bit trickier to set up in a way that balances performance and recovery from connection errors. 
    - SignalR or direct GRPC may come later
      - This is low priority, since MagicOnion also buys us an HTTP gateway, SignalR doesn't.
  