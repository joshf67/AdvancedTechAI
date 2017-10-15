using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class switch_camera : MonoBehaviour {

	public Camera cam1;
	public Camera cam2;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) {
			cam1.enabled = !cam1.enabled;
			cam2.enabled = !cam2.enabled;
		}
	}
}
