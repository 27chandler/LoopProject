﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visible_Check : MonoBehaviour
{
    [SerializeField] private List<Camera> cams;
    private Plane[] planes;
    private Collider obj_collider;

    public bool is_seen = false;
    // Start is called before the first frame update
    void Start()
    {
        obj_collider = GetComponent<Collider>();
    }

    public void Add_Camera(Camera i_cam)
    {
        cams.Add(i_cam);
    }

    // Update is called once per frame
    void Update()
    {
        is_seen = false;
        foreach (var camera in cams)
        {
            planes = GeometryUtility.CalculateFrustumPlanes(camera);
            if (GeometryUtility.TestPlanesAABB(planes, obj_collider.bounds))
            {
                Vector3 ray_direction = camera.transform.position - transform.position;

                RaycastHit hit;
                Physics.Raycast(transform.position, ray_direction, out hit);
                Debug.DrawRay(transform.position, ray_direction);
                if (hit.transform != null)
                {
                    if (hit.transform.parent == camera.transform.parent)
                    {
                        Debug.Log(gameObject.name + " has been detected!");
                        is_seen = true;
                    }
                }


            }
        }

    }
}
