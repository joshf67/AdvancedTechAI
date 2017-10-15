using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class customGrid2 : MonoBehaviour {

	public Vector3 size;
	public Vector3 actualSize;
	public List<gridSpace> areas = new List<gridSpace>();
	public float radius;
	public Collider[] objectsInArea;
	public List<GameObject> objs = new List<GameObject>();
	public List<Vector3> vertexPoints = new List<Vector3>();
	public bool showAir = false;
	public bool showSolid = false;
	public bool showSurround = false;
	float time;
	bool done = false;

	// Use this for initialization
	void Start () {
		time = Time.realtimeSinceStartup;

		objectsInArea = Physics.OverlapBox (size / 2, size / 2);//Physics.BoxCastAll (size / 2, size / 2, Vector3.zero);
		foreach (Collider hit in objectsInArea) {
			objs.Add (hit.transform.gameObject);
		}
		foreach (GameObject obj in objs) {
			MeshFilter temp = obj.GetComponent<MeshFilter> ();
			List<Vector3> tempVert = new List<Vector3>();
			temp.mesh.GetVertices (tempVert);
			Vector3 pos;
			Vector3 objSize = obj.transform.lossyScale;
			Vector3 objPos = obj.transform.position;
			foreach (Vector3 vert in tempVert) {
				pos = new Vector3((vert.x * objSize.x) + objPos.x , (vert.y * objSize.y) + objPos.y , (vert.z * objSize.z) + objPos.z );
				vertexPoints.Add (pos);
			}
		}


		actualSize = size / radius;

		for(int a = 0; a < actualSize.x; a++) {
			for(int b = 0; b < actualSize.y; b++) {
				for(int c = 0; c < actualSize.z; c++) {
					gridSpace temp = new gridSpace (new Vector3 (a, b, c) * radius, 0);
					if (c > 0) {
						areas [(int)((a * (actualSize.z * actualSize.y)) + (b * actualSize.z) + c - 1)].front = temp;
						temp.back = areas [(int)((a * (actualSize.z * actualSize.y)) + (b * actualSize.z) + c - 1)];
					}

					if (b > 0) {
						areas [(int)((a * (actualSize.z * actualSize.y)) + ((b - 1) * actualSize.z) + c)].up = temp;
						temp.down = areas [(int)((a * (actualSize.z * actualSize.y)) + ((b - 1) * actualSize.z) + c)];
					}

					if (a > 0) {
						areas [(int)(((a - 1) * (actualSize.z * actualSize.y)) + (b * actualSize.z) + c)].up = temp;
						temp.down = areas [(int)(((a - 1) * (actualSize.z * actualSize.y)) + (b * actualSize.z) + c)];
					}
						
					areas.Add(temp);
				}
			}
		}
		//int i = 0;
		foreach (Vector3 vert in vertexPoints) {
			int x = Mathf.RoundToInt(vert.x / radius);
			int y = Mathf.RoundToInt(vert.y / radius);
			int z = Mathf.RoundToInt(vert.z / radius);
			areas [(int)(    (x * (actualSize.z  * actualSize.y))    + (y * actualSize.z) + z)].area = 1;
		}
	
		done = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (done) {
			Debug.Log ("Time: " + (Time.realtimeSinceStartup - time));
			done = false;
		}
	}

	void OnDrawGizmosSelected() {
		Gizmos.DrawWireCube ((size - (Vector3.one * radius)) / 2, size - (Vector3.one * radius));
		Vector3 temp = Vector3.one * (radius / 2);
		for(int a = 0; a < areas.Count; a++) {
			if (areas [a].area == 0) {
				if (showAir) {
					Gizmos.color = Color.green;
					Gizmos.DrawWireCube (areas [a].pos, temp / 8);
				}
			} else if (areas [a].area == 1) {
				if (showSolid) {
					Gizmos.color = Color.red;
					Gizmos.DrawWireCube (areas [a].pos, temp);
				}
			} else {
				if (showSurround) {
					Gizmos.color = Color.yellow;
					Gizmos.DrawWireCube (areas [a].pos, temp / 4);
				}
			}
		}
	}

	void OnDrawGizmos() {
	}
		
}

public class gridSpace {
	public gridSpace front, back, left, right, up, down;
	public Vector3 pos;
	public int area;

	public gridSpace(bool defaultSetup) {
		pos = -Vector3.one;
		area = -1;
	}

	public gridSpace(Vector3 _pos, int _area) {
		pos = _pos;
		area = _area;
	}

	public gridSpace(Vector3 _pos, int _area, gridSpace left, gridSpace back, gridSpace down) {
		pos = _pos;
		area = _area;
	}

	public void setup(Vector3 _pos, int _area) {
		pos = _pos;
		area = _area;
	}
}