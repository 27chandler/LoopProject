using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Receptor_Control : MonoBehaviour
{
    [SerializeField] private float recept_range = 100.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Vector3 Recept(GameObject receptor_object)
    {
        GameObject[] receptors = GameObject.FindGameObjectsWithTag("Receptor");

        float receptor_distance = recept_range;
        Transform nearest_receptor = null;

        foreach (var obj in receptors)
        {
            Receptor_Pedestal temp_recept = obj.GetComponentInChildren<Receptor_Pedestal>();
            if (temp_recept.receptor_tag == receptor_object.tag)
            {
                // Check if closest
                float distance = Vector3.Distance(this.transform.position, obj.transform.position);
                if (distance < receptor_distance)
                {
                    // Check if space is free
                    if (!Physics.CheckSphere(temp_recept.origin_transform.position,0.01f))
                    {
                        nearest_receptor = temp_recept.origin_transform;
                        receptor_distance = distance;
                    }
                }
            }
        }

        return nearest_receptor.position;
    }
}
