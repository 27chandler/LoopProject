﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pickup_Loop : MonoBehaviour
{
    [SerializeField] private Camera player_cam;
    [SerializeField] private Visible_Check vc;

    public bool is_picked_up;
    public Vector3 hold_pos;

    private float current_time = 0.0f;
    [SerializeField] private int timestamp_index = 0;

    private Rigidbody rb;

    [SerializeField] public float delay = 30.0f;

    [SerializeField] private List<Record_Data> position_buffer = new List<Record_Data>();

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
        vc.Add_Camera(player_cam);
        rb = GetComponent<Rigidbody>();

        Add_To_Buffer(transform.position, transform.localRotation, current_time);
        Add_To_Buffer(transform.position, transform.localRotation, current_time);
    }

    // Update is called once per frame
    void Update()
    {
        //Run_Playback();

        if (is_picked_up)
        {
            float move_force = Vector3.Distance(hold_pos, transform.position) / 3.0f;
            rb.useGravity = false;
            rb.AddForce((hold_pos - transform.position));
        }
        else
        {
            rb.useGravity = true;
        }

        if (vc.is_seen)
        {
            foreach(var cam in vc.seen_cams)
            {
                cam.GetComponentInParent<Movement_Playback>().Add_To_Object_Memory(transform.position,gameObject.name);
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
