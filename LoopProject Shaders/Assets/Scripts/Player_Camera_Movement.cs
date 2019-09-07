using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Camera_Movement : MonoBehaviour
{
    private float rotation_y = 0.0f;
    private float rotation_x = 0.0f;

    [SerializeField] private Transform rotation_origin;
    private Movement_Playback mp;
    public bool is_controlled = false;

    [Space]
    [SerializeField] private GameObject fov_prefab;
    private Field_Of_View fov;
    // Start is called before the first frame update
    void Start()
    {
        if (!is_controlled)
        {
            GameObject this_fov = Instantiate(fov_prefab);
            this_fov.GetComponent<Field_Of_View>().target_transform = transform.parent;
            fov = this_fov.GetComponent<Field_Of_View>();
        }
        else
        {
            if (rotation_origin == null)
            {
                rotation_origin = this.transform;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (is_controlled)
        {
            rotation_y += Input.GetAxis("Mouse Y");
            rotation_x += Input.GetAxis("Mouse X");
            rotation_origin.localEulerAngles = new Vector3(-rotation_y, rotation_x, transform.localEulerAngles.z);
        }
        else
        {
            fov.Set_View_Dir(transform.forward);
        }
    }
}
