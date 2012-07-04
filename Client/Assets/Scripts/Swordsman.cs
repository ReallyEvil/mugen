using System;
using System.Collections.Generic;

using UnityEngine;

public class Swordsman: MonoBehaviour
{
	private enum Movement
	{
		Run,
		Decelerate
	}

	#region Editor Configurables
	public float _attackSpeed = 0.1f;

	public float _xVelocityFactor = 0.0006f;
	public float _xVelocityMax = 0.5f;
	public float _xAccelerationMax = 0.02f;
	public float _xDecelerate = 0.01f;
	public float _xDecelerateChangeDir = 0.02f;
	public float _gravity = -0.007f;

	public float _jumpAngleMin = 0.25f;
	public float _jumpFactor = 0.0015f;
	public int _jumpConsecutive = 2;

	public int _moveGestureRadius = 50;

	public float _minActionGestureLen = 5f;

	public float _dashVelocity = 0.4f;
	public float _dashPeriod = 1f;
	public float _dashInvinciblePeriod = 0.5f;
	public float _dashInputPeriod = 0.2f;

	#endregion Editor Configurables

	public const string SWORD_TAG = "Sword";
	public const string PLAYER_TAG = "Player";

	private const int GESTURE_MOUSE_BUTTON = 0;

	private const string ARM = "/Swordsman/Arm";
	private const string SWORD = "/Swordsman/Arm/Sword/Cylinder";

	private const string INVINCIBLE_MAT = "Materials/SwordsmanInvincible";

	private const string CIRCLE_TEXT = "Textures/circle";

	private const float MIN_VELOCITY = 0.01f;
	
	public const int MIN_HEALTH = 0;
	public const int MAX_HEALTH = 100;

	private static Swordsman s_player;
	public static Swordsman player { get { return s_player; } }

	private Rect _rectCircle;
	private Texture2D _circle;

	private List<Vector2> _moveGesture = new List<Vector2>();
	private List<Vector2> _actionGesture = new List<Vector2>();

	private Vector3 _velocity = Vector3.zero;

	private int _jumps;

	private Vector2 _screenPos;

	private GameObject _arm;
	private GameObject _sword;
	
	private float _swordTime = Single.MinValue;
	private float _actionGestureTime = Single.MaxValue;

	private float _invincibleTime = Single.MinValue;
	private float _dashTime = Single.MinValue;

	public bool isInvincible { get { return _invincibleTime > Time.time; } }
	public bool isDashing { get { return _dashTime > Time.time; } }
	public bool isSlashing { get { return _swordTime > Time.time; } }

	private string _healthStr;

	private int _health;
	public int health
	{
		get { return _health; }
		set
		{
			_health = Mathf.Clamp(value, MIN_HEALTH, MAX_HEALTH);
			_healthStr = "Health: " + _health;
			_rectHealth.width = Screen.width * ((float)_health)/MAX_HEALTH;
		}
	}

	private string _scoreStr;

	private int _score;
	public int score
	{
		get { return _score; }
		set
		{
			_score = value;
			_scoreStr = "Score: " + _score;
		}
	}

	private Rect _rectHealth = new Rect(0, 0, Screen.width, 20);

	private Rect _rectScore = new Rect(0, 20, 100, 20);

	private Material _normalMaterial;
	private Material _invincibleMaterial;

	private void Awake()
	{
		s_player = this;

		_jumps = _jumpConsecutive;

		useGUILayout = false;

		_screenPos = Camera.main.WorldToScreenPoint(transform.position);
		_rectCircle.x = _screenPos.x - _moveGestureRadius;
		_rectCircle.y = Screen.height - _screenPos.y - _moveGestureRadius;
		_rectCircle.width = 2*_moveGestureRadius;
		_rectCircle.height = 2*_moveGestureRadius;

		_circle = Resources.Load(CIRCLE_TEXT) as Texture2D;

		_arm = GameObject.Find(ARM);

		_sword = GameObject.Find(SWORD);
		_sword.active = false;

		health = 100;
		score = 0;

		_normalMaterial = _arm.renderer.material;
		_invincibleMaterial = (Material)
			GameObject.Instantiate(Resources.Load(INVINCIBLE_MAT));
	}

	private void OnGUI()
	{
		GUI.DrawTexture(_rectCircle, _circle);
		GUI.Box(_rectHealth, _healthStr);
		GUI.Box(_rectScore, _scoreStr);
	}

	private void invincible(float period)
	{
		_invincibleTime = Time.time + period;
		_arm.renderer.material = _invincibleMaterial;
	}

