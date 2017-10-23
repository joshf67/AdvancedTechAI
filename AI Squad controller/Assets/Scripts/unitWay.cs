using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class unitWay : MonoBehaviour {

	public bool follow = false;
	public GameObject enemyTarget = null;
	public GameObject target = null;
	public List<GameObject> follower = null;
	public List<pathData> path = new List<pathData>();
	public float minDist = 0;
	public float totalDelay = 0;
	public float delay = 0;
	public bool followingPath = false;
	public bool main = true;
	public float followingCount = 0;
	public bool selected = false;
	public bool forceMove = false;
	public bool crouch = false;
	public Vector3 pos = Vector3.zero;
	public int index = 0;
	public float fire = 0;
	public float startSpeed = 0;
	public Vector3 startScale = Vector3.zero;
	public float crouchCoolDown = 0;
	public float peakCoolDown = 0;
	public float peakingCoolDown = 0;
	public bool requestCover = false;
	public bool inCover = false;
	public bool foundEnemy = false;
	public Vector3 foundEnemyLastSeen = Vector3.zero;
	public bool previousFollow = false;
	public GameObject prevObj = null;

    void OnDrawGizmos()
    {

        foreach(Vector3 temp in GetComponent<NavMeshAgent>().path.corners)
        {

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(temp, 1);

        }

    }

	// Use this for initialization
	void Start () {
		GetComponent<NavMeshAgent> ().stoppingDistance = minDist;
		startSpeed = GetComponent<NavMeshAgent> ().speed;
		startScale = transform.lossyScale;
		//deal with setting up following count for distance checks
		if (target != null) {
			follow = true;
			followingCount = gatherFollowing ();
			main = false;
			target.GetComponent<unitWay> ().follower.Add(gameObject);
		} else {
			follow = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
		//check if enemy target exists
		if (!enemyTarget) {
			//loop through all enemies and selected closest one
			GameObject[] enemies = GameObject.FindGameObjectsWithTag ("Enemy");
			float curDist = Mathf.Infinity;
			GameObject enemyVar = null;
			foreach (GameObject enemy in enemies) {
				Vector3 dist = enemy.transform.position - transform.position;
				float distMag = dist.sqrMagnitude;
				if (!Physics.Raycast (transform.position + new Vector3 (0, transform.localScale.y / 2, 0), enemy.transform.position - transform.position, distMag, 1 << 8)) {
					if (distMag < curDist) {
						enemyVar = enemy;
						curDist = distMag;
					}
				}
			}
			enemyTarget = enemyVar;
		} 
		//shoot enemy if one exists
		else {
			if (fire <= 0) {
				RaycastHit hit;
				//check if unit is standing
				if (transform.localScale == startScale) {
					//check if unit can see enemy
					Debug.DrawRay(transform.position + new Vector3 (0, transform.localScale.y / 2, 0), enemyTarget.transform.position - transform.position);
					if (Physics.Raycast (transform.position + new Vector3 (0, transform.localScale.y / 2, 0), enemyTarget.transform.position - transform.position, out hit)) {
						//check if object hit is enemy
						if (hit.collider.gameObject == enemyTarget) {
							//deal damage to enemy and display tracer
							Debug.DrawRay (transform.position + new Vector3 (0, transform.localScale.y / 2, 0), enemyTarget.transform.position - transform.position, Color.yellow);
							enemyTarget.GetComponent<Health> ().hp -= 1;
						} else {
							//enemyTarget = null;
						}
					} else {
						//enemyTarget = null;
					}
					fire = 0.2f;
				}
			} else {
				fire -= Time.deltaTime;
			}
		}

		//draw selection line above unit if selected
		if (selected) {
			GetComponent<LineRenderer> ().enabled = true;
		} else {
			GetComponent<LineRenderer> ().enabled = false;
		}

		//check if cover pos is a value
		if (pos != Vector3.zero) {
			//check if distance to cover spot is less than min dist plus height
			if (Vector3.Distance (pos, transform.position) < minDist + transform.localScale.y &&
			    GetComponent<NavMeshAgent> ().remainingDistance < minDist) {
				inCover = true;
				//check if cover is a low cover
				if (GameObject.FindObjectOfType<unitWaypoint> ().cover [index].lowCover) {
					//check if crouching and if not crouch
					if (crouch == false) {
						transform.localScale = new Vector3 (0.5f, 0.4f, 0.5f);
						crouch = true;
					} else {
						//if crouching check cooldown on standing up
						if (crouchCoolDown <= 0) {
							//crouch if standing up too long
							if (transform.localScale == startScale) {
								transform.localScale = new Vector3 (0.5f, 0.4f, 0.5f);
							} 
							//stand up if crouched too long
							else {
								transform.localScale = startScale;
							}
							crouchCoolDown = 2;
						} else {
							crouchCoolDown -= Time.deltaTime;
						}
					}
				} else {
					// {
					if (peakingCoolDown <= 0) {	
						if (foundEnemyLastSeen != Vector3.zero) {
							GetComponent<NavMeshAgent> ().SetDestination (foundEnemyLastSeen);
							if (GetComponent<NavMeshAgent> ().remainingDistance < minDist) {
								if (!enemyTarget) {
									foundEnemyLastSeen = Vector3.zero;
								}
							}
						}
					} else {
						peakingCoolDown -= Time.deltaTime;
					}
					//}
				}



			} else {
				if (inCover) {
					if (peakCoolDown <= 0) {
						inCover = false;
						peakCoolDown = 2;
					} else {
						peakCoolDown -= Time.deltaTime;
					}
				} else {
					//set destination to cover and allow travel
					peakingCoolDown = 2;
					if (GetComponent<NavMeshAgent> ().destination != pos) {
						GetComponent<NavMeshAgent> ().SetDestination (pos);
					}
				}
			}
			if (foundEnemyLastSeen == Vector3.zero) {
				List<GameObject> temp = new List<GameObject> ();
				temp.Add (this.gameObject);
				GameObject.FindObjectOfType<unitWaypoint> ().resetCoverForSelected (transform.position, temp);
			}
		} 
		//if unit is not going for cover
		else {

			//make the unit stand up and be able to travel
			transform.localScale = startScale;
			crouch = false;

			//check if the unit is following another
			if (follow) {
				if (!enemyTarget) {
					if (target != null) {
						prevObj = target;
					}
					delay -= Time.deltaTime;
					if (delay <= 0) {
						if (followingPath) {
							//deal with following path
							checkPath ();
						} else if (target != null) {
							//deal with following player
							if (Vector3.Distance (transform.position, target.transform.position) > minDist * 2) {
								GetComponent<NavMeshAgent> ().SetDestination (target.transform.position);
							} else {
								delay = totalDelay;
							}
						}
					}
				} else {
					if (!forceMove) {
						GameObject.FindObjectOfType<unitWaypoint> ().requestCover (this.gameObject);
						foundEnemyLastSeen = transform.position;
					} else {
						if (GetComponent<NavMeshAgent> ().remainingDistance < minDist) {
							forceMove = false;
						}
					}
				}
			}
			//check if the unit is following a path
			else {
				if (prevObj) {
					setupFollower (prevObj);
				}
				if (!enemyTarget) {
					delay -= Time.deltaTime;
					if (delay <= 0) {
						checkPath ();
					}
				} else {
					if (!forceMove) {
						GameObject.FindObjectOfType<unitWaypoint> ().requestCover (this.gameObject);
						foundEnemyLastSeen = transform.position;
					} else {
						if (GetComponent<NavMeshAgent> ().remainingDistance < minDist) {
							forceMove = false;
						}
					}
				}
			} 
		}
	}

	//take inputs and apply path to unit/followers
	public void setPath(List<pathData> _path, float _delay, float parentDelay) {
		//set unit path to input
		for (int a = 0; a < _path.Count; a++) {
			pathData temp = new pathData ();
			temp.path = _path[a].path;
			temp.speed = _path[a].speed;
			path.Add (temp);
		}

		//add artificial delay based on inputs
		if (_delay != 0) {
			totalDelay = _delay + parentDelay;
			delay = totalDelay;
			followingPath = true;
		}

		//check if any followers
		if (follower != null) {
			foreach (GameObject obj in follower) {
				obj.GetComponent<unitWay> ().setPath (_path, _delay, totalDelay);
			}
		}
	}

	//loop through followers and remove _follower if found
	public void removeFollwer(GameObject _follower) {
		for (int a = 0; a < follower.Count; a++) {
			if (follower [a] == _follower) {
				follower.RemoveAt (a);
				return;
			}
		}
		gatherFollowers ();
	}

	//check current path data
	void checkPath() {
		if (path.Count != 0) {
			GetComponent<NavMeshAgent> ().speed = path [0].speed * (startSpeed * 0.5f);
			switch (path.Count) {
			//if unit is on last path apply follower distance
			case 1:
				if (followingCount == 0) {
					pathCheckMinDist (minDist);
				} else {
					pathCheckMinDist (minDist * followingCount);
				}
				break;
			//else apply normal check
			default:
				pathCheckMinDist (minDist);
				break;
			}
		} else {
			followingPath = false;
		}
	}

	//check the distance from the unit to the next point
	void pathCheckMinDist(float dist) {
		//check if the unit has a path
		if (!GetComponent<NavMeshAgent> ().hasPath) {
			GetComponent<NavMeshAgent> ().SetDestination (path [0].path);
		}

		//if unit has a path check distance remaining
		if (GetComponent<NavMeshAgent> ().hasPath) {
			if (GetComponent<NavMeshAgent> ().remainingDistance < dist) {
				//if distance remaining is less than input set destination to next node
				if (main) {
					//delete visual nodes if main
					GameObject.FindObjectOfType<unitWaypoint> ().destroyPathObj (path.Count);
				}
				path.RemoveAt (0);
				GetComponent<NavMeshAgent> ().ResetPath ();
			}
		}
	}

	//reset the unit back to not following
	public void resetFollow() {
		main = true;
		//remove this unit from targets follower data
		if (target != null) {
			if (target.GetComponent<unitWay> ()) {
				target.GetComponent<unitWay> ().removeFollwer (gameObject);
			}
		}
		follow = false;
		target = null;
		followingCount = 0;
		path.Clear ();
	}

	//check how many followers are attached to this unit
	public int gatherFollowers() {
		List<GameObject> temp = returnAllFollowers ();
		return temp.Count;
	}

	public int gatherFollowing() {
		unitWay temp = this;
		int value = 0;
		while (temp.target != null) {
			value += 1;
			temp = temp.target.GetComponent<unitWay>();
		}
		return value;
	}

	//setup following new target
	public void setupFollower(GameObject other) {
		resetFollow ();
		main = false;
		target = other;
		follow = true;
		followingCount = gatherFollowers ();
		if (other.GetComponent<unitWay> ()) {
			other.GetComponent<unitWay> ().follower.Add (this.gameObject);
			if (other.GetComponent<unitWay> ().pos != Vector3.zero) {
				requestCover = true;
			}
		}
	}

	//gather all followers of this and followers
	public List<GameObject> returnAllFollowers() {
		//store temporary holder of followers
		List<GameObject> temp = new List<GameObject> ();
		//loop through all followers of this unit
		foreach (GameObject obj in follower) {
			//add this unit to storage
			temp.Add (obj);
			//check if followers of follower exist
			if (obj.GetComponent<unitWay> ().follower.Count != 0) {
				//iterate through finding their followers and adding them to temp
				List<GameObject> temp2 = obj.GetComponent<unitWay> ().returnAllFollowers ();
				foreach (GameObject obj2 in temp2) {
					//check if follower does not exist in temp already
					if (!temp.Contains (obj2)) {
						temp.Add (obj2);
					}
				}
			}
		}
		//return all followers
		return temp;
	}
}
