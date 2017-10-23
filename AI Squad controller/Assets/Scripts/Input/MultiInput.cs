using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MultiInput : MonoBehaviour {

    //storage for keys
	[SerializeField]
	public List<keyInfo> keys = new List<keyInfo>();
	[SerializeField]
	public List<bool> visible = new List<bool>();

    // Update is called once per frame
    void Update()
    {
        //loop through keys and test input
		if (Input.anyKey) {
			foreach (keyInfo info in keys) {
				foreach (KeyCode key in info.key) {
					if (Input.GetKey (key)) {
						Debug.Log (key.ToString ());
						info.keyEvent.Invoke ();
					}
				}
			}
		}
	}

	public void debug() {
		Debug.Log ("Worked");
	}

}

//default class for key input data
[System.Serializable]
public class keyInfo
{
    public List<KeyCode> key;
	public UnityEvent keyEvent;
}