	private void Update()
	{
		// Penalty for dashing is the lack of input
		if (!isDashing)
		{
			input();
		}
		
		// Invincibility
		if (!isInvincible && _arm.renderer.material != _normalMaterial)
		{
			_arm.renderer.material = _normalMaterial;
		}

		// Stop sword slashing
		if (_sword.active && !isSlashing)
		{
			_sword.active = false;
			_sword.renderer.enabled = false;
		}

		Vector3 pos = transform.position;

		// Process x axis movement
		if (_moveGesture.Count > 1)
		{
			float xVelocity =
				(_moveGesture[_moveGesture.Count-1] - _moveGesture[0]).x *
				_xVelocityFactor;

			// Faster
			if (_velocity.x == 0f ||
				((_velocity.x > 0f && _velocity.x < xVelocity) ||
				(_velocity.x < 0f && _velocity.x > xVelocity)))
			{
				_velocity.x = Mathf.MoveTowards(
					_velocity.x, xVelocity, _xAccelerationMax);
			}
			// Change dir
			else if (Mathf.Sign(_velocity.x) != Mathf.Sign(xVelocity))
			{
				_velocity.x += _velocity.x > 0f ?
					-_xDecelerateChangeDir : _xDecelerateChangeDir;
			}
		}
		// Decelerate if there is no input
		else if (_velocity.x != 0)
		{
			float sign = Mathf.Sign(_velocity.x);

			_velocity.x += _velocity.x > 0f ? -_xDecelerate : _xDecelerate;

			// Check sign to avoid flip floping around the equilibrium
			if (_velocity.x != 0f && Mathf.Sign(_velocity.x) != sign)
			{
				_velocity.x = 0f;
			}
		}

		_velocity.x = Mathf.Clamp(_velocity.x, -_xVelocityMax, _xVelocityMax);

		// Movement along the Y axis
		if (pos.y > 0f && !isDashing && !isSlashing)
		{
			_velocity.y += _gravity;
		}

		pos += _velocity;
		
		// Don't go below the ground
		if (transform.position.y < 0f)
		{
			pos.y = 0f;
			_velocity.y = 0f;
			_jumps = _jumpConsecutive;
		}

		transform.position = pos;
	}

	private void input()
	{
		if (Application.platform == RuntimePlatform.Android ||
			Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if (Input.touchCount > 0)
			{
				onInput(Input.touches[0].position);
			}
			else if (Input.touchCount == 0 && _moveGesture.Count > 0)
			{
				onMoveGesture();
			}
			else if (Input.touchCount == 0 && _actionGesture.Count > 0)
			{
				onActionGesture();
			}
		}
		else if (Application.platform == RuntimePlatform.WindowsEditor)
		{
			if (Input.GetMouseButton(GESTURE_MOUSE_BUTTON))
			{
				onInput(Input.mousePosition);
			}
			else if (Input.GetMouseButtonUp(GESTURE_MOUSE_BUTTON) &&
				_moveGesture.Count > 0)
			{
				onMoveGesture();
			}
			else if (Input.GetMouseButtonUp(GESTURE_MOUSE_BUTTON) &&
				_actionGesture.Count > 0)
			{
				onActionGesture();
			}
		}
	}

	private void onInput(Vector2 pos)
	{
		bool isMove = false;

		// Gestures started in the circle are  movements, outside are actions
		if (_moveGesture.Count == 0 || _actionGesture.Count == 0)
		{
			isMove = Vector2.Distance(_screenPos, pos) < _moveGestureRadius;
		}
		else
		{
			isMove = _moveGesture.Count > 0;
		}

		if (isMove)
		{
			_moveGesture.Add(pos);
		}
		else
		{
			_actionGesture.Add(pos);
		}
	}

	private void onMoveGesture()
	{
		Vector3 dir =
			_moveGesture[_moveGesture.Count-1] - _moveGesture[0];
		float dot = Vector3.Dot(dir.normalized, Vector2.up);

		if (_jumps > 0 && dot > _jumpAngleMin)
		{
			_velocity += dir * _jumpFactor;
			--_jumps;
		}

		_moveGesture.Clear();
	}

	private void onActionGesture()
	{
		// Rotate the swordman's arm
		Vector3 dir = _actionGesture[_actionGesture.Count-1] - _actionGesture[0];

		bool isAction = dir.magnitude > _minActionGestureLen;
		
		if (isAction)
		{
			float z = Vector3.Dot(dir.normalized, -Vector3.right) * 90;
			z = dir.y < 0f ? 180 - z : z;
			z = snapDirection(z);
			_arm.transform.eulerAngles = new Vector3(0f, 0f, z);

			_sword.active = true;
			_sword.renderer.enabled = true;
			_swordTime = Time.time + _attackSpeed;

			// Slashing cancels Y velocity
			_velocity.y = 0f;
		}
		else if (_dashTime < Time.time &&
			_dashInputPeriod > Time.time - _actionGestureTime)
		{
			_dashTime = Time.time + _dashPeriod;

			// Dashing cancels jumps
			_velocity.y = 0f;

			invincible(_dashInvinciblePeriod);

			_velocity.x += _actionGesture[0].x > Screen.width/2 ?
				_dashVelocity : -_dashVelocity;
		}

		_actionGestureTime = Time.time;
		_actionGesture.Clear();
	}

	// Snaps 360 degress into the 8 directional attack directions
	private float snapDirection(float angle)
	{
		if (angle > -22.5f && angle <= 22.5f)
		{
			return 0f;
		}
		else if (angle > 22.5f && angle <= 67.5f)
		{
			return 45f;
		}
		else if (angle > 67.5 && angle <= 112.5f)
		{
			return 90f;
		}
		else if (angle > 112.5f && angle <= 157.5f)
		{
			return 135f;
		}
		else if (angle > 157.5f && angle <= 202.5f)
		{
			return 180f;
		}
		else if (angle > 202.5f && angle <= 247.5f)
		{
			return 225f;
		}
		else if (angle > 247.5f || angle < -67.5f)
		{
			return 270f;
		}
		else if (angle > -67.5f && angle < -22.5f)
		{
			return 315f;
		}

		Debug.Log("Error in snapping angle to 8 directions");
		return 0;
	}
}
