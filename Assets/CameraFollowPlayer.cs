using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour {

    // private TrackComponent currentTrack;
    private PlayerControl playerControl;
    // Use this for initialization
    void Start () {
		
	}
    void Awake()
    {
        //GameObject track = GameObject.Find("Track");
        //if (track)
        //{
        //    currentTrack = track.GetComponent<TrackComponent>();            
        //}
        GameObject player = GameObject.Find("Player");
        if (player)
        {
            playerControl = player.GetComponent<PlayerControl>();
        }
    }

    // Update is called once per frame
    void Update () {        
        var from = playerControl.transform.position - playerControl.transform.forward * 10f + playerControl.transform.up * 2f;
        var to = playerControl.transform.position;
        var up = Vector3.Cross(playerControl.transform.right, (to - from).normalized);
        transform.position = from;
        transform.LookAt(to, -up);
    }
}
