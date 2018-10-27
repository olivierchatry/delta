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
    }

    void OnValidate()
    {
        if (uniformIndex != null) uniformIndex.ReIndex();
        StartCoroutine(GenerateCoRoutine());
    }

    public IEnumerator GenerateCoRoutine()
    {        
        Generate();
        yield return null;
    }

    public void Generate()
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
            var timeForward = 0f;
            var index = 0;

            List<int> ib = new List<int>(count * 6);
            List<Vector3> vb = new List<Vector3>(count);
            List<Vector2> uvb = new List<Vector2>(count);

            float deltaAngle = (Mathf.PI * 2) / (float) trackCircularSubDivide;

            float v = 0;
            bool continueToGenerate = true;
            Quaternion previousRotation = Quaternion.LookRotation(GetForward(time), Vector3.up);
            do
            {

                if (time >= 1)
                {
                    time = 0;
                    continueToGenerate = false;
                }
                else
                {
                    timeForward = time;
                }

                var current = GetPoint(time);


                //var up = GetUp(time);
                //var direction = GetForward(time);
                //var right = Vector3.Cross(up, direction);

                var rotation = Quaternion.LookRotation(GetForward(timeForward), Vector3.up);
                // When the angle of the rotation compared to the last segment is too high
                // smooth the rotation a little bit. Optimally we would smooth the entire sections array.
                if (Quaternion.Angle(previousRotation, rotation) > 20)
                {
                    rotation = Quaternion.Slerp(previousRotation, rotation, 0.5f);
                }                    
                previousRotation = rotation;
                var matrix = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
                Vector3 right = matrix.GetColumn(0);
                Vector3 up = matrix.GetColumn(1);

                float uDeltaRepeat = trackCircularTextureRepeat / (float) trackCircularSubDivide;
                float vDeltaRepeat = trackLinearTextureRepeat / (float) generatePointEvery;

                float u = 0;
                for (var i = 0; i < trackCircularSubDivide; ++i)
                {
                    var angle = deltaAngle * i;
                    
                    vb.Add(current + (right * Mathf.Cos(angle) * trackRadius + up * Mathf.Sin(angle) * trackRadius));

                    uvb.Add(new Vector2(u, v));
                    if (continueToGenerate)
                    {
                        int b = (i + 1) % trackCircularSubDivide;

                        var i1 = (index + i);
                        var i2 = (i1 + trackCircularSubDivide) % count;
                        var i3 = (index + b);
                        var i4 = (i3 + trackCircularSubDivide) % count;

                        ib.Add(i1);
                        ib.Add(i3);
                        ib.Add(i2);

                        ib.Add(i2);
                        ib.Add(i3);
                        ib.Add(i4);
                    }


                    u += uDeltaRepeat;
                }
                index += trackCircularSubDivide;
                // TODO : need to generate a new mesh in case we reach max indices.
                v += vDeltaRepeat;
                time += delta;
            } while (continueToGenerate);
            //} while (time + delta < 1) ;
            var toRemoveCount = (trackCircularSubDivide * 6) * toRemove;
            if (toRemoveCount > 0)
            {
                ib.RemoveRange(ib.Count - toRemoveCount, toRemoveCount);
                ib.RemoveRange(0, toRemoveCount);
            }
            mesh.vertices = vb.ToArray();
            mesh.triangles = ib.ToArray();
            mesh.uv = uvb.ToArray();
            mesh.RecalculateNormals();
        }


    }
}
