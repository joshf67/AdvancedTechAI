using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour {

	public float height = 0;
	public float maxClimb = 0;
	public bool awaitingClimb;
	public GameObject target;

	void Update() {
		if (GetComponent<NavMeshAgent> ().remainingDistance < 1 && awaitingClimb) {
			if (target.GetComponent<navigatableObject> ().clickedOn (this)) {
				climbOver ();
			} else {
				GetComponent<NavMeshAgent> ().ResetPath ();
			}
			awaitingClimb = false;
		}
	}

	public void climbOver()
	{
		Vector3 mirrorPos = Vector3.zero;

		if ((target.transform.position.x - target.transform.localScale.x) < transform.position.x
			&& transform.position.x < (target.transform.position.x + target.transform.localScale.x)) {
			transform.position += new Vector3 (0, 0, (target.transform.position.z - transform.position.z) * 2);
		}

		if ((target.transform.position.z - target.transform.localScale.z) < transform.position.z
			&& transform.position.z < (target.transform.position.z + target.transform.localScale.z)) {
			transform.position += new Vector3 ((target.transform.position.x - transform.position.x) * 2, 0, 0);
		}

		GetComponent<NavMeshAgent> ().ResetPath ();
		GetComponent<NavMeshAgent> ().velocity = Vector3.zero;
	}

}
