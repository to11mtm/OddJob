//Please do not remove this copyright notice on this file.
//This is the only file that should have this notice.
// See Notice.MD for full attribution
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
//-----------------------------------------------------------------------
// <copyright file="SynchronizationContextManager.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2019 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2019 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Runtime.CompilerServices;
using System.Threading;


internal static class SynchronizationContextManager
{
    public static ContextRemover RemoveContext { get; } = new ContextRemover();
}

internal class ContextRemover : INotifyCompletion
{
    public bool IsCompleted => SynchronizationContext.Current == null;

    public void OnCompleted(Action continuation)
    {
        var prevContext = SynchronizationContext.Current;

        try
        {
            SynchronizationContext.SetSynchronizationContext(null);
            continuation();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(prevContext);
        }
    }

    public ContextRemover GetAwaiter()
    {
        return this;
    }

    public void GetResult()
    {
        // empty on purpose
    }
}