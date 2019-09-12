using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Time_Sink : MonoBehaviour
{
    private GameObject player;
    private Timeline_Manager tm;

    [SerializeField] private float time_speed;
    [SerializeField] private float effect_size = 50.0f;
    [SerializeField] private bool is_inverted = false;
    [SerializeField] private float distortion_strength = 2.0f;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        tm = GameObject.FindGameObjectWithTag("Timeline_Manager").GetComponent<Timeline_Manager>();
    }

    // Update is called once per frame
    void Update()
    {
        time_speed = Vector3.Distance(transform.position, player.transform.position);
        if (is_inverted)
        {
            time_speed = (1.0f - (Mathf.Clamp(time_speed, 1.0f, effect_size) / (effect_size/ distortion_strength))) + distortion_strength;
        }
        else
        {
            time_speed = Mathf.Clamp(time_speed, 1.0f, effect_size) / effect_size;
        }

        tm.time_speed = time_speed;
    }
}
