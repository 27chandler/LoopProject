using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hold_Object : MonoBehaviour
{
    public bool trigger_grab = false;
    public bool is_holding = false;
    private Pickup_Loop grabbed_item;
    public GameObject grabbed_item_obj;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (trigger_grab)
        {
            trigger_grab = false;
            if (!is_holding)
            {
                RaycastHit hit;
                Physics.Raycast(transform.position, transform.forward, out hit, 5.0f);

                if (hit.collider != null)
                {
                    Pickup_Loop pickup = hit.collider.GetComponent<Pickup_Loop>();
                    grabbed_item_obj = pickup.gameObject;
                    if (pickup != null)
                    {
                        is_holding = true;
                        pickup.is_picked_up = true;
                        grabbed_item = pickup;
                    }

                }
            }
            else
            {
                if (grabbed_item != null)
                {
                    grabbed_item.is_picked_up = false;
                }
                grabbed_item = null;
                is_holding = false;
            }
        }

        if (grabbed_item != null)
        {
            grabbed_item.hold_pos = transform.position + (transform.forward * 3.0f);
        }


    }
}
