﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private Timeline_Manager tm;
    private BoxCollider col;
    private MeshRenderer mr;

    [SerializeField] public bool is_activated = false;
    [SerializeField] public bool is_open = false;
    // Start is called before the first frame update
    void Start()
    {
        tm = GameObject.FindGameObjectWithTag("Timeline_Manager").GetComponent<Timeline_Manager>();
        col = GetComponent<BoxCollider>();
        mr = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (is_activated)
        {
            mr.enabled = true;
            col.enabled = true;
        }
        else
        {
            mr.enabled = false;
            col.enabled = false;
        }

        if (is_open)
        {
            col.isTrigger = true;
        }
        else
        {
            col.isTrigger = false;
        }
    }
}
