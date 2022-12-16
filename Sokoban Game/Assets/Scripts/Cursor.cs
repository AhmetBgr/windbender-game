using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    private Camera cam;
    public bool snapToGrid = false;
    public Vector3 cursorPos;

    public static Cursor instance = null;

    void Awake()
    {
        // if the singleton hasn't been initialized yet
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            cam = Camera.main;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        if (snapToGrid)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            cursorPos = new Vector3(HalfRound(mouseWorldPos.x), HalfRound(mouseWorldPos.y), 0f);
            transform.position = cursorPos;
        }
        else
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mouseWorldPos;
        }
    }

    public void HideCursor()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
        snapToGrid = false;
    }

    public void ShowCursor()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = true;
        snapToGrid = true;
    }

    float HalfRound(float value)
    {
        float floor = Mathf.FloorToInt(value);

        return floor += 0.5f;
    }
}
