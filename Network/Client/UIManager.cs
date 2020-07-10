using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Create a canvas, call it menu, change UI scale mode to scale with screensize and set reference resolution to 1920x by 1280y
public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject startMenu;
    public InputField usernameField;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Debug.Log("Instance already exists, destroying object");
            Destroy(this);
        }
    }
    public void ConnectToServer()
    {
        startMenu.SetActive(false);
        usernameField.interactable = false;
        Client.instance.ConnectToServer();
    }
}
