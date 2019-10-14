using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_Movement : MonoBehaviour
{
    private CharacterController cc;
    private Timeline_Manager tm;

    
    [Range(-1.0f, 1.0f)] [SerializeField] private float climb_offset_normal = 0.2f;
    [Range(-1.0f, 1.0f)] [SerializeField] private float climb_offset_crouch = 0.2f;

    [SerializeField] private float movement_speed;
    [SerializeField] private float sprinting_speed;
    [SerializeField] private KeyCode sprint_key;
    private Vector3 jump_movement = new Vector3();
    [SerializeField] private float jump_strength;
    [SerializeField] private Transform object_dir;
    private Movement_Playback mp;

    private Vector3 movement = new Vector3();
    private float speed_multiplier = 0.0f;

    private float default_height;
    [SerializeField] private float crouch_height = 0.7f;
    [SerializeField] private float climb_time;
    private Vector3 climb_dir = new Vector3(0.0f, 0.0f, 0.0f);
    [SerializeField] private float climb_force;

    private bool is_crouching = false;
    private bool is_crouch_climb_valid = false;
    private bool is_climb_valid = false;
    private bool is_uncrouch_movement_ready = false;

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

    private IEnumerator Climb(float i_offset)
    {
        Vector3 flat_forward = object_dir.parent.transform.forward;
        flat_forward.y = 0.0f;
        flat_forward.Normalize();

        Vector3 climb_waypoint = (object_dir.parent.transform.position + flat_forward + (Vector3.up * i_offset));
        Vector3 start_pos = transform.position;
        float climb_timer = 0.0f;
        cc.enabled = false;
        
        while (climb_timer < 1.0f)
        {
            transform.position = Vector3.Lerp(start_pos, climb_waypoint, climb_timer);
            climb_timer += 0.05f;
            yield return null;
        }

        cc.enabled = true;
        yield return null;
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

    private void Update()
    {
        Vector3 flat_forward = object_dir.parent.transform.forward;
        flat_forward.y = 0.0f;
        flat_forward.Normalize();

        // Standing valid climbing system
        is_climb_valid = false;
        float climb_offset = 1.0f;

        if (Physics.CheckSphere(object_dir.parent.transform.position + flat_forward, 0.1f))
        {
            if (!Physics.CheckCapsule(object_dir.parent.transform.position + flat_forward + (Vector3.up * 1.0f), object_dir.parent.transform.position + flat_forward + (Vector3.up * (1.0f + default_height + climb_offset_normal)), 0.1f))
            {
                Debug.Log("Normal climb valid (UPPER EDGE)");
                is_climb_valid = true;
            }


        }
        else if (Physics.CheckSphere(object_dir.parent.transform.position + flat_forward + (Vector3.down * 0.5f), 0.1f))
        {
            if (!Physics.CheckCapsule(object_dir.parent.transform.position + flat_forward + (Vector3.down * 0.5f) + (Vector3.up * 1.0f), object_dir.parent.transform.position + flat_forward + (Vector3.down * 0.5f) + (Vector3.up * (1.0f + default_height + climb_offset_normal)), 0.1f))
            {
                Debug.Log("Normal climb valid (WAIST EDGE)");
                is_climb_valid = true;
                climb_offset = 1.0f;
            }
        }

        // Crouching valid climbing system
        is_crouch_climb_valid = false;

        if (Physics.CheckSphere(object_dir.parent.transform.position + flat_forward, 0.1f))
        {
            if (!Physics.CheckCapsule(object_dir.parent.transform.position + flat_forward + (Vector3.up * 1.0f), object_dir.parent.transform.position + flat_forward + (Vector3.up * (1.0f + crouch_height + climb_offset_crouch)), 0.1f))
            {
                Debug.Log("Crouch climb valid (UPPER EDGE)");
                is_crouch_climb_valid = true;
            }


        }
        else if (Physics.CheckSphere(object_dir.parent.transform.position + flat_forward + (Vector3.down * 0.5f), 0.1f))
        {
            if (!Physics.CheckCapsule(object_dir.parent.transform.position + flat_forward + (Vector3.down * 0.5f) + (Vector3.up * 1.0f), object_dir.parent.transform.position + flat_forward + (Vector3.down * 0.5f) + (Vector3.up * (1.0f + crouch_height + climb_offset_crouch)), 0.1f))
            {
                Debug.Log("Crouch climb valid (WAIST EDGE)");
                is_crouch_climb_valid = true;
                climb_offset = 1.0f;
            }
        }

        // Jump controls
        float feet_distance = cc.height * 0.55f;

        if (is_crouching)
        {
            feet_distance += 0.15f;
        }

        Debug.DrawLine(transform.position, transform.position + (Vector3.down * (feet_distance)));

        bool is_jump_valid = false;

        if (Physics.Raycast(transform.position, Vector3.down, feet_distance))
        {
            is_jump_valid = true;
            if (jump_movement.y < 0.0f)
            {
                //Debug.Log("ok");
                //jump_movement.y = -(0.8f * Time.deltaTime);
                jump_movement.y = 0.0f;
            }
        }
        else
        {
            jump_movement.y -= (9.8f * Time.deltaTime);
        }

        bool do_force_crouch = false;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (is_climb_valid)
            {
                StartCoroutine(Climb(climb_offset));
            }
            else if (is_crouch_climb_valid)
            {
                do_force_crouch = true;
                StartCoroutine(Climb(climb_offset));
            }
            else if (is_jump_valid)
            {
                jump_movement.y = jump_strength;
            }
        }

        if (Input.GetKeyDown(KeyCode.C) || (do_force_crouch && !is_crouching))
        {
            Debug.Log("Crouch Activated");

            if (do_force_crouch && !is_crouching)
            {
                is_crouching = true;
                //Debug.Log("Up by: " + (default_height - cc.height).ToString());
                cc.height = crouch_height;
            }
            else
            {
                Vector3 ceiling_check_pos = transform.position;
                ceiling_check_pos.y += (default_height);

                if (!Physics.CheckSphere(ceiling_check_pos, 0.1f))
                {
                    is_crouching = !is_crouching;

                    if (!is_crouching)
                    {
                        //Debug.Log("Up by: " + (default_height - cc.height).ToString());
                        is_uncrouch_movement_ready = true;
                        cc.height = default_height;
                    }
                    else
                    {
                        cc.height = crouch_height;
                    }
                }
            }


        }


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

        time_device_display.text = "Jumps: " + num_of_jumps;

        speed_multiplier = movement_speed;

        if (Input.GetKey(sprint_key))
        {
            speed_multiplier = sprinting_speed;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (is_uncrouch_movement_ready)
        {
            is_uncrouch_movement_ready = false;
            movement += new Vector3(0.0f, (default_height * 3.5f), 0.0f);
        }

        cc.Move(((movement * speed_multiplier) + jump_movement + (climb_dir * climb_force)) * Time.deltaTime);
        movement = new Vector3(0.0f, 0.0f, 0.0f);
    }
}
