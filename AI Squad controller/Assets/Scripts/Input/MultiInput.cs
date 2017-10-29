using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

public class MultiInput : MonoBehaviour {

    //storage for keys
	[SerializeField]
	public List<keyInfo> keys = new List<keyInfo>();
	[SerializeField]
	public List<bool> visible = new List<bool>();

	void Update() {
		foreach (keyInfo key in keys) {
			for (int a = 0; a < key.axis.Count; a++) {
				//do axis stuff here
				if (!key.axis [a].multiAxis) {
					//poll axis and test deadzone
					key.axis [a].keyData.data.axisValue = Input.GetAxisRaw (key.axis [a].axis);
					if (key.axis [a].keyData.data.axisValue < key.axis [a].axisDeadZone.y &&
						key.axis [a].keyData.data.axisValue > key.axis [a].axisDeadZone.x) {
						key.axis [a].keyData.data.axisValue = 0;
					}

					//activate if above is not 0
					if (key.axis [a].keyData.data.axisValue != 0) {
						activate (key, 0, a);
					}
				} else {
					//poll axis 1 and test deadzone
					key.axis [a].keyData.data.multiAxisValue.x = Input.GetAxisRaw (key.axis [a].axis);
					if (key.axis [a].keyData.data.multiAxisValue.x < key.axis [a].axisDeadZone.y &&
						key.axis [a].keyData.data.multiAxisValue.x > key.axis [a].axisDeadZone.x) {
						key.axis [a].keyData.data.multiAxisValue.x = 0;
					}

					//poll axis 2 and test deadzone
					key.axis [a].keyData.data.multiAxisValue.y = Input.GetAxisRaw (key.axis [a].axis2);
					if (key.axis [a].keyData.data.multiAxisValue.y < key.axis [a].axis2DeadZone.y &&
						key.axis [a].keyData.data.multiAxisValue.y > key.axis [a].axis2DeadZone.x) {
						key.axis [a].keyData.data.multiAxisValue.y = 0;
					}

					//activate if either above is not 0
					if (key.axis [a].keyData.data.multiAxisValue != Vector2.zero) {
						activate (key, 0, a);
					}
				}
			}
			for (int a = 0; a < key.buttons.Count; a++) {
				//do button stuff here
				if ((Input.GetKey (key.buttons[a].button) && key.buttons[a].keyType == keyInputType.getKey) ||
					(Input.GetKeyDown (key.buttons[a].button) && key.buttons[a].keyType == keyInputType.getKeyDown) ||
					(Input.GetKeyUp (key.buttons[a].button) && key.buttons[a].keyType == keyInputType.getKeyUp)) {
					activate(key, 1, a);
				}
			}
		}
	}

	void activate(keyInfo _key, int type, int index) {
		//deal with 1st layer pass on key
		basicKeyInfo key = new basicKeyInfo();
		if (type == 0) {
			key = _key.axis [index].keyData;
		} else if (type == 1) {
			key = _key.buttons [index].keyData;
		}

		//check if key has a set function
		if (key.function) {
			//check if the key has an object
			if (key.functionObject) {
				//check if the key has variable input
				if (key.variableInput) {
					//carry our custom function
					customVariableInvoke (key.functionObject, key.functionName, key.data);
				} else {
					//carry our custom function
					customInvoke (key.functionObject, key.functionName);
				}
			} else {
				Debug.Log ("You need to add an object to activate this key: " + key.functionName);
			}
		} else {
			//check if key has public fuction
			if (key.publicFunction != null) {
				//invoke unity event
				key.publicFunction.Invoke ();
			} else {
				Debug.Log ("You need to add an event to activate this key: " + gameObject.name);
			}
		}
	}

	void customInvoke(GameObject info, string name) {
		if (name != "") {
			info.SendMessage (name);
		}
	}

	void customVariableInvoke(GameObject info, string name, variableData variable) {
		if (name != "") {
			info.SendMessage (name, variable);
		}
	}

	void testKeyboardInputs() {
		if (Input.anyKey) {
			Debug.Log (Input.inputString);
		}
	}

	void testControllerInputs() {
		for (int a = 0; a < 8; a++) {
			for(int b = 0; b < 19; b++) {
				if (a == 0) {
					if (Input.GetKey ((KeyCode)System.Enum.Parse (typeof(KeyCode), "JoystickButton" + b))) {
						Debug.Log ("Joystick 0 Button " + b);
					}
				} else {
					if (Input.GetKey ((KeyCode)System.Enum.Parse (typeof(KeyCode), ("Joystick" + a + "Button" + b)))) {
						Debug.Log ("Joystick " + a + " Button " + b);
					}
				}
			}
		}
	}
}

//default class for key input data
[System.Serializable]
public class keyInfo
{
	//basic variables for input names should explain it
	public int keyInputsAccepted;

	public bool globalFunction;
	public bool globalFunctionObj;

	public GameObject globalFunctionObject;
	public string globalFunctionName;

	public List<axisInfo> axis;
	public List<buttonInfo> buttons;

}

[System.Serializable]
public class axisInfo {
	public string axis;
	public bool advancedDeadZone;
	public Vector2 axisDeadZone;
	public string axis2;
	public Vector2 axis2DeadZone;
	public bool multiAxis;
	public basicKeyInfo keyData;
}

[System.Serializable]
public class buttonInfo {
	public KeyCode button;
	public basicKeyInfo keyData;
	public keyInputType keyType;
}

[System.Serializable]
public class basicKeyInfo {
	//basic data
	public bool variableInput;
	public bool expandInput;
	public bool viewable;
	public variableData data;

	//deal with private functions
	public bool function;
	public string functionName;
	public GameObject functionObject;

	//deal with public functions
	public UnityEvent publicFunction;
}

[System.Serializable]
public class variableData {
	//axis data
	public float axisValue;
	public Vector2 multiAxisValue;

	//all data types needed
	public bool b;
	public byte by;
	public char c;
	public float f;
	public int i;
	public GameObject obj;
	public MonoBehaviour sc;
	public string s;
	public Transform t;
	public UnityEvent u;
	public Vector2 v2;
	public Vector3 v3;
	public Vector4 v4;
}

public enum keyInputType {
	getKey, getKeyDown, getKeyUp
}
	
[System.Serializable]
public enum button {
	none,
	released,
	pressed,
	pressedFirstUpdate,
	releasedFirstUpdate
}