using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class unitWay : MonoBehaviour {

	public bool follow = false;
	public GameObject target = null;
	public GameObject follower = null;
	public List<Vector3> path = new List<Vector3>();
	//public List<Vector3> previousDest = new List<Vector3>();
	public float minDist = 0;
	//public float maxTimeSinceMove = 1;
	//public float timeSinceMove = 0;
	//public float speedMultOnFollow = 0.9f;
	//public float startSpeed = 0;
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

	// Use this for initialization
	void Start () {
		//startSpeed = GetComponent<NavMeshAgent> ().speed;
		GetComponent<NavMeshAgent> ().stoppingDistance = minDist;
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
			target.GetComponent<unitWay> ().follower = gameObject;
		} else {
			follow = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (pos != Vector3.zero) {
			if (GetComponent<NavMeshAgent> ().remainingDistance < minDist && crouch == false) {
				transform.localScale = new Vector3 (0.5f, 0.4f, 0.5f);
				crouch = true;
			}
		} else {
			transform.localScale = new Vector3 (0.5f, 0.8f, 0.5f);
			crouch = false;
		}
		if (selected) {
			GetComponent<LineRenderer> ().enabled = true;
		} else {
			GetComponent<LineRenderer> ().enabled = false;
		}
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
			follower.GetComponent<unitWay> ().setPath (_path, _delay, totalDelay);
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
		target.GetComponent<unitWay> ().resetFollower ();
		target = null;
		followingCount = gatherFollowers ();
		path.Clear ();
	}

	public void resetFollower() {
		follower = null;
		followingCount = gatherFollowers ();
		followingCount -= 1;
	}

	public int gatherFollowers() {
		unitWay temp = this;
		int value = 0;
		while (temp.follower != null) {
			value += 1;
			temp = temp.follower.GetComponent<unitWay>();
		}
		return value;
	}
}
