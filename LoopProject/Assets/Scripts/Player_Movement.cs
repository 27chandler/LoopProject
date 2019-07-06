using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    private CharacterController cc;

    [SerializeField] private float movement_speed;
    [SerializeField] private Transform object_dir;
    private Movement_Playback mp;

    private bool is_controlled = false;

    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();

        if (object_dir == null)
        {
            object_dir = this.transform;
        }

        mp = GetComponent<Movement_Playback>();
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
            Vector3 movement = new Vector3();

            if (Input.GetKey(KeyCode.D))
            {
                movement += object_dir.right;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                movement -= object_dir.right;
            }

            if (Input.GetKey(KeyCode.W))
            {
                movement += object_dir.forward;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                movement -= object_dir.forward;
            }

            cc.SimpleMove(movement * Time.deltaTime * movement_speed);
        }
    }
}
