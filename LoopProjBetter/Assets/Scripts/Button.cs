using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    [SerializeField] private Collider trigger_area;
    [SerializeField] private List<Door_Activation> doors = new List<Door_Activation>();
    private Timeline_Manager tm;

    public bool is_activated = false;
    private int iter_num_last;
    // Start is called before the first frame update
    void Start()
    {
        tm = GameObject.FindGameObjectWithTag("Timeline_Manager").GetComponent<Timeline_Manager>();

        foreach (var door in doors)
        {
            door.Add_Object_Button(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (tm.iteration_num != iter_num_last)
        {
            iter_num_last = tm.iteration_num;
            is_activated = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Box(Clone)")
        {
            is_activated = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Box(Clone)")
        {
            is_activated = false;
        }
    }
}
