using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class CONTROLLER {

	public static bool isLaunched;

	public static bool isRestartedWithVideo;
	public static float rewardedCooldown = 3f;
	public static bool videoRespawn = false;

	public static int maxGhostSpawnCount = 10;	//2 -> Testing
	public static int suitcaseCount;

	public static List<Vector3> ghostwayPoints = new List<Vector3> ();
	public static string currentDirection;

	public static bool isgameStart = true;
	public static bool isgamePause = false;

	// Powerup
	public static int powerupMaxCount;
	public static int currentPowerup;
	public static bool isPowerupEnabled = false;
	// Powerup

	public static float m_ghostSpeed = 8f;

	public static int speedIncreaseCount = 5;

	public static int m_score = 0;
	public static int m_highScore = 0;

	public static string currentScreen = string.Empty;

    public static LevelDetail[] leveldetail;
    public static int currentlevel;
    public static int currentEnemy;
    public static int currentMode = 0;  // 0-> Endless, 1-> Level

	public static string currentPage;
}

public class LevelDetail {
    public float playerXpos;
    public float playerZpos;
    public enemyPos[] enemyPosition;
    public float seed;
    public int fillPercent;
}

public class enemyPos {
    public float xPos;
    public float zPos;
}
