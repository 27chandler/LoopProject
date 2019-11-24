using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paradox_Logger : MonoBehaviour
{
    private Timeline_Manager tm;
    // Start is called before the first frame update

    private float present_time;
    private float modified_current_time;

    [SerializeField] private bool do_check = false;
    [SerializeField] private GameObject check_obj;
    private Vector3 check_pos;
    private Collider check_obj_collider;

    public bool is_done = true;
    public bool is_seen = false;
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
                        yield return null;
                        Camera_Check(tm.timeline_memory[i].normalized_timestamp);
                        activate_camera_check = false;
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
                Debug.Log("This object is in an UNSAFE place");
                Debug.Log("Lowest seen is: " + lowest_seen_time);
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
                    tc.Add_Pickup_To_Safety(check_obj);
                }
            }

            check_pos = new Vector3(9999.0f, 9999.0f, 9999.0f);

            if (controlled_player_object_holder.doors.Count >= 1)
            {
                controlled_player_object_holder.doors.Clear();
            }
        }
        else
        {
            Debug.Log("Check object for paradox logger came back as NULL?");
        }
        is_done = true;
        yield return null;
    }

    void Set_Check_Position(int i_index)
    {
        check_player.transform.position = tm.timeline_memory[i_index].position;
        check_player_rotator.localRotation = tm.timeline_memory[i_index].view_rotation;
    }

    void Camera_Check(float i_normalized_time)
    {
        Plane[] planes;

        planes = GeometryUtility.CalculateFrustumPlanes(check_player_camera);
        if (GeometryUtility.TestPlanesAABB(planes, check_obj_collider.bounds))
        {
            is_seen = true;
            lowest_seen_time = i_normalized_time;
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
