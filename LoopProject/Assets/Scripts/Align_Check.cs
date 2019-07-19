using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Align_Check : MonoBehaviour
{
    [SerializeField] private MeshRenderer renderer;
    // Start is called before the first frame update
    void Start()
    {
        renderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] found_objs = Physics.OverlapSphere(transform.position, 1.0f);

        bool is_obj_present = false;

        foreach (var collider in found_objs)
        {
            if (collider.name == "Box(Clone)" || collider.name == "Box")
            {
                is_obj_present = true;
            }
        }

        if (is_obj_present)
        {
            renderer.enabled = false;
        }
        else
        {
            renderer.enabled = true;
        }
    }
}
