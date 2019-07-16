using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Timeline_Manager : MonoBehaviour
{
    [SerializeField] private float iteration_delay;
    [SerializeField] private int iteration_num = 0;
    [SerializeField] private float time_speed = 1.0f;
    [Space]
    // Player
    [SerializeField] public Transform player_target;
    [SerializeField] public Transform player_look_pivot;

    //
    [Space]

    // Player looping
    [SerializeField] private Visible_Check vis;
    [SerializeField] private GameObject loop_obj;
    [SerializeField] private GameObject original_obj;
    private GameObject previous_obj;

    private Movement_Playback new_obj;
    private Movement_Playback old_obj;

    [SerializeField] private List<Duplicate_Data> duplicate_obj_list = new List<Duplicate_Data>();

    [SerializeField] private List<Record_Data> position_buffer = new List<Record_Data>();
    [Serializable]
    public struct Record_Data
    {
        public Vector3 position;
        public Quaternion view_rotation;
        public float timestamp;
    };

    [Serializable]
    public class Duplicate_Data
    {
        public GameObject obj;
        public Transform obj_look_pivot;
        public int timestamp;
    };
    //

    private float current_time = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        current_time = 0.0f;

        Add_To_Buffer(player_target.position, player_look_pivot.localRotation, current_time);
        Add_To_Buffer(player_target.position, player_look_pivot.localRotation, current_time);
        Add_To_Buffer(player_target.position, player_look_pivot.localRotation, current_time);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            time_speed -= 0.1f;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            time_speed += 0.1f;
        }

        current_time += Time.deltaTime * time_speed;
        Run_Playback();

        if (current_time >= iteration_delay * iteration_num)
        {
            iteration_num++;
            Restart_Loop();
        }
    }

    void Run_Playback()
    {


        // Record Movement
        if ((position_buffer[position_buffer.Count - 1].position != player_target.position) || (position_buffer[position_buffer.Count - 1].view_rotation != player_look_pivot.localRotation))
        {
            Add_To_Buffer(player_target.position, player_look_pivot.localRotation, current_time);
        }
        // Record Objects
        if (player_target != transform)
        {
            //Movement_Playback target_mp = player_target.GetComponent<Movement_Playback>();
            //if (target_mp.current_temp_memory.Count >= 1)
            //{
            //    foreach (var obj in target_mp.current_temp_memory)
            //    {
            //        object_buffer.Add(obj);
            //    }
            //}
        }
        //

        for (int i = 0; i < duplicate_obj_list.Count; i++)
        {
            bool is_done = false;

            while(!is_done)
            {
                if (time_speed >= 0.0f)
                {
                    if (position_buffer[duplicate_obj_list[i].timestamp + 1].timestamp <= (current_time - (iteration_delay * (i + 1))))
                    {
                        duplicate_obj_list[i].timestamp++;
                        duplicate_obj_list[i].obj.transform.position = position_buffer[duplicate_obj_list[i].timestamp].position;
                        duplicate_obj_list[i].obj_look_pivot.localRotation = position_buffer[duplicate_obj_list[i].timestamp].view_rotation;
                    }
                    else
                    {
                        is_done = true;
                    }
                }
                else
                {
                    if (position_buffer[duplicate_obj_list[i].timestamp - 1].timestamp >= (current_time - (iteration_delay * (i + 1))))
                    {
                        if (duplicate_obj_list[i].timestamp <= 1)
                        {
                            duplicate_obj_list[i].timestamp = 1;
                            is_done = true;
                        }
                        else
                        {
                            duplicate_obj_list[i].timestamp--;
                        }
                        
                        duplicate_obj_list[i].obj.transform.position = position_buffer[duplicate_obj_list[i].timestamp].position;
                        duplicate_obj_list[i].obj_look_pivot.localRotation = position_buffer[duplicate_obj_list[i].timestamp].view_rotation;
                    }
                    else
                    {
                        is_done = true;
                    }
                }
            }


        }
        // Play Movement
        //if (position_buffer[timestamp_index + 1].timestamp <= current_time - iteration_delay)
        //{
        //    timestamp_index++;
        //    transform.position = position_buffer[timestamp_index].position;
        //    player_look_pivot.localRotation = position_buffer[timestamp_index].view_rotation;
        //}
        // Check Objects

        //
    }

    void Add_To_Buffer(Vector3 i_pos, Quaternion i_local_rot, float i_time)
    {
        Record_Data input_data = new Record_Data();
        input_data.position = i_pos;
        input_data.timestamp = i_time;
        input_data.view_rotation = i_local_rot;

        position_buffer.Add(input_data);
    }

    void Restart_Loop()
    {
        GameObject spawned_obj = Instantiate(loop_obj);

        Duplicate_Data dupe_data = new Duplicate_Data();
        dupe_data.obj = spawned_obj;
        dupe_data.timestamp = 1;
        dupe_data.obj_look_pivot = spawned_obj.GetComponent<Movement_Playback>().this_pivot;


        new_obj = spawned_obj.GetComponent<Movement_Playback>();

        

        vis.Add_Camera(spawned_obj.GetComponentInChildren<Camera>());

        new_obj.delay = iteration_delay;
        if (old_obj == null)
        {
            new_obj.target = original_obj.transform;
            new_obj.pivot_target = original_obj.GetComponent<Movement_Playback>().this_pivot;
            
        }
        else
        {
            new_obj.target = old_obj.transform;
            new_obj.pivot_target = old_obj.this_pivot;
        }

        duplicate_obj_list.Add(dupe_data);

        old_obj = new_obj;
    }
}
