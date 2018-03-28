using UnityEngine;

public class Background: MonoBehaviour
{
	private const string PREFAB = "Prefabs/Tile";

	private Transform _tile;
	private Transform _nextTile;

	private Vector3 _offsetLeft;
	private Vector3 _offsetRight;

	private Vector3 _extentLeft;
	private Vector3 _extentRight;

	private void Awake()
	{
		// Only need to repeat two tiles that take up the entire screen
		Object prefab = Resources.Load(PREFAB);
		_tile = (Instantiate(prefab) as GameObject).transform;
		_nextTile = (Instantiate(prefab) as GameObject).transform;

		float width = _tile.GetComponent<Renderer>().bounds.size.x;
		_offsetLeft = new Vector3(-width, 0f, 0f);
		_offsetRight = new Vector3(width, 0f, 0f);

		_extentLeft = _tile.position + _offsetLeft/2;
		_extentRight = _tile.position + _offsetRight/2;
	}

	private void OnPreRender()
	{
		if (transform.position.x > _extentRight.x ||
			transform.position.x < _extentLeft.x)
		{
			Transform temp = _tile;
			_tile = _nextTile;
			_nextTile = temp;

			_extentLeft = _tile.position + _offsetLeft/2;
			_extentRight = _tile.position + _offsetRight/2;
		}

		if (transform.position.x > _tile.position.x &&
			_nextTile.position.x <= _tile.position.x)
		{
			_nextTile.position = _tile.position + _offsetRight;
		}

		if (transform.position.x < _tile.position.x &&
			_nextTile.position.x >= _tile.position.x)
		{
			_nextTile.position = _tile.position + _offsetLeft;
		}
	}
}
