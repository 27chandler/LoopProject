using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pickup_Loop : MonoBehaviour
{
    [SerializeField] private Visible_Check vc;

    [SerializeField] private Material hold_mat;
    private Material default_mat;

    public bool is_picked_up;
    public Vector3 hold_pos;
    public GameObject object_holding_this; // The player object which is holding the object

    Collider col;
    MeshRenderer meshrenderer;

    private float current_time = 0.0f;
    [SerializeField] private int timestamp_index = 0;

    private Rigidbody rb;

    [SerializeField] public float delay = 30.0f;
    [SerializeField] public Vector3 snap_pos;

    private Vector3 last_pos;
    bool is_moving = false;

    [SerializeField] private List<Record_Data> position_buffer = new List<Record_Data>();
    private Timeline_Manager timeline_manager;
    private int last_iteration_num;

    [Serializable]
    public struct Record_Data
    {
        public Vector3 position;
        public Quaternion view_rotation;
        public float timestamp;
    };
    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider>();
        meshrenderer = GetComponent<MeshRenderer>();
        default_mat = meshrenderer.material;

        vc.Add_Camera(GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<Camera>());
        rb = GetComponent<Rigidbody>();

        timeline_manager = GameObject.FindGameObjectWithTag("Timeline_Manager").GetComponent<Timeline_Manager>();
        last_iteration_num = timeline_manager.iteration_num;

        last_pos = transform.position;

        //Add_To_Buffer(transform.position, transform.localRotation, current_time);
        //Add_To_Buffer(transform.position, transform.localRotation, current_time);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Run_Playback();

        if (last_pos != transform.position)
        {
            is_moving = true;
            last_pos = transform.position;
        }
        else
        {
            is_moving = false;
        }

        if (is_picked_up)
        {
            gameObject.layer = 2;
            meshrenderer.material = hold_mat;
            if (object_holding_this.activeInHierarchy == false)
            {
                col.enabled = false;
                meshrenderer.enabled = false;
                vc.enabled = false;
                vc.seen_cams.Clear();
                vc.is_seen = false;
            }
            else if (col.enabled == false)
            {
                col.enabled = true;
                meshrenderer.enabled = true;
                vc.enabled = true;
            }

            rb.MovePosition(Vector3.Lerp(rb.position,hold_pos,0.2f));

            //float distance = Vector3.Distance(hold_pos, transform.position);
            //float move_force = distance * 60.0f;
            rb.useGravity = false;

            ////if (rb. > move_force)
            ////{

            ////}

            //move_force /= 5.0f - distance;

            ////move_force = Mathf.Pow(move_force, 2.0f);

            //rb.AddForce((hold_pos - transform.position).normalized * move_force);
        }
        else
        {
            gameObject.layer = 0;
            meshrenderer.material = default_mat;
            rb.useGravity = true;
            if ((Vector3.Distance(transform.position,snap_pos) <= 0.5f) && (rb.velocity.magnitude <= 0.1f))
            {
                transform.position = snap_pos;
            }
        }

        if (vc.is_seen)
        {
            foreach(var cam in vc.seen_cams)
            {
                if (cam.gameObject.activeInHierarchy)
                {
                    cam.GetComponentInParent<Movement_Playback>().Add_To_Object_Memory(transform.position, gameObject.name);
                }
            }
        }

        if (last_iteration_num != timeline_manager.iteration_num)
        {
            last_iteration_num = timeline_manager.iteration_num;

            if (is_picked_up == false)
            {
                Destroy(gameObject);
            }
        }
    }

    void Run_Playback()
    {
        current_time += Time.deltaTime;

        if (timestamp_index < position_buffer.Count - 1)
        {
            if (position_buffer[timestamp_index + 1].timestamp <= current_time)
            {
                timestamp_index++;
            }
        }

        if (vc.is_seen)
        {
            Add_To_Buffer(transform.position, transform.localRotation, current_time);
        }

        Reset_Loop();
    }

    void Reset_Loop()
    {
        if (current_time >= delay)
        {
            current_time = 0.0f;
            timestamp_index = 0;
        }
    }

    void Add_To_Buffer(Vector3 i_pos, Quaternion i_local_rot, float i_time)
    {
        Record_Data input_data = new Record_Data();
        input_data.position = i_pos;
        input_data.timestamp = i_time;
        input_data.view_rotation = i_local_rot;

        //position_buffer.Add(input_data);
        position_buffer.Insert(timestamp_index, input_data);
    }
}
