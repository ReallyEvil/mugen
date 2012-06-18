using System.Collections.Generic;

using UnityEngine;

public class Mugen: MonoBehaviour
{
	private enum Movement
	{
		Idle,
		Run,
		Decelerate
	}

	private const int GESTURE_MOUSE_BUTTON = 0;

	private const float SPEED_MIN = 0.1f;
	private const float SPEED_FACTOR = 0.003f;
	private const float DECELERATION = 0.01f;

	private const string CIRCLE_TEXT = "Textures/circle";

	private const float CIRCLE_ALPHA_MIN = 0.4f;
	private const float CIRCLE_ALPHA_MAX = 0.8f;
	private const int CIRCLE_RADIUS = 100;

	private Rect _rectCircle;
	private Texture2D _circle;

	private List<Vector2> _gesturePoints = new List<Vector2>();

	private float _speed = 0f;

	private Vector2 _screenPos;

	private Movement _movement = Movement.Idle;

	private void Awake()
	{
		useGUILayout = false;

		_screenPos = Camera.main.WorldToScreenPoint(transform.position);
		_rectCircle.x = _screenPos.x - CIRCLE_RADIUS;
		_rectCircle.y = Screen.height - _screenPos.y - CIRCLE_RADIUS;
		_rectCircle.width = 2*CIRCLE_RADIUS;
		_rectCircle.height = 2*CIRCLE_RADIUS;

		_circle = Resources.Load(CIRCLE_TEXT) as Texture2D;
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
			_gesturePoints[_gesturePoints.Count-1] - _gesturePoints[0];

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

		// Translate
		Vector3 pos = transform.position;
		pos.x += _speed;
		transform.position = pos;

		// Decelerate
		_speed += _speed > 0f ? -DECELERATION : DECELERATION;

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
			else if (Input.touchCount == 0 && _gesturePoints.Count > 0)
			{
				onGesture();
			}
		}
		else if (Application.platform == RuntimePlatform.WindowsEditor)
		{
			if (Input.GetMouseButton(GESTURE_MOUSE_BUTTON))
			{
				onInput(Input.mousePosition);
			}
			else if (Input.GetMouseButtonUp(GESTURE_MOUSE_BUTTON) &&
				_gesturePoints.Count > 0)
			{
				onGesture();
			}
		}
	}

	private void onInput(Vector2 pos)
	{
		// First point needs to be withing the movement circle
		if (_gesturePoints.Count > 0 ||
			Vector2.Distance(_screenPos, pos) < CIRCLE_RADIUS)
		{
			_movement = Movement.Run;

			_gesturePoints.Add(pos);
		}
	}

	private void onGesture()
	{
		_movement = Movement.Decelerate;

		_gesturePoints.Clear();
	}
}
