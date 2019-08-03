using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_State_Check : MonoBehaviour
{
    [SerializeField] private MeshRenderer renderer;
    private GameObject satisfying_obj;
    private Hold_Object holder;
    [SerializeField] public float completion_time;
    [SerializeField] public float current_time;
    private int health;
    [SerializeField] private int max_health;

    private bool is_player_altering = false;
    private Timeline_Manager tm;


    private Door_Activation door_activation;
    public bool expected_state = false;
    // Start is called before the first frame update
    void Start()
    {
        renderer.enabled = false;
        health = max_health;

        holder = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetComponentInChildren<Hold_Object>();
        tm = GameObject.FindGameObjectWithTag("Timeline_Manager").GetComponent<Timeline_Manager>();




        //Collider[] found_objs = Physics.OverlapSphere(transform.position, 3.0f);

        //foreach (Collider collider in found_objs)
        //{
        //    if (door_activation == null)
        //    {
        //        door_activation = collider.gameObject.GetComponentInChildren<Door_Activation>();
        //    }
        //}
    }

    public void Set_Linked_Door(Door_Activation i_door_activation)
    {
        door_activation = i_door_activation;
    }

    // Update is called once per frame
    void Update()
    {

        if (completion_time > (tm.iteration_num * tm.iteration_delay))
        {
            Destroy(gameObject);
        }

        if (expected_state == door_activation.is_open)
        {
            renderer.enabled = false;
        }
        else
        {
            renderer.enabled = true;
        }


        if (current_time >= completion_time - 0.1f)
        {
            if (expected_state == door_activation.is_open)
            {
                Destroy(gameObject);
            }
            else
            {
                health--;
            }
        }

        if (health <= 0)
        {
            Debug.Log("FAILED");
            Destroy(this.gameObject);
        }

    }
}

