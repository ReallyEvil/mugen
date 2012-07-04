using UnityEngine;

public class Grunt: MonoBehaviour
{
	public const string TAG = "Grunt";
	public const string PREFAB = "Prefabs/Grunt";

	private const int HEALTH_Y = 200;
	private const int HEALTH_HEIGHT = 10;

	private readonly Vector3 ROT_RIGHT = new Vector3(0, 0, 0);
	private readonly Vector3 ROT_LEFT = new Vector3(0, 180, 0);

	#region Editor Configurables
	public int _maxHealth = 10;
	public int _points = 1;
	public float _speed = 0.05f;
	public float _minDistance = 0.5f;
	public float _hitStunPeriod = 1f;
	public float _alertRadius = 10f;
	#endregion Editor Configurables

	private int _health;

	private Rect _rectHealth;

	private float _maxHealthWidth;

	private float _hitStun = 0;

	private void Start()
	{
		_health = _maxHealth;

		Vector3 screenMin = Camera.main.WorldToScreenPoint(renderer.bounds.min);
		Vector3 screenMax = Camera.main.WorldToScreenPoint(renderer.bounds.max);
		_maxHealthWidth = screenMax.x - screenMin.x;

		_rectHealth = new Rect(0, HEALTH_Y, _maxHealthWidth, HEALTH_HEIGHT);
	}

	private void FixedUpdate()
	{
		if (_hitStun > Time.time)
		{
			return;
		}

		Vector3 dir = Swordsman.player.gameObject.transform.position - 
			gameObject.transform.position;
		dir.y = 0f;
		dir.z = 0f;
		
		float absX = Mathf.Abs(dir.x);

		if (absX > _minDistance && absX < _alertRadius)
		{
			gameObject.transform.position += _speed*dir.normalized;
			gameObject.transform.eulerAngles = dir.x > 0 ? ROT_RIGHT : ROT_LEFT;
		}
	}

	private void OnGUI()
	{
		_rectHealth.width = _maxHealthWidth * _health / _maxHealth;
		_rectHealth.x = Camera.main.WorldToScreenPoint(transform.position).x -
			_rectHealth.width/2;

		GUI.Box(_rectHealth, "");
	}

	private void OnCollisionEnter(Collision collision)
	{
		string otherTag = collision.gameObject.tag;

		if (otherTag.Equals(Swordsman.SWORD_TAG))
		{
			_hitStun = Time.time + _hitStunPeriod;

			if (--_health == 0)
			{
				Swordsman.player.score += _points;
				Destroy(gameObject);
			}
		}
		else if (otherTag.Equals(Swordsman.PLAYER_TAG))
		{
			if (!Swordsman.player.isInvincible)
			{
				--Swordsman.player.health;
			}
		}
	}
}
