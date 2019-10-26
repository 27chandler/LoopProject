using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reconstructor : MonoBehaviour
{
    private Timeline_Manager tm;

    [SerializeField] private Collider input_zone;
    [SerializeField] private Collider output_zone;

    [SerializeField] private Click_Button button;

    [SerializeField] private GameObject object_template;

    [SerializeField] private bool is_activated = false;
    // Start is called before the first frame update
    void Start()
    {
        tm = GameObject.FindGameObjectWithTag("Timeline_Manager").GetComponent<Timeline_Manager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (button.is_activated)
        {
            is_activated = true;
            button.is_activated = false;
        }

        if (is_activated)
        {
            is_activated = false;

            Collider[] colliders = Physics.OverlapSphere(input_zone.transform.position, 0.1f);

            bool has_converted = false;
            for (int i = 0; i < colliders.Length; i++)
            {
                if (!has_converted)
                {
                    if ((colliders[i] != null) && (!colliders[i].isTrigger))
                    {
                        tm.Create_Object(object_template.tag, output_zone.transform.position);
                        Destroy(colliders[i].gameObject);
                        has_converted = true;
                    }
                }
            }
        }
    }
}
