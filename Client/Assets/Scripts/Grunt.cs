using UnityEngine;

public class Grunt: MonoBehaviour
{
	public const string TAG = "Grunt";
	public const string PREFAB = "Prefabs/Grunt";

	private const float SPEED = 0.1f;
	private const float MIN_DIST = 1f;

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
		if (collision.gameObject.tag.Equals(Swordsman.SWORD_TAG))
		{
			Destroy(gameObject);
		}
	}
}
