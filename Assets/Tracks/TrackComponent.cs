using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackComponent : SplineComponent {
    [Range(0, 40)]
    public int toRemove = 0;

    [Range(0.01f, 10.0f)]
    public float generatePointEvery = 1.0f;
    [Range(4, 40)]
    public int trackCircularSubDivide = 10;
    [Range(0.1f, 100.0f)]
    public float trackRadius = 10f;

    [Range(0.1f, 100.0f)]
    public float trackLinearTextureRepeat   = 5.0f;
    [Range(0.1f, 100.0f)]
    public float trackCircularTextureRepeat = 5.0f;
    
    void Update()
    {       
    }

    void Start()
    {
        Generate();

        var mesh = new Mesh();
        var meshFilter= GetComponent<MeshFilter>();
        var meshCollider = GetComponent<MeshCollider>();
        mesh.MarkDynamic();
        meshCollider.sharedMesh = mesh;
        meshFilter.sharedMesh = mesh;
    }

    void OnValidate()
    {
        if (uniformIndex != null) uniformIndex.ReIndex();
        StartCoroutine(CoGenerate());
    }

    IEnumerator CoGenerate()
    {
        yield return null;
        Generate();
    }
    public void Generate()
    {
        float length      = GetLength();
        float sliceCount  = length / this.generatePointEvery;
        float delta       = this.generatePointEvery / length;

        int   count  = Mathf.FloorToInt(sliceCount) * trackCircularSubDivide;


        if (points.Count > 0 && delta > Mathf.Epsilon)
        {
            var time = 0f;
            var index = 0;

            List<Vector2> uvs = new List<Vector2>(count);
            List<Vector3> vertices = new List<Vector3>(count);
            List<int>     triangles = new List<int>(count * 6);

            float deltaAngle = (Mathf.PI * 2) / (float)trackCircularSubDivide;

            float v = 0;
            do
            {
                var current = GetPoint(time);

                var rotation = Quaternion.LookRotation(GetForward(time), Vector3.up);
                var matrix = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
                Vector3 right = matrix.GetColumn(0);
                Vector3 up = matrix.GetColumn(1);

                float uDeltaRepeat = trackCircularTextureRepeat / (float)trackCircularSubDivide;
                float vDeltaRepeat = trackLinearTextureRepeat / (float)generatePointEvery;

                float u = 0;
                for (var i = 0; i < trackCircularSubDivide; ++i)
                {
                    var angle = deltaAngle * i;

                    vertices.Add(current + (right * Mathf.Cos(angle) * trackRadius + up * Mathf.Sin(angle) * trackRadius));

                    uvs.Add(new Vector2(u, v));
                    int b = (i + 1) % trackCircularSubDivide;

                    var i1 = (index + i);
                    var i2 = (i1 + trackCircularSubDivide) % count;
                    var i3 = (index + b);
                    var i4 = (i3 + trackCircularSubDivide) % count;
                        
                    triangles.Add(i1);
                    triangles.Add(i3);
                    triangles.Add(i2);

                    triangles.Add(i2);
                    triangles.Add(i3);
                    triangles.Add(i4);

                    u += uDeltaRepeat;
                }
                index += trackCircularSubDivide;
                v += vDeltaRepeat;
                time += delta;
            } while (time + delta < 1);
            var toRemoveCount = (trackCircularSubDivide * 6) * toRemove;
            if (toRemoveCount > 0)
            {
                triangles.RemoveRange(triangles.Count - toRemoveCount, toRemoveCount);
                triangles.RemoveRange(0, toRemoveCount);
            }
            var meshFilter = GetComponent<MeshFilter>();
            var mesh = meshFilter.sharedMesh;

            mesh.Clear();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var meshCollider = GetComponent<MeshCollider>();
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;

            gameObject.SetActive(false);
            gameObject.SetActive(true);



        }
    }
}
