using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (Rigidbody))]
public class PlayerController : MonoBehaviour {

	public static PlayerController instance;

	public Camera mainCamera;
	private Vector3 offset;

	//public GameObject LeftArrow;
	//public GameObject RightArrow;
	//public GameObject Arrow;

	public Minimap PlayerMap;
	public Minimap TargetMap;
	private Minimap MainMapScript;
	public GameObject PowerupMap;

	private float m_speed = 8f;
	private Rigidbody m_rigidbody;
	private Vector3 playerInput;
	private Vector3 playerVelocity;

	public GameObject ghostGO;
	public GameObject targetGO;

	// Updating the position to add it in List
	private Vector3 startPosition;

	// Spawn Position Restriction
	//private const int m_planeSize = 5;
	//private float m_groundScale;
	//private float m_groundSize;	
	private float m_leftwidth = 0.0f;
	private float m_rightwidth = 0.0f;
	private float m_upheight = 0.0f;
	private float m_downheight = 0.0f;
	private Vector3 m_targetPos;
	private float m_targetSapawnDistance;

	private Vector2 initialPos, finalPos;
	private Vector2 swipeDirection;
	private bool newTouch = false;

	private bool cameraTweenRight;
	private bool cameraTweenLeft;

	private List<GameObject> GhostHolder;
	public GameObject GhostParent;

	public static event System.Action OnCollision;

	public GameObject Powerup;
	public Texture2D ShieldTex, FreezeTex, SlowtimeTex;
	private bool isShieldpowerup;

	void Start () {
		instance = this;
		GhostHolder = new List<GameObject> ();
		mainCamera = Camera.main;
		offset = mainCamera.transform.position - this.transform.position;
		this.gameObject.GetComponent<Animation> ().Play ("metarig|ANI_Walk");
		//m_groundScale = GameObject.Find ("Ground").transform.localScale.x;
		//m_groundSize = m_groundScale * m_planeSize - 0.5f;
		m_leftwidth = GameObject.FindGameObjectWithTag ("Left").transform.position.x;
		m_rightwidth = GameObject.FindGameObjectWithTag ("Right").transform.position.x;
		m_upheight = GameObject.FindGameObjectWithTag ("Top").transform.position.z;
		m_downheight = GameObject.FindGameObjectWithTag ("Bottom").transform.position.z;
		m_rigidbody = GetComponent<Rigidbody> ();
		MainMapScript = UIController.instance.MinimapGO.GetComponent<Minimap> ();
	}

	private GameObject GO;
	public void SpawnTargets () {
        //Debug.Log("SpawnTarget:" + CONTROLLER.currentMode + "::" + CONTROLLER.currentlevel);
        if(CONTROLLER.currentMode == 0)
        {
            m_targetPos = new Vector3(Random.Range(m_leftwidth, m_rightwidth), 0.05f, Random.Range(m_upheight, m_downheight));
            m_targetSapawnDistance = Vector3.Distance(this.transform.position, m_targetPos);
            while (m_targetSapawnDistance < 10)
            {
                m_targetPos = new Vector3(Random.Range(m_leftwidth, m_rightwidth), 0.05f, Random.Range(m_upheight, m_downheight));
                m_targetSapawnDistance = Vector3.Distance(this.transform.position, m_targetPos);
            }
			CONTROLLER.suitcaseCount++;
            CONTROLLER.speedIncreaseCount++;
			if(CONTROLLER.isPowerupEnabled)
				CONTROLLER.powerupMaxCount++;

            if (CONTROLLER.speedIncreaseCount > 5)	//15
            {
				CONTROLLER.speedIncreaseCount = 0;
                CONTROLLER.m_ghostSpeed += 1f;
				CONTROLLER.isPowerupEnabled = true;
            }

			if (CONTROLLER.powerupMaxCount > 5) {	// 5
				CONTROLLER.powerupMaxCount = 0;
				GameObject localPowerup = Instantiate (Powerup, new Vector3 (Random.Range (m_leftwidth, m_rightwidth), 0.5f, Random.Range (m_upheight, m_downheight)), Quaternion.Euler (new Vector3 (0, 0, 180))) as GameObject;
				CONTROLLER.currentPowerup = Random.Range (0, 3);
				PowerupMap.GetComponent<RectTransform> ().localPosition = MainMapScript.TransformPosition (localPowerup.transform.position);
				PowerupMap.SetActive (true);
				localPowerup.name = ((CONTROLLER.currentPowerup == 0) ? "Freeze" : ((CONTROLLER.currentPowerup == 1) ? "Shield" : "Time"));
				localPowerup.GetComponent<MeshRenderer>().material.mainTexture = ((CONTROLLER.currentPowerup == 0) ? FreezeTex : ((CONTROLLER.currentPowerup == 1) ? ShieldTex : SlowtimeTex));
			}

            GO = Instantiate(targetGO, m_targetPos, Quaternion.identity) as GameObject;
			TargetMap.CaseTarget = GO.transform;
            GO.transform.localEulerAngles = new Vector3(0, 0, 90);
			//Vector3 newDirection = Vector3.RotateTowards (transform.forward, m_targetPos, 5 * Time.deltaTime, 0.0f);
			//Arrow.transform.rotation = Quaternion.LookRotation (newDirection);
//			Debug.Log ("Angle : "+ Vector3.Angle (Arrow.transform.position, m_targetPos));
//			float angle = Vector3.Angle (Arrow.transform.position, GO.transform.position);
//			Arrow.transform.localRotation = Quaternion.Euler (new Vector3 (0, 0, (angle)));
//			Arrow.SetActive (true);
//			StartCoroutine ("HideAfterDelay");
			//Arrow.transform.LookAt (GO.transform);
        } else {
            SpawnLevelEnemies(CONTROLLER.currentEnemy);
        }
	}

