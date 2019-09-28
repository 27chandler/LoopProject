using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alarm_Trip : MonoBehaviour
{
    private Timeline_Manager tm;
    private Pop_Up_Control puc;
    private int iter_num_last;

    private float activation_time;

    [SerializeField] private Player_Movement pm;
    [SerializeField] private GameObject object_check; //The alarm will trigger when this object is destroyed
    private int detection_counter = 0;

    [SerializeField] private string trigger_tag;
    [SerializeField] private List<Door_Activation> door_list = new List<Door_Activation>();

    [SerializeField] private string info_text;
    [SerializeField] private float info_text_display_time;

    [SerializeField] private string warning_text;
    [SerializeField] private float warning_text_display_time;

    [SerializeField] private Portal current_level_portal;
    [SerializeField] private Portal next_level_portal;

    [SerializeField] private bool is_activated = false;
    [SerializeField] private bool has_alarm_started = false;

    private bool is_first_activation = true;

    private bool has_time_device = false;


    // Start is called before the first frame update
    void Start()
    {
        tm = GameObject.FindGameObjectWithTag("Timeline_Manager").GetComponent<Timeline_Manager>();
        puc = GameObject.FindGameObjectWithTag("HUD").GetComponent<Pop_Up_Control>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!is_activated)
        {
            if (!Physics.CheckSphere(transform.position, 0.1f))
            {
                detection_counter++;
            }
            else
            {
                detection_counter = 0;
            }

            if (detection_counter >= 10)
            {
                is_activated = true; //Starts sequence of shutting all doors and givig the player the ability to time jump
            }
        }


        if (tm.modified_current_time < activation_time)
        {
            if (has_time_device)
            {
                StartCoroutine(Show_Warning());
                has_time_device = false;
            }

            if (is_activated)
            {
                foreach (var door in door_list)
                {
                    door.is_door_opening = false;
                }
            }

            iter_num_last = tm.iteration_num;
            is_activated = false;
            has_alarm_started = false;
            detection_counter = 0;
        }

        if (is_activated && !has_alarm_started)
        {
            has_alarm_started = true;
            StartCoroutine(Activate_Alarm());
        }
    }

    private IEnumerator Show_Warning()
    {
        yield return new WaitForSeconds(0.5f);

        puc.Set_Pop_Up(warning_text, warning_text_display_time);

        yield return null;
    }

    private IEnumerator Activate_Alarm()
    {
        activation_time = tm.modified_current_time;

        foreach (var door in door_list)
        {
            door.is_door_opening = true;
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(2.0f);

        if (is_first_activation)
        {
            puc.Set_Pop_Up(info_text, info_text_display_time);
            next_level_portal.is_activated = true;
            next_level_portal.is_open = true;
            is_first_activation = false;

            yield return new WaitForSeconds(info_text_display_time);
            has_time_device = true;
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
