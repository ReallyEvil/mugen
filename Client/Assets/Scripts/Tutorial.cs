using System.Collections.Generic;

using UnityEngine;

public class Tutorial: MonoBehaviour
{
	private const string TUTORIAL_0 = "Textures/Tutorial Screen 00";
	private const string TUTORIAL_1 = "Textures/Tutorial Screen 01";
	private const string TUTORIAL_2 = "Textures/Tutorial Screen 02";
	private const string TUTORIAL_3 = "Textures/Tutorial Screen 03";

	private List<Texture2D> _tutorials = new List<Texture2D>();

	private int _idx = 0;

	private Rect _rectTutorial;

	private Vector2 _startPos = Vector2.zero;

	private void Awake()
	{
		_rectTutorial = new Rect(0, 0, Screen.width, Screen.height);

		_tutorials.Add(Resources.Load(TUTORIAL_0) as Texture2D);
		_tutorials.Add(Resources.Load(TUTORIAL_1) as Texture2D);
		_tutorials.Add(Resources.Load(TUTORIAL_2) as Texture2D);
		_tutorials.Add(Resources.Load(TUTORIAL_3) as Texture2D);
	}

	private void OnGUI()
	{
		GUI.DrawTexture(_rectTutorial, _tutorials[_idx]);
	}

	private void Update()
	{
		if (Application.platform == RuntimePlatform.Android ||
			Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if (Input.touchCount == 0)
			{
				return;
			}

			Touch touch = Input.touches[0];

			if (touch.phase == TouchPhase.Began)
			{
				_startPos = touch.position;
			}
			if (touch.phase == TouchPhase.Ended)
			{
				onSwipe(touch.position.x > _startPos.x);
			}
		}
		else if (Application.platform == RuntimePlatform.WindowsEditor)
		{
			if (Input.GetMouseButtonDown(0))
			{
				_startPos = Input.mousePosition;
			}
			else if (Input.GetMouseButtonUp(0))
			{
				onSwipe(Input.mousePosition.x > _startPos.x);
			}
		}
	}

	private void onSwipe(bool isRight)
	{
		if (isRight)
		{
			++_idx;
		}
		else if (_idx != 0)
		{
			--_idx;
		}

		if (_idx >= _tutorials.Count)
		{
			Destroy(this);
			Application.LoadLevel(Mugen.LEVEL_NAME);
		}
	}
}
