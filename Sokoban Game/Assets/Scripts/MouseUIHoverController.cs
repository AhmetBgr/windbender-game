using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseUIHoverController : MonoBehaviour
{
    public void OnPointerEnter()
    {
        //GameManager.instance.drawingController.isHoveringUI = true;
        Cursor.instance.HideCursor();
    }

    public void OnPointerExit()
    {
        //GameManager.instance.isHoveringUI = false;
        Cursor.instance.ShowCursor();
    }
}
