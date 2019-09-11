using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alarm_Trip : MonoBehaviour
{
    private Timeline_Manager tm;
    private int iter_num_last;

    [SerializeField] private List<string> trigger_tag = new List<string>();
    [SerializeField] private List<Door_Activation> door_list = new List<Door_Activation>();

    private bool is_activated = false;
    private bool has_alarm_started = false;


    // Start is called before the first frame update
    void Start()
    {
        tm = GameObject.FindGameObjectWithTag("Timeline_Manager").GetComponent<Timeline_Manager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (tm.iteration_num != iter_num_last)
        {
            iter_num_last = tm.iteration_num;
            is_activated = false;
            has_alarm_started = false;
        }

        if (is_activated && !has_alarm_started)
        {
            has_alarm_started = true;
            StartCoroutine(Activate_Alarm());
        }
    }

    private IEnumerator Activate_Alarm()
    {
        foreach (var door in door_list)
        {
            door.is_door_opening = true;
            yield return new WaitForSeconds(0.5f);
        }

        yield return null;
    }


    private void OnTriggerEnter(Collider other)
    {
        
        foreach (var trigger in trigger_tag)
        {
            if (other.CompareTag(trigger))
            {
                is_activated = true;
            }
        }
    }
}
