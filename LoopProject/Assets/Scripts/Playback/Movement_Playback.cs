using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Movement_Playback : MonoBehaviour
{
    [SerializeField] public bool is_playing;
    [Space]
    [SerializeField] private Transform target;
    [SerializeField] private Transform pivot_target;

    [Space]
    [SerializeField] private Transform this_pivot;
    [SerializeField] private float delay;

    private float current_time = 0.0f;
    private int timestamp_index = 0;
    private Vector3 last_position = new Vector3();
    [SerializeField] private List<Record_Data> position_buffer = new List<Record_Data>();

    [Serializable] public struct Record_Data
    {
        public Vector3 position;
        public Quaternion view_rotation;
        public float timestamp;
    };

    // Start is called before the first frame update
    void Start()
    {
        Add_To_Buffer(target.position, pivot_target.localRotation, current_time);
        Add_To_Buffer(target.position, pivot_target.localRotation, current_time);
    }

    // Update is called once per frame
    void Update()
    {
        
        if (is_playing)
        {
            Run_Playback();
        }
    }

    void Run_Playback()
    {
        current_time += Time.deltaTime;

        if ((position_buffer[position_buffer.Count - 1].position != target.position) || (position_buffer[position_buffer.Count - 1].view_rotation != pivot_target.localRotation))
        {
            Add_To_Buffer(target.position, pivot_target.localRotation, current_time);
        }

        if (position_buffer[timestamp_index + 1].timestamp <= current_time - delay)
        {
            timestamp_index++;
            transform.position = position_buffer[timestamp_index].position;
            this_pivot.localRotation = position_buffer[timestamp_index].view_rotation;
        }
    }

    void Add_To_Buffer(Vector3 i_pos,Quaternion i_local_rot, float i_time)
    {
        Record_Data input_data = new Record_Data();
        input_data.position = i_pos;
        input_data.timestamp = i_time;
        input_data.view_rotation = i_local_rot;

        position_buffer.Add(input_data);
    }
}
