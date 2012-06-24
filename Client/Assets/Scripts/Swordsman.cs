using System;
using System.Collections.Generic;

using UnityEngine;

public class Swordsman: MonoBehaviour
{
	private enum Movement
	{
		Idle,
		Run,
		Decelerate
	}

	public const string SWORD_TAG = "Sword";

	private const int GESTURE_MOUSE_BUTTON = 0;

	private const string LEFT_SWORD = "/Swordsman/LeftSword";
	private const string RIGHT_SWORD = "/Swordsman/RightSword";
	private const float TIME_SWORD = 0.1f;

	private const float SPEED_MIN = 0.1f;
	private const float SPEED_FACTOR = 0.003f;
	private const float DECELERATION = 0.01f;

	private const float JUMP_FACTOR_ANGLE = 20f;
	private const float JUMP_DOT_MIN = 0.25f;
	private const float JUMP_MAX = 10;
	private const float GRAVITY = -0.3f;

	private const string CIRCLE_TEXT = "Textures/circle";

	private const float CIRCLE_ALPHA_MIN = 0.4f;
	private const float CIRCLE_ALPHA_MAX = 0.8f;
	private const int CIRCLE_RADIUS = 100;

	private static Swordsman s_player;
	public static Swordsman player { get { return s_player; } }

	private Rect _rectCircle;
	private Texture2D _circle;

	private List<Vector2> _moveGesture = new List<Vector2>();
	private List<Vector2> _actionGesture = new List<Vector2>();

	private float _speed = 0f;

	private float _jumpPower = 0f;

	private Vector2 _screenPos;

	private Movement _movement = Movement.Idle;

	private float _swordTime = Single.MaxValue;

	private GameObject _leftSword;
	private GameObject _rightSword;

	private void Awake()
	{
		s_player = this;

		useGUILayout = false;

		_screenPos = Camera.main.WorldToScreenPoint(transform.position);
		_rectCircle.x = _screenPos.x - CIRCLE_RADIUS;
		_rectCircle.y = Screen.height - _screenPos.y - CIRCLE_RADIUS;
		_rectCircle.width = 2*CIRCLE_RADIUS;
		_rectCircle.height = 2*CIRCLE_RADIUS;

		_circle = Resources.Load(CIRCLE_TEXT) as Texture2D;

		_leftSword = GameObject.Find(LEFT_SWORD);
		_rightSword = GameObject.Find(RIGHT_SWORD);

		_leftSword.active = false;
		_rightSword.active = false;
	}

	private void OnGUI()
	{
		float lerp = Mathf.Abs(_speed / (Screen.width/2 * SPEED_FACTOR));
		lerp = Mathf.Clamp(lerp, CIRCLE_ALPHA_MIN, CIRCLE_ALPHA_MAX);
		GUI.color = Color.Lerp(Color.clear, Color.white, lerp);
		
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

		// Y translation
		Vector3 pos = transform.position;
		if (_jumpPower > 0)
		{
			pos.y -= GRAVITY;
			_jumpPower += GRAVITY;
			_jumpPower = Mathf.Max(0f, _jumpPower);
		}
		else if (pos.y > 0f)
		{
			pos.y += GRAVITY;
			pos.y = Mathf.Max(0f, pos.y);
			pos.y = Mathf.Clamp(pos.y, 0f, JUMP_MAX);
		}

		transform.position = pos;

		// X translation
		switch (_movement)
		{
			case Movement.Idle:
				idle();
				break;
			case Movement.Run:
				run();
				break;
			case Movement.Decelerate:
				decelerate();
				break;
		}
	}

	private void idle()
	{
		// TODO: Animate
	}

	private void run()
	{
		// TODO: Animate

		Vector3 dir =
			_moveGesture[_moveGesture.Count-1] - _moveGesture[0];

		// Just the x axis for now
		dir.y = 0;
		dir.z = 0;

		_speed = dir.x * SPEED_FACTOR;

		Vector3 pos = transform.position;
		pos.x += _speed;
		transform.position = pos;
	}

	private void decelerate()
	{
		// TODO: Animate
	
		Vector3 pos = transform.position;

		// Translate
		pos.x += _speed;
		transform.position = pos;

		// Decelerate
		// No air friction
		if (pos.y == 0f)
		{
			_speed += _speed > 0f ? -DECELERATION : DECELERATION;
		}

		if (Mathf.Abs(_speed) < SPEED_MIN)
		{
			_movement = Movement.Idle;
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
			isMove = Vector2.Distance(_screenPos, pos) < CIRCLE_RADIUS;
		}
		else
		{
			isMove = _moveGesture.Count > 0;
		}

		if (isMove)
		{
			_movement = Movement.Run;
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

		if (dot > JUMP_DOT_MIN)
		{
			_jumpPower = JUMP_FACTOR_ANGLE * dot;
		}

		_movement = Movement.Decelerate;

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

		_swordTime = Time.time + TIME_SWORD;

		_actionGesture.Clear();
	}
}
