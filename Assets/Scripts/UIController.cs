using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using SimpleJSON;

public class UIController : MonoBehaviour {

	public static UIController instance;

	public Tutorial TutorialScript;

	public Image m_fadePlane;
	public GameObject pauseGO;
	public GameObject gameoverGO;
	public GameObject inGameUIGO;
	public GameObject MinimapGO;

	public GameObject WatchVideoGO;
	public GameObject DemoAdGO;

    public GameObject NextlevelButton;

	public Text scoreText;
    //public Text levelScoreText;
	public Text gameOverScore;
	public Text suitcaseCount;
	public Text highScoreText;
	public Text PowerupTimeText;
	public Text LoadingText;
	public Text TimerText;

    public MapGenerator mapGeneratorScript;
    public TextAsset jsonfile;

	void Awake () {
		instance = this;
		CONTROLLER.currentScreen = "menu";
		PlayerController.OnCollision += OnCollided;
		if (CONTROLLER.currentMode == 0) {
			mapGeneratorScript.gameObject.SetActive (false);
			EndlessMode ();
		} else if (CONTROLLER.currentMode == 1) {
			//mapGeneratorScript.gameObject.SetActive (true);
			//LevelClicked ();
			EndlessMode ();
		}
//		if(PlayerPrefs.HasKey ("hScore")) {
//			CONTROLLER.m_highScore = PlayerPrefs.GetInt ("hScore");
//		}
        //ParseFile(jsonfile.ToString());
	}

	private void HighScoreCalc () {
		if (CONTROLLER.m_highScore < CONTROLLER.m_score) {
			CONTROLLER.m_highScore = CONTROLLER.m_score;
			PlayerPrefs.SetInt ("hScore", CONTROLLER.m_highScore);
		}
	}

	private void OnCollided () {
		inGameUIGO.SetActive (false);
		m_fadePlane.gameObject.SetActive (true);
		MinimapGO.SetActive (false);
		CONTROLLER.isgameStart = false;
		CONTROLLER.isgamePause = true;
		if (!CONTROLLER.isRestartedWithVideo) {
			WatchVideoGO.SetActive (true);
			CONTROLLER.isRestartedWithVideo = true;
		} else {
			WatchVideo_Cancelled ();
		}
		Time.timeScale = 0.0f;
		CONTROLLER.currentScreen = "gameover";
	}

	public void GameMode_Clicked () {
		string click = EventSystem.current.currentSelectedGameObject.name;
		if (click == "Endless") {
			EndlessMode ();
		} else if (click == "Levels") {
		}
	}

    public void EndlessMode () {
        Time.timeScale = 1.0f;
		if (CONTROLLER.currentMode == 1) {
			TimerText.gameObject.SetActive (true);
			StartCoroutine ("EnableTimer");
		} else {
			TimerText.gameObject.SetActive (false);
		}
        StartCoroutine(FadeInOut("menu", Color.white, Color.clear, 2, 1.0f));
        CONTROLLER.currentScreen = "in-game";
    }

	private IEnumerator EnableTimer () {
		// For Starting level user will get around 60 seconds of time based on how much they collect time will be increased.
		// Game over logic will includes the time out and also collision between ghost and players.
		float localTime = 30 + Time.deltaTime;
		float remaining = 0f;
		while (Time.time < localTime) {
			remaining = localTime - Time.time;
			TimerText.text = "" + string.Format ("{0:00}", remaining);
			yield return null;
		}
	}

	public void QuitClicked () {
		PlayerController.OnCollision -= OnCollided;
		Application.Quit ();
	}

	public void PauseClicked () {
		StartCoroutine (FadeInOut ("in-game2Pause", Color.clear, Color.white, 0.1f, 0.0f));
	}

	public void ResumeClicked () {
		Time.timeScale = 1.0f;
		pauseGO.SetActive (false);
		StartCoroutine (FadeInOut ("pause2resume", Color.white, Color.clear, 0.1f, 1.0f));
	}

	public void MenuClicked () {
		m_fadePlane.color = new Color (1, 1, 1, 1);
		m_fadePlane.gameObject.SetActive (true);
		LoadingText.text = "Loading...";
		HighScoreCalc ();
		PlayerController.OnCollision -= OnCollided;
		PlayerController.instance.ResetVariables ();
		pauseGO.SetActive (false);
		gameoverGO.SetActive (false);
		SceneManager.LoadScene ("Mainmenu");
		CONTROLLER.currentScreen = "menu";
	}

	public void RetryClicked () {
		Time.timeScale = 1.0f;
		PlayerController.instance.ResetVariables ();
		gameoverGO.SetActive (false);
		MinimapGO.SetActive (true);
		StartCoroutine (FadeInOut ("gameover2retry", Color.white, Color.clear, 2, 1.0f));
	}

	public void WatchVideo_Clicked () {
		// Show Rewarded Video and after completion resume game.
		DemoAdGO.SetActive (true);
		WatchVideoGO.SetActive (false);
	}

	public void VideoCompleted () {
		CONTROLLER.isgameStart = true;
		DemoAdGO.SetActive (false);
		WatchVideoGO.SetActive (false);
		Time.timeScale = 1.0f;
		CONTROLLER.videoRespawn = true;
		StartCoroutine (FadeInOut ("pause2resume", Color.white, Color.clear, 0.1f, 1.0f));
		StartCoroutine (CoolDownCheck (CONTROLLER.rewardedCooldown));
	}

