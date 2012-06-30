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

	public float _xVelocityFactor = 0.01f;
	public float _xVelocityMax = 2f;
	public float _xDecelerate = 0.075f;
	public float _xDecelerateChangeDir = 0.2f;
	public float _gravity = -0.075f;

	public float _jumpAngleMin = 0.25f;
	public float _jumpAngleFactor = 2f;
	public int _jumpConsecutive = 2;

	public int _moveGestureRadius = 100;

	public float _swordSpeed = 30f;

	#endregion Editor Configurables

	public const string SWORD_TAG = "Sword";

	private const int GESTURE_MOUSE_BUTTON = 0;

	private const string ARM = "/Swordsman/Arm";
	private const string SWORD = "/Swordsman/Arm/Sword";

	private const string CIRCLE_TEXT = "Textures/circle";

	private const float MIN_VELOCITY = 0.01f;
	
	private readonly Vector3 SWORD_ROT_START = new Vector3(0, 90, 90);
	private readonly Vector3 SWORD_ROT_END = new Vector3(0, 270, 90);

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
	
	private float _swordLerp;

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
	}

	private void OnGUI()
	{
		GUI.DrawTexture(_rectCircle, _circle);
	}

	private void Update()
	{
		input();
		
		if (_sword.active)
		{
			if (_swordLerp < 1f)
			{
				_sword.transform.localEulerAngles = Vector3.Lerp(
					SWORD_ROT_START, SWORD_ROT_END, _swordLerp);

				_swordLerp += Time.fixedDeltaTime * _swordSpeed;

				if (_swordLerp >= 1f)
				{
					_sword.transform.localEulerAngles = SWORD_ROT_START;
					_sword.active = false;
				}
			}
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
				_velocity.x = xVelocity;
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
		if (pos.y > 0f)
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
			_velocity.y = _jumpAngleFactor*dot;
			--_jumps;
		}

		_moveGesture.Clear();
	}

	private void onActionGesture()
	{
		_sword.active = true;
		_swordLerp = 0f;

		// Rotate the swordman's arm
		Vector3 dir = _actionGesture[_actionGesture.Count-1] - _actionGesture[0];
		float z = Vector3.Dot(dir.normalized, Vector3.up) * 90;
		z = dir.x < 0f ? 180 - z : z;
		_arm.transform.eulerAngles = new Vector3(0f, 0f, z);

		_actionGesture.Clear();
	}
}
