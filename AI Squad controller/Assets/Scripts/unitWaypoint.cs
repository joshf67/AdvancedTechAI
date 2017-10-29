using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class unitWaypoint : MonoBehaviour {

	public float pathCount;
	public float delay;
	public List<pathData> path = new List<pathData>();
	public Material line;
	public List<GameObject> pathObj = new List<GameObject>();
	public Cover[] cover;
	public List<unitWay> selected = new List<unitWay>();
	public Vector3 hitPos;
	public float radius = 0.1f;
	public bool scriptEnabled = false;

	public UIinputs main = null;

	void Start () {
		setupCover ();
		main = GameObject.FindObjectOfType<UIinputs> ();
	}

	// Update is called once per frame
	void Update () {
		if (scriptEnabled) {
			if (main.waypoint) {
				delay = GameObject.FindObjectOfType<UIinputs> ().time;
			}

			if (Input.GetMouseButton (0) && !EventSystem.current.IsPointerOverGameObject ()) {
				if (main.selection) {
					RaycastHit hit;
					if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit)) {
						GetComponent<LineRenderer> ().enabled = true;
						hitPos = hit.point + new Vector3 (0, 0.1f, 0);
					}
					radius += Time.unscaledDeltaTime;
					displayRadius ();
				}
			}

			if (Input.GetMouseButtonUp (0) && !EventSystem.current.IsPointerOverGameObject ()) {
				if (main.selection) {
					foreach (unitWay obj in selected) {
						obj.selected = false;
					}
					selected.Clear ();
					if (hitPos != -Vector3.one) {
						Collider[] temp = Physics.OverlapSphere (hitPos, radius);
						foreach (Collider obj in temp) {
							if (obj.GetComponent<unitWay> ()) {
								selected.Add (obj.GetComponent<unitWay> ());
								obj.GetComponent<unitWay> ().selected = true;
							}
						}
					}
					radius = 0.1f;
					GetComponent<LineRenderer> ().enabled = false;
				}
			}

			if (Input.GetMouseButtonUp (1) && !EventSystem.current.IsPointerOverGameObject ()) {
				if (main.waypoint) {
					if (path.Count != 0) {
						bool hasUnit = false;
						for (int a = 0; a < selected.Count; a++) {
							if (selected [a].GetComponent<unitWay> ().follow) {
								selected [a].GetComponent<unitWay> ().resetFollow ();
							}
							selected [a].GetComponent<unitWay> ().follow = true;
							if (selected [a].GetComponent<unitWay> ().main) {
								hasUnit = true;
								selected [a].GetComponent<unitWay> ().setPath (path, 0, 0);
							}
							selected [a].GetComponent<unitWay> ().coverPos = Vector3.zero;
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

			if (Input.GetMouseButtonDown (0) && !EventSystem.current.IsPointerOverGameObject ()) {
				if (main.waypoint) {
					RaycastHit hit;
					if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit)) {
						//add current pos + delay to path
						pathData tempPath = new pathData (hit.point, delay);
						path.Add (tempPath);

						//setup drawing of path
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
							findCoverForSelected (hit.point, hit.collider, selected, true, true, true);
						} else {
							resetCoverForSelected (hit.point, selected, true, true, true);
						}
					}
				} else if (main.follow) {
					RaycastHit hit;
					if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit)) {
						if (hit.collider.tag == "Unit") {
							foreach (unitWay obj in selected) {
								obj.setupFollower (hit.collider.gameObject);
							}
						}
					}
				}
			}
		}
	}

	public void left() {
		if (scriptEnabled) transform.Translate (Vector3.left);
	}

	public void right() {
		if (scriptEnabled) transform.Translate (Vector3.right);
	}

	public void up() {
		if (scriptEnabled) transform.Translate (Vector3.up);
	}

	public void down() {
		if (scriptEnabled) transform.Translate (Vector3.down);
	}

	public void requestCover(unitWay requester, bool requiresPath = true, bool includeChildren = false, bool resetFollow = false, List<int> ignoreIndex = null) {
		findCoverForSelected (requester.transform.position, requester.GetComponent<Collider>(), requester, requiresPath, includeChildren, resetFollow, ignoreIndex);
	}

	Vector3 findClosest (Vector3 temp, Collider other, Vector3 pos, out int _index, List<int> ignoreIndex = null) {
		//setup default values
		Vector3 ret = Vector3.zero;
		float curDist = float.PositiveInfinity;
		float navDist = 0;
		int index = 0;
		float danger = 1;
		Collider[] cols = null;
		NavMeshHit navMeshEdge;

		//setup offset for object height to floor
		Vector3 offset = (temp - other.transform.position) + new Vector3(0, other.bounds.size.y/2,0);
		//Debug.DrawLine (temp, temp - offset, Color.red, 10);

		//find nearest position on navmesh
		if (NavMesh.SamplePosition (temp - new Vector3 (0, offset.y, 0), out navMeshEdge, 2, NavMesh.AllAreas)) {

			//find all objects within distance to navmesh
			cols = Physics.OverlapSphere (navMeshEdge.position, 5);
			//Debug.DrawLine (temp, navMeshEdge.position, Color.green, 10);
			for (int a = 0; a < cols.Length; a++) {
				if (cols [a].tag == "CoverSpot") {
					bool ignore = false;
					if (ignoreIndex != null) {
						foreach (int i in ignoreIndex) {
							if (i == cols [a].GetComponent<Cover> ().index) {
								ignore = true;
							}
						}
					}

					//check if the object is cover
					if (!ignore) {
						Cover cov = cols [a].GetComponent<Cover> ();
						if (!Physics.Raycast (pos, cov.transform.position - pos, (cov.transform.position - pos).magnitude, 1 << 8)) {

							if (cov.free == true) {
								//check if the cover can be seen by enemies
								danger = pollDanger (cov);

								//create new navmesh path
								NavMeshPath path = new NavMeshPath ();
								if (NavMesh.CalculatePath (navMeshEdge.position, cov.pos, NavMesh.AllAreas, path)) {
									//check the navigation cost of the path
									navDist = 0;
									for (int b = 0; b < path.corners.Length - 1; b++) {
										navDist += Vector3.Distance (path.corners [b], path.corners [b + 1]);
									}

									//check if distance is less than current distance
									if (Vector3.Distance (cov.transform.position, temp) + navDist + danger < curDist) {
										ret = cov.transform.position;
										curDist = Vector3.Distance (cov.transform.position, temp) + danger + navDist;
										index = cov.index;
									}
								}
							}
						}
					}
				}
			}
		}
		//set cover to be full
		cover [index].free = false;
		_index = index;
        if (ret == Vector3.zero)
        {
            return Vector3.zero;
        }
		return ret - new Vector3 (0, 1, 0);
	}

	float pollDanger(Cover cover) {
		//loop through all enemies and check if they can see the object
		GameObject[] enemies = GameObject.FindGameObjectsWithTag ("Enemy");
		float _val = 0;
		Vector3 startScale = cover.gameObject.transform.lossyScale;
		foreach (GameObject enemy in enemies) {
			//test if the enemy can see the coverSpot
			if (Physics.Raycast (enemy.transform.position, cover.pos - enemy.transform.position, Vector3.Distance (enemy.transform.position, cover.transform.position), 1 << 8)) {
				Debug.DrawLine (enemy.transform.position, cover.pos, Color.green, 1);
			} else {
				_val += 1000;
				Debug.DrawLine (enemy.transform.position, cover.pos, Color.red, 1);
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
		//destroy visual waypoint
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
		//get all cover and give it it's index
		cover = GameObject.FindObjectsOfType<Cover> ();
		for (int a = 0; a < cover.Length; a++) {
			cover [a].index = a;
		}
	}

	public void findCoverForSelected(Vector3 hit, Collider otherCol, unitWay other, bool requiresPath = true, bool includeChildren = false, bool resetFollow = false, List<int> ignoreIndex = null) {
		List<unitWay> temp = new List<unitWay>();
		temp.Add (other);
		findCoverForSelected (hit, otherCol, temp, requiresPath, includeChildren, resetFollow, ignoreIndex);
	}

	public void findCoverForSelected(Vector3 hit, Collider otherCol, List<unitWay> others, bool requiresPath = true, bool includeChildren = false, bool resetFollow = false, List<int> ignoreIndex = null) {

		//resetCoverForSelected
		resetCoverForSelected (hit, others, requiresPath, includeChildren, resetFollow);

		//setup new list to check if object has been removed or not
		List<unitWay> temp = new List<unitWay>();

		//setup cover index
		int index = -1;

		//loop through all others and find cover
		for (int a = 0; a < others.Count; a++) {

			//loop through all selected object and find a cover
			Vector3 closest = findClosest (hit, otherCol, others [a].transform.position, out index, ignoreIndex);
			temp.Add (others [a]);

			//setup all cover data for unit
			others [a].coverPos = closest;
			others [a].index = index;

			if (includeChildren) {
				//loop through all selected children and setup cover data for them
				foreach (unitWay obj in others [a].returnAllFollowers()) {
					if (obj.GetComponent<unitWay> ().coverPos == Vector3.zero && !temp.Contains (obj)) {
						temp.Add (obj);
						Vector3 closest2 = findClosest (hit, otherCol, obj.transform.position, out index);
						obj.GetComponent<unitWay> ().coverPos = closest2;
						obj.GetComponent<unitWay> ().index = index;
					}
				}
			}
		}
	}

	public void resetCoverforselected(Vector3 hit, unitWay other, bool requiresPath = true, bool includeChildren = false, bool resetFollow = false) {

		//reset data for selected
		if (resetFollow) {
			other.resetFollow ();
			other.foundEnemyLastSeen.Clear ();
		}

		if (requiresPath) {
			pathData tempPath = new pathData (hit, 2);
			other.setPath(tempPath,0,0);
		}

		cover [other.index].free = true;
		other.coverPos = Vector3.zero;

		other.follow = true;

		//repeat for all children of selected
		if (includeChildren) {
			foreach (unitWay obj in other.returnAllFollowers()) {
				obj.coverPos = Vector3.zero;
				cover [obj.index].free = true;
			}
		}

	}

	public void resetCoverForSelected(Vector3 hit, List<unitWay> other, bool requiresPath = true, bool includeChildren = false, bool resetFollow = false)
	{
		foreach (unitWay obj in other) {
			resetCoverforselected (hit, obj, requiresPath, includeChildren, resetFollow);
		}
	}
}

[System.Serializable]
public class pathData{
	public Vector3 path;
	public float speed;

	public pathData(Vector3 _path, float _speed) {
		path = _path;
		speed = _speed;
	}
}































/*
	public void requestCover(GameObject requester) {
		List<GameObject> temp = new List<GameObject> ();
		temp = requester.GetComponent<unitWay> ().returnAllFollowers ();
		temp.Add (requester);
		findCoverForSelected (requester.transform.position, requester.GetComponent<Collider>(), temp);
	} 

	public void resetCover(unitWay requester) {
		List<unitWay> temp = new List<unitWay> ();
		temp = requester.GetComponent<unitWay> ().returnAllFollowers ();
		temp.Add (requester);
		findCoverForSelected (requester.transform.position, requester.GetComponent<Collider>(), temp);
	} */


/*public void resetCoverForSelected(Vector3 hit, unitWay other, bool includeChildren, bool hasEnemy) {

		//reset game object path
		other.crouch = false;
		if (other.follow) {
			other.resetFollow ();
		}

		//set cover data to blank and set destination to click
		cover [other.index].free = true;
		other.coverPos = Vector3.zero;
		other.forceMove = true;
		other.inCover = false;
		other.followingPath = true;
		other.follow = true;

		if (hasEnemy) {
			//setup new position for unit
			List<pathData> tempPath = new List<pathData> ();
			pathData tempPathData = new pathData ();
			tempPathData.speed = 2;
			tempPathData.path = hit;
			tempPath.Add(tempPathData);
			other.setPath (tempPath, 0, 0);
			other.foundEnemyLastSeen = Vector3.zero;
		}


		//reset cover data for children
		if (includeChildren) {
			foreach (unitWay obj in other.returnAllFollowers()) {
				obj.GetComponent<unitWay> ().pos = Vector3.zero;
				cover [obj.GetComponent<unitWay> ().index].free = true;
			}
		}

	}*/




/*void findCoverForSelected(Vector3 hit, Collider other, List<unitWay> _selected) {
//reset cover for the selected object
resetCoverForSelected (hit, _selected);

//setup new list to check if object has been removed or not
List<unitWay> temp = new List<unitWay>();
for (int a = 0; a < _selected.Count; a++) {

	//loop through all selected object and find a cover
	int index = 0;
	Vector3 closest = findClosest (hit, other, _selected [a].transform.position, out index);
	temp.Add (_selected [a]);
	_selected [a].forceMove = false;

	//setup all cover data for unit
	_selected [a].coverPos = closest;
	_selected [a].index = index;
	if (_selected [a].follow) {
		_selected [a].resetFollow ();
	}

	//loop through all selected children and setup cover data for them
	foreach (unitWay obj in _selected [a].GetComponent<unitWay>().returnAllFollowers()) {
		if (obj.GetComponent<unitWay> ().coverPos == Vector3.zero && !temp.Contains (obj)) {
			temp.Add (obj);
			Vector3 closest2 = findClosest (hit, other, obj.transform.position, out index);
			obj.GetComponent<unitWay> ().coverPos = closest2;
			obj.GetComponent<unitWay> ().index = index;
		}
	}
}
} */



/*public void resetCoverForSelected(List<GameObject> _selected) {
for (int a = 0; a < _selected.Count; a++) {
	cover [_selected [a].GetComponent<unitWay> ().index].free = true;
	_selected [a].GetComponent<unitWay> ().index = -1;
	_selected [a].GetComponent<unitWay> ().coverPos = Vector3.zero;
}
}*/