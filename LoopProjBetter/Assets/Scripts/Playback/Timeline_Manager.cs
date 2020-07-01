using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

public class Timeline_Manager : MonoBehaviour
{
    //[SerializeField] public UnityEvent Reset_Time_Event;
    [SerializeField] private UnityEvent Paradox_Death_Event;

    private TimePoint_Corrector tc;

    private bool is_jumping_to_custom_time_point = false;

    [SerializeField] private List<int> time_jump_timestamps = new List<int>();

    [SerializeField] public float iteration_delay;
    [SerializeField] public int iteration_num = 0;
    [SerializeField] public float time_speed = 1.0f;
    [SerializeField] private int paradox_sensitivity = 100;
    [SerializeField] private float paradox_regeneration = 0.05f;
    private bool loop_restarted = false;

    public float time_of_present = 0.0f; // The furthest point in the future which the player has been at

    private bool is_recording = true;

    [SerializeField] private List<GameObject> snap_markers = new List<GameObject>();
    [SerializeField] private GameObject marker;

    [SerializeField] private GameObject door_marker;

    private Time_Jump_Effect tm_jump_effect;

    [SerializeField] public float current_time = 0.0f;
    [SerializeField] public float modified_current_time;
    [SerializeField] private Text time_display;
    [SerializeField] private Text health_display;
    [SerializeField] private Text selected_jump_display;
    public float health = 100.0f;
    public bool is_jumping = false;
    private float last_update_time = 0.0f;
    [SerializeField] float update_frequency;
    [Space]
    // Player
    [SerializeField] public Transform player_target;
    [SerializeField] private Camera player_cam;
    [SerializeField] public Transform player_look_pivot;
    [SerializeField] private Hold_Object player_obj_holder;
    private bool is_grabbing = false;
    [SerializeField] private Vector3 hidden_start_pos; // This is the position the timeloop duplicates will be held before their iteration begins
    private int last_obj_seen_timestamp = 0;

    //
    [Space]

    // Player looping
    [SerializeField] private Visible_Check vis;
    [SerializeField] private GameObject loop_obj;
    [SerializeField] private GameObject original_obj;
    private GameObject previous_obj;

    private Movement_Playback new_obj;
    private Movement_Playback old_obj;

    [SerializeField] private List<Duplicate_Data> duplicate_player_list = new List<Duplicate_Data>();

    [SerializeField] public Dictionary<int, Record_Data> timeline_memory = new Dictionary<int, Record_Data>();

    [SerializeField] private List<Duplicate_Data> dupe_objs = new List<Duplicate_Data>();
    [SerializeField] private List<Door_Activation> door_list = new List<Door_Activation>();

    [Serializable]
    public class Door_Data
    {
        public bool last_state;
        public Door_Activation door_activation;
        public GameObject door_obj;
    }


    [Serializable]
    public class Object_Dupe_Tracking_Data
    {
        public Vector3 position;
        public float timestamp;
    };

    // Objects
    [SerializeField] private List<Visible_Check> objects_vis = new List<Visible_Check>();

    [SerializeField] public List<Object_Type> object_type_list = new List<Object_Type>();
    private List<Transform> moveable_object_spawn_transforms = new List<Transform>();
    private List<Object_Spawns> moveable_object_spawns = new List<Object_Spawns>();

    [Serializable]
    public struct Object_Type
    {
        public string tag;
        public GameObject obj;
        public Vector3 position;
    };

    [Serializable]
    public class Self_Jumping_Object
    {
        public string tag;
        public Vector3 position;
        public float destination_time;
        public bool is_cooldown_active;
    };
    public List<Self_Jumping_Object> objects_to_time_travel = new List<Self_Jumping_Object>();
    public List<Self_Jumping_Object> time_travelling_objects = new List<Self_Jumping_Object>();

    [Serializable]
    public struct Object_Spawns
    {
        public Vector3 position;
        public GameObject obj;
    };

    [SerializeField] private GameObject box_prefab;

    [Serializable]
    public class Record_Data
    {
        public Vector3 position;

        public bool is_obj_seen;
        public Object_Type[] seen_objs;
        public int next_obj_seen_timestamp; // the timestamp in which the player will next observe an object

        public Door_Data[] door_data_record;

        public Quaternion view_rotation;
        public bool is_jumping;
        public bool is_grab_activated;
        public bool is_holding_object;
        public string held_object_tag;
        public float timestamp;
        public float normalized_timestamp;
    };

    [Serializable]
    public class Duplicate_Data
    {
        public GameObject obj;
        public Visible_Check vis;
        public Transform obj_look_pivot;
        public Hold_Object object_holder;
        public bool has_grabbed_this_interval;
        public int iter_num;
        public float paradox_suspicion;
        public bool is_seen;

        public bool has_obj_check_completed;
        public bool has_door_check_completed;
        public int timestamp;
    };
    //

    [Serializable]
    public class Time_Point // Stores all data required at that time
    {
        public int timestamp_index;
        public float timestamp;
        public float normalized_timestamp;

        public List<int> dupe_timestamp_indexes = new List<int>();

        // Object setting
        public List<Transform> object_location_transforms = new List<Transform>();
        public List<Object_Spawns> object_locations = new List<Object_Spawns>();
        //

