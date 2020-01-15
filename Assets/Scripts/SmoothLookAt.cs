using UnityEngine;
using System.Collections;

public class SmoothLookAt : MonoBehaviour {

	public Transform Target;
	private float damping = 6f;
	public bool smooth;

	private Transform _myTransform;

	void Awake () {
		_myTransform = transform;
	}

	void Update () {
		if (Target) {
			Quaternion rotation = Quaternion.LookRotation (Target.position - _myTransform.position);
			_myTransform.rotation = Quaternion.Slerp (_myTransform.rotation, rotation, Time.deltaTime * damping);
		}
	}
}
