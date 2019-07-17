using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Timeline_Manager : MonoBehaviour
{
    [SerializeField] private float iteration_delay;
    [SerializeField] public int iteration_num = 0;
    [SerializeField] private float time_speed = 1.0f;

    private float current_time = 0.0f;
    private float last_update_time = 0.0f;
    [SerializeField] float update_frequency;
    [Space]
    // Player
    [SerializeField] public Transform player_target;
    [SerializeField] public Transform player_look_pivot;
    [SerializeField] private Hold_Object player_obj_holder;

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

    [SerializeField] private Dictionary<int, Record_Data> timeline_memory = new Dictionary<int, Record_Data>();

    // Objects
    //[SerializeField] private Dictionary<int, Object_Memory_Data> object_timeline_memory = new Dictionary<int, Object_Memory_Data>();
    [SerializeField] private List<Object_Memory_Data> object_timeline_memory = new List<Object_Memory_Data>();
    [SerializeField] private List<Visible_Check> moveable_objects_vis = new List<Visible_Check>();

    [SerializeField] private List<Transform> moveable_object_spawn_transforms = new List<Transform>();
    private List<Vector3> moveable_object_spawns = new List<Vector3>();

    [SerializeField] private GameObject box_prefab;
    [SerializeField] private int object_timestamp_index = 0;

    [Serializable]
    public struct Object_Memory_Data
    {
        public Vector3 position;
        public string name;
        public float timestamp;
    };
    //

    [Serializable]
    public struct Record_Data
    {
        public Vector3 position;
        public Quaternion view_rotation;
        public bool is_jumping;
        public bool is_grab_activated;
        public float timestamp;
    };

    [Serializable]
    public class Duplicate_Data
    {
        public GameObject obj;
        public Transform obj_look_pivot;
        public Hold_Object object_holder;
        public bool has_grabbed_this_interval;
        public int timestamp;
    };
    //


    // Start is called before the first frame update
    void Start()
    {
        foreach (var spawn in moveable_object_spawn_transforms)
        {
            moveable_object_spawns.Add(spawn.position);
            moveable_objects_vis.Add(spawn.gameObject.GetComponent<Visible_Check>());
        }

        current_time = 0.0f;

        Add_To_Buffer(player_target.position, player_look_pivot.localRotation, current_time);
        Add_To_Buffer(player_target.position, player_look_pivot.localRotation, current_time);
        Add_To_Buffer(player_target.position, player_look_pivot.localRotation, current_time);

        Add_Object_Data(moveable_objects_vis[0].transform.position, (current_time - (iteration_delay * (iteration_num - 1))));
        Add_Object_Data(moveable_objects_vis[0].transform.position, (current_time - (iteration_delay * (iteration_num - 1))));
        Add_Object_Data(moveable_objects_vis[0].transform.position, (current_time - (iteration_delay * (iteration_num - 1))));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Add_To_Buffer(player_target.position, player_look_pivot.localRotation, current_time,true);
            current_time += 25.0f;
            Add_To_Buffer(player_target.position, player_look_pivot.localRotation, current_time,false);
        }
        if (Input.GetKey(KeyCode.F))
        {
            time_speed = 0.1f;
        }
        else
        {
            time_speed = 1.0f;
        }

        current_time += Time.deltaTime * time_speed;
        Run_Playback();

        if (current_time >= iteration_delay * iteration_num)
        {
            iteration_num++;
            Restart_Loop();
        }
    }

    void Add_Object_Data(Vector3 i_pos, float i_time)
    {
        Object_Memory_Data input_data = new Object_Memory_Data();
        input_data.position = i_pos;
        input_data.timestamp = i_time;

        object_timeline_memory.Insert(object_timestamp_index, input_data);
        //Debug.Log("INSERTED AT: " + object_timestamp_index);
    }

    void Run_Playback()
    {

        if (Input.GetKeyDown(KeyCode.E))
        {
            player_obj_holder.trigger_grab = true;
            Add_To_Buffer(player_target.position, player_look_pivot.localRotation, current_time,false,true);
        }

        // Record Movement
        if (current_time >= last_update_time + update_frequency)
        {
            last_update_time = current_time;
            if ((timeline_memory[timeline_memory.Count - 1].position != player_target.position) || (timeline_memory[timeline_memory.Count - 1].view_rotation != player_look_pivot.localRotation))
            {
                Add_To_Buffer(player_target.position, player_look_pivot.localRotation, current_time);
            }

            if (moveable_objects_vis[0].is_seen)
            {
                Add_Object_Data(moveable_objects_vis[0].transform.position, (current_time - (iteration_delay * (iteration_num - 1))));
                //Debug.Log(moveable_objects[0].transform.position);
            }
        }

        for (int i = 0; i < duplicate_obj_list.Count; i++)
        {
            bool is_done = false;

            while(!is_done)
            {
                if (time_speed >= 0.0f)
                {
                    if (timeline_memory[duplicate_obj_list[i].timestamp + 1].timestamp <= (current_time - (iteration_delay * (i + 1))))
                    {
                        duplicate_obj_list[i].timestamp++;
                        //duplicate_obj_list[i].obj.transform.position = position_buffer[duplicate_obj_list[i].timestamp].position;
                        //duplicate_obj_list[i].obj_look_pivot.localRotation = position_buffer[duplicate_obj_list[i].timestamp].view_rotation;

                        duplicate_obj_list[i].has_grabbed_this_interval = false;
                    }
                    else
                    {
                        is_done = true;
                    }
                }
                else
                {
                    if (timeline_memory[duplicate_obj_list[i].timestamp - 1].timestamp >= (current_time - (iteration_delay * (i + 1))))
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

                        duplicate_obj_list[i].has_grabbed_this_interval = false;

                        //Set_Playback_States(duplicate_obj_list[i]);
                    }
                    else
                    {
                        is_done = true;
                    }
                }
            }
            Set_Playback_States(duplicate_obj_list[i]);


        }

        // Check Objects
        if (object_timestamp_index < object_timeline_memory.Count)
        {
            //Debug.Log("CURRENT: " + (current_time - (iteration_delay * (iteration_num-1))) + " TIMESTAMP: " + object_timeline_memory[object_timestamp_index].timestamp);
            if (current_time - (iteration_delay * (iteration_num - 1)) >= object_timeline_memory[object_timestamp_index].timestamp)
            {
                Check_Object_State();
                object_timestamp_index++;
            }
        }
    }

    void Check_Object_State()
    {
        Collider[] found_objs = Physics.OverlapSphere(object_timeline_memory[object_timestamp_index].position, 1.0f);

        //Debug.Log("FOUND: " + found_objs.Length);
    }

    void Set_Playback_States(Duplicate_Data i_dupe)
    {
        i_dupe.obj.transform.position = Vector3.Lerp(i_dupe.obj.transform.position, timeline_memory[i_dupe.timestamp].position, 0.4f);
        i_dupe.obj_look_pivot.localRotation = timeline_memory[i_dupe.timestamp].view_rotation;
        i_dupe.obj.SetActive(!timeline_memory[i_dupe.timestamp].is_jumping);

        if ((timeline_memory[i_dupe.timestamp].is_grab_activated) && (i_dupe.has_grabbed_this_interval == false))
        {
            i_dupe.has_grabbed_this_interval = true;
            Debug.Log("GRABBED");
            i_dupe.object_holder.trigger_grab = true;
        }
    }

    void Add_To_Buffer(Vector3 i_pos, Quaternion i_local_rot, float i_time)
    {
        Record_Data input_data = new Record_Data();
        input_data.position = i_pos;
        input_data.timestamp = i_time;
        input_data.view_rotation = i_local_rot;
        input_data.is_jumping = false;
        input_data.is_grab_activated = false;

        timeline_memory.Add(timeline_memory.Count,input_data);
    }

    void Add_To_Buffer(Vector3 i_pos, Quaternion i_local_rot, float i_time, bool i_jump_state)
    {
        Record_Data input_data = new Record_Data();
        input_data.position = i_pos;
        input_data.timestamp = i_time;
        input_data.view_rotation = i_local_rot;
        input_data.is_jumping = i_jump_state;
        input_data.is_grab_activated = false;

        timeline_memory.Add(timeline_memory.Count, input_data);
    }

    void Add_To_Buffer(Vector3 i_pos, Quaternion i_local_rot, float i_time, bool i_jump_state, bool i_is_grab_toggled)
    {
        Record_Data input_data = new Record_Data();
        input_data.position = i_pos;
        input_data.timestamp = i_time;
        input_data.view_rotation = i_local_rot;
        input_data.is_jumping = i_jump_state;
        input_data.is_grab_activated = i_is_grab_toggled;

        timeline_memory.Add(timeline_memory.Count, input_data);
    }

    void Restart_Loop()
    {
        foreach(var spawnpoint in moveable_object_spawns)
        {
            Instantiate(box_prefab, spawnpoint,new Quaternion());
        }

        object_timestamp_index = 0;

        GameObject spawned_obj = Instantiate(loop_obj);

        Duplicate_Data dupe_data = new Duplicate_Data();
        dupe_data.obj = spawned_obj;
        dupe_data.timestamp = 1;
        dupe_data.obj_look_pivot = spawned_obj.GetComponent<Movement_Playback>().this_pivot;
        dupe_data.object_holder = spawned_obj.GetComponentInChildren<Hold_Object>();
        dupe_data.has_grabbed_this_interval = false;


        //new_obj = spawned_obj.GetComponent<Movement_Playback>();



        vis.Add_Camera(spawned_obj.GetComponentInChildren<Camera>());

        //new_obj.delay = iteration_delay;
        //if (old_obj == null)
        //{
        //    new_obj.target = original_obj.transform;
        //    new_obj.pivot_target = original_obj.GetComponent<Movement_Playback>().this_pivot;

        //}
        //else
        //{
        //    new_obj.target = old_obj.transform;
        //    new_obj.pivot_target = old_obj.this_pivot;
        //}

        duplicate_obj_list.Add(dupe_data);

        //old_obj = new_obj;
    }
}
