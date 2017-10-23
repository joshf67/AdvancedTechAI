using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class switch_camera : MonoBehaviour {

	public GameObject cam1;
	public GameObject cam2;

	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.Locked;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) {

			if (cam1.GetComponent<Camera>().enabled) {
				cam1.GetComponent<Camera> ().enabled = false;
				cam1.GetComponent<AudioListener> ().enabled = false;
				cam1.GetComponent<unitWaypoint> ().scriptEnabled = false;
				cam2.GetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>().enabled = true;
				cam2.GetComponentInChildren<Camera> ().enabled = true;
				cam2.GetComponentInChildren<AudioListener> ().enabled = true;
				Cursor.lockState = CursorLockMode.Locked;
				Time.timeScale = 1;

			} else {
				cam1.GetComponent<Camera> ().enabled = true;
				cam1.GetComponent<AudioListener> ().enabled = true;
				cam1.GetComponent<unitWaypoint> ().scriptEnabled = true;
				cam2.GetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>().enabled = false;
				cam2.GetComponentInChildren<Camera> ().enabled = false;
				cam2.GetComponentInChildren<AudioListener> ().enabled = false;
				Cursor.lockState = CursorLockMode.None;
				Time.timeScale = 0.25f;

			}
		}
	}
}