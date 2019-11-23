using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_Activation : MonoBehaviour
{
    [SerializeField] private List<Button> buttons = new List<Button>();
    [SerializeField] private Click_Button open_button;
    [SerializeField] private Click_Button open_button_opposite;
    [SerializeField] public bool last_click_state;
    [SerializeField] private bool last_click_state_opposite;

    [SerializeField] private Renderer door_renderer;
    [SerializeField] private Collider door_collider;

    public bool is_open = false;
    public bool is_door_opening = false;

    [SerializeField] public bool is_inverted = false;
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

    public void Add_Object_Button(Button i_button_to_add)
    {
        buttons.Add(i_button_to_add);
    }

    // Update is called once per frame
    void Update()
    {

        //    if (buttons.Count >= 1)
        //    {
        //        is_door_opening = true;

        //        foreach (var input in buttons)
        //        {
        //            if (!input.is_activated)
        //            {
        //                is_door_opening = false;
        //            }
        //        }
        //    }

        //    if (is_open && is_door_opening && !last_click_state && !last_click_state_opposite) // To prevent the door from becoming stuck when time travel occurs
        //    {
        //        last_click_state = true;
        //        last_click_state_opposite = true;
        //        is_open = true;
        //        is_door_opening = false;
        //        if (open_button != null)
        //        {
        //            open_button.is_activated = true;
        //        }

        //        if (open_button_opposite != null)
        //        {
        //            open_button_opposite.is_activated = true;
        //        }
        //    }




        //    if (open_button != null)
        //    {
        //        if (last_click_state != open_button.is_activated)
        //        {
        //            last_click_state = open_button.is_activated;

        //        }

        //        if ((open_button_opposite != null) && (last_click_state != last_click_state_opposite))
        //        {
        //            last_click_state_opposite = last_click_state;
        //            open_button_opposite.is_activated = last_click_state_opposite;
        //        }
        //    }

        //    if (open_button_opposite != null)
        //    {
        //        if (last_click_state_opposite != open_button_opposite.is_activated)
        //        {
        //            last_click_state_opposite = open_button_opposite.is_activated;

        //        }

        //        if ((open_button != null) && (last_click_state != last_click_state_opposite))
        //        {
        //            last_click_state = last_click_state_opposite;
        //            open_button.is_activated = last_click_state;
        //        }
        //    }

        // Section for object buttons
        if (buttons.Count >= 1)
        {
            is_door_opening = true;
            foreach (var button in buttons)
            {
                if (!button.is_activated)
                {
                    is_door_opening = false;
                }
            }
        }
        //

        if ((is_door_opening))
        {
            door_renderer.enabled = is_inverted;
            door_collider.enabled = is_inverted;

            is_open = !is_inverted;
        }
        else
        {
            door_renderer.enabled = !is_inverted;
            door_collider.enabled = !is_inverted;

            is_open = is_inverted;
        }
    }
}
