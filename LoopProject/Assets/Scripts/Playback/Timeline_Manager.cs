using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Timeline_Manager : MonoBehaviour
{
    [SerializeField] private float iteration_delay;
    [SerializeField] public int iteration_num = 0;
    [SerializeField] private float time_speed = 1.0f;
    private bool loop_restarted = false;

    [SerializeField] private List<GameObject> snap_markers = new List<GameObject>();
    [SerializeField] private GameObject marker;

    private float current_time = 0.0f;
    [SerializeField] private Text time_display;
    private float last_update_time = 0.0f;
    [SerializeField] float update_frequency;
    [Space]
    // Player
    [SerializeField] public Transform player_target;
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

    [SerializeField] private Dictionary<int, Record_Data> timeline_memory = new Dictionary<int, Record_Data>();

    [SerializeField] private List<Duplicate_Data> dupe_objs = new List<Duplicate_Data>();


    [Serializable]
    public class Object_Dupe_Tracking_Data
    {
        public Vector3 position;
        public float timestamp;
    };

    // Objects

    //[SerializeField] private List<Object_Memory_Data> object_timeline_memory = new List<Object_Memory_Data>();
    [SerializeField] private List<Visible_Check> moveable_objects_vis = new List<Visible_Check>();

    [SerializeField] private List<Transform> moveable_object_spawn_transforms = new List<Transform>();
    private List<Vector3> moveable_object_spawns = new List<Vector3>();

    [SerializeField] private GameObject box_prefab;
    [SerializeField] private int object_timestamp_index = 0;

    //[Serializable]
    //public class Object_Memory_Data
    //{
    //    public Vector3 position;
    //    public string name;
    //    public int id;
    //    public float timestamp;
    //};
    ////

    [Serializable]
    public class Record_Data
    {
        public Vector3 position;

        public bool is_obj_seen;
        public Vector3[] seen_objs_positions;
        public int next_obj_seen_timestamp; // the timestamp in which the player will next observe an object

        public Quaternion view_rotation;
        public bool is_jumping;
        public bool is_grab_activated;
        public float timestamp;
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
        public bool is_seen;
        public int timestamp;
    };
    //


    // Start is called before the first frame update
    void Start()
    {
        foreach (var spawn in moveable_object_spawn_transforms)
        {
            moveable_object_spawns.Add(spawn.position);
        }

        foreach (var obj in dupe_objs)
        {
            obj.vis = obj.obj.GetComponent<Visible_Check>();
            //snap_markers.Add(Instantiate(marker));
        }

        current_time = 0.0f;

        Add_To_Buffer(hidden_start_pos, player_look_pivot.localRotation, current_time);
        Add_To_Buffer(hidden_start_pos, player_look_pivot.localRotation, current_time);
        Add_To_Buffer(hidden_start_pos, player_look_pivot.localRotation, current_time);

        //Add_To_Obj_Timeline(hidden_start_pos, 0, current_time);
        //Add_To_Obj_Timeline(hidden_start_pos, 0, current_time);
        //Add_To_Obj_Timeline(hidden_start_pos, 0, current_time);
    }

    // Update is called once per frame
    void Update()
    {
        time_display.text = Mathf.Ceil(current_time - (iteration_delay * (iteration_num-1))).ToString();
        if (loop_restarted)
        {
            loop_restarted = false;
            for (int i = 0; i < moveable_objects_vis.Count; i++)
            {
                if (moveable_objects_vis[i] == null)
                {
                    moveable_objects_vis.RemoveAt(i);
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

    //void Add_Object_Data(Vector3 i_pos, int i_id, float i_time)
    //{
    //    Object_Memory_Data input_data = new Object_Memory_Data();
    //    input_data.position = i_pos;
    //    input_data.timestamp = i_time;
    //    input_data.id = i_id;

    //    object_timeline_memory.Insert(object_timestamp_index, input_data);
    //}

    void Run_Playback()
    {

        if (Input.GetKeyDown(KeyCode.E))
        {
            is_grabbing = true;
        }

        // Record Movement
        if (current_time >= last_update_time + update_frequency)
        {
            last_update_time = current_time;

            bool is_seen_by_player_original = false;
            List<Vector3> obj_pos_array = new List<Vector3>();
            foreach (var dupe in dupe_objs)
            {
                if (dupe.vis.is_seen)
                {
                    foreach (var cam in dupe.vis.seen_cams)
                    {
                        if (cam.gameObject == player_look_pivot.GetComponentInChildren<Camera>().gameObject)
                        {
                            is_seen_by_player_original = true;
                            obj_pos_array.Add(dupe.obj.transform.position);
                        }
                    }
                }
            }

            if ((timeline_memory[timeline_memory.Count - 1].position != player_target.position)
                || (timeline_memory[timeline_memory.Count - 1].view_rotation != player_look_pivot.localRotation)
                || (is_seen_by_player_original) || (is_grabbing))
            {
                if (is_seen_by_player_original)
                {


                    Add_To_Buffer(player_target.position, player_look_pivot.localRotation, obj_pos_array.ToArray(), current_time);
                    timeline_memory[last_obj_seen_timestamp].next_obj_seen_timestamp = timeline_memory.Count-1;

                    //Debug.Log("----------------------------");
                    //Debug.Log("FROM: " + last_obj_seen_timestamp);
                    //if (last_obj_seen_timestamp != 0)
                    //{
                    //    if (timeline_memory[last_obj_seen_timestamp].seen_objs_positions != null)
                    //    {
                    //        Debug.Log("OBJECT: " + timeline_memory[last_obj_seen_timestamp].seen_objs_positions[0]);
                    //    }
                    //    else
                    //    {
                    //        Debug.Log("OBJECT: NULL");
                    //    }
                        
                    //}
                    
                    //Debug.Log("TO: " + (timeline_memory.Count-1));
                    //Debug.Log("----------------------------");

                    last_obj_seen_timestamp = timeline_memory.Count-1;



                }
                else
                {
                    Add_To_Buffer(player_target.position, player_look_pivot.localRotation, current_time);
                }

                if (is_grabbing)
                {
                    is_grabbing = false;
                    Add_To_Buffer(player_target.position, player_look_pivot.localRotation, current_time, false, true);
                    player_obj_holder.trigger_grab = true;
                }
                
            }

            int num_of_seen_objs = 0;
            foreach(var obj in dupe_objs)
            {
                foreach(var cam in obj.vis.seen_cams)
                {
                    if (cam.gameObject == player_look_pivot.GetComponentInChildren<Camera>().gameObject)
                    {
                        num_of_seen_objs++;
                    }
                }
            }
        }

        for (int i = 0; i < duplicate_player_list.Count; i++)
        {
            bool is_done = false;

            while(!is_done)
            {
                if (time_speed >= 0.0f)
                {
                    if (timeline_memory[duplicate_player_list[i].timestamp + 1].timestamp <= (current_time - (iteration_delay * (i + 1))))
                    {
                        duplicate_player_list[i].timestamp++;
                        duplicate_player_list[i].has_grabbed_this_interval = false;
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
            }
            Set_Playback_States(duplicate_player_list[i]);
        }

        //foreach (var marker in snap_markers)
        //{
        //    Destroy(marker.gameObject);
        //}

        //snap_markers.Clear();

        foreach (var dupe in duplicate_player_list)
        {
            // Find next time this object is seen
            if (timeline_memory[dupe.timestamp].is_obj_seen == true)
            {
                bool is_completion_time_found = false;
                int current_timestamp_check = timeline_memory[dupe.timestamp].next_obj_seen_timestamp;

                int counter = 0;
                while (!is_completion_time_found)
                {
                    if ((current_timestamp_check == 0) || (timeline_memory[current_timestamp_check].timestamp >= (iteration_delay * (dupe.iter_num))))
                    {
                        is_completion_time_found = true;
                        break;
                    }

                    foreach (var pos_check in timeline_memory[dupe.timestamp].seen_objs_positions)
                    {
                        if (timeline_memory[current_timestamp_check].seen_objs_positions == null)
                        {
                            is_completion_time_found = true;
                            break;
                        }

                        foreach (var obj in timeline_memory[current_timestamp_check].seen_objs_positions)
                        {
                            if (Vector3.Distance(pos_check, obj) <= 1.0f)
                            {
                                is_completion_time_found = true;

                                GameObject new_marker = Instantiate(marker, obj, new Quaternion());
                                snap_markers.Add(new_marker);
                                new_marker.GetComponent<Align_Check>().completion_time = timeline_memory[current_timestamp_check].timestamp + (iteration_delay * dupe.iter_num);
                            }
                        }
                    }

                    if (!is_completion_time_found)
                    {
                        current_timestamp_check = timeline_memory[current_timestamp_check].next_obj_seen_timestamp;
                    }
                    counter++;
                }
                
            }
        }

        List<GameObject> temp_markers = new List<GameObject>();

        foreach (var marker in snap_markers)
        {
            if (marker != null)
            {
                marker.GetComponent<Align_Check>().current_time = current_time;
                temp_markers.Add(marker);
            }
        }

        snap_markers.Clear();
        snap_markers = temp_markers;
    }

    //void Check_Object_State()
    //{
    //    Collider[] found_objs = Physics.OverlapSphere(object_timeline_memory[object_timestamp_index].position, 1.0f);
    //}

    void Set_Playback_States(Duplicate_Data i_dupe)
    {
        i_dupe.obj.transform.position = Vector3.Lerp(i_dupe.obj.transform.position, timeline_memory[i_dupe.timestamp].position, 0.4f);
        i_dupe.obj_look_pivot.localRotation = timeline_memory[i_dupe.timestamp].view_rotation;
        i_dupe.obj.SetActive(!timeline_memory[i_dupe.timestamp].is_jumping);

        if ((timeline_memory[i_dupe.timestamp].is_grab_activated) && (i_dupe.has_grabbed_this_interval == false))
        {
            i_dupe.has_grabbed_this_interval = true;
            i_dupe.object_holder.trigger_grab = true;
        }

        //if (timeline_memory[i_dupe.timestamp].is_obj_seen)
        //{
        //    foreach (var obj in timeline_memory[i_dupe.timestamp].seen_objs_positions)
        //    {
        //        Debug.Log(obj);
        //        Debug.DrawLine(obj, obj + new Vector3(0.0f, 0.1f, 0.0f), new Color(1.0f, 0.0f, 0.0f), 5.0f);
        //        //snap_markers[0].transform.position = timeline_memory[i_dupe.timestamp].seen_obj_position;
        //    }
        //}
    }

    void Add_To_Buffer(Vector3 i_pos, Quaternion i_local_rot, float i_time)
    {
        Record_Data input_data = new Record_Data();
        input_data.position = i_pos;
        input_data.timestamp = i_time;
        input_data.view_rotation = i_local_rot;
        input_data.is_jumping = false;
        input_data.is_grab_activated = false;

        timeline_memory.Add(timeline_memory.Count, input_data);
    }

    void Add_To_Buffer(Vector3 i_pos, Quaternion i_local_rot, Vector3[] i_objs_pos, float i_time) // Seen object version
    {
        Record_Data input_data = new Record_Data();
        input_data.position = i_pos;
        input_data.timestamp = i_time;
        input_data.view_rotation = i_local_rot;
        input_data.is_jumping = false;
        input_data.is_grab_activated = false;

        input_data.is_obj_seen = true;
        input_data.seen_objs_positions = i_objs_pos;

        timeline_memory.Add(timeline_memory.Count, input_data);
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
        loop_restarted = true;
        foreach (var spawnpoint in moveable_object_spawns)
        {
            GameObject created_obj = Instantiate(box_prefab, spawnpoint,new Quaternion());
            Duplicate_Data input_data = new Duplicate_Data();
            input_data.obj = created_obj;
            input_data.vis = created_obj.GetComponent<Visible_Check>();
            input_data.timestamp = 0;
            input_data.iter_num = iteration_num;

            dupe_objs.Add(input_data);

            foreach (var dupe in duplicate_player_list)
            {
                int index = 0;
                while (index < timeline_memory.Count)
                {
                    if (timeline_memory[index].seen_objs_positions != null)
                    {
                        foreach (var pos in timeline_memory[index].seen_objs_positions)
                        {
                            if (Vector3.Distance(pos, spawnpoint) <= 1.0f)
                            {
                                GameObject new_marker = Instantiate(marker, pos, new Quaternion());
                                snap_markers.Add(new_marker);
                                new_marker.GetComponent<Align_Check>().completion_time = timeline_memory[index].timestamp + (iteration_delay * dupe.iter_num);

                                index = timeline_memory.Count;
                            }
                        }
                    }
                    index++;
                }
            }
            //moveable_objects_vis.Add(created_obj.GetComponent<Visible_Check>());
        }

        //foreach (var marker in snap_markers)
        //{
        //    Destroy(marker.gameObject);
        //}

        //snap_markers.Clear();

        object_timestamp_index = 0;

        GameObject spawned_obj = Instantiate(loop_obj);

        Duplicate_Data dupe_data = new Duplicate_Data();
        dupe_data.obj = spawned_obj;
        dupe_data.timestamp = 1;
        dupe_data.iter_num = iteration_num;
        dupe_data.obj_look_pivot = spawned_obj.GetComponent<Movement_Playback>().this_pivot;
        dupe_data.object_holder = spawned_obj.GetComponentInChildren<Hold_Object>();
        dupe_data.has_grabbed_this_interval = false;

        vis.Add_Camera(spawned_obj.GetComponentInChildren<Camera>());

        duplicate_player_list.Add(dupe_data);
    }
}
