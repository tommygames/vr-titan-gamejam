using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public void Start()
    {
        if (BoltManager.IsHeadlessMode()
            || Application.platform == RuntimePlatform.Android)
        {
            OnStartServerButton();
        }
    }

    public void OnPlayButton()
    {
        BoltLauncher.StartClient();
        DisableButtons();
    }

    public void OnStartServerButton()
    {
        BoltLauncher.StartServer();
        DisableButtons();
    }

    void DisableButtons()
    {
        var buttons = gameObject.GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
            button.gameObject.SetActive(false);
        }
    }
}
