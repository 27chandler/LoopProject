using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport_Blink : MonoBehaviour
{
    [SerializeField] private GameObject obj_to_blink;
    [SerializeField] private float blink_distance;

    [SerializeField] private KeyCode teleport_key;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(teleport_key))
        {
            obj_to_blink.transform.position += (transform.forward * blink_distance);
        }
    }
}
