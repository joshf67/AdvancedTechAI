using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class customGrid : MonoBehaviour {

	//public List<gridSection> mapGrid = new List<gridSection>();
	public List<List<List<List<gridSection>>>> mapGrid = new List<List<List<List<gridSection>>>> ();
	List<gridSection> check = new List<gridSection> ();
	gridSection tempSection = new gridSection (new Vector3(-1,-1,-1), false, -1);
	public float radius;
	public float maxHeight;
	public float iterations = 0;
	public float ignored = 0;
	public float Checked = 0;
	bool done = false;
	float time = 0;

	// Use this for initialization
	void Start () {
		Collider col = GetComponent<Collider> ();
		Vector3 size = col.bounds.size;
		for (int a = 0; a < size.x; a++) {
			mapGrid.Add (new List<List<List<gridSection>>>());
			for (int b = 0; b < size.z; b++) {
				mapGrid[a].Add (new List<List<gridSection>>());
				for (int c = 0; c < size.y; c++) {
					mapGrid[a][b].Add (new List<gridSection>());
				}
			}
		}

		RaycastHit hit = new RaycastHit();
		GameObject[] objsInScene = GameObject.FindObjectsOfType<GameObject> ();
		for (int a = 0; a < objsInScene.Length; a++) {
			if (objsInScene [a].GetComponent<Collider> ()) {
				Collider objCol = objsInScene [a].GetComponent<Collider> ();
				if (objCol.bounds.min.x > 0 && objCol.bounds.min.y > 0 && objCol.bounds.min.z > 0) {
					Vector3 rand = objCol.bounds.center + new Vector3 (0, objCol.bounds.size.y, 0);
					if (Physics.Raycast (rand, Vector3.down, out hit)) {
						check.Add (new gridSection (hit.point + new Vector3(0, radius, 0), true, 0));
						Debug.Log ("start");
					}
				}
			}
		}

		iterations = 500 * check.Count;
		time = Time.realtimeSinceStartup;
		spawn ();
		done = true;

	}

	gridSection find(Vector3 _pos) {
		foreach (gridSection grid in mapGrid[Mathf.FloorToInt(_pos.x)][Mathf.FloorToInt(_pos.y)][Mathf.FloorToInt(_pos.z)]) {
			if (Vector3.Distance (grid.gridPos, _pos) < radius) {
				return grid;
			}
		}
		foreach (gridSection grid in check) {
			if (Vector3.Distance (grid.gridPos, _pos) < radius) {
				return grid;
			}
		}
		return new gridSection(new Vector3(0,0,0), false, -1);
	}

	bool exists(Vector3 _pos) {
		foreach (gridSection grid in mapGrid[Mathf.FloorToInt(_pos.x)][Mathf.FloorToInt(_pos.y)][Mathf.FloorToInt(_pos.z)]) {
			if (grid == _pos) {
				return true;
			}
		}
		return false;
	}

	void spawn() {
		RaycastHit hit;
		while (check.Count != 0) {
			gridSection exist = find (check [0].gridPos);
			if (exist.pos != Vector3.zero) {
				//Debug.Log ("Ignored");
				ignored++;
				exist.addNeighbours (check [0].neighbours);
			} else {
				Checked++;
				//Debug.Log ("Checked");
				if (Physics.Raycast (check [0].gridPos, Vector3.down, maxHeight) || check [0].floorDistance < maxHeight || check[0].neighbours.climable) {
					Vector3 effect = check [0].gridPos;

					//check if inside obj
					if (Physics.SphereCast(check [0].gridPos, 0.1f, Vector3.forward, out hit)) {
						check.RemoveAt (0);
						//return;
						continue;
					}

					//down
					if (check [0].neighbours.down == null) {
						if (!Physics.Raycast (check [0].gridPos, Vector3.down, maxHeight)) {
							if (!checkSurroundingClimable (check [0].gridPos)) {
								check.RemoveAt (0);
								//return;
								continue;
							} else {
								tempSection = ScriptableObject.CreateInstance <gridSection> ();
								tempSection.setup (check [0].gridPos + (Vector3.down * radius), false, check [0].floorDistance - radius);
								tempSection.neighbours.up = check [0];
								check.Add (tempSection);
							}
						} else {
							Physics.Raycast (check [0].gridPos, Vector3.down, out hit, maxHeight);
							check [0].floorDistance = hit.distance;
							check [0].neighbours.objsDown.Add (hit.collider.gameObject);
							if (check [0].floorDistance <= radius) {
								check [0].floor = true;
								effect.y = hit.point.y + radius;
								//if (check [0].pos != new Vector3 (check [0].pos.x, hit.point.y + radius, check [0].pos.z)) {
									//check [0].pos = new Vector3 (check [0].pos.x, hit.point.y + radius, check [0].pos.z);
									//if (check [0].neighbours.up != null) {
										//if (((gridSection)check [0].neighbours.up).pos.y != check[0].pos.y + radius) {
											//setNeighboursValues (check [0]);
											//correctYValue (check [0]);
										//}
									//}
								//}
							} else if (check [0].floorDistance < maxHeight) {
								tempSection = ScriptableObject.CreateInstance <gridSection> ();
								tempSection.setup (check [0].gridPos + (Vector3.down * radius), false, check [0].floorDistance - radius);
								tempSection.neighbours.up = check [0];
								check.Add (tempSection);
							}
								
						}
					} else {
						if (!((gridSection)(check [0].neighbours.down)).neighbours.up) {
							((gridSection)(check [0].neighbours.down)).neighbours.up = check [0];
						}
					}

					//forawrd
					if (check [0].neighbours.forward == null) {
						if (!Physics.Raycast (check [0].gridPos, Vector3.forward, out hit, radius)) {
							tempSection = ScriptableObject.CreateInstance <gridSection> ();
							tempSection.setup (check [0].gridPos + (Vector3.forward * radius), false, check [0].floorDistance);
							tempSection.neighbours.back = check [0];
							check.Add (tempSection);
						} else {
							check [0].neighbours.objsForward.Add (hit.collider.gameObject);
							effect.z = hit.point.z - radius;
						}
					} else {
						if (!((gridSection)(check [0].neighbours.forward)).neighbours.back) {
							((gridSection)(check [0].neighbours.forward)).neighbours.back = check [0];
						}
					}

					//back
					if (check [0].neighbours.back == null) {
						if (!Physics.Raycast (check [0].gridPos, Vector3.back, out hit, radius)) {
							tempSection = ScriptableObject.CreateInstance <gridSection> ();
							tempSection.setup (check [0].gridPos + (Vector3.back * radius), false, check [0].floorDistance);
							tempSection.neighbours.forward = check [0];
							check.Add (tempSection);
						} else {
							check [0].neighbours.objsBack.Add (hit.collider.gameObject);
							effect.z = hit.point.z + radius;
						}
					} else {
						if (!((gridSection)(check [0].neighbours.back)).neighbours.forward) {
							((gridSection)(check [0].neighbours.back)).neighbours.forward = check [0];
						}
					}

					//left
					if (check [0].neighbours.left == null) {
						if (!Physics.Raycast (check [0].gridPos, Vector3.left, out hit, radius)) {
							tempSection = ScriptableObject.CreateInstance <gridSection> ();
							tempSection.setup (check [0].gridPos + (Vector3.left * radius), false, check [0].floorDistance);
							tempSection.neighbours.right = check [0];
							check.Add (tempSection);
						} else {
							check [0].neighbours.objsLeft.Add (hit.collider.gameObject);
							effect.x = hit.point.x + radius;
						}
					} else {
						if (!((gridSection)(check [0].neighbours.left)).neighbours.right) {
							((gridSection)(check [0].neighbours.left)).neighbours.right = check [0];
						}
					}

					//right
					if (check [0].neighbours.right == null) {
						if (!Physics.Raycast (check [0].gridPos, Vector3.right, out hit, radius)) {
							tempSection = ScriptableObject.CreateInstance <gridSection> ();
							tempSection.setup (check [0].gridPos + (Vector3.right * radius), false, check [0].floorDistance);
							tempSection.neighbours.left = check [0];
							check.Add (tempSection);
						} else {
							check [0].neighbours.objsRight.Add (hit.collider.gameObject);
							effect.x = hit.point.x - radius;
						}
					} else {
						if (!((gridSection)(check [0].neighbours.right)).neighbours.left) {
							((gridSection)(check [0].neighbours.right)).neighbours.left = check [0];
						}
					}

					check[0].neighbours.climable = checkNeighbourObjs (check[0]);

					//up
					if (check [0].neighbours.up == null || check[0].neighbours.climable) {
						if (!Physics.Raycast (check [0].gridPos, Vector3.up, out hit, radius)) {
							tempSection = ScriptableObject.CreateInstance <gridSection> ();
							tempSection.setup (check [0].gridPos + (Vector3.up * radius), false, check [0].floorDistance + radius);
							tempSection.neighbours.down = check [0];
							tempSection.neighbours.climable = check[0].neighbours.climable;
							check.Add (tempSection);
						} else {
							check [0].neighbours.objsUp.Add (hit.collider.gameObject);
							effect.y = hit.point.y - radius;
						}
					} else {
						if (!((gridSection)(check [0].neighbours.up)).neighbours.down) {
							((gridSection)(check [0].neighbours.up)).neighbours.down = check [0];
						}
					}
						
					mapGrid[Mathf.FloorToInt(check [0].gridPos.x)][Mathf.FloorToInt(check [0].gridPos.y)][Mathf.FloorToInt(check [0].gridPos.z)].Add (check [0]);
					check [0].pos = effect;
					//mapGrid.Add (check [0]);
				}
			}
			check.RemoveAt (0);
		}
	}

	bool checkSurroundingClimable(Vector3 pos) {
		RaycastHit hit;
		if (Physics.Raycast(pos, Vector3.right, out hit, radius)) {
			if (hit.transform.tag == "Climable") {
				return true;
			}
		}
		if (Physics.Raycast(pos, Vector3.left, out hit, radius)) {
			if (hit.transform.tag == "Climable") {
				return true;
			}
		}
		if (Physics.Raycast(pos, Vector3.forward, out hit, radius)) {
			if (hit.transform.tag == "Climable") {
				return true;
			}
		}
		if (Physics.Raycast(pos, Vector3.back, out hit, radius)) {
			if (hit.transform.tag == "Climable") {
				return true;
			}
		}
		if (Physics.Raycast (pos, Vector3.up, out hit, radius)) {
			if (hit.transform.tag == "Hangable") {
				return true;
			}
		}
		return false;
	}

	void correctYValue (gridSection grid) {
		gridSection above = (gridSection)grid.neighbours.up;
		gridSection current = grid;
		Debug.Log ("Checking");
		while (above != null) {

			above.pos = current.pos + new Vector3 (0, radius, 0);
			current = above;

			if (above.neighbours.up != null) {
				above = (gridSection)above.neighbours.up;
			} else {
				return;
			}
		}
	}

	void setNeighboursValues(gridSection grid) {
		if (grid.neighbours.forward != null) {
			((gridSection)grid.neighbours.forward).pos = grid.pos;
		}

		if (grid.neighbours.back != null) {
			((gridSection)grid.neighbours.back).pos = grid.pos;
		}

		if (grid.neighbours.left != null) {
			((gridSection)grid.neighbours.left).pos = grid.pos;
		}

		if (grid.neighbours.right != null) {
			((gridSection)grid.neighbours.right).pos = grid.pos;
		}
	}

	bool checkNeighbourObjs(gridSection check) {
		if (check.neighbours.objsRight.Count != 0) {
			if (checkClimable (check.neighbours.objsRight)) {
				return true;
			}
		}
		if (check.neighbours.objsLeft.Count != 0) {
			if (checkClimable (check.neighbours.objsLeft)) {
				return true;
			}
		}
		if (check.neighbours.objsForward.Count != 0) {
			if (checkClimable (check.neighbours.objsForward)) {
				return true;
			}
		}
		if (check.neighbours.objsBack.Count != 0) {
			if (checkClimable (check.neighbours.objsBack)) {
				return true;
			}
		}
		return false;
	}

	bool checkClimable(List<GameObject> objs) {
		foreach (GameObject obj in objs) {
			if (obj.transform.tag == "Climable") {
				return true;
			}
		}
		return false;
	}

	
	// Update is called once per frame
	void Update () {

		if (done) {
			time = Time.realtimeSinceStartup - time;
			Debug.Log ("Time taken: " + time + " seconds");
			Debug.Log ("ignored: " + ignored);
			Debug.Log ("checked: " + Checked);
			float gridPos = 0;
			foreach (List<List<List<gridSection>>> gridSecx in mapGrid) {
				foreach (List<List<gridSection>> gridSecy in gridSecx) {
					foreach (List<gridSection> gridSecz in gridSecy) {
						foreach (gridSection grid in gridSecz) {
							gridPos++;
						}
					}
				}

			}

			Debug.Log ("created grid with: " + gridPos + " positions");
			done = false;
		}
		/////////////////////////////////////////
		//int i = 0;
		//while (i < iterations) {
		//	spawn ();
		//	i++;
		//}

		/////////////////////////////////////////
		/*if (check.Count != 0) {
			foreach (List<List<List<gridSection>>> gridSecx in mapGrid) {
				foreach (List<List<gridSection>> gridSecy in gridSecx) {
					foreach (List<gridSection> gridSecz in gridSecy) {
						foreach (gridSection grid in gridSecz) {
							if (grid.temp == 0) {
								grid.temp = -1;
								Debug.DrawRay (grid.pos, Vector3.up * radius, Color.yellow, 0.5f);
								Debug.DrawRay (grid.pos, Vector3.down * radius, Color.white, 0.5f);
								Debug.DrawRay (grid.pos, Vector3.right * radius, Color.red, 0.5f);
								Debug.DrawRay (grid.pos, Vector3.left * radius, Color.green, 0.5f);
								Debug.DrawRay (grid.pos, Vector3.forward * radius, Color.magenta, 0.5f);
								Debug.DrawRay (grid.pos, Vector3.back * radius, Color.blue, 0.5f);
							}
						}
					}
				}
			}
		}*/
	}

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.green;
		foreach (List<List<List<gridSection>>> gridSecx in mapGrid) {
			foreach (List<List<gridSection>> gridSecy in gridSecx) {
				foreach (List<gridSection> gridSecz in gridSecy) {
					foreach (gridSection grid in gridSecz) {
						if (check.Count == 0 || grid.temp == 0) {
							Gizmos.DrawSphere (grid.gridPos, radius);
							grid.temp = -1;
						}
					}
				}
			}
			
		}
	}

}

