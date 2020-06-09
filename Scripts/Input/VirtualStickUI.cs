using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualStickUI : MonoBehaviour
{
    protected GameObject _socket;
    protected GameObject _stick;

	// Use this for initialization
	void Awake ()
    {
        var ui = Resources.Load("VirtualStickUI_pre");
        var go = GameObject.Instantiate(ui) as GameObject;
        _socket = go.transform.Find("Socket").gameObject;
        _stick = _socket.transform.Find("Stick").gameObject;

        Hide();
	}
	
	// Update is called once per frame
	void Update ()
    {	
	}

    public void Show()
    {
        _socket.SetActive(true);
    }

    public void Hide()
    {
        _socket.SetActive(false);
    }

    public void SetRadius(float radius)
    {
        _socket.GetComponent<RectTransform>().sizeDelta = new Vector2(radius*2, radius*2);
        _stick.GetComponent<RectTransform>().sizeDelta = new Vector2(radius * 2 * 0.65f, radius * 2 * 0.65f);

    }

    public void SetPositions(Vector2 socket, Vector2 stick)
    {
        _socket.transform.position = socket;
        _stick.transform.localPosition = stick;
    }
}
