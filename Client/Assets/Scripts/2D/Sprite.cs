using System;
using System.Collections.Generic;

using UnityEngine;

public class Sprite: MonoBehaviour
{
    private const float DEFAULT_FRAME_TIME = 0.2f;

    private Vector2[] _uvs;
    private Texture _tex;

    private Mesh _mesh;

    private List<Rect> _frames = new List<Rect>();

    private int _idx;

    private float _nextTime = Single.MinValue;

    public float _frameTime = DEFAULT_FRAME_TIME;

    public void Awake()
    {
        _frames.Add(new Rect(0, 0, 250, 250));
        _frames.Add(new Rect(250, 0, 250, 250));
        _frames.Add(new Rect(500, 0, 250, 250));
        _frames.Add(new Rect(750, 0, 250, 250));
        _frames.Add(new Rect(0, 250, 250, 250));
        _frames.Add(new Rect(250, 250, 250, 250));
        _frames.Add(new Rect(500, 250, 250, 250));
        _frames.Add(new Rect(750, 250, 250, 250));
        _frames.Add(new Rect(0, 500, 250, 250));

        _mesh = mesh();

        GetComponent<MeshFilter>().mesh = _mesh;

        _uvs = new Vector2[_mesh.uv.Length];
        _tex = GetComponent<Renderer>().sharedMaterial.mainTexture;
    }

    private void Update()
    {
        if (_nextTime > Time.time)
        {
            return;
        }

        playFrame(_idx);

        _nextTime = Time.time + _frameTime;

        if (_idx++ == _frames.Count-1)
        {
            _idx = 0;
        }
    }

    private void playFrame(int idx)
    {
        Rect frame = _frames[idx];

        Vector2 min = new Vector2(
            (float)frame.x / (float)_tex.width,
            1f - ((float)frame.y / (float)_tex.height));

        Vector2 dim = new Vector2(
            (float)frame.width / (float)_tex.width,
            -((float)frame.height / (float)_tex.height));

        _uvs[0] = min + new Vector2(dim.x * 0.0f, dim.y * 1.0f);
        _uvs[1] = min + new Vector2(dim.x * 1.0f, dim.y * 1.0f);
        _uvs[2] = min + new Vector2(dim.x * 0.0f, dim.y * 0.0f);
        _uvs[3] = min + new Vector2(dim.x * 1.0f, dim.y * 0.0f);

        _mesh.uv = _uvs;
    }

    private Mesh mesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(1, 1,  0),
            new Vector3(1, -1, 0),
            new Vector3(-1, 1,  0),
            new Vector3(-1, -1, 0),
        };

        Vector2[] uv = new Vector2[]
        {
            new Vector2(1, 1),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(0, 0),
        };

        int[] triangles = new int[]
        {
            0, 1, 2,
            2, 1, 3,
        };

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        return mesh;
    }
}
