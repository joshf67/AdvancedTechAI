using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AdjustWidth : MonoBehaviour {

	public bool X;
	public bool Y;
	public bool Z;
	public bool Floor;
	public bool main;
	public int Modify = 0;
	public bool finished = false;

	// Use this for initialization
	void Start () {
		NavMeshLink[] link = GetComponents<NavMeshLink> ();
		Vector3 temp = Vector3.zero;
		if (X) {
			link[0].width = transform.parent.localScale.x;
			temp = link[0].startPoint;
			temp.y = (transform.parent.localScale.z/2) * Modify;
			link[0].startPoint = temp;
			temp = link[0].endPoint;
			temp.y = (transform.parent.localScale.z/2) * Modify;
			link[0].endPoint = temp;

			link[1].width = transform.parent.localScale.x;
			temp = link[1].startPoint;
			temp.y = (transform.parent.localScale.z/2) * Modify;
			link[1].startPoint = temp;
			temp = link[1].endPoint;
			temp.y = (transform.parent.localScale.z/2) * Modify;
			link[1].endPoint = temp;

		} else if (Z) {
			
			link[0].width = transform.parent.localScale.z;
			temp = link[0].startPoint;
			temp.y = (transform.parent.localScale.x/2) * Modify;
			link[0].startPoint = temp;
			temp = link[0].endPoint;
			temp.y = (transform.parent.localScale.x/2) * Modify;
			link[0].endPoint = temp;

			link[1].width = transform.parent.localScale.z;
			temp = link[1].startPoint;
			temp.y = (transform.parent.localScale.x/2) * Modify;
			link[1].startPoint = temp;
			temp = link[1].endPoint;
			temp.y = (transform.parent.localScale.x/2) * Modify;
			link[1].endPoint = temp;
		}
			
		GetComponent<NavMeshSurface> ().BuildNavMesh ();
	}
}
