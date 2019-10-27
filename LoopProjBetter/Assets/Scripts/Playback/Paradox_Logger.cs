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
    private Collider check_obj_collider;

    private bool is_seen = false;

    [SerializeField] private const int NUM_OF_CHECKS_PER_FRAME = 1000;

    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private GameObject check_player;
    [SerializeField] private Transform check_player_rotator;
    [SerializeField] private Camera check_player_camera;

    [SerializeField] private Hold_Object controlled_player_object_holder;
    private GameObject previous_held_object;
    void Start()
    {
        tm = GetComponent<Timeline_Manager>();
    }

    public void Activate_Check()
    {
        do_check = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (controlled_player_object_holder.is_holding)
        {
            previous_held_object = controlled_player_object_holder.grabbed_item_obj;
        }

        if ((previous_held_object != null) && (!controlled_player_object_holder.is_holding))
        {
            check_obj = previous_held_object;
            previous_held_object = null;
            Activate_Check();
        }

        if (do_check)
        {
            do_check = false;
            StartCoroutine(Run_Check());
        }
    }

    IEnumerator Run_Check()
    {
        present_time = tm.time_of_present;
        modified_current_time = tm.modified_current_time;
        check_obj_collider = check_obj.GetComponent<Collider>();

        tm.time_speed = 0.0f;

        is_seen = false;
        int check_counter = 0;

        for (int i = 0; i < tm.timeline_memory.Count; i++)
        {
            tm.time_speed = 0.0f;
            if ((tm.timeline_memory[i].normalized_timestamp < present_time)
                && (tm.timeline_memory[i].normalized_timestamp > modified_current_time)
                && (tm.timeline_memory[i].timestamp / tm.iteration_delay < tm.iteration_num - 1))
            {
                Set_Check_Position(i);
                Visibility_Check();

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
        }

        check_player.transform.position = new Vector3(9999.0f, 9999.0f, 9999.0f);

        yield return null;
    }

    void Set_Check_Position(int i_index)
    {
        check_player.transform.position = tm.timeline_memory[i_index].position;
        check_player_rotator.localRotation = tm.timeline_memory[i_index].view_rotation;
    }

    void Visibility_Check()
    {
        Plane[] planes;

        planes = GeometryUtility.CalculateFrustumPlanes(check_player_camera);
        if (GeometryUtility.TestPlanesAABB(planes, check_obj_collider.bounds))
        {
            Vector3 ray_direction = check_player_camera.transform.position - check_obj.transform.position;

            RaycastHit hit;
            Physics.Raycast(check_obj.transform.position, ray_direction, out hit, Mathf.Infinity, obstacleMask);
            Debug.DrawLine(check_obj.transform.position, hit.point, Color.blue);

            if ((hit.collider != null)/* && (Vector3.Distance(hit.point, check_player_camera.transform.position) <= 1.0f)*/)
            {
                if (hit.collider.gameObject.tag == "Player_Dupe")
                {
                    is_seen = true;
                    Debug.DrawLine(check_obj.transform.position, hit.point, Color.green, 10.0f);
                }
                else
                {
                    Debug.DrawLine(check_obj.transform.position, hit.point, Color.black, 10.0f);
                }

                //if (hit.transform.parent == camera.transform.parent)
                //{
                //    is_seen = true;
                //    seen_cams.Add(camera);
                //}
            }
            else
            {
                is_seen = true;
                Debug.DrawLine(check_obj.transform.position, hit.point, Color.green, 100.0f);
            }


        }
    }
}
