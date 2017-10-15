using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIinputs : MonoBehaviour {

	public bool selection = true;
	public bool waypoint = false;
	public bool move = false;
	public Image ui = null;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			ui.color = Color.green;
			selection = true;
			waypoint = false;
			move = false;
		}
		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			ui.color = Color.red;
			selection = false;
			waypoint = true;
			move = false;
		}
		if (Input.GetKeyDown (KeyCode.Alpha3)) {
			ui.color = Color.gray;
			selection = false;
			waypoint = false;
			move = true;
		}
	}
}