    private void SpawnLevelEnemies(int enemyPos) {
        m_targetPos = new Vector3(CONTROLLER.leveldetail[CONTROLLER.currentlevel - 1].enemyPosition[enemyPos].xPos, 0.05f, CONTROLLER.leveldetail[CONTROLLER.currentlevel - 1].enemyPosition[enemyPos].zPos);
        GameObject GO = Instantiate(targetGO, m_targetPos, Quaternion.identity) as GameObject;
        GO.transform.localEulerAngles = new Vector3(270, 0, 0);
        CONTROLLER.currentEnemy++;
    }

	Touch[] touches;

	void Update () {
		UIController.instance.scoreText.text = "Score : " + CONTROLLER.m_score.ToString ("00000");
		if (CONTROLLER.isgameStart && !CONTROLLER.isgamePause) {
			if (Input.GetKeyDown (KeyCode.LeftArrow) || Input.GetKeyDown (KeyCode.A) || CONTROLLER.currentDirection == "left") {
				CONTROLLER.ghostwayPoints.Add (this.transform.position);
			} else if (Input.GetKeyDown (KeyCode.RightArrow) || Input.GetKeyDown (KeyCode.D) || CONTROLLER.currentDirection == "right") {
				CONTROLLER.ghostwayPoints.Add (this.transform.position);
			} else if (Input.GetKeyDown (KeyCode.UpArrow) || Input.GetKeyDown (KeyCode.W) || CONTROLLER.currentDirection == "up") {
				CONTROLLER.ghostwayPoints.Add (this.transform.position);
			} else if (Input.GetKeyDown (KeyCode.DownArrow) || Input.GetKeyDown (KeyCode.S) || CONTROLLER.currentDirection == "down") {
				CONTROLLER.ghostwayPoints.Add (this.transform.position);
			}
			#if UNITY_EDITOR
			playerInput = new Vector3 (Input.GetAxis ("Horizontal"), 0, Input.GetAxis ("Vertical"));
			playerVelocity = playerInput.normalized * m_speed;
			#else
			touches = Input.touches;
			if (Input.GetMouseButtonDown (0)) {
				initialPos = Input.mousePosition;
				newTouch = true;
			}
			if (Input.GetMouseButtonUp (0)) {
				if (newTouch) {
					finalPos = Input.mousePosition;
					CalculateDirection ();
				}
			}
			if (touches.Length > 0) {
				if (touches [0].phase == TouchPhase.Moved) {
					playerInput = new Vector3 (touches [0].deltaPosition.x, 0, touches [0].deltaPosition.y);
					playerVelocity = playerInput.normalized * m_speed;
				}
			}
			#endif
		}

		/*
		 * Debug.Log ("DST BW Camera & Player :" + Vector3.Distance (mainCamera.transform.position, this.transform.position));
		if (this.transform.position.x > 5 && !cameraTweenRight) {
			cameraTweenRight = true;
			StartCoroutine ("TweenRight");
		}
		if (this.transform.position.x < -5 && !cameraTweenLeft) {
			cameraTweenLeft = true;
			StartCoroutine ("TweenLeft");
		}
		*/

		//mainCamera.transform.position = this.transform.position + offset;
		mainCamera.transform.position = new Vector3 (this.transform.position.x + offset.x, this.transform.position.y + offset.y, mainCamera.transform.position.z);
	}


