using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alarm_Trip : MonoBehaviour
{
    [SerializeField] private string trigger_tag;

    [SerializeField] private List<Door_Activation> door_list = new List<Door_Activation>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator Activate_Alarm()
    {
        foreach (var door in door_list)
        {
            door.is_door_opening = true;
            yield return new WaitForSeconds(0.5f);
        }

        yield return null;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(trigger_tag))
        {
            StartCoroutine(Activate_Alarm());
        }
    }
}
