using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour {

	public float height = 0;
	public float maxClimb = 0;
	public float smallestHeight = 0;
	public bool awaitingClimb;
	public GameObject target;

	void Update() {
		if (GetComponent<NavMeshAgent> ().remainingDistance < 1 && awaitingClimb) {
			Debug.Log (GetComponent<NavMeshAgent> ().remainingDistance);
			navigatableObject other = target.GetComponent<navigatableObject> ();
			if (other.clickedOn (this)) {
				if (other.canWalkThrough) {
					walkThrough ();
				} else if (other.canClimbOver) {
					climbOver ();
				}
			} else {
				GetComponent<NavMeshAgent> ().ResetPath ();
			}
			awaitingClimb = false;
		}
	}


	void climbOver()
	{
		mirrorPos ();

		GetComponent<NavMeshAgent> ().ResetPath ();
		GetComponent<NavMeshAgent> ().velocity = Vector3.zero;
	}

	void climbOnto() {

	}

	void walkThrough() {
		mirrorPos ();

		GetComponent<NavMeshAgent> ().ResetPath ();
		GetComponent<NavMeshAgent> ().velocity = Vector3.zero;
	}

	void mirrorPos() {
		Vector3 mirrorPos = Vector3.zero;

		if ((target.transform.position.x - target.transform.localScale.x/2) < transform.position.x
			&& transform.position.x < (target.transform.position.x + target.transform.localScale.x/2)) {
			transform.position += new Vector3 (0, 0, (target.transform.position.z - transform.position.z) * 2);
		}

		if ((target.transform.position.z - target.transform.localScale.z/2) < transform.position.z
			&& transform.position.z < (target.transform.position.z + target.transform.localScale.z/2)) {
			transform.position += new Vector3 ((target.transform.position.x - transform.position.x) * 2, 0, 0);
		}
	}

}
