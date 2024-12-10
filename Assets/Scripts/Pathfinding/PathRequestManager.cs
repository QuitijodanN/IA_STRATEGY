using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

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

    public static void RequestPath((int, int) pathStart, (int, int) pathEnd, Action<Node[], bool> callback, int distance) {
		PathRequest newRequest = new PathRequest(pathStart,pathEnd,callback, distance);
		instance.pathRequestQueue.Enqueue(newRequest);
		instance.TryProcessNext(distance);
	}

	void TryProcessNext(int distance) {
		if (!isProcessingPath && pathRequestQueue.Count > 0) {
			currentPathRequest = pathRequestQueue.Dequeue();
            isProcessingPath = true;
			pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd, distance);
		}
	}

	public void FinishedProcessingPath(Node[] path, bool success, int distance) {
		currentPathRequest.callback(path,success);
		isProcessingPath = false;
		TryProcessNext(distance);
	}

	struct PathRequest {
		public (int, int) pathStart;
		public (int, int) pathEnd;
		public Action<Node[], bool> callback;
		public int distance;


        public PathRequest((int, int) _start, (int, int) _end, Action<Node[], bool> _callback, int _distance) {
			pathStart = _start;
			pathEnd = _end;
			callback = _callback;
            distance = _distance;

        }

	}
}
