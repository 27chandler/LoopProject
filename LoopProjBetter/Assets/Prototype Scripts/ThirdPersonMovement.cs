using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    [SerializeField] private float speed;

    private CharacterController controller;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = new Vector3();

        movement.x = Input.GetAxis("Horizontal");
        movement.z = Input.GetAxis("Vertical");

        controller.Move(movement * speed * Time.deltaTime);
    }
}
