using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class unitWay : MonoBehaviour {

	/*
	public bool followingPath = false;
	public bool forceMove = false;
	public bool crouch = false;
	public float crouchCoolDown = 0;
	public bool inCover = false;
	public bool foundEnemy = false;
	public bool previousFollow = false;
	public GameObject prevObj = null; */

	public bool requestCover = false;
	public float minDist = 0;

	public bool main = true;
	public List<pathData> path = new List<pathData>();
	public pathData prevPath = null;

	public float totalDelay = 0;
	public float delay = 0;

	public bool selected = false;

	public GameObject target = null;

	public Vector3 coverPos = Vector3.zero;
	public int index = 0;
	public List<int> ignoreIndex = new List<int>();

	public Health enemyTarget = null;
	public List<enemyData> foundEnemyLastSeen = new List<enemyData>();

	public List<unitWay> follower = null;
	public bool follow = false;
	public float followerCount = 0;

	public float fire = 0;
	public float peakCoolDown = 0;
	public float peakingCoolDown = 0;

	public float startSpeed = 0;
	public Vector3 startScale = Vector3.zero;

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
			followerCount = gatherFollowing () + 1;
			main = false;
			target.GetComponent<unitWay> ().follower.Add(this);
		} else {
			follow = false;
		}
	}
	
	// Update is called once per frame
	void Update () {

		//reset height
		transform.localScale = startScale;
		
		//check if enemy target exists
		if (!enemyTarget) {
			//loop through all enemies and selected closest one
			GameObject[] enemies = GameObject.FindGameObjectsWithTag ("Enemy");
			float curDist = Mathf.Infinity;
			GameObject enemyVar = null;
			foreach (GameObject enemy in enemies) {
				float dist = Vector3.Distance(enemy.transform.position, transform.position);
				RaycastHit hit;
				Debug.DrawRay (transform.position + new Vector3 (0, transform.lossyScale.y/2, 0), transform.position + new Vector3 (0, transform.lossyScale.y/2, 0));
				if (Physics.Raycast (transform.position + new Vector3 (0, transform.lossyScale.y/2, 0), enemy.transform.position - (transform.position + new Vector3 (0, transform.lossyScale.y/2, 0)), out hit, dist)) {
					if (hit.transform.tag == "Enemy") {
						if (dist < curDist) {
							enemyVar = enemy;
							curDist = dist;
						}
					}
				}
			}
			if (enemyVar) {
				enemyTarget = enemyVar.GetComponent<Health> ();
			}
		}


		//draw selection line above unit if selected
		if (selected) {
			GetComponent<LineRenderer> ().enabled = true;
		} else {
			GetComponent<LineRenderer> ().enabled = false;
		}


		//state Cover
		if (enemyTarget || foundEnemyLastSeen.Count != 0) {

			//go to position
			//if enemy go to cover
			//do peaking until enemy visible
			//shoot enemy
			//then hide again
			//if enemy dead follow path again

			if (coverPos != Vector3.zero) {

				if (peakingCoolDown <= 0) {

					statePeaking ();

				} else {

					stateTakeCover ();

				}
					
			} else {

				grabCover();

			}
				
			//update all free squad members of enemy
			foreach (unitWay unit in follower) {
				if (unit.foundEnemyLastSeen.Count == 0) {
					Vector3 dist = unit.transform.position - transform.position;
					float distMag = dist.sqrMagnitude;
					if (!Physics.Raycast (transform.position + new Vector3 (0, transform.localScale.y / 2, 0), unit.transform.position - transform.position, distMag, 1 << 8)) {
						unit.foundEnemyLastSeen.Add(new enemyData(foundEnemyLastSeen[0].lastSeenPos, foundEnemyLastSeen[0].enemy));
					}
				}
			}
		} else {
			//state following
			if (follow) {
				//follow teamMember
				//if enemy do cover
				//else if teamMember enemy
				//move to teamMember

				stateFollow ();
			} 
			//state stationary
			else {
				//test enemy
				//if enemy go to cover
				//do peaking until enemy visible
				//shoot enemy
				//then hide again
				//if enemy dead go back to start pos
				stateStationary();

			}
		}
		enemyTarget = null;
	}

	void grabCover() {
		if (enemyTarget) {
			GameObject.FindObjectOfType<unitWaypoint> ().requestCover (this, false);
			foundEnemyLastSeen.Insert(0,new enemyData(transform.position, enemyTarget));
		} else {
			/*if (target != null) {
				if (target.GetComponent<unitWay> ()) {
					GameObject.FindObjectOfType<unitWaypoint> ().requestCover (target.GetComponent <unitWay> (), false);
					foundEnemyLastSeen.Insert(0,new enemyData(transform.position, enemyTarget));
				} else {
					stateFollow ();
					foundEnemyLastSeen.RemoveAt (0);
				}
			} else {*/
				stateFollow ();
				foundEnemyLastSeen.RemoveAt (0);
			//}
		}
	}

	void stateTakeCover() {
		//move to cover
		Debug.Log(Vector3.Distance (coverPos, transform.position));
		if (Vector3.Distance (coverPos, transform.position) < minDist + transform.localScale.y &&
			GetComponent<NavMeshAgent> ().remainingDistance < minDist) {

			//recharge peak cool down
			peakingCoolDown -= Time.deltaTime;

			//do crouching actions
			if (GameObject.FindObjectOfType<unitWaypoint> ().cover [index].lowCover &&
				peakingCoolDown > 0) {
				if (transform.localScale == startScale) {
					transform.localScale = new Vector3 (0.5f, 0.4f, 0.5f);
				} 
			} else {

				//test if enemy can still see me
				if (enemyTarget) {
					Debug.DrawLine (transform.position, enemyTarget.transform.position);
					if (path.Count != 0) {
						if (path [0] == prevPath) {
							if (!ignoreIndex.Contains (index)) {
								ignoreIndex.Add (index);
							}
						} else {
							ignoreIndex.Clear ();
						}
						prevPath = path [0];
					} else {
						if (!ignoreIndex.Contains (index)) {
							ignoreIndex.Add (index);
						}
					}
					GameObject.FindObjectOfType<unitWaypoint> ().requestCover (this, false, false, false, ignoreIndex);
				}
			}

		} else {

			//move to cover pos
			if (GetComponent<NavMeshAgent> ().destination != coverPos) {
				GetComponent<NavMeshAgent> ().SetDestination (coverPos);
			}


		}
	}

	void statePeaking() {
		//move to last seen enemy
		if (enemyTarget) {

			if (foundEnemyLastSeen.Count != 0) {
				if (enemyTarget != foundEnemyLastSeen [0].enemy) {
					if (foundEnemyLastSeen [0].enemy == null) {
						foundEnemyLastSeen.RemoveAt (0);
					} else {
						if (Vector3.Distance (foundEnemyLastSeen [0].lastSeenPos, transform.position) > minDist ||
						   Vector3.Distance (foundEnemyLastSeen [0].lastSeenPos, coverPos) < minDist) {
							GameObject.FindObjectOfType<unitWaypoint> ().requestCover (this, false);
							foundEnemyLastSeen.Insert (0, new enemyData (transform.position, enemyTarget));
							GetComponent<NavMeshAgent> ().SetDestination (foundEnemyLastSeen [0].lastSeenPos);
						}
					}
				} else {
					stateFiring ();
				}
			} else {
				GameObject.FindObjectOfType<unitWaypoint> ().requestCover (this, false);
				foundEnemyLastSeen.Insert (0, new enemyData (transform.position, enemyTarget));
				GetComponent<NavMeshAgent> ().SetDestination (foundEnemyLastSeen [0].lastSeenPos);
			}

		} else {
			//check enemy exists
			//move to enemy pos
			if (Vector3.Distance (foundEnemyLastSeen [0].lastSeenPos, transform.position) > minDist) {
				GetComponent<NavMeshAgent> ().SetDestination (foundEnemyLastSeen [0].lastSeenPos);
			} else {
				foundEnemyLastSeen.RemoveAt (0);
			}

		}
	}

	void stateFiring () {
		//fire
		if (fire <= 0) {
			RaycastHit hit;
			//check if unit is standing
			if (transform.localScale == startScale) {
				//check if unit can see enemy
				Debug.DrawRay (transform.position + new Vector3 (0, transform.localScale.y / 2, 0), enemyTarget.transform.position - transform.position);
				if (Physics.Raycast (transform.position + new Vector3 (0, transform.localScale.y / 2, 0), enemyTarget.transform.position - transform.position, out hit)) {
					//check if object hit is enemy
					if (hit.collider.gameObject == enemyTarget.gameObject) {
						//deal damage to enemy and display tracer
						Debug.DrawRay (transform.position + new Vector3 (0, transform.localScale.y / 2, 0), enemyTarget.transform.position - transform.position, Color.yellow);
						enemyTarget.GetComponent<Health> ().hp -= 1;
					} else {
						enemyTarget = null;
					}
				} else {
					enemyTarget = null;
				}
				fire = 0.2f;
			}
		} else {
			fire -= Time.deltaTime;
		}
		peakCoolDown -= Time.deltaTime;

		if (peakCoolDown <= 0) {
			peakCoolDown = 2;
			peakingCoolDown = 2;
		}
	}

	void stateStationary() {
		if (enemyTarget) {
			GameObject.FindObjectOfType<unitWaypoint> ().requestCover (this, false);
			foundEnemyLastSeen.Insert(0,new enemyData(transform.position, enemyTarget));
		}
	}

	void stateFollow() {
		if (enemyTarget) {
			GameObject.FindObjectOfType<unitWaypoint> ().requestCover (this, false);
			foundEnemyLastSeen.Insert (0, new enemyData (transform.position, enemyTarget));
		} else {
			delay -= Time.deltaTime;
			if (delay <= 0) {
				if (target == null && path.Count != 0) {
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
		}
	}

	public void setPath(pathData _path, float _delay, float parentDelay) {
		List<pathData> temp = new List<pathData> ();
		temp.Add (_path);
		setPath(temp, _delay, parentDelay);
	}

	//take inputs and apply path to unit/followers
	public void setPath(List<pathData> _path, float _delay, float parentDelay) {
		//set unit path to input
		for (int a = 0; a < _path.Count; a++) {
			pathData temp = new pathData (_path[a].path, _path[a].speed);
			path.Add (temp);
		}

		//add artificial delay based on inputs
		if (_delay != 0) {
			totalDelay = _delay + parentDelay;
			delay = totalDelay;
		}

		//check if any followers
		if (follower != null) {
			foreach (unitWay obj in follower) {
				obj.setPath (_path, _delay, totalDelay);
			}
		}
	}

	//loop through followers and remove _follower if found
	public void removeFollower(unitWay _follower) {
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
				if (followerCount == 0) {
					pathCheckMinDist (minDist);
				} else {
					pathCheckMinDist (minDist * followerCount);
				}
				break;
			//else apply normal check
			default:
				pathCheckMinDist (minDist);
				break;
			}
		}
	}

	//check the distance from the unit to the next point
	void pathCheckMinDist(float dist) {
		//check if the unit has a path
		if (!GetComponent<NavMeshAgent> ().hasPath ||
			GetComponent<NavMeshAgent> ().destination != path [0].path) {
			GetComponent<NavMeshAgent> ().SetDestination (path [0].path);
		}

		//if unit has a path check distance remaining
		if (GetComponent<NavMeshAgent> ().hasPath) {
			if (GetComponent<NavMeshAgent> ().remainingDistance < dist) {
				if (Vector3.Distance (transform.position, path [0].path) < dist + transform.lossyScale.y) {
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
	}

	//reset the unit back to not following
	public void resetFollow() {
		main = true;
		//remove this unit from targets follower data
		if (target != null) {
			if (target.GetComponent<unitWay> ()) {
				target.GetComponent<unitWay> ().removeFollower (this);
			}
		}
		follow = false;
		target = null;
		followerCount = 1;
		path.Clear ();
	}

	//check how many followers are attached to this unit
	public int gatherFollowers() {
		List<unitWay> temp = returnAllFollowers ();
		return temp.Count;
	}

	public int gatherFollowing() {
		unitWay temp = this;
		int value = 0;
		while (temp.target.GetComponent<unitWay>()) {
			value += 1;
			temp = temp.target.GetComponent<unitWay> ();
		}
		return value;
	}

	//setup following new target
	public void setupFollower(GameObject other) {
		resetFollow ();
		main = false;
		target = other;
		follow = true;
		followerCount = gatherFollowers ();
		if (other.GetComponent<unitWay> ()) {
			other.GetComponent<unitWay> ().follower.Add (this);
			if (other.GetComponent<unitWay> ().coverPos != Vector3.zero) {
				GameObject.FindObjectOfType<unitWaypoint> ().requestCover (this,
					false, this, true);
			}
		}
	}

	//gather all followers of this and followers
	public List<unitWay> returnAllFollowers() {
		//store temporary holder of followers
		List<unitWay> temp = new List<unitWay> ();
		//loop through all followers of this unit
		foreach (unitWay obj in follower) {
			//add this unit to storage
			temp.Add (obj);
			//check if followers of follower exist
			if (obj.follower.Count != 0) {
				//iterate through finding their followers and adding them to temp
				List<unitWay> temp2 = obj.returnAllFollowers ();
				foreach (unitWay obj2 in temp2) {
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

[System.Serializable]
public class enemyData {
	public Vector3 lastSeenPos;
	public Health enemy;

	public enemyData(Vector3 pos, Health _enemy) {
		lastSeenPos = pos;
		enemy = _enemy;
	}

}























/*


			//if (!enemyTarget) {
//	if (Vector3.Distance (pos, transform.position) < minDist + transform.localScale.y &&
//	    GetComponent<NavMeshAgent> ().remainingDistance < minDist) {
//===============================================



/*
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
						if (peakingCoolDown <= 0) {	
							if (foundEnemyLastSeen != Vector3.zero) {
								GetComponent<NavMeshAgent> ().SetDestination (foundEnemyLastSeen);
								if (enemyTarget) {
									foundEnemyLastSeen = transform.position;
									GameObject.FindObjectOfType<unitWaypoint> ().requestCover (this.gameObject);
								} else {
									if (GetComponent<NavMeshAgent> ().remainingDistance < minDist) {
										if (!enemyTarget) {
											foundEnemyLastSeen = Vector3.zero;
											GameObject.FindObjectOfType<unitWaypoint> ().resetCover (this.gameObject);
										}
									}
								}
							}
						} else {
							peakingCoolDown -= Time.deltaTime;
						}
					}



					//===============================================
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
			} else {
				foundEnemyLastSeen = transform.position;
				GameObject.FindObjectOfType<unitWaypoint> ().requestCover (this.gameObject);
				//save currentPos
				//cover
			}

			*/






/*} else {
					if (!forceMove) {
						GameObject.FindObjectOfType<unitWaypoint> ().requestCover (this.gameObject);
						foundEnemyLastSeen = transform.position;
					} else {
						if (GetComponent<NavMeshAgent> ().remainingDistance < minDist) {
							forceMove = false;
						}
					}
				}*/


/*

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
*/