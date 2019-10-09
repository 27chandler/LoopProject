using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pop_Up_Control : MonoBehaviour
{
    [SerializeField] private Text pop_up_text;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Set_Pop_Up(string i_text, float i_time)
    {
        StartCoroutine(Display_Text(i_text, i_time));
    }

    private IEnumerator Display_Text(string i_text,float i_time)
    {
        pop_up_text.text = i_text;
        yield return new WaitForSeconds(i_time);
        pop_up_text.text = "";
        yield return null;
    }
}
