using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FindNearestCover : MonoBehaviour {

	public bool findCover = false;

	Vector3 findNearest(Vector3 position) {
		GameObject[] cover = GameObject.FindGameObjectsWithTag ("Cover");
		GameObject closests = cover[0];
		float curDistance = Vector3.Distance(position, closests.transform.position);

		foreach (GameObject obj in cover) {
			float dist = Vector3.Distance (position, obj.transform.position);
			if (dist < curDistance) {
				closests = obj;
				curDistance = dist;
			}
		}

		return closests.transform.position;
	}

	void Update() {
		if (findCover) {
			foreach (NavMeshAgent unit in GameObject.FindObjectsOfType<NavMeshAgent>()) {
				Vector3 point = findNearest (unit.transform.position);
				if (point != Vector3.zero) {
					unit.SetDestination (point);
					findCover = false;
				}
			}
		}
	}
}
