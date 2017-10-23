using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIinputs : MonoBehaviour {

	public bool selection = true;
	public bool waypoint = false;
	public bool move = false;
	public bool follow = false;
	public Image ui;
	public Sprite selectionUI = null;
	public Sprite waypointUI = null;
	public Sprite moveUI = null;
	public Sprite followUI = null;
	public int time = 1;

	public Text text = null;
	public Button but1 = null;
	public Button but2 = null;


	// Use this for initialization
	void Start () {
		ui.sprite = selectionUI;
	}

	public void increase() {
		if (time <= 2) {
			time += 1;
		}
	}

	public void decrease() {
		if (time > 1) {
			time -= 1;
		}
	}
	
	// Update is called once per frame
	void Update () {
		text.text = time.ToString();
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			ui.sprite = selectionUI;
			selection = true;
			waypoint = false;
			follow = false;
			move = false;
			disableButton (but1);
			disableButton (but2);
			text.enabled = false;
		}
		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			ui.sprite = waypointUI;
			selection = false;
			waypoint = true;
			move = false;
			follow = false;
			enableButton (but1);
			enableButton (but2);
			text.enabled = true;
		}
		if (Input.GetKeyDown (KeyCode.Alpha3)) {
			ui.sprite = moveUI;
			selection = false;
			waypoint = false;
			move = true;
			follow = false;
			disableButton (but1);
			disableButton (but2);
			text.enabled = false;
		}
		if (Input.GetKeyDown (KeyCode.Alpha4)) {
			ui.sprite = followUI;
			selection = false;
			waypoint = false;
			move = false;
			follow = true;
			disableButton (but1);
			disableButton (but2);
			text.enabled = false;
		}
	}

	void disableButton(Button but) {
		but.GetComponentInChildren<Text> ().enabled = false;
		but.GetComponent<Image> ().enabled = false;
		but.enabled = false;
	}

	void enableButton(Button but) {
		but.GetComponentInChildren<Text> ().enabled = true;
		but.GetComponent<Image> ().enabled = true;
		but.enabled = true;
	}
}