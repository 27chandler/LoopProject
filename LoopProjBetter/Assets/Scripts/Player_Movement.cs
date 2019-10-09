using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_Movement : MonoBehaviour
{
    private CharacterController cc;
    private Timeline_Manager tm;

    [SerializeField] private float movement_speed;
    [SerializeField] private float sprinting_speed;
    [SerializeField] private KeyCode sprint_key;
    private Vector3 jump_movement = new Vector3();
    [SerializeField] private float jump_strength;
    [SerializeField] private Transform object_dir;
    private Movement_Playback mp;

    private float default_height;
    [SerializeField] private float crouch_height = 0.7f;

    private bool is_crouching = false;

    public bool is_controlled = false;

    private bool is_flying = false;

    [SerializeField] private Text time_device_display;
    [SerializeField] private bool are_time_jumps_regening = false;
    [SerializeField] private float regen_time = 60.0f;
    private float regen_time_counter = 0.0f;
    [SerializeField] public int num_of_jumps = 0;

    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
        tm = GameObject.FindGameObjectWithTag("Timeline_Manager").GetComponent<Timeline_Manager>();

        default_height = cc.height;

        if (object_dir == null)
        {
            object_dir = this.transform;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Levitate")
        {
            is_flying = true;
            //Debug.Log("ENTER");
        }
        else if (other.CompareTag("Time_Frozen"))
        {
            tm.time_speed = 0.0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Levitate")
        {
            is_flying = false;
            //Debug.Log("EXIT");
        }
        else if (other.CompareTag("Time_Frozen"))
        {
            tm.time_speed = 1.0f;
        }
        else if (other.CompareTag("Time_Jump_Portal"))
        {
            tm.is_jumping = true;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (is_controlled)
        {


            Vector3 movement = new Vector3();

            if ((are_time_jumps_regening) && (num_of_jumps <= 0))
            {
                regen_time_counter += Time.deltaTime;
                if (regen_time_counter >= regen_time)
                {
                    num_of_jumps++;
                    regen_time_counter = 0.0f;
                }
            }

            if (Input.GetKeyDown(KeyCode.Q) && (num_of_jumps > 0))
            {
                are_time_jumps_regening = true;
                tm.is_jumping = true;
                num_of_jumps--;
            }

            float feet_distance = cc.height * 0.55f;

            if (is_crouching)
            {
                feet_distance += 0.15f;
            }

            Debug.DrawLine(transform.position, transform.position + (Vector3.down * (feet_distance)));

            if (Physics.Raycast(transform.position,Vector3.down, feet_distance))
            {
                if (jump_movement.y < 0.0f)
                {
                    //Debug.Log("ok");
                    //jump_movement.y = -(0.8f * Time.deltaTime);
                    jump_movement.y = 0.0f;
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
                Debug.Log("C Pressed");

                Vector3 ceiling_check_pos = transform.position;
                ceiling_check_pos.y += (default_height);

                if (!Physics.CheckSphere(ceiling_check_pos, 0.1f))
                {
                    is_crouching = !is_crouching;

                    if (!is_crouching)
                    {
                        //Debug.Log("Up by: " + (default_height - cc.height).ToString());
                        movement += new Vector3(0.0f, (default_height*3.5f), 0.0f);
                        cc.height = default_height;
                    }
                    else
                    {
                        cc.height = crouch_height;
                    }
                }

            }

            time_device_display.text = "Jumps: " + num_of_jumps;

            float speed_multiplier = movement_speed;

            if (Input.GetKey(sprint_key))
            {
                speed_multiplier = sprinting_speed;
            }

            cc.Move((movement + jump_movement) * Time.deltaTime * speed_multiplier);
        }
        else if (tag == "Player")
        {
            //Debug.Log("Not controlled");
        }
    }
}
