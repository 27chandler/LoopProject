using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Click_Button : MonoBehaviour
{
    private Timeline_Manager tm;
    private int iter_num_last;

    public bool is_activated = false;

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
        }
    }
}
