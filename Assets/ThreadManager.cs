using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class ThreadManager : MonoBehaviour {

    public static ThreadManager instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<ThreadManager>();
            }
            return _instance;
        }
    }
    private static ThreadManager _instance;

    private void Awake() {
        _instance = this;       //if the first call is used in a thread, it will throw a exception, since sub-thread can't use FindObjectOfType 
    }

    private Queue<Action> _callbackQueue = new Queue<Action>();

    public void AddThreadCallback(Action callback) {
        lock (_callbackQueue) {
            _callbackQueue.Enqueue(callback);
        }
    }
    // Update is called once per frame
    void Update () {
        lock (_callbackQueue) {
            while (_callbackQueue.Count > 0) {
                _callbackQueue.Dequeue()();
            }
        }
	}
}
