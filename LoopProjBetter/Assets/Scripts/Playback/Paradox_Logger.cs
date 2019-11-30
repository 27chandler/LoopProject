using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paradox_Logger : MonoBehaviour
{
    private Timeline_Manager tm;
    // Start is called before the first frame update

    [SerializeField] Pardox_Warning_Popup warning_system;

    private float present_time;
    private float modified_current_time;

    [SerializeField] private bool do_check = false;
    [SerializeField] private GameObject check_obj;
    private Vector3 check_pos;
    private Collider check_obj_collider;

    public bool is_done = true;
    public bool is_seen = false;

    public bool is_grab = true;

    public bool is_obj_timeline_original = false; // If the object is valid in the original timeline
    private bool activate_camera_check = false;
    private float lowest_seen_time = 9999999.0f;

    [SerializeField] private const int NUM_OF_CHECKS_PER_FRAME = 1000;

    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private GameObject check_player;
    [SerializeField] private Transform check_player_rotator;
    [SerializeField] private Camera check_player_camera;

    [SerializeField] private Hold_Object controlled_player_object_holder;

    private TimePoint_Corrector tc;
    private GameObject previous_held_object;
    void Start()
    {
        tm = GetComponent<Timeline_Manager>();
        tc = GetComponent<TimePoint_Corrector>();
    }

    public void Activate_Check()
    {
        do_check = true;
    }

    public void Activate_Custom_Check(GameObject i_check_obj)
    {
        check_obj = i_check_obj;
        Activate_Check();
    }

    public void Activate_Object_Check(Hold_Object.Object_Interaction i_new_obj_interaction)
    {
        check_obj = i_new_obj_interaction.objects;
        is_grab = i_new_obj_interaction.grab_state;
        Activate_Check();
    }

    // Update is called once per frame
    void Update()
    {
        //// Doors section
        //if (controlled_player_object_holder.doors.Count >= 1)
        //{
        //    check_obj = controlled_player_object_holder.doors[0].gameObject;
        //    Activate_Check();
        //}

        ////

        //if (controlled_player_object_holder.is_holding)
        //{
        //    previous_held_object = controlled_player_object_holder.grabbed_item_obj;
        //}

        //if ((previous_held_object != null) && (!controlled_player_object_holder.is_holding))
        //{
        //    check_obj = previous_held_object;
        //    previous_held_object = null;
        //    Activate_Check();
        //}

        if (do_check)
        {
            do_check = false;
            StartCoroutine(Run_Check());
        }
    }

    IEnumerator Run_Check()
    {
        if (check_obj != null)
        {
            lowest_seen_time = 99999999.0f;

            present_time = tm.time_of_present;
            modified_current_time = tm.modified_current_time;
            check_obj_collider = check_obj.GetComponent<Collider>();

            //tm.time_speed = 0.1f;

            is_seen = false;
            int check_counter = 0;

            for (int i = 0; i < tm.timeline_memory.Count; i++)
            {
                if ((tm.timeline_memory[i].normalized_timestamp < present_time)
                    && (tm.timeline_memory[i].normalized_timestamp > modified_current_time)
                    && (tm.timeline_memory[i].timestamp / tm.iteration_delay < tm.iteration_num - 1)
                    && (tm.timeline_memory[i].normalized_timestamp < lowest_seen_time))
                {

                    check_pos = tm.timeline_memory[i].position;
                    Visibility_Check();

                    if (activate_camera_check)
                    {
                        Set_Check_Position(i);
                        activate_camera_check = false;
                        yield return null;
                        Camera_Check(tm.timeline_memory[i].normalized_timestamp,i);
                        //activate_camera_check = false;
                    }

                    check_counter++;
                }

                if (check_counter >= NUM_OF_CHECKS_PER_FRAME)
                {
                    check_counter = 0;
                    yield return new WaitForSecondsRealtime(0.2f);
                }

                //Debug.Log(tm.timeline_memory[i].normalized_timestamp);
            }

            if (is_seen)
            {
                //Debug.Log("This object is in an UNSAFE place");
                //Debug.Log("Lowest seen is: " + lowest_seen_time);
                if (is_obj_timeline_original)
                {
                    //Debug.Log("Object is valid");
                }
                else
                {
                    //Debug.Log("Object is INVALID");
                }
                //Debug.Log("IS GRAB = " + is_grab);

                if (is_obj_timeline_original == is_grab)
                {
                    warning_system.Add_Warning("WARNING, PARADOX IMMINENT", is_obj_timeline_original,check_obj.tag, lowest_seen_time, check_obj.transform.position);
                }
            }
            else
            {
                Door_Activation temp_door = check_obj.GetComponentInChildren<Door_Activation>();
                if (temp_door != null)
                {
                    tc.Add_Door_To_Safety(temp_door);
                }
                else
                {
                    tc.Add_Pickup_To_Safety(check_obj,check_obj.transform.position);
                }
            }

            check_pos = new Vector3(9999.0f, 9999.0f, 9999.0f);

            if (controlled_player_object_holder.doors.Count >= 1)
            {
                controlled_player_object_holder.doors.Clear();
            }

            if (controlled_player_object_holder.objects.Count >= 1)
            {
                controlled_player_object_holder.objects.Clear();
            }
        }
        else
        {
            Debug.LogAssertion("Check object for paradox logger came back as NULL?");
        }
        is_done = true;
        yield return null;
    }

    void Set_Check_Position(int i_index)
    {
        check_player.transform.position = tm.timeline_memory[i_index].position;
        check_player_rotator.localRotation = tm.timeline_memory[i_index].view_rotation;
    }

    void Camera_Check(float i_normalized_time, int timestamp_index)
    {
        Plane[] planes;

        planes = GeometryUtility.CalculateFrustumPlanes(check_player_camera);
        if (GeometryUtility.TestPlanesAABB(planes, check_obj_collider.bounds))
        {
            is_seen = true;
            lowest_seen_time = i_normalized_time;

            bool is_original = false;

            foreach (var obj in tm.timeline_memory[timestamp_index].seen_objs)
            {
                Pickup_Loop temp_pickup = check_obj.GetComponent<Pickup_Loop>();
                Vector3 check_pos;
                if (temp_pickup != null)
                {
                    check_pos = temp_pickup.last_pos;
                }
                else
                {
                    check_pos = check_obj.transform.position;
                }


                if (Vector3.Distance(obj.position, check_pos) <= 0.4f)
                {
                    is_original = true;
                }
            }

            if (is_original)
            {
                is_obj_timeline_original = true;
            }
            else
            {
                is_obj_timeline_original = false;
            }
        }
    }

    void Visibility_Check()
    {
        Vector3 ray_direction = check_pos - check_obj.transform.position;
        float ray_distance = Vector3.Distance(check_pos, check_obj.transform.position);

        RaycastHit hit;
        Physics.Raycast(check_obj.transform.position, ray_direction, out hit, ray_distance, obstacleMask);
        Debug.DrawLine(check_obj.transform.position, hit.point, Color.blue);

        if ((hit.collider == null)/* && (Vector3.Distance(hit.point, check_player_camera.transform.position) <= 1.0f)*/)
        {
            activate_camera_check = true;
            Debug.DrawLine(check_obj.transform.position, hit.point, Color.green, 10.0f);

            //if (hit.transform.parent == camera.transform.parent)
            //{
            //    is_seen = true;
            //    seen_cams.Add(camera);
            //}
        }
        else
        {
            Debug.DrawLine(check_obj.transform.position, hit.point, Color.black, 10.0f);
        }


        //}
    }
}
