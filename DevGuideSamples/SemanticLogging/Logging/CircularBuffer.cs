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

using System.Collections.Generic;

namespace SemanticLoggingExample
{
  // Very basic implemantation of a circular buffer for demonstration purposes.
  public class CircularBuffer<T>
  {
    private readonly int size;
    private Queue<T> queue;

    public CircularBuffer(int size)
    {
      this.queue = new Queue<T>(size);
      this.size = size;
    }

    public void Add(T obj)
    {
      if (this.queue.Count == this.size)
      {
        this.queue.Dequeue();
        this.queue.Enqueue(obj);
      }
      else
        this.queue.Enqueue(obj);
    }

    public IEnumerable<T> TakeAll()
    {
      var list = new List<T>(queue.Count);
      while (this.queue.Count > 0)
      {
        list.Add(this.queue.Dequeue());
      }

      return list;
    }
  }
}
