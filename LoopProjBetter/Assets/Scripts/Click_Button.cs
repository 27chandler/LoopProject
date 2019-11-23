using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Click_Button : MonoBehaviour
{
    private Timeline_Manager tm;
    private int iter_num_last;

    private Material mat_normal;
    private MeshRenderer mr;
    [SerializeField] private Material mat_highlighted;
    [SerializeField] private Material mat_clicked;

    [SerializeField] private List<Door_Activation> doors = new List<Door_Activation>();


    public bool is_activated = false;
    public bool is_highlighted = false;
    [SerializeField] private bool is_auto_reset = false;

    // Start is called before the first frame update
    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        tm = GameObject.FindGameObjectWithTag("Timeline_Manager").GetComponent<Timeline_Manager>();
        mat_normal = mr.material;
    }

    // Update is called once per frame
    void Update()
    {
        if (is_activated)
        {
            is_activated = false;

            foreach (var door in doors)
            {
                door.is_door_opening = !door.is_door_opening;
            }
        }

        if (tm.iteration_num != iter_num_last)
        {
            iter_num_last = tm.iteration_num;
            is_activated = false;
        }

        if (is_highlighted)
        {
            mr.material = mat_highlighted;
            is_highlighted = false;
        }
        else if (is_activated)
        {
            mr.material = mat_clicked;
        }
        else
        {
            mr.material = mat_normal;
        }



    }
}