public class gridSection : ScriptableObject {
	public Neighbours neighbours;
	public Vector3 pos;
	public Vector3 gridPos;
	public bool floor;
	public float floorDistance;
	public float temp;

	public gridSection (Vector3 _pos, bool _floor, float _floorDistance) {
		gridPos = _pos;
		floor = _floor;
		floorDistance = _floorDistance;
		neighbours = new Neighbours ();
	}

	public void setup (Vector3 _pos, bool _floor, float _floorDistance) {
		gridPos = _pos;
		floor = _floor;
		floorDistance = _floorDistance;
		neighbours = new Neighbours ();
	}

	public void addNeighbours(Neighbours other) {
		if (other.up) {
			neighbours.up = other.up;
		}
		if (other.down) {
			neighbours.down = other.down;
		}
		if (other.back) {
			neighbours.back = other.back;
		}
		if (other.forward) {
			neighbours.forward = other.forward;
		}
		if (other.left) {
			neighbours.left = other.left;
		}
		if (other.right) {
			neighbours.right = other.right;
		}
	}

	public static bool within(gridSection self, gridSection other, float dist) {
		return ((self.gridPos - other.gridPos).sqrMagnitude <= dist && (self.gridPos - other.gridPos).sqrMagnitude >= -dist);
	}

	public static bool operator == (gridSection self, Vector3 other) {
		return (self.gridPos == other);
	}

	public static bool operator != (gridSection self, Vector3 other) {
		return (self.gridPos != other);
	}

}

public class Neighbours {
	public ScriptableObject up, down, forward, back, left, right;
	public List<GameObject> objsUp, objsDown, objsForward, objsBack, objsLeft, objsRight;
	public bool climable;

	public Neighbours () {
		climable = false;
		objsUp = new List<GameObject> ();
		objsDown = new List<GameObject> ();
		objsRight = new List<GameObject> ();
		objsLeft = new List<GameObject> ();
		objsForward = new List<GameObject> ();
		objsBack = new List<GameObject> ();
	}
}