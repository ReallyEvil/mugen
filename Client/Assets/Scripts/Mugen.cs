using UnityEngine;

public class Mugen: MonoBehaviour
{
	private const int GRUNT_COUNT = 3;
	private const int SPAWN_DIST_MIN = 20;
	private const int SPAWN_DIST_MAX = 25;

	private Object _gruntPrefab;

	private void Awake()
	{
		_gruntPrefab = Resources.Load(Grunt.PREFAB);

		DontDestroyOnLoad(this);
	}

	public void Update()
	{
		Vector3 spawnPos = Swordsman.player.gameObject.transform.position;

		// Continously spawn an enemy near the swordsman
		int spawnCount =
			GRUNT_COUNT - GameObject.FindGameObjectsWithTag(Grunt.TAG).Length;

		for (int i = 0; i < spawnCount; ++i)
		{
			float dist = Random.Range(SPAWN_DIST_MIN, SPAWN_DIST_MAX) *
				(Random.value > 0.5f ? -1f : 1f);

			GameObject grunt = Instantiate(_gruntPrefab) as GameObject;
			grunt.transform.position = new Vector3(spawnPos.x + dist, 0f, 0f);
		}
	}
}
