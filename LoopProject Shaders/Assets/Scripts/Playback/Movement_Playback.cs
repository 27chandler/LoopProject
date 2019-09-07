using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Movement_Playback : MonoBehaviour
{
    [SerializeField] public bool is_playing;
    [Space]
    [SerializeField] public Transform target;
    [SerializeField] public Transform pivot_target;

    [Space]
    [SerializeField] public Transform this_pivot;
    [SerializeField] public float delay;

    private float current_time = 0.0f;
    private int timestamp_index = 0;
    private Vector3 last_position = new Vector3();
    [SerializeField] private List<Record_Data> position_buffer = new List<Record_Data>();
    [SerializeField] private List<Object_Memory_Data> object_buffer = new List<Object_Memory_Data>();

    public List<Object_Memory_Data> current_temp_memory = new List<Object_Memory_Data>();
    private int object_index = 0;

    [Serializable] public struct Record_Data
    {
        public Vector3 position;
        public Quaternion view_rotation;
        public float timestamp;
    };

    [Serializable] public struct Object_Memory_Data
    {
        public Vector3 position;
        public string name;
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
        current_temp_memory.Clear();
        current_time += Time.deltaTime;
        if (is_playing)
        {
            //Run_Playback();
        }
    }

    void Run_Playback()
    {
        

        // Record Movement
        if ((position_buffer[position_buffer.Count - 1].position != target.position) || (position_buffer[position_buffer.Count - 1].view_rotation != pivot_target.localRotation))
        {
            Add_To_Buffer(target.position, pivot_target.localRotation, current_time);
        }
        // Record Objects
        if (target != transform)
        {
            Movement_Playback target_mp = target.GetComponent<Movement_Playback>();
            if (target_mp.current_temp_memory.Count >= 1)
            {
                foreach (var obj in target_mp.current_temp_memory)
                {
                    object_buffer.Add(obj);
                }
            }
        }
        //

        // Play Movement
        if (position_buffer[timestamp_index + 1].timestamp <= current_time - delay)
        {
            timestamp_index++;
            transform.position = position_buffer[timestamp_index].position;
            this_pivot.localRotation = position_buffer[timestamp_index].view_rotation;
        }
        // Check Objects

        //
    }

    public void Add_To_Object_Memory(Vector3 i_pos, string i_name)
    {
        Object_Memory_Data input_data = new Object_Memory_Data();
        input_data.position = i_pos;
        input_data.name = i_name;
        input_data.timestamp = current_time;

        current_temp_memory.Add(input_data);

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
