using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Paradox_Effect : MonoBehaviour
{
    [SerializeField] PostProcessProfile profile;

    private Timeline_Manager tm;
    private Camera player_cam;

    private Vignette v;

    // Start is called before the first frame update
    void Start()
    {
        player_cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        tm = GameObject.FindGameObjectWithTag("Timeline_Manager").GetComponent<Timeline_Manager>();

        v = profile.GetSetting<Vignette>();
    }

    // Update is called once per frame
    void Update()
    {
        v.intensity.Override((100.0f - tm.health)/100.0f);
    }

    private void OnDestroy()
    {
        v.intensity.Override(0.0f);
    }
}
