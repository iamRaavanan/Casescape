using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GhostController : MonoBehaviour {

	//private float m_speed = 3.0f;
	public GameObject DestoryEffect;
	private bool isReached = true;
	private int wayPointLength;
	private List<Vector3> m_wayPoints;

	private Vector2 initialPos, finalPos;
	private Vector2 swipeDirection;

	public void FollowTargetPath (List<Vector3> wayPoints) {
		m_wayPoints = new List<Vector3> ();
		wayPointLength = wayPoints.Count;
		for (int i = 0; i < wayPoints.Count; i++) {
			m_wayPoints.Add (wayPoints [wayPoints.Count - 1 - i]);
		}
		isReached = false;
	}

	void Update () {
		if (!isReached && wayPointLength > 0) {
			this.gameObject.transform.position = Vector3.MoveTowards (this.transform.position, m_wayPoints [wayPointLength - 1], CONTROLLER.m_ghostSpeed * Time.deltaTime);
			//Vector3 newDir = Vector3.RotateTowards (transform.forward, m_wayPoints [wayPointLength - 1], (5 * Time.deltaTime), 0f);
			//transform.rotation = Quaternion.LookRotation (newDir);
			if (this.gameObject.transform.position == m_wayPoints [wayPointLength - 1]) {
				wayPointLength--;
				transform.LookAt (m_wayPoints [wayPointLength]);
				//initialPos = finalPos;
				//finalPos = m_wayPoints [wayPointLength - 2];
				//CalculateDirection ();
			}
		} else {
			isReached = true;
			List<Vector3> tempList = m_wayPoints;
			FollowTargetPath (tempList);
		}
	}

	void OnDisable () {
		if (CONTROLLER.isgameStart) {
			Destroy(Instantiate (DestoryEffect, new Vector3 (this.transform.position.x, 1, this.transform.position.z), Quaternion.identity) as GameObject, 1);
		}
	}
}
