using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardsMouse : MonoBehaviour
{
    [SerializeField] private Camera cam;

    void Update()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null)
            {
                Vector3 look_pos = hit.point;
                look_pos.y = transform.position.y; // Make look position height be player height so player isn't looking into ground or sky
                transform.LookAt(look_pos);
            }
        }
    }
}
