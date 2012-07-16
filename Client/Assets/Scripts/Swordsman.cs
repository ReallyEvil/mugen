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
	public float _slashPeriod = 0.1f;
	public float _slashVelocityY = 0.001f;
	public float _slashVelocityX = 0f;
	public float _slashVelocityXZeroDelta = 0.01f;

	public float _xVelocityFactorGround = 0.0006f;
	public float _xVelocityFactorAir = 0.0003f;
	public float _xVelocityMax = 0.5f;
	public float _xAccelerationMax = 0.02f;
	public float _friction = 0.01f;
	public float _frictionChangeDir = 0.02f;
	public float _gravity = -0.003f;

	public float _jumpAngleMin = 0.25f;
	public float _jumpFactor = 0.0015f;
	public int _jumpConsecutive = 2;

	public int _moveGestureRadius = 50;

	public float _minActionGestureLen = 5f;

	public float _dashVelocityGround = 0.3f;
	public float _dashPeriodGround = 0.5f;
	public float _dashVelocityAir = 0.3f;
	public float _dashPeriodAir = 0.4f;
	public float _dashDampening = 5f;
	public float _dashStun = 0.35f;
	public float _dashInvinciblePeriod = 0.5f;
	public float _dashInputPeriod = 0.2f;
	#endregion Editor Configurables

	public const string TAG_SWORD = "Sword";
	public const string TAG_PLAYER = "Player";
	public const string TAG_WALL = "Wall";

	private const int GESTURE_MOUSE_BUTTON = 0;

	private const string ARM = "/Swordsman/Arm";
	private const string SWORD = "/Swordsman/Arm/Sword/Cylinder";

	private const string INVINCIBLE_MAT = "Materials/SwordsmanInvincible";

	private const string CIRCLE_TEXT = "Textures/circle";

	private const float MIN_VELOCITY = 0.01f;
	
	public const int MIN_HEALTH = 0;
	public const int MAX_HEALTH = 100;

	public readonly Vector3 GIRTH = new Vector3(0.5f, 0, 0);

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
	
	private float _slashTime = Single.MinValue;
	private float _actionGestureTime = Single.MaxValue;

	private float _invincibleTime = Single.MinValue;

	private float _dashTime = Single.MinValue;
	private float _dashLerp = 1f;

	public bool isDashing { get { return _dashLerp < 1f; } }

	public bool isInvincible { get { return _invincibleTime > Time.time; } }
	public bool isSlashing { get { return _slashTime > Time.time; } }

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

		// Penalty for dashing is the lack of input
		if (isDashing)
		{
			updateDashing();
		}
		else
		{
			input();
		
			// Process x axis movement when on the ground
			if (pos.y == 0f)
			{
				updateGround();
			}
			// Process x axis movement when in the air
			else if (_moveGesture.Count > 1)
			{
				updateAir();
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

	private void updateDashing()
	{
		// Dashing broken up into two stages
		// 1) Constant velocity
		// 2) Deceleration
		if (_dashTime < Time.time)
		{
			_velocity.x = Mathf.Lerp(_velocity.x, 0, _dashLerp);

			_dashLerp += Time.deltaTime * _dashDampening;

			// Finished dashing 
			if (_dashLerp >= 1f)
			{
				_dashTime = Time.time + _dashStun;
				_dashLerp = 1f;
				_velocity.x = 0f;
			}
		}
	}

	private void updateGround()
	{
		if (_moveGesture.Count > 1)
		{
			float xVelocity =
				(_moveGesture[_moveGesture.Count-1] - _moveGesture[0]).x *
				_xVelocityFactorGround;

			// Ignore slower velocities in the same direction
			if (_velocity.x == 0f ||
				((_velocity.x > 0f && _velocity.x < xVelocity) ||
				(_velocity.x < 0f && _velocity.x > xVelocity)))
			{
				_velocity.x = Mathf.MoveTowards(
					_velocity.x, xVelocity, _xAccelerationMax);
			}
			// Decelerate when changing dir
			else if (Mathf.Sign(_velocity.x) != Mathf.Sign(xVelocity))
			{
				_velocity.x += _velocity.x > 0f ?
					-_frictionChangeDir : _frictionChangeDir;
			}
		}
		// Decelerate if there is no input and on the ground
		else if (_velocity.x != 0)
		{
			float sign = Mathf.Sign(_velocity.x);

			_velocity.x += _velocity.x > 0f ? -_friction : _friction;

			// Check sign to avoid flip floping around the equilibrium
			if (_velocity.x != 0f && Mathf.Sign(_velocity.x) != sign)
			{
				_velocity.x = 0f;
			}
		}
	}

	private void updateAir()
	{
		float xVelocity =
			(_moveGesture[_moveGesture.Count-1] - _moveGesture[0]).x *
			_xVelocityFactorAir;

		// Faster
		if (_velocity.x == 0f ||
			((_velocity.x > 0f && _velocity.x < xVelocity) ||
			(_velocity.x < 0f && _velocity.x > xVelocity)) ||
			Mathf.Sign(_velocity.x) != Mathf.Sign(xVelocity))
		{
			_velocity.x = Mathf.MoveTowards(
				_velocity.x, xVelocity, _xAccelerationMax);
		}
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
		Vector3 dir =
			_actionGesture[_actionGesture.Count-1] - _actionGesture[0];

		bool isAction = dir.magnitude > _minActionGestureLen;
		
		if (isAction)
		{
			float z = Vector3.Dot(dir.normalized, -Vector3.right) * 90;
			z = dir.y < 0f ? 180 - z : z;
			z = snapDirection(z);
			_arm.transform.eulerAngles = new Vector3(0f, 0f, z);

			_sword.active = true;
			_sword.renderer.enabled = true;
			_slashTime = Time.time + _slashPeriod;

			// Velocity effect y when falling
			if (_velocity.y < 0f)
			{
				_velocity.y += _slashVelocityY;
			}

			// Velocity effect x
			_velocity.x += _slashVelocityX * (dir.x > 0 ? 1 : -1);

			// Velocity zeroing
			_velocity.x =
				Mathf.MoveTowards(_velocity.x, 0, _slashVelocityXZeroDelta);
		}
		else if (_dashTime < Time.time &&
			_actionGestureTime != Single.MaxValue &&
			_dashInputPeriod > Time.time - _actionGestureTime)
		{
			// Only invincible when dashing along the ground
			// Dashing has different parameters in the air
			if (transform.position.y == 0f)
			{
				invincible(_dashInvinciblePeriod);

				_dashTime = Time.time + _dashPeriodGround;

				_velocity.x = _actionGesture[0].x > Screen.width/2 ?
					_dashVelocityGround : -_dashVelocityGround;
			}
			else
			{
				_dashTime = Time.time + _dashPeriodAir;

				_velocity.x = _actionGesture[0].x > Screen.width/2 ?
					_dashVelocityAir : -_dashVelocityAir;
				_velocity.y = 0f;
			}

			// Dashing cancels jumps
			_velocity.y = 0f;

			_dashLerp = 0f;
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

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag.Equals(TAG_WALL))
		{
			_velocity.x = 0f;
			transform.position += GIRTH *
				(collision.transform.position.x < transform.position.x ? 1:-1);
		}
	}
}
