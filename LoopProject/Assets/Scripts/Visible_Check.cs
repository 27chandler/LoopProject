using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visible_Check : MonoBehaviour
{
    [SerializeField] private Camera cam;
    private Plane[] planes;
    private Collider obj_collider;
    // Start is called before the first frame update
    void Start()
    {
        obj_collider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        planes = GeometryUtility.CalculateFrustumPlanes(cam);
        if (GeometryUtility.TestPlanesAABB(planes, obj_collider.bounds))
        {
            Vector3 ray_direction = cam.transform.position - transform.position;

            RaycastHit hit;
            Physics.Raycast(transform.position, ray_direction, out hit);
            Debug.DrawRay(transform.position, ray_direction);
            if (hit.transform.parent == cam.transform.parent)
            {
                Debug.Log(gameObject.name + " has been detected!");
            }
            
        }
    }
}
