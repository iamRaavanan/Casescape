using UnityEngine;
using System.Collections;

public class Minimap : MonoBehaviour {

	public Transform target;

	public Transform PlayerTarget;
	public Transform CaseTarget;

	public Minimap map;

	public bool isPlayer;
	public bool isCase;

	private float zoomlevel = 2.75f;
	public RectTransform PlayerRectTransform;
	public RectTransform TargetRectTransform;

	public Vector2 TransformPosition (Vector3 position) {
		Vector3 offset = position - target.position;
		Vector2 newPos = new Vector2 (offset.x, offset.z);
		newPos *= zoomlevel;
		return newPos;
	}


	void LateUpdate () {
		if (isPlayer) {
			Vector2 newPosition = map.TransformPosition (PlayerTarget.position);
			PlayerRectTransform.localPosition = newPosition;
		}
		if (isCase && CaseTarget != null) {
			Vector2 newPosition = map.TransformPosition (CaseTarget.position);
			TargetRectTransform.localPosition = newPosition;
		}
	}
}
