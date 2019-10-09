﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Time_Jump_Effect : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;
    [SerializeField] public bool is_activated = false;
    private bool is_routine_running = false;
    public bool has_jump_occured = false;

    [Space]

    [SerializeField] PostProcessProfile profile;

    private float timer = 0.0f;
    [SerializeField] private float max_time = 1.0f;
    [SerializeField] private float max_fov_increase = 30.0f;

    private Camera player_cam;
    private float default_fov;
    // Start is called before the first frame update
    void Start()
    {
        player_cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        default_fov = player_cam.fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        if (is_activated && !is_routine_running)
        {
            is_routine_running = true;
            is_activated = false;
            StartCoroutine(Activate_Jump_Effect());
        }
    }

    IEnumerator Activate_Jump_Effect()
    {
        float curve_value = 0.0f;
        bool has_triggered_jump_this_cycle = false;
        while(timer < max_time*2.0f)
        {
            timer += Time.deltaTime;

            curve_value = curve.Evaluate(timer / max_time);

            player_cam.fieldOfView = default_fov + (curve_value * max_fov_increase);
            profile.GetSetting<ChromaticAberration>().intensity.Override(curve_value);
            profile.GetSetting<LensDistortion>().intensity.Override(curve_value*80.0f);

            if ((timer > max_time) && (!has_triggered_jump_this_cycle))
            {
                has_triggered_jump_this_cycle = true;
                has_jump_occured = true;
            }
            yield return null;
        }
        



        timer = 0.0f;
        is_routine_running = false;
        yield return null;
    }
}
