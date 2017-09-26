using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class navigatableObject : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	public bool clickedOn (Unit other) {
		if (canClimb (other)) {
			return true;
		}
		return false;
	}

	bool canClimb (Unit other) {
		if ((transform.position.y + transform.localScale.y) - (other.transform.position.y - other.transform.localScale.y) < other.maxClimb) {
			RaycastHit hit;
			Physics.Raycast (transform.position + (Vector3.up * other.height - new Vector3 (0, -0.1f, 0)), Vector3.down, out hit, other.height);
			return hit.collider.gameObject == gameObject;
		}
		return false;
	}
}
