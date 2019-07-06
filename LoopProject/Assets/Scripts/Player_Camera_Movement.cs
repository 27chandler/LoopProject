using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Camera_Movement : MonoBehaviour
{
    private float rotation_y = 0.0f;
    private float rotation_x = 0.0f;

    [SerializeField] private Transform rotation_origin;
    private Movement_Playback mp;
    bool is_controlled = false;
    // Start is called before the first frame update
    void Start()
    {
        if (rotation_origin == null)
        {
            rotation_origin = this.transform;
        }

        mp = GetComponentInParent<Movement_Playback>();
        if (mp == null)
        {
            is_controlled = true;
        }
        else
        {
            is_controlled = !mp.is_playing;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (is_controlled)
        {
            rotation_y += Input.GetAxis("Mouse Y");
            rotation_x += Input.GetAxis("Mouse X");
            rotation_origin.localEulerAngles = new Vector3(-rotation_y, rotation_x, transform.localEulerAngles.z);
        }

    }
}