        // Door settings
        public List<Door_Data> door_data_states = new List<Door_Data>();
    };

    [SerializeField] public List<Time_Point> time_point_list = new List<Time_Point>();
    [SerializeField] List<Time_Point> time_point_global_list = new List<Time_Point>();
    [SerializeField] int selected_time_slot = 1;

    bool is_jumping_to_present = false;
    bool is_jumping_to_start = false;

    // Start is called before the first frame update
    void Start()
    {
        tc = GetComponent<TimePoint_Corrector>();
        tm_jump_effect = GetComponent<Time_Jump_Effect>();

        for (int i = 0; i < 9; i++)
        {
            time_point_list.Add(new Time_Point());
        }

        time_point_list.Add(new Time_Point()); // 9 Past (0)
        time_point_list.Add(new Time_Point()); // 10 Future (furthest point forward)

        foreach (var obj_type in object_type_list)
        {
            GameObject[] object_array = GameObject.FindGameObjectsWithTag(obj_type.tag);
            foreach (var obj in object_array)
            {
                moveable_object_spawn_transforms.Add(obj.transform);

                Object_Spawns new_spawn = new Object_Spawns();
                new_spawn.position = obj.transform.position;
                new_spawn.obj = obj_type.obj;

                moveable_object_spawns.Add(new_spawn);
            }
        }


        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");


        foreach (var door in doors)
        {
            door_list.Add(door.GetComponentInChildren<Door_Activation>());
            door.GetComponentInChildren<Visible_Check>().Add_Camera(player_cam);
        }


        foreach (var obj in dupe_objs)
        {
            obj.vis = obj.obj.GetComponentInChildren<Visible_Check>();
            obj.vis.Add_Camera(player_cam);
        }

        current_time = 0.0f;

        //Add_To_Buffer(hidden_start_pos, player_look_pivot.localRotation, current_time);
        List<Door_Data> door_data_list = new List<Door_Data>();
        List<Object_Type> obj_array = new List<Object_Type>();

        Add_To_Buffer(hidden_start_pos, player_look_pivot.localRotation, door_data_list.ToArray(), obj_array.ToArray(), current_time, false, is_grabbing);
        Log_Current_Timestamp(); // To set the initial jump timestamp for the first dupe to be skipped
        Add_To_Buffer(hidden_start_pos, player_look_pivot.localRotation, door_data_list.ToArray(), obj_array.ToArray(), current_time, false, is_grabbing);
        Add_To_Buffer(hidden_start_pos, player_look_pivot.localRotation, door_data_list.ToArray(), obj_array.ToArray(), current_time, false, is_grabbing);
        Add_To_Buffer(hidden_start_pos, player_look_pivot.localRotation, door_data_list.ToArray(), obj_array.ToArray(), current_time, false, is_grabbing);
    }

    void Check_If_Present()
    {
        if (modified_current_time >= time_of_present)
        {
            time_of_present = modified_current_time;

            Save_Time_Point(10);
        }
    }

    void Check_If_Start()
    {
        if ((modified_current_time >= 0.1f) && (modified_current_time <= 0.6f))
        {
            Save_Time_Point(9);
        }
    }

    void Health_Regen()
    {
        if (health < 100.0f)
        {
            health += (paradox_regeneration * time_speed);
        }
    }

    void Text_Update()
    {
        time_display.text = Mathf.Ceil(current_time - (iteration_delay * (iteration_num - 1))).ToString();
        health_display.text = "Health: " + Mathf.CeilToInt(health).ToString();
        selected_jump_display.text = "ID selected: " + selected_time_slot + "\nDestination: " + time_point_list[selected_time_slot - 1].normalized_timestamp;
    }

    // Update is called once per frame
    void Update()
    {
        Text_Update();

        // Regen health slowly over time
        Health_Regen();

        if (is_recording)
        {
            if (loop_restarted)
            {
                loop_restarted = false;
                for (int i = 0; i < objects_vis.Count; i++)
                {
                    if (objects_vis[i] == null)
                    {
                        objects_vis.RemoveAt(i);
                        i--;
                    }
                }

                List<Duplicate_Data> dupe_objs_temp = new List<Duplicate_Data>();

                foreach (var dupe in dupe_objs)
                {
                    if (dupe.obj != null)
                    {
                        dupe_objs_temp.Add(dupe);
                        //snap_markers.Add(Instantiate(marker));
                    }
                }


                dupe_objs.Clear();

                foreach (var dupe in dupe_objs_temp)
                {
                    dupe_objs.Add(dupe);
                }
            }
            //if (Input.GetKey(KeyCode.F))
            //{
            //    time_speed = 0.2f;
            //}
            //else
            //{
            //    time_speed = 1.0f;
            //}

            current_time += Time.deltaTime * time_speed;
            Record_Player_Actions();
            Run_Playback();
            Object_Check();
        }

        if (current_time >= iteration_delay * iteration_num)
        {
            iteration_num++;
            Restart_Loop();
        }

        modified_current_time = current_time - (iteration_delay * (iteration_num - 1));
    }

