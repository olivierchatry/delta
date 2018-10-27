using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackComponent : SplineComponent {
    [Range(0.1f, 10.0f)]
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
    }

    void OnValidate()
    {
        if (uniformIndex != null) uniformIndex.ReIndex();
        Generate();
    }

    void Generate()
    {
        var mf   = GetComponent< MeshFilter > ();
        var mesh = new Mesh();

        mf.mesh = mesh;

        float length      = GetLength();
        float sliceCount  = length / this.generatePointEvery;
        float delta       = this.generatePointEvery / length;

        int   count  = Mathf.FloorToInt(sliceCount) * trackCircularSubDivide;

        if (points.Count > 0 && delta > Mathf.Epsilon) {
            var time  = 0f;
            var index = 0;

            List<int> ib = new List<int>(count * 6);
            List<Vector3> vb = new List<Vector3>(count);
            List<Vector2> uvb = new List<Vector2>(count);

            float deltaAngle = (Mathf.PI * 2) / (float) trackCircularSubDivide;

            float v = 0;
            do
            {
                time += delta;
                var current = GetNonUniformPoint(time);
                var right = GetLeft(time);
                var up = GetUp(time);


                float uDeltaRepeat = trackCircularTextureRepeat / (float) trackCircularSubDivide;
                float vDeltaRepeat = trackLinearTextureRepeat / (float) generatePointEvery;

                float u = 0;
                for (var i = 0; i < trackCircularSubDivide; ++i)
                {
                    var angle = deltaAngle * i;

                    vb.Add(current + right * Mathf.Cos(angle) * trackRadius + up * Mathf.Sin(angle) * trackRadius);

                    uvb.Add(new Vector2(u, v));

                    int b = (i + 1) % trackCircularSubDivide;
                    
                    var i1 = (index + i);
                    var i2 = (i1 + trackCircularSubDivide) % count;
                    var i3 = (index + b);
                    var i4 = (i3 + trackCircularSubDivide) % count;

                    ib.Add(i1);
                    ib.Add(i2);
                    ib.Add(i3);

                    ib.Add(i2);
                    ib.Add(i4);
                    ib.Add(i3);


                    u += uDeltaRepeat;
                }
                index += trackCircularSubDivide;
                // TODO : need to generate a new mesh in case we reach max indices.
                v += vDeltaRepeat;
            } while (time + delta <= 1);

            mesh.vertices = vb.ToArray();
            mesh.triangles = ib.ToArray();
            mesh.uv = uvb.ToArray();
            mesh.RecalculateNormals();
        }


    }
}
