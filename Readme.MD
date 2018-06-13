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


Not-Initial-Goals (Will be done when there's a need):

 - Repeated Scheduling.

 - Perfect API Surface