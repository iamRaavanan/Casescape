using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Mainmenu : MonoBehaviour {

	public GameObject LandingPageGO;
	public GameObject GameModePageGO;
	public GameObject LevelPageGO;
	public GameObject SettingsPageGO;
	public GameObject CustomizePageGO;

	public TextAsset JsonAsset;

	void Awake () {
		if (!CONTROLLER.isLaunched) {
			CONTROLLER.isLaunched = true;
			ParseFile (JsonAsset.ToString ());
		}
		if(PlayerPrefs.HasKey ("hScore")) {
			CONTROLLER.m_highScore = PlayerPrefs.GetInt ("hScore");
		}
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			BackBtn_Clicked ();
		}
	}

	public void Login_Clicked () {
		Debug.Log ("Logging In..");
	}

	public void Play_Clicked () {
		CONTROLLER.currentPage = "Gamemode";
		LandingPageGO.SetActive (false);
		GameModePageGO.SetActive (true);
	}

	public void Quit_App () {
		Application.Quit ();
	}

	public void Settings_Clicked () {
		CONTROLLER.currentPage = "Settings";
		SettingsPageGO.SetActive (true);
		LandingPageGO.SetActive (false);
	}

	public void Character_Clicked () {
		CONTROLLER.currentPage = "Customization";
		LandingPageGO.SetActive (false);
		CustomizePageGO.SetActive (true);
	}

	private void ParseFile(string jsonData) {
		SimpleJSON.JSONNode node = SimpleJSON.JSONNode.Parse(jsonData);
		int levelcount = node["echos"]["levels"].Count;
		CONTROLLER.leveldetail = new LevelDetail[levelcount];
		for (int i = 0; i < levelcount; i++) {
			CONTROLLER.leveldetail[i] = new LevelDetail();
			CONTROLLER.leveldetail[i].playerXpos = node["echos"]["levels"][i]["playerPos"]["x"].AsFloat;
			CONTROLLER.leveldetail[i].playerZpos = node["echos"]["levels"][i]["playerPos"]["z"].AsFloat;
			int enemyCount = node["echos"]["levels"][i]["enemyPos"].Count;
			CONTROLLER.leveldetail[i].enemyPosition = new enemyPos[enemyCount];
			for (int j = 0; j < enemyCount; j++) {
				CONTROLLER.leveldetail[i].enemyPosition[j] = new enemyPos();
				CONTROLLER.leveldetail[i].enemyPosition[j].xPos = node["echos"]["levels"][i]["enemyPos"][j]["x"].AsFloat;
				CONTROLLER.leveldetail[i].enemyPosition[j].zPos = node["echos"]["levels"][i]["enemyPos"][j]["z"].AsFloat;
			}
			CONTROLLER.leveldetail[i].seed = node["echos"]["levels"][i]["seed"].AsFloat;
			CONTROLLER.leveldetail[i].fillPercent = node["echos"]["levels"][i]["fill"].AsInt;            
		}
	}

	//====================================================== GAME-MODE CLICK =========================================================================
	public void GameMode_Clicked () {
		int value = int.Parse (EventSystem.current.currentSelectedGameObject.name);
		if (value == 0) {
			CONTROLLER.currentMode = 0;
//			SceneManager.LoadScene ("Game");
		} else {
			//CONTROLLER.currentPage = "Levels";
			CONTROLLER.currentPage = "Timed";
			CONTROLLER.currentMode = 1;	
			//GameModePageGO.SetActive (false);
			//LevelPageGO.SetActive (true);
		}
		SceneManager.LoadScene ("Game");
	}
	//====================================================== GAME-MODE CLICK =========================================================================

	//====================================================== LEVEL CLICKED =========================================================================
	public void Level_Clicked () {
		int currentLevel = int.Parse (EventSystem.current.currentSelectedGameObject.name);
		CONTROLLER.currentlevel = currentLevel;
		Time.timeScale = 1.0f;
		SceneManager.LoadScene ("Game");
	}
	//====================================================== LEVEL CLICKED =========================================================================


	//====================================================== Back Button Clicked =========================================================================
	public void BackBtn_Clicked () {
		if (CONTROLLER.currentPage == "Gamemode") {
			CONTROLLER.currentPage = "Landingpage";
			LandingPageGO.SetActive (true);
			GameModePageGO.SetActive (false);
		} else if (CONTROLLER.currentPage == "Settings") {
			CONTROLLER.currentPage = "Settings";
			SettingsPageGO.SetActive (false);
			LandingPageGO.SetActive (true);
		} else if (CONTROLLER.currentPage == "Customization") {
			CONTROLLER.currentPage = "Landingpage";
			LandingPageGO.SetActive (true);
			CustomizePageGO.SetActive (false);
		} else if (CONTROLLER.currentPage == "Levels") {
			CONTROLLER.currentPage = "Gamemode";
			GameModePageGO.SetActive (true);
			LevelPageGO.SetActive (false);
		} else if (CONTROLLER.currentPage == "Landingpage") {
			Quit_App ();
		}
	}
	//====================================================== Back Button Clicked =========================================================================

}
