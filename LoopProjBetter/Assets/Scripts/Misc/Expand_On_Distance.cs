using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Expand_On_Distance : MonoBehaviour
{
    private Vector3 original_scale;
    [SerializeField] private float size_on_screen;
    [SerializeField] private Transform object_to_scale_to;
    [SerializeField] private string tag_to_scale_to;
    // Start is called before the first frame update
    void Start()
    {
        original_scale = transform.localScale;

        if (tag_to_scale_to != null)
        {
            object_to_scale_to = GameObject.FindGameObjectWithTag(tag_to_scale_to).transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = original_scale * (Vector3.Distance(object_to_scale_to.position, transform.position)*(size_on_screen));
    }
}
