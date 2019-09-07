using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visible_Check : MonoBehaviour
{
    [SerializeField] public List<Camera> cams;
    private Plane[] planes;
    private Collider obj_collider;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private Transform origin_position;
    [SerializeField] private bool use_custom_origin = false;


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

        Vector3 target_pos = new Vector3();

        if (use_custom_origin)
        {
            target_pos = origin_position.position;
        }
        else
        {
            target_pos = transform.position;
        }

        is_seen = false;
        foreach (var camera in cams)
        {
            planes = GeometryUtility.CalculateFrustumPlanes(camera);
            if (GeometryUtility.TestPlanesAABB(planes, obj_collider.bounds))
            {
                Vector3 ray_direction = camera.transform.position - target_pos;

                RaycastHit hit;
                Physics.Raycast(target_pos, ray_direction, out hit, obstacleMask);
                Debug.DrawLine(target_pos, hit.point, Color.blue);

                if ((hit.collider != null) && (Vector3.Distance(hit.point,camera.transform.position) <= 1.0f))
                {
                    if ((hit.collider.gameObject.tag == "Player"))
                    {
                        is_seen = true;
                        seen_cams.Add(camera);
                        //Debug.Log(name + " SEEN");
                        //Debug.Log("YES");
                        Debug.DrawRay(target_pos, ray_direction);
                    }
                    else if ((hit.collider.gameObject.tag == "Player_Dupe") && (tag == "Player"))
                    {
                        is_seen = true;
                        seen_cams.Add(camera);
                        tm.Activate_Paradox_Increment(1.0f);
                        Debug.DrawRay(target_pos, ray_direction,Color.red);
                    }
                    else if (hit.collider.gameObject.tag == "Player_Dupe")
                    {
                        is_seen = true;
                        seen_cams.Add(camera);
                        Debug.DrawRay(target_pos, ray_direction);
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