    void Select_Time_Point_Slot_Input()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selected_time_slot = 1;
            //jump_time_point = time_point_list[selected_time_slot - 1];
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selected_time_slot = 2;
            //jump_time_point = time_point_list[selected_time_slot - 1];
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selected_time_slot = 3;
            //jump_time_point = time_point_list[selected_time_slot - 1];
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            selected_time_slot = 4;
            //jump_time_point = time_point_list[selected_time_slot - 1];
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            selected_time_slot = 5;
            //jump_time_point = time_point_list[selected_time_slot - 1];
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            selected_time_slot = 6;
            //jump_time_point = time_point_list[selected_time_slot - 1];
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            selected_time_slot = 7;
            //jump_time_point = time_point_list[selected_time_slot - 1];
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            selected_time_slot = 8;
            //jump_time_point = time_point_list[selected_time_slot - 1];
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            selected_time_slot = 9;
            //jump_time_point = time_point_list[selected_time_slot - 1];
        }
        
    }

    void Log_Object_Positions(int i_index)
    {
        time_point_list[i_index].object_locations.Clear();
        foreach (var obj_type in object_type_list)
        {
            GameObject[] object_array = GameObject.FindGameObjectsWithTag(obj_type.tag);
            foreach (var obj in object_array)
            {
                if (obj.GetComponent<Pickup_Loop>().is_picked_up == false)
                {
                    time_point_list[i_index].object_location_transforms.Add(obj.transform);

                    Object_Spawns new_spawn = new Object_Spawns();
                    new_spawn.position = obj.transform.position;
                    new_spawn.obj = obj_type.obj;

                    time_point_list[i_index].object_locations.Add(new_spawn);
                }
            }
        }
    }

    void Log_Door_States(int i_index)
    {
        time_point_list[i_index].door_data_states.Clear();

        foreach (var door_activation in door_list)
        {
            Door_Data input_door = new Door_Data();
            input_door.door_activation = door_activation;
            input_door.door_obj = door_activation.gameObject;
            input_door.last_state = door_activation.is_open;

            time_point_list[i_index].door_data_states.Add(input_door);
        }
    }

    void Save_Global_Time_Point()
    {
        Time_Point temp_point = new Time_Point();
        
        temp_point.timestamp = current_time;
        temp_point.timestamp_index = timeline_memory.Count;
        temp_point.normalized_timestamp = current_time - (iteration_delay * (iteration_num - 1));
        temp_point.dupe_timestamp_indexes.Clear();

        temp_point.dupe_timestamp_indexes.Add(temp_point.timestamp_index);
        foreach (var dupe_player in duplicate_player_list)
        {
            temp_point.dupe_timestamp_indexes.Add(dupe_player.timestamp);
            //jump_time_point.dupe_held_objects.Add()
        }

        // Objects
        temp_point.object_locations.Clear();
        foreach (var obj_type in object_type_list)
        {
            GameObject[] object_array = GameObject.FindGameObjectsWithTag(obj_type.tag);
            foreach (var obj in object_array)
            {
                if (obj.GetComponent<Pickup_Loop>().is_picked_up == false)
                {
                    temp_point.object_location_transforms.Add(obj.transform);

                    Object_Spawns new_spawn = new Object_Spawns();
                    new_spawn.position = obj.transform.position;
                    new_spawn.obj = obj_type.obj;

                    temp_point.object_locations.Add(new_spawn);
                }
            }
        }


        // Doors
        temp_point.door_data_states.Clear();

        foreach (var door_activation in door_list)
        {
            Door_Data input_door = new Door_Data();
            input_door.door_activation = door_activation;
            input_door.door_obj = door_activation.gameObject;
            input_door.last_state = door_activation.is_open;

            temp_point.door_data_states.Add(input_door);
        }

        time_point_global_list.Add(temp_point);

    }

    void Save_Time_Point(int i_index)
    {
        time_point_list[i_index].timestamp = current_time;
        time_point_list[i_index].timestamp_index = timeline_memory.Count;
        time_point_list[i_index].normalized_timestamp = current_time - (iteration_delay * (iteration_num - 1));
        time_point_list[i_index].dupe_timestamp_indexes.Clear();

        time_point_list[i_index].dupe_timestamp_indexes.Add(time_point_list[i_index].timestamp_index);
        foreach (var dupe_player in duplicate_player_list)
        {
            time_point_list[i_index].dupe_timestamp_indexes.Add(dupe_player.timestamp);
            //jump_time_point.dupe_held_objects.Add()
        }

        Log_Object_Positions(i_index);
        Log_Door_States(i_index);
    }

    public void Reset_Time()
    {
        is_jumping_to_start = true;
    }

    void Record_Player_Actions()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            is_grabbing = true;
        }

        Select_Time_Point_Slot_Input();

        if (Input.GetKeyDown(KeyCode.T))
        {
            Save_Time_Point(selected_time_slot - 1);
        }

        Check_If_Present();

        if (Input.GetKeyDown(KeyCode.G))
        {
            tm_jump_effect.is_activated = true;
        }

        if (Input.GetKeyDown(KeyCode.H)) // Present/Future
        {
            tm_jump_effect.is_activated = true;
            is_jumping_to_present = true;
        }

        if (Input.GetKeyDown(KeyCode.R)) // Past/Start
        {
            //Reset_Time_Event.Invoke();
        }

        if (tm_jump_effect.has_jump_occured)
        {
            tm_jump_effect.has_jump_occured = false;
            is_jumping = true;
            is_jumping_to_custom_time_point = true;
        }

        // Record Movement
        if ((current_time >= last_update_time + update_frequency) || is_grabbing)
        {
            last_update_time = current_time;

            //Save_Global_Time_Point();



            bool is_seen_by_player_original = false;
            List<Object_Type> obj_pos_array = new List<Object_Type>();
            foreach (var dupe in dupe_objs)
            {
                if (dupe.obj != null)
                {
                    if (dupe.vis.is_seen)
                    {
                        foreach (var cam in dupe.vis.seen_cams)
                        {
                            if (cam.gameObject == player_look_pivot.GetComponentInChildren<Camera>().gameObject)
                            {
                                is_seen_by_player_original = true;

                                Object_Type input_obj = new Object_Type();
                                input_obj.position = dupe.obj.transform.position;
                                input_obj.tag = dupe.obj.tag;
                                obj_pos_array.Add(input_obj);
                            }
                        }
                    }
                }
            }


            bool is_door_seen_by_player_original = false;
            List<Door_Data> door_data_list = new List<Door_Data>();
            foreach (var door in door_list)
            {
                Visible_Check vis_check = door.gameObject.GetComponent<Visible_Check>();
                if (door.GetComponentInChildren<Visible_Check>().is_seen)
                {
                    foreach (var cam in vis_check.seen_cams)
                    {
                        if (cam.gameObject == player_look_pivot.GetComponentInChildren<Camera>().gameObject)
                        {
                            is_door_seen_by_player_original = true;

                            Door_Data record_data = new Door_Data();
                            record_data.door_activation = door;
                            record_data.door_obj = door.gameObject;
                            record_data.last_state = door.is_open;

                            door_data_list.Add(record_data);
                        }
                    }
                }
            }

            if ((timeline_memory[timeline_memory.Count - 1].position != player_target.position)
                || (timeline_memory[timeline_memory.Count - 1].view_rotation != player_look_pivot.localRotation)
                || (is_seen_by_player_original) || (is_grabbing) || (is_door_seen_by_player_original) || (is_jumping))
            {
                if (is_jumping)
                {
                    Log_Current_Timestamp();
                }

                Add_To_Buffer(player_target.position, player_look_pivot.localRotation, door_data_list.ToArray(), obj_pos_array.ToArray(), current_time, is_jumping, is_grabbing);

                if (is_jumping)
                {
                    float time_until_next_loop = (iteration_delay * iteration_num) - current_time;
                    current_time += time_until_next_loop;
                    Debug.Log("Jumped: " + time_until_next_loop);

                    time_point_list[selected_time_slot - 1].timestamp += iteration_delay;

                    int jump_index = selected_time_slot - 1;

                    if (is_jumping_to_custom_time_point)
                    {
                        if (is_jumping_to_present)
                        {
                            jump_index = 10;
                        }
                        else if (is_jumping_to_start)
                        {
                            jump_index = 9;
                        }

                        is_jumping_to_present = false;
                        is_jumping_to_start = false;

                        current_time += time_point_list[jump_index].normalized_timestamp;
                        Debug.Log("Jumped even more: " + time_point_list[jump_index].normalized_timestamp);
                        Skip_Dupes_To_Custom(jump_index);

                        Reset_Objects(time_point_list[jump_index].object_locations);

                        Debug.Log(current_time);
                    }
                    else
                    {
                        Skip_Dupes_To_Custom(jump_index);
                    }

                    Add_To_Buffer(player_target.position, player_look_pivot.localRotation, door_data_list.ToArray(), obj_pos_array.ToArray(), current_time, false, false);

                    is_jumping = false;

                }

                if (is_seen_by_player_original)
                {
                    timeline_memory[last_obj_seen_timestamp].next_obj_seen_timestamp = timeline_memory.Count - 1;
                    last_obj_seen_timestamp = timeline_memory.Count - 1;
                }

                if (is_grabbing)
                {
                    is_grabbing = false;
                    player_obj_holder.trigger_grab = true;
                }

            }

            int num_of_seen_objs = 0;
            foreach (var obj in dupe_objs)
            {
                foreach (var cam in obj.vis.seen_cams)
                {
                    if (cam.gameObject == player_look_pivot.GetComponentInChildren<Camera>().gameObject)
                    {
                        num_of_seen_objs++;
                    }
                }
            }
        }

        Check_If_Start();

    }

    public void Create_Object(String i_tag,Vector3 i_pos)
    {
        GameObject obj_to_be_held = null;
        foreach (var obj_type in object_type_list)
        {
            if (obj_type.tag == i_tag)
            {
                obj_to_be_held = obj_type.obj;
                break;
            }
        }

        if (obj_to_be_held != null)
        {
            GameObject new_obj = Instantiate(obj_to_be_held, i_pos, new Quaternion(0.0f, 0.0f, 0.0f, 0.0f));
            objects_vis.Add(new_obj.GetComponent<Visible_Check>());

            Duplicate_Data obj_data = new Duplicate_Data();
            obj_data.obj = new_obj;
            obj_data.vis = new_obj.GetComponent<Visible_Check>();

            dupe_objs.Add(obj_data);
        }
    }

    void Run_Playback()
    {
        // Spawn time travelling objects
        int index = 0;
        foreach (var jumping_obj in time_travelling_objects)
        {
            if (!jumping_obj.is_cooldown_active)
            {
                if ((modified_current_time >= jumping_obj.destination_time) && (modified_current_time <= jumping_obj.destination_time + 0.5f))
                {
                    Create_Object(jumping_obj.tag, jumping_obj.position);
                    time_travelling_objects[index].is_cooldown_active = true;
                }
            }
            index++;
        }

        foreach (var jumping_obj in objects_to_time_travel)
        {

            if ((modified_current_time >= jumping_obj.destination_time) && (modified_current_time <= jumping_obj.destination_time + 0.5f))
            {
                Collider[] hit_colliders = Physics.OverlapSphere(jumping_obj.position, 0.1f);

                for (int i = 0; i < hit_colliders.Length; i++)
                {
                    if (hit_colliders[i].tag == jumping_obj.tag)
                    {
                        Destroy(hit_colliders[i].gameObject);
                        break;
                    }
                }
            }

        }

                    //

        for (int i = 0; i < duplicate_player_list.Count; i++)
        {
            // Resave time point

            for (int j = 0; j < time_point_list.Count; j++)
            {
                if (duplicate_player_list[i].timestamp == time_point_list[j].timestamp_index)
                {
                    Save_Time_Point(j);

                    //Debug.Log("Saved time point from " + time_point_list[j].timestamp + " to " + jump_time_point.timestamp);
                    //time_point_list[j] = jump_time_point;

                    //jump_time_point = time_point_list[selected_time_slot - 1];
                }
            }


            // End resave



            bool is_done = false;

            is_done = true;
                //
            if (time_speed >= 0.0f)
            {
                if (timeline_memory[duplicate_player_list[i].timestamp + 1].timestamp <= (current_time - (iteration_delay * (i + 1))))
                {
                    duplicate_player_list[i].timestamp++;
                    duplicate_player_list[i].has_grabbed_this_interval = false;
                    duplicate_player_list[i].has_obj_check_completed = false;
                    duplicate_player_list[i].has_door_check_completed = false;
                }
                else
                {
                    is_done = true;
                }
            }
            else
            {
                if (timeline_memory[duplicate_player_list[i].timestamp - 1].timestamp >= (current_time - (iteration_delay * (i + 1))))
                {
                    if (duplicate_player_list[i].timestamp <= 1)
                    {
                        duplicate_player_list[i].timestamp = 1;
                        is_done = true;
                    }
                    else
                    {
                        duplicate_player_list[i].timestamp--;
                    }

                    duplicate_player_list[i].has_grabbed_this_interval = false;
                }
                else
                {
                    is_done = true;
                }
            }

                //Object_Check();
            //}
            Set_Playback_States(duplicate_player_list[i]);
        }
    }

    void Object_Check()
    {

        // Find out if an object was seen when it wasn't meant to be
        foreach (var dupe in duplicate_player_list)
        {
            if (dupe.paradox_suspicion > 0.0f)
            {
                dupe.paradox_suspicion -= (1.0f * time_speed);
            }


            foreach (var visible_obj in dupe_objs)
            {
                foreach (var cam in visible_obj.vis.seen_cams)
                {
                    if (visible_obj.obj != null)
                    {
                        if (cam == dupe.obj.GetComponentInChildren<Camera>())
                        {
                            bool is_obj_new = true;
                            if (timeline_memory[dupe.timestamp].is_obj_seen)
                            {
                                foreach (var obj in timeline_memory[dupe.timestamp].seen_objs)
                                {
                                    if (Vector3.Distance(obj.position, visible_obj.obj.transform.position) <= 1.5f)
                                    {
                                        is_obj_new = false;
                                    }
                                }
                            }



                            if (is_obj_new)
                            {
                                //Debug.Log("NEW");
                                dupe.paradox_suspicion += (2.0f * time_speed);
                                //Debug.Log("Suspicion: " + dupe.paradox_suspicion);
                            }

                            if (dupe.paradox_suspicion >= 15.0f)
                            {
                                Debug.Log("NEW: OBJECT NAME: " + visible_obj.obj.name + " + POSITION: " + visible_obj.obj.transform.position);
                                Activate_Paradox_Increment(1.0f);
                                dupe.paradox_suspicion = 0.0f;
                            }
                        }
                    }
                }
            }

            // Find next time this object is seen
            if (dupe.has_obj_check_completed == false)
            {
                if (timeline_memory[dupe.timestamp].is_obj_seen == true)
                {
                    foreach (var obj_check in timeline_memory[dupe.timestamp].seen_objs)
                    {
                        bool is_completion_time_found = false;
                        //int current_timestamp_check = timeline_memory[dupe.timestamp].next_obj_seen_timestamp;
                        int current_timestamp_check = dupe.timestamp + 1;

                        int counter = 0;
                        while (!is_completion_time_found)
                        {

                            if (timeline_memory[current_timestamp_check].seen_objs != null)
                            {

                                foreach (var obj in timeline_memory[current_timestamp_check].seen_objs)
                                {
                                    if (Vector3.Distance(obj_check.position, obj.position) <= 1.0f)
                                    {
                                        is_completion_time_found = true;

                                        if ((current_timestamp_check == 0)/* || (timeline_memory[current_timestamp_check].timestamp >= (iteration_delay * (iteration_num - dupe.iter_num)))*/)
                                        {
                                            is_completion_time_found = true;
                                        }
                                        else
                                        {
                                            GameObject new_marker = Instantiate(marker, obj.position, new Quaternion());
                                            snap_markers.Add(new_marker);
                                            Align_Check marker_script = new_marker.GetComponent<Align_Check>();
                                            marker_script.completion_time = timeline_memory[current_timestamp_check].timestamp + (iteration_delay * dupe.iter_num);
                                            marker_script.obj_tag = obj.tag;

                                        }
                                    }
                                }
                            }

                            if (!is_completion_time_found)
                            {
                                //current_timestamp_check = timeline_memory[current_timestamp_check].next_obj_seen_timestamp;
                                current_timestamp_check++;
                            }

                            if (current_timestamp_check >= timeline_memory.Count)
                            {
                                is_completion_time_found = true;
                            }
                            counter++;
                        }


                    }

                    //Debug.Log(dupe.iter_num + " : " + (current_timestamp_check - dupe.timestamp));
                }
            }

            if (dupe.has_door_check_completed == false)
            {
                if (timeline_memory[dupe.timestamp].door_data_record != null)
                {
                    foreach (var door in timeline_memory[dupe.timestamp].door_data_record)
                    {



                        bool is_completion_time_found = false;
                        //int current_timestamp_check = timeline_memory[dupe.timestamp].next_obj_seen_timestamp;
                        int current_timestamp_check = dupe.timestamp + 1;

                        int counter = 0;
                        while (!is_completion_time_found)
                        {

                            if (timeline_memory[current_timestamp_check].door_data_record != null)
                            {

                                foreach (var door_data in timeline_memory[current_timestamp_check].door_data_record)
                                {
                                    if (door.door_activation == door_data.door_activation)
                                    {
                                        is_completion_time_found = true;

                                        if ((current_timestamp_check == 0)/* || (timeline_memory[current_timestamp_check].timestamp >= (iteration_delay * (iteration_num - dupe.iter_num)))*/)
                                        {
                                            is_completion_time_found = true;
                                        }
                                        else
                                        {
                                            //Debug.Log("DOOR TIMER AT: " + timeline_memory[current_timestamp_check].timestamp);

                                            GameObject new_marker = Instantiate(door_marker, door_data.door_obj.transform.position, new Quaternion());
                                            Door_State_Check state_checker = new_marker.GetComponent<Door_State_Check>();
                                            state_checker.Set_Linked_Door(door.door_activation);
                                            state_checker.expected_state = door_data.last_state;
                                            state_checker.completion_time = timeline_memory[current_timestamp_check].timestamp + (iteration_delay * dupe.iter_num);
                                            snap_markers.Add(new_marker);
                                        }
                                    }
                                }
                            }

                            if (!is_completion_time_found)
                            {
                                //current_timestamp_check = timeline_memory[current_timestamp_check].next_obj_seen_timestamp;
                                current_timestamp_check++;
                            }

                            if (current_timestamp_check >= timeline_memory.Count)
                            {
                                is_completion_time_found = true;
                            }
                            counter++;
                        }
                    }
                }
            }

            dupe.has_obj_check_completed = true;
            dupe.has_door_check_completed = true;
        }

        List<GameObject> temp_markers = new List<GameObject>();

        foreach (var marker in snap_markers)
        {
            if (marker != null)
            {
                Align_Check object_align = marker.GetComponent<Align_Check>();
                Door_State_Check door_check = marker.GetComponent<Door_State_Check>();

                if (object_align != null)
                {
                    object_align.current_time = current_time;
                    temp_markers.Add(marker);
                }
                else if (door_check != null)
                {
                    door_check.current_time = current_time;
                    temp_markers.Add(marker);
                }
            }
        }

        snap_markers.Clear();
        snap_markers = temp_markers;
    }

    void Set_Playback_States(Duplicate_Data i_dupe)
    {
        i_dupe.obj.transform.position = Vector3.Lerp(i_dupe.obj.transform.position, timeline_memory[i_dupe.timestamp].position, 0.4f);
        i_dupe.obj_look_pivot.localRotation = timeline_memory[i_dupe.timestamp].view_rotation;
        i_dupe.obj.SetActive(!timeline_memory[i_dupe.timestamp].is_jumping);
        i_dupe.object_holder.gameObject.SetActive(!timeline_memory[i_dupe.timestamp].is_jumping);

        if ((timeline_memory[i_dupe.timestamp].is_grab_activated) && (i_dupe.has_grabbed_this_interval == false))
        {
            i_dupe.has_grabbed_this_interval = true;
            i_dupe.object_holder.trigger_grab = true;
        }
    }

    bool Is_Player_Holding_Object()
    {
        return player_obj_holder.is_holding;
    }

    void Add_To_Buffer(Vector3 i_pos, Quaternion i_local_rot, Door_Data[] i_door_data, Object_Type[] i_objs, float i_time, bool i_jump_state, bool i_is_grab_toggled)
    {
        Record_Data input_data = new Record_Data();
        input_data.position = i_pos;
        input_data.timestamp = i_time;
        input_data.normalized_timestamp = i_time - (iteration_delay * (iteration_num - 1));
        input_data.view_rotation = i_local_rot;
        input_data.is_jumping = i_jump_state;
        input_data.is_grab_activated = i_is_grab_toggled;

        input_data.is_holding_object = Is_Player_Holding_Object();

        if (input_data.is_holding_object)
        {
            input_data.held_object_tag = player_obj_holder.grabbed_item_obj.tag;
        }

        input_data.door_data_record = i_door_data;

        if (i_objs.Length > 0)
        {
            input_data.is_obj_seen = true;
        }
        else
        {
            input_data.is_obj_seen = false;
        }
        
        input_data.seen_objs = i_objs;

        timeline_memory.Add(timeline_memory.Count, input_data);
    }

    void Reset_Objects(List<Object_Spawns> i_objects_to_spawn)
    {
        foreach (var new_spawn in i_objects_to_spawn)
        {
            GameObject created_obj = Instantiate(new_spawn.obj, new_spawn.position, new Quaternion());
            Duplicate_Data input_data = new Duplicate_Data();
            input_data.obj = created_obj;
            input_data.vis = created_obj.GetComponent<Visible_Check>();
            input_data.timestamp = 0;
            input_data.iter_num = iteration_num;

            dupe_objs.Add(input_data);
        }
    }

    void Restart_Loop()
    {
        loop_restarted = true;

        for (int i = 0; i < time_travelling_objects.Count; i++)
        {
            time_travelling_objects[i].is_cooldown_active = false;
        }

        if (!is_jumping_to_custom_time_point)
        {
            Reset_Objects(moveable_object_spawns);

            foreach (var door in door_list)
            {
                door.is_door_opening = false;
            }
        }
        else
        {
            is_jumping_to_custom_time_point = false;
        }

        GameObject spawned_obj = Instantiate(loop_obj);

        Duplicate_Data dupe_data = new Duplicate_Data();
        dupe_data.obj = spawned_obj;
        dupe_data.timestamp = 1;
        dupe_data.iter_num = iteration_num;
        dupe_data.obj_look_pivot = spawned_obj.GetComponent<Movement_Playback>().this_pivot;
        dupe_data.object_holder = spawned_obj.GetComponentInChildren<Hold_Object>();
        dupe_data.has_grabbed_this_interval = false;
        
        dupe_data.paradox_suspicion = 0;

        vis.Add_Camera(spawned_obj.GetComponentInChildren<Camera>());

        foreach (var obj in dupe_objs)
        {
            if (obj != null)
            {
                foreach (var cam in vis.cams)
                {
                    obj.vis.Add_Camera(cam);
                }
            }
        }

        duplicate_player_list.Add(dupe_data);
    }

    public void Activate_Paradox_Increment(float i_value)
    {
        health -= (i_value * time_speed);

        if (health <= 0.0f)
        {
            Paradox_Death_Event.Invoke();
        }
    }

    private void Log_Current_Timestamp()
    {
        time_jump_timestamps.Add(timeline_memory.Count);
    }

    private void Skip_Dupes_To_Custom(int jump_index)
    {
        for (int i = 0; i < duplicate_player_list.Count; i++)
        {
            float skip_adjustment = 0.0f; // Skip by this amount if the playback has been skipped during time travel.
            int original_timestamp_index = duplicate_player_list[i].timestamp;

            if ((current_time - (iteration_delay * (i + 1))) >= timeline_memory[duplicate_player_list[i].timestamp + 2].timestamp + iteration_delay)
            {
                skip_adjustment = iteration_delay;
            }
                
            bool is_skip_processing_valid = true;
            while (is_skip_processing_valid)
            {
                if (timeline_memory.Count > duplicate_player_list[i].timestamp + 2)
                {
                    if ((current_time - (iteration_delay * (i + 1))) >= timeline_memory[duplicate_player_list[i].timestamp + 2].timestamp)
                    {
                        duplicate_player_list[i].timestamp++;
                        //Debug.Log("++");
                    }
                    else
                    {
                        is_skip_processing_valid = false;
                    }
                }
                else
                {
                    is_skip_processing_valid = false;
                }
            }

            //---------------------------------------------------------------------------------------//
            //----------------------------------Object spawning--------------------------------------//
            //---------------------------------------------------------------------------------------//

            if (timeline_memory[duplicate_player_list[i].timestamp].held_object_tag != timeline_memory[original_timestamp_index].held_object_tag)
            {
                if ((timeline_memory[duplicate_player_list[i].timestamp].is_holding_object) && (!timeline_memory[original_timestamp_index].is_holding_object))
                {
                    GameObject obj_to_be_held = null;
                    foreach (var obj_type in object_type_list)
                    {
                        if (timeline_memory[duplicate_player_list[i].timestamp].held_object_tag == obj_type.tag)
                        {
                            obj_to_be_held = obj_type.obj;
                        }
                    }

                    Set_Grabbed_Item(duplicate_player_list[i], obj_to_be_held);
                }
                else if ((!timeline_memory[duplicate_player_list[i].timestamp].is_holding_object) && (timeline_memory[original_timestamp_index].is_holding_object))
                {
                    Destroy_Grabbed_Item(duplicate_player_list[i]);
                }
                else
                {
                    Destroy_Grabbed_Item(duplicate_player_list[i]);

                    GameObject obj_to_be_held = null;
                    foreach (var obj_type in object_type_list)
                    {
                        if (timeline_memory[duplicate_player_list[i].timestamp].held_object_tag == obj_type.tag)
                        {
                            obj_to_be_held = obj_type.obj;
                        }
                    }

                    Set_Grabbed_Item(duplicate_player_list[i], obj_to_be_held);
                }
            }
        }

        //---------------------------------------------------------------------------------------//
        //----------------------------------Object spawning--------------------------------------//
        //---------------------------------------------------------------------------------------//

        foreach (var door_to_set in time_point_list[jump_index].door_data_states)
        {
            if (door_to_set.door_activation.is_inverted)
            {
                door_to_set.door_activation.is_door_opening = !door_to_set.last_state;
            }
            else
            {
                door_to_set.door_activation.is_door_opening = door_to_set.last_state;
            }
        }
    }

    private void Skip_Dupes_Forward()
    {
        foreach (var time_dupe in duplicate_player_list)
        {
            int dupe_timestamp = time_dupe.timestamp;

            bool is_skip_done = false;

            foreach (var jump_timestamp in time_jump_timestamps)
            {
                if (!is_skip_done)
                {
                    if (jump_timestamp == dupe_timestamp)
                    {
                        is_skip_done = true;
                    }
                    else if (jump_timestamp > dupe_timestamp)
                    {
                        if (timeline_memory[jump_timestamp].held_object_tag != timeline_memory[time_dupe.timestamp].held_object_tag)
                        {
                            if ((timeline_memory[jump_timestamp].is_holding_object) && (!timeline_memory[time_dupe.timestamp].is_holding_object))
                            {
                                GameObject obj_to_be_held = null;
                                foreach (var obj_type in object_type_list)
                                {
                                    if (timeline_memory[jump_timestamp].held_object_tag == obj_type.tag)
                                    {
                                        obj_to_be_held = obj_type.obj;
                                    }
                                }

                                Set_Grabbed_Item(time_dupe, obj_to_be_held);
                            }
                            else if ((!timeline_memory[jump_timestamp].is_holding_object) && (timeline_memory[time_dupe.timestamp].is_holding_object))
                            {
                                Destroy_Grabbed_Item(time_dupe);
                            }
                            else
                            {
                                Destroy_Grabbed_Item(time_dupe);

                                GameObject obj_to_be_held = null;
                                foreach (var obj_type in object_type_list)
                                {
                                    if (timeline_memory[jump_timestamp].held_object_tag == obj_type.tag)
                                    {
                                        obj_to_be_held = obj_type.obj;
                                    }
                                }

                                Set_Grabbed_Item(time_dupe, obj_to_be_held);
                            }
                        }

                        time_dupe.timestamp = jump_timestamp;

                        is_skip_done = true;
                    }
                }
            }
        }


        // Snap marker jump calculations
        foreach (var marker in snap_markers)
        {
            if (marker != null)
            {
                Align_Check align_marker = marker.GetComponent<Align_Check>();

                if (align_marker != null)
                {
                    float completion_time = align_marker.completion_time;

                    if (completion_time < iteration_delay * iteration_num)
                    {
                        align_marker.completion_time += iteration_delay;
                        Debug.Log("Forward");
                    }
                }
            }
        }


    }

    void Destroy_Grabbed_Item(Duplicate_Data i_time_dupe)
    {
        i_time_dupe.object_holder.is_holding = false;
        Destroy(i_time_dupe.object_holder.grabbed_item_obj);
        i_time_dupe.object_holder.grabbed_item_obj = null;
        i_time_dupe.object_holder.grabbed_item = null;
    }

    void Set_Grabbed_Item(Duplicate_Data i_time_dupe, GameObject i_object_to_be_held)
    {
        GameObject created_obj = Instantiate(i_object_to_be_held, i_time_dupe.obj.transform.position + i_time_dupe.obj.transform.forward, new Quaternion());
        Duplicate_Data input_data = new Duplicate_Data();
        input_data.obj = created_obj;
        input_data.vis = created_obj.GetComponent<Visible_Check>();
        input_data.timestamp = 0;
        input_data.iter_num = iteration_num;

        dupe_objs.Add(input_data);


        i_time_dupe.object_holder.is_holding = true;
        i_time_dupe.object_holder.grabbed_item_obj = created_obj;

        Pickup_Loop spawned_pickup = created_obj.GetComponent<Pickup_Loop>();

        i_time_dupe.object_holder.grabbed_item = spawned_pickup;
        spawned_pickup.object_holding_this = i_time_dupe.obj;
        spawned_pickup.is_picked_up = true;
    }

}