	private IEnumerator CoolDownCheck (float cooldown) {
		float local = Time.time + cooldown;
		while (local > Time.time) {
			yield return null;
		}
		CONTROLLER.videoRespawn = false;
	}

	public void WatchVideo_Cancelled () {
		CONTROLLER.isRestartedWithVideo = false;
		WatchVideoGO.SetActive (false);
		gameoverGO.SetActive (true);
		HighScoreCalc ();
		highScoreText.text = string.Empty;	// "HighScore : " + CONTROLLER.m_highScore.ToString ("00000");
		gameOverScore.text = "" + CONTROLLER.m_score.ToString ("N0");
		suitcaseCount.text = string.Empty + CONTROLLER.suitcaseCount.ToString ("N0");
		CONTROLLER.currentEnemy = 0;
		CONTROLLER.currentScreen = "gameover";
	}

	public IEnumerator FadeInOut (string present, Color from, Color to, float time, float timescale) {
		float speed = 1 / time;
		float percent = 0;
		while (percent < 1) {
			percent += Time.deltaTime * speed;
			m_fadePlane.color = Color.Lerp (from, to, percent);
			yield return null;
		}
		LoadingText.text = string.Empty;
        if (present == "menu")
        {
            CONTROLLER.isgameStart = true;
			m_fadePlane.gameObject.SetActive (false);
            inGameUIGO.SetActive(true);
            PlayerController.instance.SpawnTargets();
        }
        else if (present == "in-game2Pause")
        {
            CONTROLLER.isgamePause = true;
            inGameUIGO.SetActive(false);
            CONTROLLER.currentScreen = "pause";
            pauseGO.SetActive(true);
        }
        else if (present == "pause2resume")
        {
            CONTROLLER.isgamePause = false;
            inGameUIGO.SetActive(true);
			MinimapGO.SetActive (true);
            CONTROLLER.currentScreen = "in-game";
        }
        else if (present == "in-game2over")
        {
            CONTROLLER.currentEnemy = 0;
            gameoverGO.SetActive(true);
			MinimapGO.SetActive (false);
            CONTROLLER.currentScreen = "gameover";
        }
        else if (present == "gameover2retry")
        {
            GameObject[] Ghosts = GameObject.FindGameObjectsWithTag("Ghost");
            foreach (GameObject GO in Ghosts)
            {
                Destroy(GO);
            }
            Ghosts = GameObject.FindGameObjectsWithTag("Target");
            foreach (GameObject GO in Ghosts)
                Destroy(GO);
            CONTROLLER.isgameStart = true;
            CONTROLLER.isgamePause = false;
            inGameUIGO.SetActive(true);
            CONTROLLER.currentScreen = "in-game";
            PlayerController.instance.SpawnTargets();
        }
        else if (present == "in-levelcomplete")
        {
            CONTROLLER.currentEnemy = 0;
            if (CONTROLLER.currentlevel == CONTROLLER.leveldetail.Length)
                NextlevelButton.SetActive(false);
            CONTROLLER.currentScreen = "levelcomplete";
        }
        else if (present == "levelcomplete2next")
        {
            CONTROLLER.isgameStart = true;
            CONTROLLER.isgamePause = false;
            CONTROLLER.currentScreen = "in-game";
        }
		Time.timeScale = timescale;
		// ======================================= Enable Tutorial =======================================
		if(!PlayerPrefs.HasKey ("Tutorial")) {
			TutorialScript.gameObject.SetActive (true);
			TutorialScript.FirstTutorial ();
		}
		// ======================================= Enable Tutorial =======================================
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

	public void LevelClicked() {
//        int current = int.Parse (EventSystem.current.currentSelectedGameObject.name);
//        CONTROLLER.currentMode = 1;
//        CONTROLLER.currentlevel = current;
		mapGeneratorScript.m_seed = ""+CONTROLLER.leveldetail[CONTROLLER.currentlevel - 1].seed;
		mapGeneratorScript.m_randomFillPercent = CONTROLLER.leveldetail[CONTROLLER.currentlevel - 1].fillPercent;
        mapGeneratorScript.UpdateLevel();
       // Time.timeScale = 1.0f;
        StartCoroutine(FadeInOut("menu", Color.white, Color.clear, 2, 1.0f));
        CONTROLLER.currentScreen = "in-game";
    }

    public void NextLevelClicked() {
        
        GameObject[] Ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject GO in Ghosts) {
            Destroy(GO);
        }
        CONTROLLER.m_score = 0;
        CONTROLLER.currentlevel++;
        PlayerController.instance.gameObject.transform.position = new Vector3(CONTROLLER.leveldetail[CONTROLLER.currentlevel - 1].playerXpos, 0.2f, CONTROLLER.leveldetail[CONTROLLER.currentlevel - 1].playerZpos);
        mapGeneratorScript.m_seed = "" + CONTROLLER.leveldetail[CONTROLLER.currentlevel - 1].seed;
        mapGeneratorScript.m_randomFillPercent = CONTROLLER.leveldetail[CONTROLLER.currentlevel - 1].fillPercent;
        mapGeneratorScript.UpdateLevel();

        Time.timeScale = 1.0f;
        StartCoroutine(FadeInOut("menu", Color.white, Color.clear, 2, 1.0f));
        CONTROLLER.currentScreen = "in-game";
    }
}
