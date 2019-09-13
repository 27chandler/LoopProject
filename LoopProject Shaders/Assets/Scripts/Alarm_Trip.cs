using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alarm_Trip : MonoBehaviour
{
    private Timeline_Manager tm;
    private Pop_Up_Control puc;
    private int iter_num_last;

    [SerializeField] private Player_Movement pm;
    [SerializeField] private GameObject object_check; //The alarm will trigger when this object is destroyed

    [SerializeField] private string trigger_tag;
    [SerializeField] private List<Door_Activation> door_list = new List<Door_Activation>();

    [SerializeField] private string info_text;
    [SerializeField] private float info_text_display_time;

    private bool is_activated = false;
    private bool has_alarm_started = false;

    private bool is_first_activation = true;


    // Start is called before the first frame update
    void Start()
    {
        tm = GameObject.FindGameObjectWithTag("Timeline_Manager").GetComponent<Timeline_Manager>();
        puc = GameObject.FindGameObjectWithTag("HUD").GetComponent<Pop_Up_Control>();
    }

    // Update is called once per frame
    void Update()
    {
        if (object_check == null)
        {
            object_check = GameObject.FindGameObjectWithTag(trigger_tag);
            if (object_check == null)
            {
                is_activated = true; //Starts sequence of shutting all doors and givig the player the ability to time jump
            }
        }

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

        yield return new WaitForSeconds(2.0f);

        if (is_first_activation)
        {
            puc.Set_Pop_Up(info_text, info_text_display_time);
            is_first_activation = false;
            pm.num_of_jumps++;
        }


        yield return null;
    }


    //private void OnTriggerEnter(Collider other)
    //{
        
    //    foreach (var trigger in trigger_tag)
    //    {
    //        if (other.CompareTag(trigger))
    //        {
    //            is_activated = true;
    //        }
    //    }
    //}
}
