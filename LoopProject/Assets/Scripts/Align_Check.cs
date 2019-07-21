using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Align_Check : MonoBehaviour
{
    [SerializeField] private MeshRenderer renderer;
    [SerializeField] public float completion_time;
    [SerializeField] public float current_time;
    private int health;
    [SerializeField] private int max_health;
    // Start is called before the first frame update
    void Start()
    {
        renderer.enabled = false;
        health = max_health;
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] found_objs = Physics.OverlapSphere(transform.position, 1.0f);

        bool is_obj_present = false;

        foreach (Collider collider in found_objs)
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



        if (current_time >= completion_time-0.5f)
        {
            if (is_obj_present)
            {
                Destroy(this.gameObject);
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
