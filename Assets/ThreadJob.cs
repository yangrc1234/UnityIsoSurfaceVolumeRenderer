using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
public class ThreadJob {
    public bool isDone { get {
            int val = 1;
            Interlocked.CompareExchange(ref val,0, _isDone);
            if (val == 0)
                return true;
            return false;
        }
        set {
            _isDone = value ? 1 : 0;
        }
    }
    private int _isDone;
    protected Thread thread;

    public void Start() {
        thread = new Thread(Run);
        thread.IsBackground = true;
        thread.Start();
    }

    private void Run() {
        ThreadFunction();
        isDone = true;
    }

    protected virtual void ThreadFunction() {

    }

    public IEnumerator WaitTillDone() {
        while (!isDone)
            yield return null;
    }
}
