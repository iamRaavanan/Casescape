using UnityEngine;
using System.Collections;

public class Settings : MonoBehaviour {


	public GameObject Music_On;
	public GameObject Music_Off;

	public GameObject SFx_On;
	public GameObject SFx_Off;

	public void Music_OnClicked () {
		Music_On.SetActive (false);
		Music_Off.SetActive (true);
	}

	public void Music_OffClicked () {
		Music_On.SetActive (true);
		Music_Off.SetActive (false);
	}

	public void SFx_OnClicked () {
		SFx_On.SetActive (false);
		SFx_Off.SetActive (true);
	}

	public void SFx_OffClicked () {
		SFx_On.SetActive (true);
		SFx_Off.SetActive (false);
	}
}
