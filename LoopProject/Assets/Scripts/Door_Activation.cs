using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_Activation : MonoBehaviour
{
    [SerializeField] private List<Button> buttons = new List<Button>();
    [SerializeField] private Click_Button open_button;
    [SerializeField] private Click_Button open_button_opposite;
    [SerializeField] private bool last_click_state;
    [SerializeField] private bool last_click_state_opposite;

    [SerializeField] private Renderer door_renderer;
    [SerializeField] private Collider door_collider;

    public bool is_open = false;
    private bool is_door_opening = false;
    // Start is called before the first frame update
    void Start()
    {
        if (open_button != null)
        {
            last_click_state = open_button.is_activated;
        }
        if (open_button_opposite != null)
        {
            last_click_state_opposite = open_button_opposite.is_activated;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (buttons.Count >= 1)
        {
            is_door_opening = true;

            foreach (var input in buttons)
            {
                if (!input.is_activated)
                {
                    is_door_opening = false;
                }
            }
        }




        if (open_button != null)
        {
            if (last_click_state != open_button.is_activated)
            {
                last_click_state = open_button.is_activated;

            }

            if ((open_button_opposite != null) && (last_click_state != last_click_state_opposite))
            {
                last_click_state_opposite = last_click_state;
                open_button_opposite.is_activated = last_click_state_opposite;
            }
        }

        if (open_button_opposite != null)
        {
            if (last_click_state_opposite != open_button_opposite.is_activated)
            {
                last_click_state_opposite = open_button_opposite.is_activated;

            }

            if ((open_button != null) && (last_click_state != last_click_state_opposite))
            {
                last_click_state = last_click_state_opposite;
                open_button.is_activated = last_click_state;
            }
        }



        if ((is_door_opening) || (last_click_state))
        {
            door_renderer.enabled = false;
            door_collider.enabled = false;

            is_open = true;
        }
        else
        {
            door_renderer.enabled = true;
            door_collider.enabled = true;

            is_open = false;
        }
    }
}
