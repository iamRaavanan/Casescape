using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PowerupController : MonoBehaviour {

	public Text Countdown;
	private int counter = 10;

	void OnEnable () {
		Countdown.text = string.Empty + counter;
		StartCoroutine ("TriggerCountdown");
	}

	private IEnumerator TriggerCountdown () {
		float localTime = Time.time + 1f;
		bool zoomIn = true;
		int zoomCounter = 0;
		while (localTime > Time.time) {
			yield return null;
			if (zoomIn) {
				if (zoomCounter < 25) {
					zoomCounter++;
					PlayerController.instance.PowerupMap.transform.localScale += new Vector3 (0.01f, 0.01f, 0.01f);
				} else {
					zoomCounter = 0;
					PlayerController.instance.PowerupMap.transform.localScale = new Vector3 (1.25f, 1.25f, 1.25f);
					PlayerController.instance.PowerupMap.GetComponent<Image> ().color = Color.red;
					zoomIn = false;
				}
			} else {
				if (zoomCounter < 25) {
					zoomCounter++;
					PlayerController.instance.PowerupMap.transform.localScale -= new Vector3 (0.01f, 0.01f, 0.01f);
				} else {
					zoomCounter = 0;
					PlayerController.instance.PowerupMap.transform.localScale = Vector3.one;
					PlayerController.instance.PowerupMap.GetComponent<Image> ().color = Color.blue;
					zoomIn = true;
				}
			}
		}
		counter--;
		Countdown.text = string.Empty + counter;
		if (counter >= 0)
			StartCoroutine ("TriggerCountdown");
		else {
			CONTROLLER.currentPowerup = -1;
			PlayerController.instance.PowerupMap.SetActive (false);
			Destroy (this.gameObject);
		}
	}

	public void DestroyWhenCollide () {
		StopCoroutine ("TriggerCountdown");
		PlayerController.instance.PowerupMap.SetActive (false);
		Destroy (this.gameObject);
	}
}
