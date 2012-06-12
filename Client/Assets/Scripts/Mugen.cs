using UnityEngine;

public class Mugen: MonoBehaviour
{
	private Rect _rect = new Rect(0, 0 , 100, 100);
	
	private void OnGUI()
	{
		GUI.Box(_rect, "Mugen");
	}
}