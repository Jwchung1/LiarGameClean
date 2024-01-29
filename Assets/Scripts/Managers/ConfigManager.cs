using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigManager : MonoBehaviour
{
    private bool configOpened = false;

    public GameObject ConfigScreen;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!configOpened)
                OpenConfigScreen();
            else
                CloseConfigScreen();
        }
    }

    public void OpenConfigScreen()
    {
        ConfigScreen.SetActive(true);
        configOpened = true;
    }
    public void CloseConfigScreen()
    {
        ConfigScreen.SetActive(false);
        configOpened = false;
    }
}
