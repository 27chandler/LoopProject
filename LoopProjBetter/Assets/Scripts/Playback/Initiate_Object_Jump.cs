using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initiate_Object_Jump : MonoBehaviour
{
    private Timeline_Manager tm;
    private Pickup_Loop pl;

    private MeshRenderer mr;
    private Collider col;

    private Rigidbody rb;
    private Vector3 jump_location;

    private float current_time;
    private float return_time;
    [SerializeField] private bool is_jumping = false;
    [SerializeField] private float jump_distance;
    [SerializeField] public bool is_hidden = false;
    // Start is called before the first frame update
    void Start()
    {
        tm = GameObject.FindGameObjectWithTag("Timeline_Manager").GetComponent<Timeline_Manager>();
        pl = GetComponent<Pickup_Loop>();
        mr = GetComponent<MeshRenderer>();
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //if ((tm.modified_current_time > return_time) && (tm.modified_current_time < return_time + 0.5f) && (is_hidden) && (!tm.is_jumping))
        //{
        //    Exit_Jump();
        //}

        if (is_jumping)
        {
            Activate_Jump(jump_distance);
            is_jumping = false;
        }

        if (is_hidden)
        {
            rb.MovePosition(jump_location);
        }
    }

    //private void Exit_Jump()
    //{
    //    mr.enabled = true;
    //    col.enabled = true;
    //    is_hidden = false;
    //    pl.is_immune_to_jump_destruction = false;
    //    rb.isKinematic = false;
    //}

    public void Activate_Jump(float i_destination)
    {
        Timeline_Manager.Self_Jumping_Object object_to_jump_exit = new Timeline_Manager.Self_Jumping_Object();
        object_to_jump_exit.tag = tag;
        object_to_jump_exit.position = transform.position;
        object_to_jump_exit.destination_time = tm.modified_current_time + i_destination;
        object_to_jump_exit.is_cooldown_active = false;

        tm.time_travelling_objects.Add(object_to_jump_exit);

        Timeline_Manager.Self_Jumping_Object object_to_jump_enter = new Timeline_Manager.Self_Jumping_Object();
        object_to_jump_enter.tag = tag;
        object_to_jump_enter.position = transform.position;
        object_to_jump_enter.destination_time = tm.modified_current_time;

        tm.objects_to_time_travel.Add(object_to_jump_enter);

        //return_time = tm.modified_current_time + i_destination;
        //is_hidden = true;
        //pl.is_immune_to_jump_destruction = true;
        //mr.enabled = false;
        //col.enabled = false;

        //jump_location = transform.position;
        //rb.isKinematic = true;
    }
}
