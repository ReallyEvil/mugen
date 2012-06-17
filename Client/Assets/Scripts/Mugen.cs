using System.Collections.Generic;

using UnityEngine;

public class Mugen: MonoBehaviour
{
	private Color GESTURE_COLOR = Color.gray;
	private const int GESTURE_MOUSE_BUTTON = 0;
	private const int GESTURE_WIDTH = 5;

	private const float SPEED_MIN = 0.1f;
	private const float SPEED_FACTOR = 0.01f;
	private const float DECELERATION = 0.05f;

	private Rect _rectGesture;
	private Texture2D _gesture;

	private List<Vector2> _gesturePoints = new List<Vector2>();

	private float _speed = 0f;

	private void Awake()
	{
		_rectGesture.width = Screen.width;
		_rectGesture.height = Screen.height;

		_gesture = new Texture2D(
			Screen.width, Screen.height, TextureFormat.ARGB32, false);

		clearGesture();
	}

	private void OnGUI()
	{
		GUI.DrawTexture(_rectGesture, _gesture);
	}

	private void Update()
	{
		if (Application.platform == RuntimePlatform.WindowsEditor)
		{
			mouseInput();
		}
		else
		{
			touchInput();
		}
	}

	private void FixedUpdate()
	{
		if (_speed == 0f)
		{
			return;
		}

		if (Mathf.Abs(_speed) < SPEED_MIN)
		{
			_speed = 0f;
			clearGesture();
			return;
		}

		_speed += _speed > 0f ? -DECELERATION : DECELERATION;

		Vector3 pos = transform.position;
		pos.x += _speed;
		transform.position = pos;
	}

	private void mouseInput()
	{
		if (Input.GetMouseButton(GESTURE_MOUSE_BUTTON))
		{
			_gesturePoints.Add(Input.mousePosition);
		}
		else if (Input.GetMouseButtonUp(GESTURE_MOUSE_BUTTON))
		{
			onGesture();
		}
	}

	private void touchInput()
	{
		if (Input.touchCount > 0)
		{
			_gesturePoints.Add(Input.touches[0].position);
		}
		else if (Input.touchCount == 0 && _gesturePoints.Count > 0)
		{
			onGesture();
		}
	}

	private void onGesture()
	{
		processGesture();
		drawGesture();
		_gesturePoints.Clear();
	}

	private void processGesture()
	{
		Vector3 dir =
			_gesturePoints[_gesturePoints.Count-1] - _gesturePoints[0];

		// Just the x axis for now
		dir.y = 0;
		dir.z = 0;

		_speed = dir.x * SPEED_FACTOR;
	}

	private void drawGesture()
	{
		List<Vector2> points = new List<Vector2>();

		for (int i = 0; i < _gesturePoints.Count-1; ++i)
		{
			Vector2 start = _gesturePoints[i];
			Vector2 end = _gesturePoints[i+1];

			Vector2 dir = (end - start).normalized;

			while (Vector2.Distance(start, end) > 3f)
			{
				points.Add(start);
				start += dir;
			}
		}

		foreach (Vector2 point in points)
		{
			_gesture.SetPixel((int)point.x, (int)point.y, GESTURE_COLOR);

			for (int x = 1; x < GESTURE_WIDTH; ++x)
			{
				for (int y = 1; y < GESTURE_WIDTH; ++y)
				{
					_gesture.SetPixel((int)(point.x+x), (int)point.y-y, GESTURE_COLOR);
					_gesture.SetPixel((int)(point.x-x), (int)point.y-y, GESTURE_COLOR);
				}
			}
		}

		_gesture.Apply();
	}

	private void clearGesture()
	{
		for (int x = 0; x < _gesture.width; ++x)
		{
			for (int y = 0; y < _gesture.height; ++y)
			{
				_gesture.SetPixel(x, y, Color.clear);
			}
		}
		_gesture.Apply();
	}
}
