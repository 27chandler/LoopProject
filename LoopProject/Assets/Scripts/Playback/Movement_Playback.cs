using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Movement_Playback : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float delay;

    private float current_time = 0.0f;
    private int timestamp_index = 0;
    private Vector3 last_position = new Vector3();
    [SerializeField] private List<Position_Data> position_buffer = new List<Position_Data>();

    [Serializable] public struct Position_Data
    {
        public Vector3 position;
        public float timestamp;
    };

    // Start is called before the first frame update
    void Start()
    {
        Add_To_Buffer(target.position, current_time);
        Add_To_Buffer(target.position, current_time);
    }

    // Update is called once per frame
    void Update()
    {
        current_time += Time.deltaTime;

        if ((position_buffer[position_buffer.Count-1].position != target.position))
        {
            Add_To_Buffer(target.position,current_time);
        }

        if (position_buffer[timestamp_index+1].timestamp <= current_time - delay)
        {
            timestamp_index++;
            transform.position = position_buffer[timestamp_index].position;
        }
    }

    void Add_To_Buffer(Vector3 i_pos, float i_time)
    {
        Position_Data input_data = new Position_Data();
        input_data.position = i_pos;
        input_data.timestamp = i_time;

        position_buffer.Add(input_data);
    }
}
