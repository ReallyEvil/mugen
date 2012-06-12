using UnityEngine;

public class Mugen: MonoBehaviour
{
	private const int WIDTH = 70;
	private const int HEIGHT = 70;
	
	private Rect _rect = new Rect(
		(Screen.width-WIDTH)/2, (Screen.height-HEIGHT)/2, WIDTH, HEIGHT);
	
	private void OnGUI()
	{
		if (Input.touchCount > 0)
		{
			_rect.x = Input.touches[0].position.x;
			_rect.y = (Screen.height - Input.touches[0].position.y);
		}
	
		GUI.Box(_rect, "Mugen : " + Input.touchCount);
	}
}