using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock_Display : MonoBehaviour
{
    [SerializeField] private Timeline_Manager tm;
    [SerializeField] private float cycle_time = 0.0f;
    [SerializeField] private float rotation = 0;
    // Start is called before the first frame update
    void Start()
    {
        tm = GameObject.FindGameObjectWithTag("Timeline_Manager").GetComponent<Timeline_Manager>();
    }

    // Update is called once per frame
    void Update()
    {
        rotation = (tm.modified_current_time * 360.0f) / cycle_time;
        transform.localEulerAngles = new Vector3(0.0f, 0.0f, -rotation);
    }
}
