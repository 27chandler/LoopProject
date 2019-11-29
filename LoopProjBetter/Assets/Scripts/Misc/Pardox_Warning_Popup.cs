using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pardox_Warning_Popup : MonoBehaviour
{
    [SerializeField] Text warning_text;
    [SerializeField] GameObject warning_indicator;
    [SerializeField] float warning_indicator_float_distance;
    public List<Paradox_Warning> paradoxes = new List<Paradox_Warning>();
    private List<GameObject> warning_indicator_list = new List<GameObject>();

    public struct Paradox_Warning
    {
        public string message;
        public bool object_present_state;
        public float time;
        public string tag;
        public Vector3 position;

        public GameObject warning_indicator;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Add_Warning(string i_message, bool i_state, string i_obj_tag, float i_time, Vector3 i_pos)
    {
        GameObject temp_indicator = Instantiate(warning_indicator, i_pos + (Vector3.up * warning_indicator_float_distance), new Quaternion());
        warning_indicator_list.Add(temp_indicator);

        Paradox_Warning temp_warning = new Paradox_Warning();

        temp_warning.message = i_message;
        temp_warning.object_present_state = i_state;
        temp_warning.time = i_time;
        temp_warning.tag = i_obj_tag;
        temp_warning.position = i_pos;
        temp_warning.warning_indicator = temp_indicator;

        paradoxes.Add(temp_warning);

    }

    // Update is called once per frame
    void Update()
    {
        warning_text.text = "";
        foreach (var paradox in paradoxes)
        {
            warning_text.text += "\n" + "Warning: Paradox imminent at: " + paradox.time + " with object " + paradox.tag;
        }

        Check_Paradoxes();
    }

    void Check_Paradoxes()
    {
        List<int> paradoxes_to_remove = new List<int>();
        for (int i = 0; i < paradoxes.Count; i++)
        {
            Collider[] col_array = Physics.OverlapSphere(paradoxes[i].position, 0.3f);

            bool is_obj_present = false;
            foreach (Collider col in col_array)
            {
                Pickup_Loop temp_pickup = col.GetComponent<Pickup_Loop>();
                bool is_dropped = true;
                if (temp_pickup != null)
                {
                    is_dropped = !temp_pickup.is_picked_up;
                }
                if ((col.tag == paradoxes[i].tag) && (is_dropped))
                {
                    is_obj_present = true;
                }
            }

            if (paradoxes[i].object_present_state == is_obj_present)
            {
                paradoxes_to_remove.Add(i);
            }
        }

        foreach (var index in paradoxes_to_remove)
        {
            Destroy(paradoxes[index].warning_indicator);
            paradoxes.RemoveAt(index);
        }
    }
}
