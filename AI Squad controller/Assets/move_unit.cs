using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class move_unit : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton (0)) {
			RaycastHit hit;
			Ray mousePoint = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (mousePoint,out hit)) {
				GameObject[] objects = GameObject.FindGameObjectsWithTag ("Unit");
				foreach (GameObject obj in objects) {
					if (hit.collider.gameObject.GetComponent<navigatableObject> ()) {
						obj.GetComponent<Unit> ().target = hit.collider.gameObject;
						obj.GetComponent<Unit> ().awaitingClimb = true;
					} else {
						obj.GetComponent<Unit> ().target = null;
						obj.GetComponent<Unit> ().awaitingClimb = false;
					}
					obj.GetComponent<NavMeshAgent> ().SetDestination (hit.point);
				}
			}
		}
	}
}
