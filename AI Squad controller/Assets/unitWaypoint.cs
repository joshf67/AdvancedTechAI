using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class unitWaypoint : MonoBehaviour {

	public float pathCount;
	public float delay;
	public bool addingWaypoints = false;
	public List<Vector3> path = new List<Vector3>();
	public Material line;
	public List<GameObject> pathObj = new List<GameObject>();
	public GameObject[] cover;
	public bool[] coverOcc;
	public List<GameObject> selected = new List<GameObject>();
	public Vector3 hitPos;
	public float radius = 0.1f;

	void Start () {
		setupCover ();
	}

	// Update is called once per frame
	void Update () {

		if (Input.GetMouseButtonDown (1)) {
			if (selected.Count == 0) {
				addingWaypoints = !addingWaypoints;
			} else {
				RaycastHit hit;
				if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit)) {
					if (hit.collider.tag == "Cover") {
						for (int a = 0; a < selected.Count; a++) {
							if (!selected [a].GetComponent<unitWay> ().crouch) {
								int index = 0;
								Vector3 closest = findClosest (hit.point, selected [a].transform.position, out index);
								selected [a].GetComponent<NavMeshAgent> ().SetDestination (closest);
								selected [a].GetComponent<unitWay> ().pos = closest;
								selected [a].GetComponent<unitWay> ().index = index;
							}
						}
					} else {
						for (int a = 0; a < selected.Count; a++) {
							selected [a].GetComponent<unitWay> ().crouch = false;
							coverOcc [selected [a].GetComponent<unitWay> ().index] = false;
							selected [a].GetComponent<unitWay> ().pos = Vector3.zero;
							selected [a].GetComponent<NavMeshAgent> ().SetDestination (hit.point);
						}
					}
				}
			}
		}

		if (Input.GetMouseButton (2)) {
			RaycastHit hit;
			if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit)) {
				GetComponent<LineRenderer> ().enabled = true;
				hitPos = hit.point + new Vector3(0, 0.1f, 0);
			}
			radius += Time.deltaTime;
			displayRadius ();
		}

		if (Input.GetMouseButtonUp (2)) {
			foreach (GameObject obj in selected) {
				obj.GetComponent<unitWay> ().selected = false;
			}
			selected.Clear ();
			if (hitPos != -Vector3.one) {
				Collider[] temp = Physics.OverlapSphere (hitPos, radius);
				foreach (Collider obj in temp) {
					if (obj.GetComponent<unitWay> ()) {
						selected.Add (obj.gameObject);
						obj.GetComponent<unitWay> ().selected = true;
					}
				}
			}
			radius = 1;
			GetComponent<LineRenderer> ().enabled = false;
		}

		if (Input.GetMouseButtonDown (0)) {
			RaycastHit hit;
			Camera[] cam = Camera.allCameras;
			Ray mousePoint = cam [0].ScreenPointToRay (Input.mousePosition);
			if (addingWaypoints) {
				if (Physics.Raycast (mousePoint, out hit)) {
					path.Add (hit.point);
					GameObject temp = GameObject.CreatePrimitive (PrimitiveType.Cube);
					temp.transform.position = hit.point + new Vector3 (0, temp.transform.localScale.y, 0);
					temp.transform.localScale /= 4;
					temp.GetComponent<Renderer> ().material.color = new Color ((pathCount + 1) / 10, 10 / (pathCount + 1), 0);
					temp.GetComponent<Collider> ().enabled = false;
					temp.AddComponent <LineRenderer>();
					temp.GetComponent<LineRenderer> ().startWidth = 0.1f;
					temp.GetComponent<LineRenderer> ().endWidth = 0.1f;
					temp.GetComponent<LineRenderer> ().startColor = Color.green;
					temp.GetComponent<LineRenderer> ().endColor = Color.red;
					temp.GetComponent<LineRenderer> ().material = line;
					pathObj.Add (temp);
					pathCount = path.Count;
					if (pathCount != 1) {
						setupPath ();
					}
				}
			} else {
				if (path.Count != 0) {
					GameObject[] objects = GameObject.FindGameObjectsWithTag ("Unit");
					bool hasUnit = false;
					for (int a = 0; a < objects.Length; a++) {
						if (objects [a].GetComponent<unitWay> ().selected) {
							if (objects [a].GetComponent<unitWay> ().follow) {
								objects [a].GetComponent<unitWay> ().resetFollow ();
							}
							if (objects [a].GetComponent<unitWay> ().main) {
								hasUnit = true;
								objects [a].GetComponent<unitWay> ().setPath (path, delay, -delay);
							}
							objects [a].GetComponent<unitWay> ().pos = Vector3.zero;
						}
					}
					if (hasUnit) {
						path.Clear ();
					} else {
						path.Clear ();
						for (int a = 0; a < pathObj.Count; a++) {
							Destroy (pathObj [a]);
						}
						pathObj.Clear ();
					}
				}
			}
		}
	}

	Vector3 findClosest (Vector3 temp, Vector3 pos, out int _index) {
		Vector3 ret = Vector3.zero;
		float curDist = float.PositiveInfinity;
		int index = 0;
		float danger = 1;
		for (int a = 0; a < cover.Length; a++) {
			if (coverOcc [a] == false) {
				danger = pollDanger (cover [a]);
				if (Vector3.Distance (cover [a].transform.position, temp) + danger < curDist) {
					ret = cover [a].transform.position;
					curDist = Vector3.Distance (cover [a].transform.position, temp) + danger;
					index = a;
				}
			}
		}
		coverOcc [index] = true;
		_index = index;
		return ret - new Vector3 (0, 1, 0);
	}

	float pollDanger(GameObject cover) {
		GameObject[] enemies = GameObject.FindGameObjectsWithTag ("Enemy");
		float _val = 1;
		RaycastHit[] hit;
		foreach (GameObject enemy in enemies) {
			hit = Physics.RaycastAll (enemy.transform.position, cover.transform.position - enemy.transform.position, Vector3.Distance(enemy.transform.position, cover.transform.position));
			if (hit.Length != 0) {
				bool obscured = false;
				foreach (RaycastHit hi in hit) {
					if (hi.collider.tag != "Unit" && hi.collider.tag != "CoverSpot" && hi.collider != enemy.GetComponent<Collider>()) {
						obscured = true;
						Debug.DrawLine (enemy.transform.position, cover.transform.position, Color.green, 10);
					}
				}
				if (obscured == false) {
					Debug.DrawLine (enemy.transform.position, cover.transform.position, Color.red, 10);
					_val += 1000;
				}
			}
		}
		return _val;
	}

	void setupPath() {
		for (int a = 0; a < pathObj.Count; a++) {
			if (a != pathObj.Count - 1) {
				pathObj [a].GetComponent<LineRenderer> ().SetPosition (0, pathObj [a].transform.position);
				pathObj [a].GetComponent<LineRenderer> ().SetPosition (1, pathObj [a+1].transform.position);
			}
		}
	}

	public void destroyPathObj(float _pathCount) {
		if (_pathCount <= pathObj.Count) {
			if (pathObj.Count != 0 && _pathCount != pathObj.Count) {
				Destroy (pathObj [0]);
				pathObj.RemoveAt (0);
			}
			if (_pathCount == 1) {
				Destroy (pathObj [0]);
				pathObj.RemoveAt (0);
			}
		}
	}

	protected void displayRadius()
	{
		int i = 0;
		float x = radius * Mathf.Cos (0);
		float y = radius * Mathf.Sin (0);
		LineRenderer lr = GetComponent<LineRenderer> ();

		lr.positionCount = Mathf.CeilToInt ((2 * Mathf.PI) / 0.1f);
		lr.enabled = true;

		//render radius of selection
		for (float theta = 0; theta < 2 * Mathf.PI; theta += 0.1f) {
			x = radius * Mathf.Cos (theta);
			y = radius * Mathf.Sin (theta);

			Vector3 pos = new Vector3 (x, 0, y);
			lr.SetPosition (i, pos + hitPos);
			i += 1;
		}
	}

	void setupCover() {
		cover = GameObject.FindGameObjectsWithTag ("CoverSpot");
		coverOcc = new bool[cover.Length];
		for(int a = 0; a < coverOcc.Length; a++) {
			coverOcc[a] = false;
		}
		/*
		GameObject[] temp = GameObject.FindGameObjectsWithTag ("Cover");
		foreach (GameObject obj in temp) {
			GameObject n = GameObject.CreatePrimitive (PrimitiveType.Cube);
			List<Vector3> verticies = new List<Vector3>();
			obj.GetComponent<MeshFilter>().mesh.GetVertices(verticies);
			NavMeshHit hit;
			for (int a = 0; a < verticies.Count; a++) {
				if (NavMesh.SamplePosition (verticies[a] + obj.tr, out hit, 2.0f, NavMesh.AllAreas)) {
					n.transform.position = hit.position + new Vector3 (0, 1, 0);
					n.GetComponent<BoxCollider> ().isTrigger = true;
					//n.transform.localScale = 4;
					cover.Add (n);
					coverOcc.Add (false);
				}
			}

		}


		
		foreach (GameObject obj in temp) {
			NavMeshHit hit;
			if (NavMesh.SamplePosition(obj.transform.position, out hit, 2.0f, NavMesh.AllAreas))
			{
				float baseOff = 0.5f;
				float offset = 0;
				while (obj.transform.lossyScale.z - 1 >= offset) {
					GameObject n = GameObject.CreatePrimitive (PrimitiveType.Cube);
					n.transform.position = hit.position + new Vector3 (0, n.transform.localScale.y, offset - obj.transform.lossyScale.z/2 + (baseOff/2));
					n.transform.localScale /= 4;
					offset += baseOff;
					n.tag = "CoverSpot";
					//GameObject.Destroy(n.GetComponent<BoxCollider>());
					n.GetComponent<BoxCollider>().isTrigger = true;
					cover.Add (n);
					coverOcc.Add (false);
				}
			}
		}
		*/
	}
}