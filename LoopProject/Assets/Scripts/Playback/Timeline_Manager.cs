using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timeline_Manager : MonoBehaviour
{
    [SerializeField] private float iteration_delay;
    [SerializeField] private GameObject loop_obj;
    [SerializeField] private GameObject original_obj;
    private GameObject previous_obj;

    private Movement_Playback new_obj;
    private Movement_Playback old_obj;

    private float current_time = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        current_time = iteration_delay;
    }

    // Update is called once per frame
    void Update()
    {
        current_time += Time.deltaTime;

        if (current_time >= iteration_delay)
        {
            current_time = 0.0f;
            Restart_Loop();
        }
    }

    void Restart_Loop()
    {
        GameObject spawned_obj = Instantiate(loop_obj);
        new_obj = spawned_obj.GetComponent<Movement_Playback>();

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

        old_obj = new_obj;
    }
}