	private IEnumerator TweenRight () {
		cameraTweenLeft = false;
		while (mainCamera.transform.position.x < 7.6f) {
			mainCamera.transform.position += new Vector3 (0.1f, 0, 0);
			yield return null;
		}
		mainCamera.transform.position = new Vector3 (7.5f, mainCamera.transform.position.y, mainCamera.transform.position.z);
	}

	private IEnumerator TweenLeft () {
		cameraTweenRight = false;
		while (mainCamera.transform.position.x > -7.6f) {
			mainCamera.transform.position -= new Vector3 (0.1f, 0, 0);
			yield return null;
		}
		mainCamera.transform.position = new Vector3 (-7.5f, mainCamera.transform.position.y, mainCamera.transform.position.z);
	}

	public void ResetVariables () {
		GhostHolder = new List<GameObject> ();
		CONTROLLER.currentDirection = string.Empty;
		CONTROLLER.ghostwayPoints.Clear ();
		CONTROLLER.isgamePause = false;
		CONTROLLER.isgameStart = true;
		CONTROLLER.isRestartedWithVideo = false;
		CONTROLLER.m_ghostSpeed = 5f;
		CONTROLLER.suitcaseCount = 0;
		CONTROLLER.speedIncreaseCount = 5;
		CONTROLLER.isPowerupEnabled = false;
		CONTROLLER.powerupMaxCount = 0;
		CONTROLLER.m_score = 0;
		startPosition = new Vector3 (0,0.05f,0);
		this.gameObject.transform.position = new Vector3 (0, 0.05f, 0);
		GameObject[] ghostGO = GameObject.FindGameObjectsWithTag ("Ghost");
		foreach (GameObject go in ghostGO) {
			Destroy (go);
		}
		Destroy (GameObject.FindGameObjectWithTag ("Target"));
	}

	private void CalculateDirection () {
		newTouch = false;
		swipeDirection = new Vector2 (finalPos.x - initialPos.x, finalPos.y - initialPos.y);
		swipeDirection.Normalize ();
		if (swipeDirection.y > 0) {
			if (swipeDirection.x > -0.5f && swipeDirection.x < 0.5f) {
				CONTROLLER.currentDirection = "up";
				transform.localRotation = Quaternion.Euler (new Vector3 (0, 0, 0));
			} else if (swipeDirection.x >= 0.5f && swipeDirection.x < 1) {
				CONTROLLER.currentDirection = "upRight";
				transform.localRotation = Quaternion.Euler (new Vector3 (0, 45, 0));
			} else if (swipeDirection.x <= -0.5f && swipeDirection.x > -1) {
				CONTROLLER.currentDirection = "upLeft";
				transform.localRotation = Quaternion.Euler (new Vector3 (0, -45, 0));
			}
		} else if (swipeDirection.y < 0) {
			if (swipeDirection.x > -0.5f && swipeDirection.x < 0.5f) {
				CONTROLLER.currentDirection = "down";
				transform.localRotation = Quaternion.Euler (new Vector3 (0, 180, 0));
			} else if (swipeDirection.x >= 0.5f && swipeDirection.x < 1) {
				CONTROLLER.currentDirection = "downRight";
				transform.localRotation = Quaternion.Euler (new Vector3 (0, 135, 0));
			} else if (swipeDirection.x <= -0.5f && swipeDirection.x > -1) {
				CONTROLLER.currentDirection = "downLeft";
				transform.localRotation = Quaternion.Euler (new Vector3 (0, 225, 0));
			}
		} else if (swipeDirection.x > 0) {
			if (swipeDirection.y > -0.5f && swipeDirection.y < 0.5f) {
				CONTROLLER.currentDirection = "right";
				transform.localRotation = Quaternion.Euler (new Vector3 (0, 90, 0));
			} else if (swipeDirection.y >= 0.5f && swipeDirection.y < 1) {
				CONTROLLER.currentDirection = "upRight";
				transform.localRotation = Quaternion.Euler (new Vector3 (0, 45, 0));
			} else if (swipeDirection.y <= -0.5f && swipeDirection.y > -1) {
				CONTROLLER.currentDirection = "downRight";
				transform.localRotation = Quaternion.Euler (new Vector3 (0, 135, 0));
			}
		} else if (swipeDirection.x < 0) {
			if (swipeDirection.y > -0.5f && swipeDirection.y < 0.5f) {
				CONTROLLER.currentDirection = "left";
				transform.localRotation = Quaternion.Euler (new Vector3 (0, -90, 0));
			} else if (swipeDirection.y >= 0.5f && swipeDirection.y < 1) {
				CONTROLLER.currentDirection = "upLeft";
				transform.localRotation = Quaternion.Euler (new Vector3 (0, -45, 0));
			} else if (swipeDirection.y <= -0.5f && swipeDirection.y > -1) {
				CONTROLLER.currentDirection = "downLeft";
				transform.localRotation = Quaternion.Euler (new Vector3 (0, 225, 0));
			}
		}
		//Debug.Log ("Direction:" + CONTROLLER.currentDirection);
//		Vector3 tempVector = swipeDirection;
//		transform.LookAt (tempVector);
	}
	private void InstantitateGO (Vector3 startPos, List<Vector3> wayPoints) {
		if (GhostHolder.Count == CONTROLLER.maxGhostSpawnCount) {
			GameObject DelGO = GhostHolder [0];
			GhostHolder.RemoveAt (0);
			Destroy (DelGO);
		}
		GameObject GO = Instantiate (ghostGO, new Vector3 (startPos.x, 0.5f, startPos.z), Quaternion.identity, GhostParent.transform) as GameObject;
		GO.name = string.Empty + GhostHolder.Count;
		GhostHolder.Add (GO);
		GhostController m_script = GO.GetComponent<GhostController> ();
		m_script.FollowTargetPath (wayPoints);
	}

