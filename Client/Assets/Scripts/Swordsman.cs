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
	public float _xDeceleration = 0.1f;
	public float _yAcceleration = 0.1f;
	public float _yDeceleration = -0.1f;

	public float _jumpAngleMin = 0.25f;
	public float _jumpAngleFactor = 0.3f;
	public float _jumpHeightMax = 10;

	public int _moveGestureRadius = 100;

	#endregion Editor Configurables

	public const string SWORD_TAG = "Sword";

	private const int GESTURE_MOUSE_BUTTON = 0;

	private const string LEFT_SWORD = "/Swordsman/LeftSword";
	private const string RIGHT_SWORD = "/Swordsman/RightSword";

	private const string CIRCLE_TEXT = "Textures/circle";

	private const float MIN_VELOCITY = 0.1f;

	private static Swordsman s_player;
	public static Swordsman player { get { return s_player; } }

	private Rect _rectCircle;
	private Texture2D _circle;

	private List<Vector2> _moveGesture = new List<Vector2>();
	private List<Vector2> _actionGesture = new List<Vector2>();

	private Vector3 _velocity = Vector3.zero;

	private Vector2 _screenPos;

	private float _jumpTime = Single.MinValue;
	private float _swordTime = Single.MaxValue;

	private GameObject _leftSword;
	private GameObject _rightSword;

	private void Awake()
	{
		s_player = this;

		useGUILayout = false;

		_screenPos = Camera.main.WorldToScreenPoint(transform.position);
		_rectCircle.x = _screenPos.x - _moveGestureRadius;
		_rectCircle.y = Screen.height - _screenPos.y - _moveGestureRadius;
		_rectCircle.width = 2*_moveGestureRadius;
		_rectCircle.height = 2*_moveGestureRadius;

		_circle = Resources.Load(CIRCLE_TEXT) as Texture2D;

		_leftSword = GameObject.Find(LEFT_SWORD);
		_rightSword = GameObject.Find(RIGHT_SWORD);

		_leftSword.active = false;
		_rightSword.active = false;
	}

	private void OnGUI()
	{
		GUI.DrawTexture(_rectCircle, _circle);
	}

	private void FixedUpdate()
	{
		input();
		
		// Finish sword action
		if (_swordTime < Time.time)
		{
			_leftSword.active = false;
			_rightSword.active = false;
		}

		Vector3 pos = transform.position;

		// Movement along the X axis
		if (_moveGesture.Count > 1)
		{
			Vector3 dir =
				_moveGesture[_moveGesture.Count-1] - _moveGesture[0];
			_velocity.x = dir.x * _xVelocityFactor;
		}
		else if (Mathf.Abs(_velocity.x) > MIN_VELOCITY)
		{
			_velocity.x += _velocity.x > 0f ? -_xDeceleration : _xDeceleration;
		}
		else
		{
			_velocity.x = 0f;
		}

		// Movement along the Y axis
		if (_jumpTime > Time.time)
		{
			_velocity.y += _yAcceleration;
		}
		else if (pos.y > 0f)
		{
			_velocity.y += _yDeceleration;
		}

		pos += _velocity;
		
		// Don't go below the ground
		if (transform.position.y < 0f)
		{
			pos.y = 0f;
			_velocity.y = 0f;
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

		if (dot > _jumpAngleMin)
		{
			_jumpTime = Time.time + _jumpAngleFactor*dot;
		}

		_moveGesture.Clear();
	}

	private void onActionGesture()
	{
		Vector3 dir = _actionGesture[_actionGesture.Count-1] - _actionGesture[0];
		
		if (dir.x > 0)
		{
			_leftSword.active = false;
			_rightSword.active = true;
		}
		else if (dir.x < 0)
		{
			_leftSword.active = true;
			_rightSword.active = false;
		}

		_swordTime = Time.time + _attackSpeed;

		_actionGesture.Clear();
	}
}
