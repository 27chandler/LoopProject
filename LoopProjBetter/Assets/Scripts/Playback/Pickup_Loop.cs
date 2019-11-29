using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pickup_Loop : MonoBehaviour
{
    [SerializeField] private Visible_Check vc;

    [SerializeField] private Material hold_mat;
    private Material default_mat;

    [SerializeField] private float max_hold_distance = 4.0f;

    public bool is_immune_to_jump_destruction = false;

    private RigidbodyConstraints rb_default_constraints;

    public bool is_picked_up;
    public Vector3 hold_pos;
    public GameObject object_holding_this; // The player object which is holding the object

    private bool has_spawned = true;

    Collider col;
    MeshRenderer meshrenderer;

    private float current_time = 0.0f;
    [SerializeField] private int timestamp_index = 0;

    private Rigidbody rb;

    [SerializeField] public float delay = 30.0f;
    [SerializeField] public Vector3 snap_pos;

    public Vector3 pickup_pos;

    public Vector3 last_pos;
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
        pickup_pos = transform.position;
        col = GetComponent<Collider>();
        col.enabled = false;
        
        meshrenderer = GetComponent<MeshRenderer>();
        default_mat = meshrenderer.material;

        vc.Add_Camera(GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<Camera>());
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        timeline_manager = GameObject.FindGameObjectWithTag("Timeline_Manager").GetComponent<Timeline_Manager>();
        last_iteration_num = timeline_manager.iteration_num;

        last_pos = transform.position;

        rb_default_constraints = rb.constraints;

        //Add_To_Buffer(transform.position, transform.localRotation, current_time);
        //Add_To_Buffer(transform.position, transform.localRotation, current_time);
    }

    IEnumerator Activate_Object()
    {
        yield return new WaitForSeconds(0.1f);
        col.enabled = true;
        has_spawned = false;
        rb.isKinematic = false;
        yield return null;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (has_spawned)
        {
            StartCoroutine(Activate_Object());
        }
        //Run_Playback();

        if ((last_pos != transform.position) && (!is_picked_up))
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
            rb.constraints = rb_default_constraints;
            rb.mass = 1.0f;
            rb.velocity = new Vector3(0.0f, 0.0f, 0.0f);
            rb.angularVelocity = new Vector3(0.0f, 0.0f, 0.0f);
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
            Vector3 movement_dir = (rb.position - object_holding_this.transform.position).normalized;
            float distance_between_objects = Vector3.Distance(rb.position, object_holding_this.transform.position);
            Ray line_of_sight_ray = new Ray(object_holding_this.transform.position, movement_dir);
            //if (!Physics.Raycast(line_of_sight_ray, distance_between_objects))
            //{
            //rb.MovePosition(hold_pos);
            transform.position = Vector3.Lerp(transform.position, hold_pos, 0.7f);
                //rb.MovePosition(Vector3.Lerp(rb.position, hold_pos, 0.2f));
            //}
            //else if (distance_between_objects >= max_hold_distance)
            //{
            //    object_holding_this.GetComponent<Hold_Object>().Drop_Item();
            //}
            //Debug.DrawRay(object_holding_this.transform.position, movement_dir * distance_between_objects,Color.red,0.1f);

            rb.useGravity = false;
        }
        else
        {
            if (pickup_pos != transform.position)
            {
                pickup_pos = transform.position;
            }

            rb.constraints = rb_default_constraints | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            rb.mass = 1000.0f;
            gameObject.layer = 0;
            meshrenderer.material = default_mat;
            rb.useGravity = true;
            if ((Vector3.Distance(transform.position,snap_pos) <= 0.5f) && (!is_moving))
            {
                //transform.position = snap_pos;
                rb.MovePosition(Vector3.Lerp(transform.position, snap_pos, 0.2f));
                //transform.position = Vector3.Lerp(transform.position, snap_pos, 0.2f);
            }

            if (!is_moving)
            {
                rb.constraints = rb_default_constraints | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
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

        if ((last_iteration_num != timeline_manager.iteration_num) && (!is_immune_to_jump_destruction))
        {
            last_iteration_num = timeline_manager.iteration_num;

            if (is_picked_up == false)
            {
                Destroy(gameObject);
            }
        }
        else if (is_immune_to_jump_destruction)
        {
            last_iteration_num = timeline_manager.iteration_num;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Reset"))
        {
            if (object_holding_this != null)
            {
                object_holding_this.GetComponent<Hold_Object>().Drop_Item();
            }

            Vector3 recept_pos = other.GetComponent<Receptor_Control>().Recept(gameObject);

            if (recept_pos != null)
            {
                transform.position = recept_pos;
            }
            
        }
    }
}
