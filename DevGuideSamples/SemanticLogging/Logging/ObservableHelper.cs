//===============================================================================
// Microsoft patterns & practices
// Enterprise Library 6 Samples
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Reactive.Linq;

namespace SemanticLoggingExample
{
  // Example of how you can leverage the power of Rx to perform some filtering (or transformation) 
  // of the event stream before it is sent to the underlying sink.
  public static class ObservableHelper
  {
    // Buffers entries that do no satisfy the "shouldFlush" condition, using a circular buffer with a maximum
    // capacity. When an entry that satisfies the condition ocurrs, then it flushes the circular buffer and the new entry,
    // and starts buffering again.
    // "stream" The original stream of events.
    // "shouldFlush" The condition that defines whether the item and the buffered entries are flushed.
    // "bufferSize" The buffer size for accumulated entries.
    // Returns an observable that has this filtering capability.
    public static IObservable<T> FlushOnTrigger<T>(this IObservable<T> stream, Func<T, bool> shouldFlush, int bufferSize)
    {
      return Observable.Create<T>(observer =>
      {
        var buffer = new CircularBuffer<T>(bufferSize);
        var subscription = stream.Subscribe(newItem =>
          {
             if (shouldFlush(newItem))
             {
                foreach (var buffered in buffer.TakeAll())
                {
                   observer.OnNext(buffered);
                }

                observer.OnNext(newItem);
             }
             else
             {
                buffer.Add(newItem);
             }
          },
          observer.OnError,
          observer.OnCompleted);

        return subscription;
      });
    }
  }
}
