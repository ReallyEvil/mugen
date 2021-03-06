using System.Collections.Generic;

using UnityEngine;

public class Mugen: MonoBehaviour
{
	#region Editor Configurables
	public int _gruntCount = 5;
	public int _spawnDistMin = 5;
	public int _spawnDistMax = 20;
	#endregion Editor Configurables

	public const string LEVEL_NAME = "Mugen";

	private const int GAME_OVER_WIDTH = 400;
	private const int GAME_OVER_HEIGHT = 400;

	private Object _gruntPrefab;

	private bool _isGameOver = false;

	private List<GameObject> _grunts = new List<GameObject>();
	
	private Rect _rectGameOver;

	private void Awake()
	{
		_gruntPrefab = Resources.Load(Grunt.PREFAB);

		DontDestroyOnLoad(this);
	}

	private void Start()
	{
		_rectGameOver = new Rect(
			(Screen.width - GAME_OVER_WIDTH)/2,
			(Screen.height - GAME_OVER_HEIGHT)/2,
			GAME_OVER_WIDTH, GAME_OVER_HEIGHT);
	}

	public void Update()
	{
		Vector3 spawnPos = Swordsman.player.gameObject.transform.position;

		// Continously spawn an enemy near the swordsman
		int spawnCount =
			_gruntCount - GameObject.FindGameObjectsWithTag(Grunt.TAG).Length;

		for (int i = 0; i < spawnCount; ++i)
		{
			float dist = Random.Range(_spawnDistMin, _spawnDistMax) *
				(Random.value > 0.5f ? -1f : 1f);

			GameObject grunt = Instantiate(_gruntPrefab) as GameObject;
			grunt.transform.position = new Vector3(spawnPos.x + dist, 0f, 0f);

			_grunts.Add(grunt);
		}

		if (Swordsman.player.health == Swordsman.MIN_HEALTH)
		{
			_isGameOver = true;
		}
	}

	private void OnGUI()
	{
		if (_isGameOver && GUI.Button(_rectGameOver, "You are slashed. Kontinue?"))
		{
			// Revive the swordsman
			Swordsman.player.health = Swordsman.MAX_HEALTH;

			// Clear all enemies
			foreach (GameObject grunt in _grunts)
			{
				Destroy(grunt);
			}

			_isGameOver = false;
		}
	}
}
