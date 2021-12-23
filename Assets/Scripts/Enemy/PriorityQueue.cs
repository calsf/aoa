using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PriorityQueue<T> where T : IComparable<T>
{
    // Min heap representation
    List<T> minHeap;

    public PriorityQueue()
    {
        this.minHeap = new List<T>();
    }

    public void Enqueue(T item)
    {
        // Add item and get new item index
        minHeap.Add(item);
        int child = minHeap.Count - 1;

        // Reorder min heap
        while (child > 0)
        {
            int parent = (child - 1) / 2;

            if (minHeap[child].CompareTo(minHeap[parent]) >= 0) // Return once child is lower priority and is in proper place
            {
                return;
            }
            else // Reorder elements
            {
                T temp = minHeap[child];
                minHeap[child] = minHeap[parent];
                minHeap[parent] = temp;

                child = parent;
            }
        }
    }

    public T Dequeue()
    {
        // Get index of next head
        int last = minHeap.Count - 1;

        // Get head to return
        T head = minHeap[0];

        // Assign new head and remove old head
        minHeap[0] = minHeap[last];
        minHeap.RemoveAt(last);

        // Reorder min heap
        last--;
        int parent = 0;
        while (true)
        {
            int leftChild = parent * 2 + 1;
            int rightChild = parent * 2 + 2;

            if (leftChild > last)
            {
                return head;
            }
            else
            {
                if (rightChild <= last && minHeap[rightChild].CompareTo(minHeap[leftChild]) < 0)
                {
                    leftChild = rightChild;
                }

                if (minHeap[parent].CompareTo(minHeap[leftChild]) <= 0)
                {
                    return head;
                }
                else
                {
                    T temp = minHeap[parent];
                    minHeap[parent] = minHeap[leftChild];
                    minHeap[leftChild] = temp;

                    parent = leftChild;
                }
            }
        }
    }

    public bool Contains(T item)
    {
        return minHeap.Contains(item);
    }

    public bool IsEmpty()
    {
        return minHeap.Count <= 0 ? true : false;
    }
}
