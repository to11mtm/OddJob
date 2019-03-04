# Oddjob Manager Presenatation Layer (Websharper)

This Project is designed to be an example of how to build a Job Manager for Oddjob.

I've used Websharper here because:

  - I wanted to have an Easily Refactorable Presenatation Layer
     - Websharper lets you write HTML and Javascript in C#, allowing one to more easily utilize C# refactoring tools and compile analysis.

  - Low friction Remoting
     - This ties into the above, but Websharper makes it easy to call server-side code from client side, again with safe refactoring.

  - Simpler Packaging (theoretically, future)
     - Websharper facilitates the use of Functional code to build your websites; this should make it easier to package this as a configurable library to attach to a given website/implementatipn.



# TO DO:

This all runs in a fairly 'pure' Websharper Template project. We still want to:

 - Make it something that can be integrated into ASP.NET Core.

 - Make it more configurable, use things like ASP.NET DI.

 - Allow Auth Schemes.