using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Look_At_Tag : MonoBehaviour
{
    [SerializeField] private string tag_to_look_at;
    private GameObject target_obj;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (target_obj != null)
        {
            transform.LookAt(target_obj.transform);
        }
        else
        {
            target_obj = GameObject.FindGameObjectWithTag(tag_to_look_at);
        }
        
    }
}
