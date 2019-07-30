using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    private CharacterController cc;

    [SerializeField] private float movement_speed;
    private Vector3 jump_movement = new Vector3();
    [SerializeField] private float jump_strength;
    [SerializeField] private Transform object_dir;
    private Movement_Playback mp;

    public bool is_controlled = false;

    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();

        if (object_dir == null)
        {
            object_dir = this.transform;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (is_controlled)
        {
            Vector3 movement = new Vector3();

            if (cc.isGrounded)
            {
                if (jump_movement.y < 0.0f)
                {
                    jump_movement.y = -(0.8f * Time.deltaTime);
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    jump_movement.y = jump_strength;
                }
            }
            else
            {
                jump_movement.y -= (0.8f * Time.deltaTime);
            }

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

            movement.y = 0.0f;
            movement.Normalize();

            cc.Move((movement + jump_movement) * Time.deltaTime * movement_speed);
        }
    }
}
