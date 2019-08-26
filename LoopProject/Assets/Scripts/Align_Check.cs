﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Align_Check : MonoBehaviour
{
    [SerializeField] private MeshRenderer renderer;
    private GameObject satisfying_obj;
    private Hold_Object holder;
    [SerializeField] public float completion_time;
    [SerializeField] public float current_time;
    private int health;
    [SerializeField] private int max_health;

    private bool is_player_altering = false;
    private Timeline_Manager tm;
    // Start is called before the first frame update
    void Start()
    {
        renderer.enabled = false;
        health = max_health;

        holder = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetComponentInChildren<Hold_Object>();
        tm = GameObject.FindGameObjectWithTag("Timeline_Manager").GetComponent<Timeline_Manager>();
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] found_objs = Physics.OverlapSphere(transform.position, 1.0f);


        bool is_obj_present = false;

        foreach (Collider collider in found_objs)
        {
            if (collider.name == "Box(Clone)" || collider.name == "Box")
            {
                satisfying_obj = collider.gameObject;
                is_obj_present = true;

                Pickup_Loop obj_pickup = satisfying_obj.GetComponent<Pickup_Loop>();

                if (obj_pickup != null)
                {
                    obj_pickup.snap_pos = transform.position;
                }
            }
        }

        if (is_player_altering)
        {
            if (is_obj_present)
            {
                renderer.enabled = false;
            }
            else
            {
                renderer.enabled = true;
            }
        }

        if ((holder.grabbed_item_obj == satisfying_obj) && (satisfying_obj != null))
        {
            is_player_altering = true;
        }
        else
        {
            is_player_altering = false;
        }

        if (completion_time > (tm.iteration_num * tm.iteration_delay))
        {
            Destroy(gameObject);
        }

        //if (completion_time - current_time >= 28.5f)
        //{
        //    Debug.Log("RIP");
        //    Destroy(gameObject);
        //}


        if (current_time >= completion_time-0.1f)
        {
            if (is_obj_present)
            {
                Destroy(gameObject);
            }
            else
            {
                health--;
            }
        }

        if (health <= 0)
        {
            Debug.Log("FAILED");
            tm.Activate_Paradox_Increment(2.0f);
            Destroy(this.gameObject);
        }

    }
}
