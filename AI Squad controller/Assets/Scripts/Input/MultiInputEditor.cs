using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using System.Collections.Generic;

//create custom editor for multi-input
[CustomEditor(typeof(MultiInput))]
[CanEditMultipleObjects]
public class MultiInputEditor : Editor {

	SerializedProperty keys;
	SerializedProperty visible;
	bool selecting = false;
	int index = -1;

	void OnEnable() {
		keys = serializedObject.FindProperty("keys");
		visible = serializedObject.FindProperty ("visible");
	}

	public override void OnInspectorGUI() {

		serializedObject.Update ();

		Event e = Event.current;

		//begin whole button gui
		GUI.backgroundColor = new Color(0.2f,0.2f,0.5f,1);
		GUILayout.BeginVertical(EditorStyles.helpBox);
		GUI.backgroundColor = new Color(0.9f,0.9f,0.9f,0.5f);
		//GUI.backgroundColor = Color.white;

        //display count of key data
		EditorGUILayout.IntField("Buttons", keys.arraySize);

        //display add/remove buttons for key data
		GUILayout.BeginHorizontal(EditorStyles.helpBox);

		if (GUILayout.Button("Add"))
        {
            //add new blank key data
			keys.InsertArrayElementAtIndex (keys.arraySize);
			visible.InsertArrayElementAtIndex (visible.arraySize);
        }
		if (GUILayout.Button("Remove"))
        {
            //check if key data has data
			if (keys.arraySize > 0)
            {
				//remove last key data
				keys.DeleteArrayElementAtIndex(keys.arraySize-1);
				visible.DeleteArrayElementAtIndex(visible.arraySize-1);
            }
		}

		serializedObject.ApplyModifiedProperties ();

		GUILayout.EndHorizontal();
		GUILayout.Space(10);

        //loop through each key data and display change options
		for (int a = 0; a < keys.arraySize; a++) {
			if (visible.GetArrayElementAtIndex (a).boolValue) {
				SerializedProperty currentKey = keys.GetArrayElementAtIndex (a);
				SerializedProperty currentKeyList = currentKey.FindPropertyRelative ("key");

				//allow closing of element data
				GUI.backgroundColor = new Color(1,1,1,1);
				GUILayout.BeginHorizontal(EditorStyles.helpBox);
				//GUILayout.Space (100);
				if (GUILayout.Button ("Close Element " + a.ToString ())) {//, GUILayout.Width(300))) {
					visible.GetArrayElementAtIndex (a).boolValue = false;
				}
				GUI.backgroundColor = new Color(0.9f,0.9f,0.9f,0.5f);
				GUILayout.EndHorizontal();

				//start data section
				GUILayout.BeginVertical (EditorStyles.helpBox);

				GUILayout.BeginHorizontal (EditorStyles.helpBox);

				//GUILayout.Space (40);
				//display adding for multiple keys
				if (GUILayout.Button ("Add Key")) {
					//add new blank key data
					currentKeyList.InsertArrayElementAtIndex (currentKeyList.arraySize);
				}
				if (GUILayout.Button ("Remove Key")) {
					//check if key data has data
					if (currentKeyList.arraySize > 0) {
						//remove last key data
						currentKeyList.DeleteArrayElementAtIndex (currentKeyList.arraySize - 1);
					}
				}
				//GUILayout.Space (40);

				//split up the options
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();

				//apply update to data
				serializedObject.ApplyModifiedProperties ();

				for (int b = 0; b < currentKeyList.arraySize; b++) {

					//display popup for key input
					EditorGUILayout.LabelField ("Key");//, GUILayout.Width (40));
					EditorGUILayout.PropertyField (currentKeyList.GetArrayElementAtIndex (b), GUIContent.none); /*GUILayout.Width (275));
					/*
					if (b == index) {
						if (selecting) {
							GUILayout.Label ("Selecting Key", GUI.skin.button);
							switch (e.type) {
							case EventType.
							}
							if (e.keyCode || e.isMouse) {
								((MultiInput)target).keys[a].key[b] = e.keyCode;
								selecting = false;
								index = -1;
							}
						}
					} else {
						if (GUILayout.Button ("Select Key")) {
							index = b;
							selecting = true;
						}
					}
					*/

					//split up the options
					GUILayout.EndHorizontal ();
					GUILayout.Space (2);
					GUILayout.BeginHorizontal ();

				}
				

				//display string for key event
				EditorGUILayout.LabelField ("Event");//, GUILayout.Width (40));
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				EditorGUILayout.PropertyField (currentKey.FindPropertyRelative ("keyEvent"));//, GUILayout.Width (415));

				//end data section
				GUILayout.EndHorizontal ();
				GUILayout.EndVertical ();
				GUILayout.Space (10);

				serializedObject.ApplyModifiedProperties ();
			} else {
				//allow opening of element
				GUI.backgroundColor = new Color(1,1,1,1);
				GUILayout.BeginHorizontal(EditorStyles.helpBox);
				//GUILayout.Space (100);
				if (GUILayout.Button ("Show Element " + a.ToString ())) {//, GUILayout.Width(300))) {
					visible.GetArrayElementAtIndex (a).boolValue = true;
				}
				GUI.backgroundColor = new Color(0.9f,0.9f,0.9f,0.5f);
				GUILayout.EndHorizontal();
				GUILayout.Space (10);
			}
		}
			
		GUILayout.EndVertical();
		
		serializedObject.ApplyModifiedProperties ();
	}

}