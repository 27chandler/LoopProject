using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TimePoint_Corrector : MonoBehaviour
{
    private Timeline_Manager tm;

    [SerializeField] private Hold_Object controlled_player_object_holder;

    private Paradox_Logger pl;
    private GameObject held_obj;
    private bool was_holding_obj = false;

    [SerializeField] public List<Door_Activation> safe_doors = new List<Door_Activation>();
    [SerializeField] public List<Object_Data> safe_objects = new List<Object_Data>();

    private Queue<GameObject> door_queue = new Queue<GameObject>();
    private Queue<Hold_Object.Object_Interaction> object_queue = new Queue<Hold_Object.Object_Interaction>();

    private bool is_past = false;

    [Serializable]
    public struct Object_Data
    {
        public GameObject obj;
        public Vector3 pos;
    }

    [Serializable]
    public class Time_Point_Correction_Data
    {
        public int index;
        public bool is_remove;
        public int removal_index;

        public bool is_create;
        public Vector3 create_pos;
        public GameObject obj_type;
    }

    [SerializeField] private List<Time_Point_Correction_Data> time_point_corrections = new List<Time_Point_Correction_Data>();
    // Start is called before the first frame update
    void Start()
    {
        tm = GetComponent<Timeline_Manager>();
        pl = GetComponent<Paradox_Logger>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controlled_player_object_holder.is_holding)
        {
            was_holding_obj = true;
        }

        if (tm.modified_current_time < tm.time_of_present)
        {
            is_past = true;
        }
        else
        {
            is_past = false;
        }

        if (is_past)
        {
            Log_Objects();
        }
        else
        {
            Reset_Safe_Objects();
            controlled_player_object_holder.objects.Clear();
            controlled_player_object_holder.doors.Clear();
        }

        if (safe_doors.Count > 0)
        {
            Door_Correction();
            safe_doors.Clear();
        }

        if (safe_objects.Count > 0)
        {
            Object_Correction();
            safe_objects.Clear();
        }
        

        if ((door_queue.Count > 0) && (pl.is_done))
        {
            pl.is_done = false;
            pl.Activate_Custom_Check(door_queue.Dequeue());
        }
        else if ((object_queue.Count > 0) && (pl.is_done))
        {
            pl.is_done = false;
            pl.Activate_Object_Check(object_queue.Dequeue());
        }

        held_obj = controlled_player_object_holder.grabbed_item_obj;
    }

    private void Log_Objects()
    {
        // Doors section
        if (controlled_player_object_holder.doors.Count >= 1)
        {
            for (int i = 0; i < controlled_player_object_holder.doors.Count; i++)
            {
                door_queue.Enqueue(controlled_player_object_holder.doors[i].gameObject);
            }
            controlled_player_object_holder.doors.Clear();
        }

        // Objects section
        if (controlled_player_object_holder.objects.Count >= 1)
        {
            for (int i = 0; i < controlled_player_object_holder.objects.Count; i++)
            {
                object_queue.Enqueue(controlled_player_object_holder.objects[i]);
            }
            controlled_player_object_holder.objects.Clear();
        }

        //
    }

    public void Reset_Safe_Objects()
    {
        safe_doors.Clear();
        safe_objects.Clear();
    }

    private void Door_Correction()
    {
        //Debug.Log("Running door calc");
        for (int i = 0; i < tm.time_point_list.Count; i++)
        {
            if (tm.time_point_list[i].normalized_timestamp >= tm.modified_current_time)
            {
                if (safe_doors.Count > 0)
                {
                    foreach (var door in safe_doors)
                    {
                        for (int j = 0; j < tm.time_point_list[i].door_data_states.Count; j++)
                        {

                            Door_Activation temp_door = door.GetComponentInChildren<Door_Activation>();
                            if (temp_door == tm.time_point_list[i].door_data_states[j].door_activation)
                            {
                                //Debug.Log("MATCHING DOOR");
                                tm.time_point_list[i].door_data_states[j].last_state = temp_door.is_open;
                            }

                        }
                    }
                }
            }
        }
    }

    private void Object_Correction()
    {
        Debug.Log("Running object calc");
        for (int i = 0; i < tm.time_point_list.Count; i++)
        {
            if (tm.time_point_list[i].normalized_timestamp >= tm.modified_current_time)
            {
                if (safe_objects.Count > 0)
                {
                    foreach (var data in safe_objects)
                    {
                        bool has_match = false;
                        Time_Point_Correction_Data temp_correction = new Time_Point_Correction_Data();
                        temp_correction.index = i;
                        for (int j = 0; j < tm.time_point_list[i].object_locations.Count; j++)
                        {
                            Pickup_Loop temp_pickup = data.obj.GetComponent<Pickup_Loop>();
                            if (Vector3.Distance(data.pos, tm.time_point_list[i].object_locations[j].position) < 0.5f)
                            {
                                if (tm.time_point_list[i].object_locations[j].obj.tag == "Blue_Box")
                                {
                                    Debug.Log("BLUE BOX");
                                }
                                //Debug.Log("MATCHING OBJECT");
                                has_match = true;

                                temp_correction.is_remove = true;
                                temp_correction.removal_index = j;

                                tm.time_point_list[i].object_locations.RemoveAt(j);
                                Debug.Log("Object Removed");
                                held_obj = temp_pickup.gameObject;

                                j = tm.time_point_list[i].object_locations.Count;
                            }

                        }

                        if (!has_match)
                        {
                            //for (int j = 0; j < tm.time_point_list[i].object_locations.Count; j++)
                            //{
                            //    Debug.Log("");
                            //    if (Vector3.Distance(data.pos, tm.time_point_list[i].object_locations[j].position) < 0.4f)
                            //    {
                                    
                            //    }

                            //}

                            foreach (var obj_type in tm.object_type_list)
                            {
                                if (obj_type.tag == data.obj.tag)
                                {
                                    temp_correction.is_create = true;
                                    temp_correction.obj_type = obj_type.obj;
                                    temp_correction.create_pos = data.pos;

                                    //Debug.Log("NO MATCH SPAWN");
                                    Debug.Log("Object Added");
                                    Timeline_Manager.Object_Spawns temp_spawn = new Timeline_Manager.Object_Spawns();
                                    temp_spawn.position = data.pos;
                                    temp_spawn.obj = obj_type.obj;

                                    tm.time_point_list[i].object_locations.Add(temp_spawn);
                                    //temp_spawn = tm.time_point_list[i].object_locations[j];
                                }
                            }
                        }

                        time_point_corrections.Add(temp_correction);
                    }
                }
            }
        }
        Debug.Log("Finished check");
        Debug.Log("==========================");
    }

    public void Add_Door_To_Safety(Door_Activation i_door)
    {
        bool is_door_already_present = false;

        foreach (var door in safe_doors)
        {
            if (door == i_door)
            {
                is_door_already_present = true;
            }
        }

        if (!is_door_already_present)
        {
            safe_doors.Add(i_door);
        }
    }

    public void Add_Pickup_To_Safety(GameObject i_obj, Vector3 i_pos)
    {
        bool is_object_already_present = false;

        foreach (var data in safe_objects)
        {
            if (data.obj == i_obj)
            {
                is_object_already_present = true;
            }
        }

        if (!is_object_already_present)
        {
            Object_Data temp_data = new Object_Data();
            temp_data.obj = i_obj;
            temp_data.pos = i_pos;

            safe_objects.Add(temp_data);
        }
    }
}
