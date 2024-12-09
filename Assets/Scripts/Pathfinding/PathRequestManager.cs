using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PathRequestManager : MonoBehaviour {

	Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
	PathRequest currentPathRequest;

	static PathRequestManager instance;
	Pathfinding pathfinding;

	bool isProcessingPath;

	void Awake() {
		instance = this;
		pathfinding = GetComponent<Pathfinding>();
	}

    public static void RequestPath((int, int) pathStart, (int, int) pathEnd, Action<Node[], bool> callback, bool longDistance) {
		PathRequest newRequest = new PathRequest(pathStart,pathEnd,callback,longDistance);
		instance.pathRequestQueue.Enqueue(newRequest);
		instance.TryProcessNext(longDistance);
	}

	void TryProcessNext(bool longDistance) {
		if (!isProcessingPath && pathRequestQueue.Count > 0) {
			currentPathRequest = pathRequestQueue.Dequeue();
			isProcessingPath = true;
			pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd, longDistance);
		}
	}

	public void FinishedProcessingPath(Node[] path, bool success, bool longDistance) {
		currentPathRequest.callback(path,success);
		isProcessingPath = false;
		TryProcessNext(longDistance);
	}

	struct PathRequest {
		public (int, int) pathStart;
		public (int, int) pathEnd;
		public Action<Node[], bool> callback;
		public bool longDistance;


        public PathRequest((int, int) _start, (int, int) _end, Action<Node[], bool> _callback, bool _longDistance) {
			pathStart = _start;
			pathEnd = _end;
			callback = _callback;
			longDistance = _longDistance;

        }

	}
}
