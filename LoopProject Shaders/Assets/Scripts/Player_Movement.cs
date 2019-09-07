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

    private float default_height;
    [SerializeField] private float crouch_height = 0.6f;

    private bool is_crouching = false;

    public bool is_controlled = false;

    private bool is_flying = false;

    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();

        default_height = cc.height;

        if (object_dir == null)
        {
            object_dir = this.transform;
        }
    }

    void Levitate()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Levitate")
        {
            is_flying = true;
            Debug.Log("ENTER");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Levitate")
        {
            is_flying = false;
            Debug.Log("EXIT");
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

            if (is_flying)
            {
                movement += Vector3.up;
                jump_movement.y = -(0.8f * Time.deltaTime);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                RaycastHit hit;

                Vector3 ceiling_check_pos = transform.position;
                ceiling_check_pos.y += (default_height / 2.0f);

                Physics.Raycast(ceiling_check_pos, Vector3.up, out hit, default_height/2.0f);

                if (hit.collider == null)
                {
                    is_crouching = !is_crouching;

                    if (!is_crouching)
                    {
                        Debug.Log("Up by: " + (default_height - cc.height).ToString());
                        movement += new Vector3(0.0f, (default_height - cc.height), 0.0f);
                        cc.height = default_height;
                    }
                    else
                    {
                        cc.height = crouch_height;
                    }
                }

            }

            cc.Move((movement + jump_movement) * Time.deltaTime * movement_speed);
        }
    }
}
