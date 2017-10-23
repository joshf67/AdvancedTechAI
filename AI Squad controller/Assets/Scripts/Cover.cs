using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cover : MonoBehaviour {

	public Vector3 pos = Vector3.zero;
	public int index = 0;
	public bool free = true;
	public bool lowCover = false;
	public bool rightCover = false;
	public float rightDistance = 0;
	public bool leftCover = false;
	public float leftDistance = 0;

	// Use this for initialization
	void Start () {
		pos = transform.position;
		Collider[] hit = Physics.OverlapBox (pos, transform.localScale / 2);
		if (hit.Length != 0) {
			for (int a = 0; a < hit.Length; a++) {
				if (hit [a] != this.GetComponent<Collider> () && hit [a].gameObject.GetComponent<Cover>() == null) {
					free = false;
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
