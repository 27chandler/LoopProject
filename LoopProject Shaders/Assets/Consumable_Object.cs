using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumable_Object : MonoBehaviour
{
    private Pickup_Loop pl;
    private Player_Movement pm;
    private Hold_Object object_holder;
    // Start is called before the first frame update
    void Start()
    {
        pl = GetComponent<Pickup_Loop>();
        pm = GameObject.FindGameObjectWithTag("Player").GetComponent<Player_Movement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pl.object_holding_this != null)
        {
            object_holder = pl.object_holding_this.GetComponent<Hold_Object>();
            object_holder.is_holding = false;
            object_holder.grabbed_item = null;
            object_holder.grabbed_item_obj = null;
            


            // Activate Action
            switch(tag)
            {
                case "Time_Device":
                    {
                        Add_Time_Jump();
                        break;
                    }
            }

            Destroy(gameObject);
        }


    }

    void Add_Time_Jump()
    {
        if (object_holder.transform.parent.CompareTag("Player"))
        {
            pm.num_of_jumps++;
            Debug.Log("Added a jump");
        }
    }
}
