using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Tutorial : MonoBehaviour {
	
	public Text Tutorial_Text;

	public GameObject Proceed_Btn;
	public Text Proceed_Text;

	public AudioClip TypingSound;

	public GameObject MinimapClone;
	public GameObject Arrow;

	private int Tutorial_Counter;

	private string[] FirstText = new string[] {
		"Welcome to Casescape!!",
		"This game is about finding suitcase which will be sapwned at different position.",
		"Direction you used to grab each suitcase will be noted by enemy group and follow you once you captured the case.",
		"This will be continued for each case you grab.",
		"Main aim is to collect more cases without hitting enemies.",
		"Use minimap to find the case and powerup spawned location"
	};


	public void FirstTutorial () {
		StartCoroutine ("StartTyping");
	}

	private IEnumerator StartTyping () {
		foreach (char letter in FirstText[Tutorial_Counter].ToCharArray ()) {
			Tutorial_Text.text += letter;
			AudioSource.PlayClipAtPoint (TypingSound, Vector3.zero);
			yield return new WaitForSeconds (0.075f);
			//yield return null;
		}
		Proceed_Text.text = (FirstText.Length == (Tutorial_Counter + 1)) ? "Done" : "Proceed";
		Proceed_Btn.SetActive (true);
		//yield return null;
	}

	public void Proceed_Clicked () {
		Tutorial_Counter++;
		Proceed_Btn.SetActive (false);
		if (FirstText.Length == Tutorial_Counter) {
			MinimapClone.GetComponent<RectTransform> ().SetSiblingIndex (6);
			Arrow.SetActive (false);
			this.gameObject.SetActive (false);
			PlayerPrefs.SetInt ("Tutorial", 1);
		} else {
			if (Tutorial_Counter == (FirstText.Length - 1)) {
				Debug.Log ("SEt");
				MinimapClone.GetComponent<RectTransform> ().SetSiblingIndex (8);
				Arrow.SetActive (true);
			}
			Tutorial_Text.text = string.Empty;
			StartCoroutine ("StartTyping");
		}
	}
}
