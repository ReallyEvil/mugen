using UnityEngine;

public class Grunt: MonoBehaviour
{
	public const string TAG = "Grunt";
	public const string PREFAB = "Prefabs/Grunt";

	private const float SPEED = 0.1f;
	private const float MIN_DIST = 0.5f;

	private void FixedUpdate()
	{
		Vector3 dir = Swordsman.player.gameObject.transform.position - 
			gameObject.transform.position;
		dir.y = 0f;
		dir.z = 0f;

		if (Mathf.Abs(dir.x) > MIN_DIST)
		{
			gameObject.transform.position += SPEED*dir.normalized;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		string otherTag = collision.gameObject.tag;

		if (otherTag.Equals(Swordsman.SWORD_TAG))
		{
			Destroy(gameObject);
		}
		else if (otherTag.Equals(Swordsman.PLAYER_TAG))
		{
			--Swordsman.player.health;
		}
	}
}
