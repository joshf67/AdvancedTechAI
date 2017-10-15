using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class unitWay : MonoBehaviour {

	public bool follow = false;
	public GameObject enemyTarget = null;
	public GameObject target = null;
	public List<GameObject> follower = null;
	public List<Vector3> path = new List<Vector3>();
	public float minDist = 0;
	public float totalDelay = 0;
	public float delay = 0;
	public bool followingPath = false;
	public Vector3 prevDest;
	public bool main = true;
	public float followingCount = 0;
	public bool selected = false;
	public bool crouch = false;
	public Vector3 pos = Vector3.zero;
	public int index = 0;
	public float fire = 0;
	public float startSpeed = 0;
	public float crouchCoolDown = 0;

	// Use this for initialization
	void Start () {
		GetComponent<NavMeshAgent> ().stoppingDistance = minDist;
		startSpeed = GetComponent<NavMeshAgent> ().speed;
		//deal with setting up following count for distance checks
		if (target != null) {
			follow = true;
			GameObject followCount = target;
			followingCount++;
			while (followCount.GetComponent<unitWay> ().follow) {
				if (followCount.GetComponent<unitWay> ().target != null) {
					followCount = followCount.GetComponent<unitWay> ().target;
					followingCount++;
				} else {
					break;
				}
			}
			main = false;
			target.GetComponent<unitWay> ().follower.Add(gameObject);
		} else {
			follow = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!enemyTarget) {
			GameObject[] enemies = GameObject.FindGameObjectsWithTag ("Enemy");
			float curDist = Mathf.Infinity;
			GameObject enemyVar = null;
			foreach (GameObject enemy in enemies) {
				Vector3 dist = enemy.transform.position - transform.position;
				float distMag = dist.sqrMagnitude;
				if (distMag < curDist) {
					enemyVar = enemy;
					curDist = distMag;
				}
			}
			enemyTarget = enemyVar;
		} else {
			if (fire <= 0) {
				RaycastHit hit;
				if (transform.localScale == new Vector3 (0.5f, 0.8f, 0.5f)) {
					if (Physics.Raycast (transform.position + new Vector3 (0, transform.localScale.y / 2, 0), enemyTarget.transform.position - transform.position, out hit)) {
						if (hit.collider.gameObject == enemyTarget) {
							Debug.DrawRay (transform.position + new Vector3 (0, transform.localScale.y / 2, 0), enemyTarget.transform.position - transform.position, Color.yellow);
							enemyTarget.GetComponent<Health> ().hp -= 1;
						}
					}
					fire = 0.2f;
				}
			} else {
				fire -= Time.deltaTime;
			}
		}
		if (selected) {
			GetComponent<LineRenderer> ().enabled = true;
		} else {
			GetComponent<LineRenderer> ().enabled = false;
		}
		if (pos != Vector3.zero) {
			if (GetComponent<NavMeshAgent> ().remainingDistance < minDist) {
				if (GameObject.FindObjectOfType<unitWaypoint> ().cover [index].lowCover) {
					if (crouch == false) {
						transform.localScale = new Vector3 (0.5f, 0.4f, 0.5f);
						crouch = true;
					} else {
						if (crouchCoolDown <= 0) {
							if (transform.localScale == new Vector3 (0.5f, 0.8f, 0.5f)) {
								transform.localScale = new Vector3 (0.5f, 0.4f, 0.5f);
							} else {
								transform.localScale = new Vector3 (0.5f, 0.8f, 0.5f);
							}
							crouchCoolDown = 2;
						} else {
							crouchCoolDown -= Time.deltaTime;
						}
					}
				}
			}
		} else {
			GetComponent<NavMeshAgent> ().speed = startSpeed;
			transform.localScale = new Vector3 (0.5f, 0.8f, 0.5f);
			crouch = false;
		
			if (follow) {
				delay -= Time.deltaTime;
				if (delay <= 0) {
					if (followingPath) {
						//deal with following path
						checkPath ();
					} else if (target != null) {
						//deal with following player
						if (Vector3.Distance (transform.position, target.transform.position) > minDist * 2) {
							GetComponent<NavMeshAgent> ().SetDestination (target.transform.position);
							prevDest = target.transform.position;
						} else {
							GetComponent<NavMeshAgent> ().SetDestination (transform.position);
							prevDest = target.transform.position;
							delay = totalDelay;
						}
					}
				}
			} else {
				delay -= Time.deltaTime;
				if (delay <= 0) {
					checkPath ();
				}
			}
		}
	}

	public void setPath(List<Vector3> _path, float _delay, float parentDelay) {
		for (int a = 0; a < _path.Count; a++) {
			path.Add (new Vector3(_path[a].x, _path[a].y, _path[a].z));
		}
		if (_delay != 0) {
			totalDelay = _delay + parentDelay;
			delay = totalDelay;
			followingPath = true;
		}
		if (follower != null) {
			foreach (GameObject obj in follower) {
				obj.GetComponent<unitWay> ().setPath (_path, _delay, totalDelay);
			}
		}
	}

	public void removeFollwer(GameObject _follower) {
		for (int a = 0; a < follower.Count; a++) {
			if (follower [a] == _follower) {
				follower.RemoveAt (a);
				return;
			}
		}
	}

	void checkPath() {
		if (path.Count != 0) {
			switch (path.Count) {
			case 1:
				if (followingCount == 0) {
					pathCheckMinDist (minDist);
				} else {
					pathCheckMinDist (minDist * followingCount);
				}
				break;
			default:
				pathCheckMinDist (minDist);
				break;
			}
		} else {
			followingPath = false;
		}
	}

	void pathCheckMinDist(float dist) {
		if (!GetComponent<NavMeshAgent> ().hasPath) {
			GetComponent<NavMeshAgent> ().SetDestination (path [0]);
		}
		if (GetComponent<NavMeshAgent> ().hasPath) {
			if (GetComponent<NavMeshAgent> ().remainingDistance < dist) {
				if (main) {
					GameObject.FindObjectOfType<unitWaypoint> ().destroyPathObj (path.Count);
				}
				path.RemoveAt (0);
				GetComponent<NavMeshAgent> ().ResetPath ();
			}
		}
	}

	public void resetFollow() {
		main = true;
		//target.GetComponent<unitWay> ().resetFollower ();
		target.GetComponent<unitWay> ().removeFollwer(gameObject);
		follow = false;
		target = null;
		followingCount = gatherFollowers ();
		path.Clear ();
	}

	/*
	public void resetFollower() {
		follower = null;
		followingCount = gatherFollowers ();
		followingCount -= 1;
	}
	*/

	public int gatherFollowers() {
		unitWay temp = this;
		int value = 0;
		while (temp.target != null) {
			value += 1;
			temp = temp.target.GetComponent<unitWay>();
		}
		return value;
	}

	public void setupFollower(GameObject other) {
		main = false;
		target = other;
		other.GetComponent<unitWay>().follower.Add(this.gameObject);
		follow = true;
		gatherFollowers ();
	}

	public List<GameObject> returnAllFollowers() {
		List<GameObject> temp = new List<GameObject> ();
		foreach (GameObject obj in follower) {
			temp.Add (obj);
			if (obj.GetComponent<unitWay> ().follower.Count != 0) {
				List<GameObject> temp2 = obj.GetComponent<unitWay> ().returnAllFollowers ();
				foreach (GameObject obj2 in temp2) {
					if (!temp.Contains (obj2)) {
						temp.Add (obj2);
					}
				}
			}
		}
		return temp;
	}
}
