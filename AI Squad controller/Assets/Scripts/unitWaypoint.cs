using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class unitWaypoint : MonoBehaviour {

	public float pathCount;
	public float delay;
	public List<Vector3> path = new List<Vector3>();
	public Material line;
	public List<GameObject> pathObj = new List<GameObject>();
	public Cover[] cover;
	public List<GameObject> selected = new List<GameObject>();
	public Vector3 hitPos;
	public float radius = 0.1f;

	public UIinputs main = null;

	void Start () {
		setupCover ();
		main = GameObject.FindObjectOfType<UIinputs> ();
	}

	// Update is called once per frame
	void Update () {

		if (Input.GetMouseButton (0)) {
			if (main.selection) {
				RaycastHit hit;
				if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit)) {
					GetComponent<LineRenderer> ().enabled = true;
					hitPos = hit.point + new Vector3 (0, 0.1f, 0);
				}
				radius += Time.deltaTime;
				displayRadius ();
			}
		}

		if (Input.GetMouseButtonUp (0)) {
			if (main.selection) {
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
		}

		if (Input.GetMouseButtonUp (1)) {
			if (main.waypoint) {
				if (path.Count != 0) {
					GameObject[] objects = GameObject.FindGameObjectsWithTag ("Unit");
					bool hasUnit = false;
					delay = GameObject.FindObjectOfType<UIinputs> ().time;
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

		if (Input.GetMouseButtonDown (0)) {
			if (main.waypoint) {
				RaycastHit hit;
				if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit)) {
					path.Add (hit.point);
					GameObject temp = GameObject.CreatePrimitive (PrimitiveType.Cube);
					temp.transform.position = hit.point + new Vector3 (0, temp.transform.localScale.y, 0);
					temp.transform.localScale /= 4;
					temp.GetComponent<Renderer> ().material.color = new Color ((pathCount + 1) / 10, 10 / (pathCount + 1), 0);
					temp.GetComponent<Collider> ().enabled = false;
					temp.AddComponent <LineRenderer> ();
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
			} else if (main.move) {
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
								if (selected [a].GetComponent<unitWay> ().follow) {
									selected [a].GetComponent<unitWay> ().resetFollow ();
								}
								foreach (GameObject obj in selected [a].GetComponent<unitWay>().returnAllFollowers()) {
									if (!selected.Contains (obj) && obj.GetComponent<unitWay>().pos == Vector3.zero) {
										Vector3 closest2 = findClosest (hit.point, obj.transform.position, out index);
										obj.GetComponent<NavMeshAgent> ().SetDestination (closest2);
										obj.GetComponent<unitWay> ().pos = closest2;
										obj.GetComponent<unitWay> ().index = index;
									}
								}
							}
						}
					} else {
						for (int a = 0; a < selected.Count; a++) {
							selected [a].GetComponent<unitWay> ().crouch = false;
							if (selected [a].GetComponent<unitWay> ().follow) {
								selected [a].GetComponent<unitWay> ().resetFollow ();
							}
							cover [selected [a].GetComponent<unitWay> ().index].free = true;
							selected [a].GetComponent<unitWay> ().pos = Vector3.zero;
							selected [a].GetComponent<NavMeshAgent> ().SetDestination (hit.point);

							foreach (GameObject obj in selected [a].GetComponent<unitWay>().returnAllFollowers()) {
								obj.GetComponent<unitWay> ().pos = Vector3.zero;
								cover [obj.GetComponent<unitWay> ().index].free = true;
							}
						}
					}
				}
			} else if (main.follow) {
				RaycastHit hit;
				if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit)) {
					if (hit.collider.tag == "Unit") {
						foreach (GameObject obj in selected) {
							obj.GetComponent<unitWay> ().setupFollower(hit.collider.gameObject);
						}
					}
				}
			}
		}
	}

	Vector3 findClosest (Vector3 temp, Vector3 pos, out int _index) {
		Vector3 ret = Vector3.zero;
		float curDist = float.PositiveInfinity;
		float navDist = 0;
		int index = 0;
		float danger = 1;
		for (int a = 0; a < cover.Length; a++) {
			if (cover [a].free == true) {
				danger = pollDanger (cover [a]);
				NavMeshPath path = new NavMeshPath();
				if (NavMesh.CalculatePath(pos, cover [a].pos, NavMesh.AllAreas, path)) {
					navDist = 0;
					for (int b = 0; b < path.corners.Length - 1; b++) {
						navDist += Vector3.Distance (path.corners [b], path.corners [b + 1]);
					}
					if (Vector3.Distance (cover [a].transform.position, temp) /*+ navDist*/ + danger < curDist) {
						ret = cover [a].transform.position;
						curDist = Vector3.Distance (cover [a].transform.position, temp) + danger;
						index = a;
					}
				}
			}
		}
		cover [index].free = false;
		_index = index;
		return ret - new Vector3 (0, 1, 0);
	}

	float pollDanger(Cover cover) {
		GameObject[] enemies = GameObject.FindGameObjectsWithTag ("Enemy");
		float _val = 1;
		RaycastHit[] hit;
		foreach (GameObject enemy in enemies) {
			hit = Physics.RaycastAll (enemy.transform.position, cover.pos - enemy.transform.position, Vector3.Distance(enemy.transform.position, cover.transform.position));
			if (hit.Length != 0) {
				bool obscured = false;
				foreach (RaycastHit hi in hit) {
					if (hi.collider.tag != "Unit" && hi.collider.tag != "CoverSpot" && hi.collider != enemy.GetComponent<Collider>()) {
						obscured = true;
						Debug.DrawLine (enemy.transform.position, cover.pos, Color.green, 1);
					}
				}
				if (obscured == false) {
					Debug.DrawLine (enemy.transform.position, cover.pos, Color.red, 1);
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
		cover = GameObject.FindObjectsOfType<Cover> ();
	}
}