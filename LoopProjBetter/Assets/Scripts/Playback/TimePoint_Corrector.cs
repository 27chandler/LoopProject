using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimePoint_Corrector : MonoBehaviour
{
    private Timeline_Manager tm;

    [SerializeField] private Hold_Object controlled_player_object_holder;

    private Paradox_Logger pl;
    private GameObject held_obj;
    private bool was_holding_obj = false;

    [SerializeField] public List<Door_Activation> safe_doors = new List<Door_Activation>();
    [SerializeField] public List<GameObject> safe_objects = new List<GameObject>();

    private Queue<GameObject> door_queue = new Queue<GameObject>();

    private bool is_past = false;
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
            pl.Activate_Custom_Check(controlled_player_object_holder.objects[0]);
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
        Debug.Log("Running door calc");
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
                                Debug.Log("MATCHING DOOR");
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
                    foreach (var obj in safe_objects)
                    {
                        bool has_match = false;
                        for (int j = 0; j < tm.time_point_list[i].object_locations.Count; j++)
                        {
                            Pickup_Loop temp_pickup = obj.GetComponent<Pickup_Loop>();
                            if (Vector3.Distance(temp_pickup.pickup_pos, tm.time_point_list[i].object_locations[j].position) < 0.5f)
                            {
                                Debug.Log("MATCHING OBJECT");
                                has_match = true;

                                tm.time_point_list[i].object_locations.RemoveAt(j);
                                held_obj = temp_pickup.gameObject;

                                j = tm.time_point_list[i].object_locations.Count;
                            }

                        }

                        if (!has_match)
                        {
                            //if ((held_obj != controlled_player_object_holder.grabbed_item_obj) && (controlled_player_object_holder.grabbed_item_obj == null))
                            //{
                                foreach (var obj_type in tm.object_type_list)
                                {
                                    if (obj_type.tag == obj.tag)
                                    {
                                        Debug.Log("NO MATCH SPAWN");

                                        Timeline_Manager.Object_Spawns temp_spawn;
                                        temp_spawn.position = obj.transform.position;
                                        temp_spawn.obj = obj_type.obj;

                                        tm.time_point_list[i].object_locations.Add(temp_spawn);
                                        //temp_spawn = tm.time_point_list[i].object_locations[j];
                                    }


                                }
                            //}
                        }
                    }
                }
            }
        }
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

    public void Add_Pickup_To_Safety(GameObject i_obj)
    {
        bool is_object_already_present = false;

        foreach (var obj in safe_objects)
        {
            if (obj == i_obj)
            {
                is_object_already_present = true;
            }
        }

        if (!is_object_already_present)
        {
            safe_objects.Add(i_obj);
        }
    }
}
