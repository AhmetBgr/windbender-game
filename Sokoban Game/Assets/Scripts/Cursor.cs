using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    private Camera cam;
    public bool snapToGrid = false;
    //public bool onlyUpdateOnHover = false;
    public Vector3 pos;
    public Vector3 realPos;

    public bool isHiden = false;
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

    void LateUpdate()
    {
        if (snapToGrid)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos = new Vector3(HalfRound(mouseWorldPos.x), HalfRound(mouseWorldPos.y), 0f);
            realPos = mouseWorldPos;
            transform.position = pos;
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
        isHiden = true;
    }

    public void ShowCursor()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = true;
        snapToGrid = true;
        isHiden = false;
    }

    float HalfRound(float value)
    {
        float floor = Mathf.FloorToInt(value);

        return floor += 0.5f;
    }
}
