using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CheckHeight : MonoBehaviour {

	GlobalVars vars;

	// Use this for initialization
	void Start () {
		vars = GameObject.FindObjectOfType<GlobalVars> ();
		//checkAllChildren ();
		//transform.rotation = Quaternion.Euler (Vector3.zero);
	}

	void checkAllChildren() {
		for (int a = 0; a < transform.childCount; a++) {
			Transform child = transform.GetChild (a);
			switch (child.transform.name) {
			case "Top":
				checkTop (child);
				break;
			case "Front":
				checkFront (child);
				break;
			}
		}

	}

	void Update() {
		Debug.DrawRay (transform.position + new Vector3 (0, vars.playerHeight, 0), Vector3.down, Color.white);
		Debug.DrawRay (transform.position + Vector3.forward * vars.playerRadius, -Vector3.forward, Color.red);
		Debug.DrawRay (transform.position - (Vector3.forward * vars.playerRadius), Vector3.forward, Color.green);
		Debug.DrawRay (transform.position - (Vector3.left * vars.playerRadius), Vector3.left, Color.blue);
		Debug.DrawRay (transform.position + Vector3.left * vars.playerRadius, -Vector3.left, Color.yellow);
	}

	void checkTop(Transform child) {
		RaycastHit hit = new RaycastHit();
		if (Physics.Raycast (transform.position + new Vector3(0, vars.playerHeight, 0), Vector3.down, out hit)) {
			compareTransforms (child, hit.transform);
		}
	}

	void checkFront(Transform child) {
		RaycastHit hit = new RaycastHit();
		if (Physics.Raycast (transform.position + (Vector3.forward * vars.playerRadius), -Vector3.forward, out hit)) {
			compareTransforms (child, hit.transform);
		}
	}

	void checkBack(Transform child) {
		RaycastHit hit = new RaycastHit();
		if (Physics.Raycast (transform.position + (Vector3.forward * vars.playerRadius), -Vector3.forward, out hit)) {
			compareTransforms (child, hit.transform);
		}
	}

	void disableNav(Transform child) {
		child.GetComponent<NavMeshLink> ().enabled = false;
	}

	void compareTransforms(Transform child, Transform hit) {
		if (hit != child.transform) {
			if (hit != transform) {
				disableNav (child);
			}
		}
	}
}
