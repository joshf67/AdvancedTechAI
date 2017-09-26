using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class navigatableObject : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	public bool clickedOn (float height) {
		if (gameObject.isStatic) {
			canClimb (height);
			return true;
		}
		return false;
	}

	bool canClimb (float height) {
		RaycastHit hit;
		Physics.Raycast (transform.position + (Vector3.up * height), Vector3.down, out hit, height);
		return hit.collider.gameObject == gameObject;
	}
}
