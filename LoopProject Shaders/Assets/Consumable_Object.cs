using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumable_Object : MonoBehaviour
{
    private Pickup_Loop pl;
    // Start is called before the first frame update
    void Start()
    {
        pl = GetComponent<Pickup_Loop>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pl.object_holding_this != null)
        {
            Hold_Object object_holder = pl.object_holding_this.GetComponent<Hold_Object>();
            object_holder.is_holding = false;
            object_holder.grabbed_item = null;
            object_holder.grabbed_item_obj = null;
            Destroy(gameObject);
        }
    }
}