	void FixedUpdate () {
		if (!CONTROLLER.isgamePause) {
			m_rigidbody.MovePosition (m_rigidbody.position + playerVelocity * Time.fixedDeltaTime);
//			if(playerVelocity != Vector3.zero)
//				transform.LookAt (playerVelocity);
		}
	}

	void OnCollisionEnter (Collision other) {
		if (!isShieldpowerup) {
			if (other.gameObject.tag == "Target") {
				Destroy (other.gameObject);
				CONTROLLER.ghostwayPoints.Add (this.transform.position);
				if (CONTROLLER.currentMode == 0)
					SpawnTargets ();
				else {
					//UIController.instance.levelScoreText.text = "Score : " + CONTROLLER.m_score.ToString ("00000");
					//Debug.Log("EnemySpawn:" + CONTROLLER.currentEnemy + ":" +CONTROLLER.leveldetail[CONTROLLER.currentlevel - 1].enemyPosition.Length);
					if (CONTROLLER.currentEnemy < CONTROLLER.leveldetail [CONTROLLER.currentlevel - 1].enemyPosition.Length)
						SpawnLevelEnemies (CONTROLLER.currentEnemy);
					else
						StartCoroutine (UIController.instance.FadeInOut ("in-levelcomplete", Color.clear, Color.white, 0.1f, 0.0f));
				}
                

				InstantitateGO (startPosition, CONTROLLER.ghostwayPoints);
				CONTROLLER.ghostwayPoints.Clear ();
				CONTROLLER.m_score += 20;
				startPosition = this.transform.position;
			}
			if (other.gameObject.tag == "Ghost") {
				if (OnCollision != null && !CONTROLLER.videoRespawn) {
					if (CONTROLLER.isRestartedWithVideo)
						StopCoroutine ("ActivateShield");
					OnCollision ();
				}
			}
			if (other.gameObject.tag == "Powerup") {
				other.gameObject.GetComponent<PowerupController> ().DestroyWhenCollide ();
				ActivatePowerup ();
			}
		}
	}


	//====================================================================== POWER UP ACTIVATEION ==================================================================================================================================
	public void ActivatePowerup () {
		StartCoroutine ("ActivateShield");
	}

	private IEnumerator ActivateShield () {
		float previousEnemySpeed = 0f;
		float delay = Time.time + 5f;
		if (CONTROLLER.currentPowerup == 0) {
			previousEnemySpeed = CONTROLLER.m_ghostSpeed;
			CONTROLLER.m_ghostSpeed = 0f;
		} else if (CONTROLLER.currentPowerup == 1) {
			isShieldpowerup = true;
		} else if (CONTROLLER.currentPowerup == 2) {
			previousEnemySpeed = CONTROLLER.m_ghostSpeed;
			CONTROLLER.m_ghostSpeed = 3f;
		}
		while (Time.time < delay) {
			UIController.instance.PowerupTimeText.text = "Powerup Ends in " + (int)(delay - Time.time);
			yield return null;
		}
		UIController.instance.PowerupTimeText.text = string.Empty;
		if (CONTROLLER.currentPowerup == 0) {
			CONTROLLER.m_ghostSpeed = previousEnemySpeed;
		} else if (CONTROLLER.currentPowerup == 1) {
			isShieldpowerup = false;
		} else if (CONTROLLER.currentPowerup == 2) {
			CONTROLLER.m_ghostSpeed = previousEnemySpeed;
		}
	}
	//====================================================================== POWER UP ACTIVATEION ==================================================================================================================================
}
