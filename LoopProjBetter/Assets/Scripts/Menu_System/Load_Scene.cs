using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Load_Scene : MonoBehaviour
{
    public void Load_Defined_Scene(string i_name)
    {
        SceneManager.LoadScene(i_name);
    }
}
