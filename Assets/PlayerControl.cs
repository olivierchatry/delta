using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour {
    private TrackComponent currentTrack;

    public float velocity;
    public float acceleration;

	// Use this for initialization
	void Start () {
		
	}

    void Awake()
    {
        GameObject track = GameObject.Find("Track");
        if (track)
        {
            currentTrack = track.GetComponent<TrackComponent>();
            Debug.Log("found track");
        }
        if (currentTrack)
        {
            var start = currentTrack.GetPoint(0);
            var forward = currentTrack.GetForward(0);
            var rotation = Quaternion.LookRotation(forward, Vector3.up);
            var matrix = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);            
            Vector3 up = matrix.GetColumn(1);
            var position = start + up * 100;

            RaycastHit hit;
            if (!Physics.Raycast(new Ray(start, up), out hit))
                return;


            transform.position = hit.point + up;
            transform.up = up;
            transform.forward = forward;


        }

    }
    // Update is called once per frame
    void Update ()
    {
        var start = currentTrack.transform.TransformPoint(currentTrack.GetPoint(0));
        var forward = currentTrack.GetForward(0);
        var rotation = Quaternion.LookRotation(forward, Vector3.up);
        var matrix = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
        Vector3 up = matrix.GetColumn(1);
        var position = start + up * 100;

        Debug.DrawRay(position, up);
        RaycastHit hit;
        if (!Physics.Raycast(new Ray(start, up), out hit))
            return;
        

        transform.position = hit.point + up;
        transform.up = up;
        transform.forward = forward;
    
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    GetComponent<Renderer>().material.color = Color.red;
        //}
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    GetComponent<Renderer>().material.color = Color.green;
        //}
        //if (Input.GetKeyDown(KeyCode.B))
        //{
        //    GetComponent<Renderer>().material.color = Color.blue;
        //}
    }
}
