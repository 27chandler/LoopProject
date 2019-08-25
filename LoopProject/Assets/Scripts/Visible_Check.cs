﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visible_Check : MonoBehaviour
{
    [SerializeField] public List<Camera> cams;
    private Plane[] planes;
    private Collider obj_collider;
    [SerializeField] private LayerMask obstacleMask;

    public bool is_seen = false;
    public List<Camera> seen_cams = new List<Camera>();

    private Timeline_Manager tm;
    // Start is called before the first frame update
    void Start()
    {
        obj_collider = GetComponent<Collider>();
        tm = GameObject.FindGameObjectWithTag("Timeline_Manager").GetComponent<Timeline_Manager>();
    }

    public void Add_Camera(Camera i_cam)
    {
        cams.Add(i_cam);
    }

    // Update is called once per frame
    void Update()
    {
        seen_cams.Clear();

        is_seen = false;
        foreach (var camera in cams)
        {
            planes = GeometryUtility.CalculateFrustumPlanes(camera);
            if (GeometryUtility.TestPlanesAABB(planes, obj_collider.bounds))
            {
                Vector3 ray_direction = camera.transform.position - transform.position;

                RaycastHit hit;
                Physics.Raycast(transform.position, ray_direction, out hit, obstacleMask);
                Debug.DrawRay(transform.position, ray_direction);
                //Debug.Log(hit.collider.gameObject.transform.parent.name + " has been detected!");
                if (hit.collider != null)
                {
                    if ((hit.collider.gameObject.tag == "Player"))
                    {
                        is_seen = true;
                        seen_cams.Add(camera);
                        //Debug.Log(name + " SEEN");
                        //Debug.Log("YES");
                    }
                    else if ((hit.collider.gameObject.tag == "Player_Dupe") && (tag == "Player"))
                    {
                        tm.health -= 10.0f;
                    }

                    //if (hit.transform.parent == camera.transform.parent)
                    //{
                    //    is_seen = true;
                    //    seen_cams.Add(camera);
                    //}
                }


            }
        }

    }
}
