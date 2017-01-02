using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class AsyncCallback : MonoBehaviour
{
    private static AsyncCallback instance;
    public static AsyncCallback Instance
    {
        get
        {
            return instance;
        }
        private set { instance = value; }
    }

    AsyncCallback()
    {
        instance = this;
    }

    private Queue<System.Action> Queue = new Queue<System.Action>();
    private System.Object QueueLock = new System.Object();

    public void Invoke(System.Action Callback)
    {
        lock (Queue)
        {
            Queue.Enqueue(Callback);
        }
    }


    void Update()
    {
        lock (QueueLock)
        {
            if (Queue.Count > 0)
            {
                int i = 0;
                System.Action Callback;
                while (Queue.Count > 0 && i++ < 10)
                {
                    Callback = Queue.Dequeue();
                    Callback.Invoke();
                }
            }
        }
    }
}
