using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hold_Object : MonoBehaviour
{
    public bool trigger_grab = false;
    public bool is_holding = false;
    public Pickup_Loop grabbed_item;
    public GameObject grabbed_item_obj;
    [SerializeField] LayerMask layer_collision_raycasting;

    public List<Door_Activation> doors = new List<Door_Activation>();
    public List<Object_Interaction> objects = new List<Object_Interaction>();

    public struct Object_Interaction
    {
        public GameObject objects;
        public bool grab_state;
    }




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit, 5.0f, layer_collision_raycasting);

        if (hit.collider != null)
        {
            Click_Button click_button = hit.collider.GetComponent<Click_Button>();

            if (click_button != null)
            {
                click_button.is_highlighted = true;
            }
        }

        if (trigger_grab)
        {
            //Debug.Log("Trigger Grab Ran");
            trigger_grab = false;
            if (!is_holding)
            {
                //Debug.Log("is not holding");
               // Debug.Log("HIT: " + hit.collider);

                if (hit.collider != null)
                {
                    Pickup_Loop pickup = hit.collider.GetComponent<Pickup_Loop>();
                    Click_Button click_button = hit.collider.GetComponent<Click_Button>();

                    if (pickup != null)
                    {
                        //Debug.Log("Picked up");
                        grabbed_item_obj = pickup.gameObject;
                        is_holding = true;
                        pickup.is_picked_up = true;
                        grabbed_item = pickup;

                        Object_Interaction temp_obj_interaction = new Object_Interaction();
                        temp_obj_interaction.objects = pickup.gameObject;
                        temp_obj_interaction.grab_state = true;

                        objects.Add(temp_obj_interaction);
                    }
                    else if (click_button != null)
                    {
                        click_button.is_activated = !click_button.is_activated;

                        foreach (var door in click_button.doors)
                        {
                            doors.Add(door);
                        }
                        
                    }

                }
            }
            else
            {
                Physics.Raycast(grabbed_item.transform.position, -transform.forward, out hit);

                if ((hit.collider.gameObject.CompareTag("Player")) || (hit.collider.gameObject.CompareTag("Player_Dupe")))
                {
                    Object_Interaction temp_obj_interaction = new Object_Interaction();
                    temp_obj_interaction.objects = grabbed_item.gameObject;
                    temp_obj_interaction.grab_state = false;

                    objects.Add(temp_obj_interaction);
                    //Debug.Log("is holding");
                    if (grabbed_item != null)
                    {
                        //Debug.Log("Dropped");
                        grabbed_item.is_picked_up = false;
                    }
                    grabbed_item = null;
                    is_holding = false;
                }

            }
        }

        if (grabbed_item != null)
        {
            grabbed_item.hold_pos = transform.position + (transform.forward * 1.5f);
            grabbed_item.object_holding_this = this.gameObject;
        }


    }

    public void Drop_Item()
    {
        Object_Interaction temp_obj_interaction = new Object_Interaction();
        temp_obj_interaction.objects = grabbed_item.gameObject;
        temp_obj_interaction.grab_state = false;

        objects.Add(temp_obj_interaction);
        grabbed_item.is_picked_up = false;
        grabbed_item = null;
        grabbed_item_obj = null;
        is_holding = false;
    }
}
