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
					obj.GetComponent<NavMeshAgent> ().SetDestination (hit.point);
				}
				if (hit.collider.gameObject.GetComponent<navigatableObject>()) {
					if (hit.collider.gameObject.GetComponent<navigatableObject> ().clickedOn(1)) {
						bool repeat = true;
						List<GameObject> checking = new List<GameObject> ();
						foreach (GameObject obj in objects) {
							checking.Add (obj);
						}
						while (repeat) {
							foreach (GameObject obj in checking) {
								
								if (Vector3.Distance (obj.transform.position, (new Vector3 (hit.point.x, 0, hit.point.z))) < 1) {
									obj.transform.position += Vector3.Angle (obj.transform.position, hit.point) * (new Vector3 (hit.collider.transform.localScale.x, 0, hit.collider.transform.localScale.z));
									checking.Remove (obj);
								}
							}
							if (checking.Count == 0) {
								repeat = false;
							}
						}
					}
				}
			}
		}
	}
}
