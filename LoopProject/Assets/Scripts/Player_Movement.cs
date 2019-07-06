using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    private CharacterController cc;

    [SerializeField] private float movement_speed;

    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = new Vector3();

        if (Input.GetKey(KeyCode.D))
        {
            movement += transform.right;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            movement -= transform.right;
        }

        if (Input.GetKey(KeyCode.W))
        {
            movement += transform.forward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            movement -= transform.forward;
        }

        cc.SimpleMove(movement * Time.deltaTime * movement_speed);
    }
}
