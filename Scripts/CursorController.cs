using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    [SerializeField] Texture2D mouseCursor;

    CursorMode cursorMode = CursorMode.Auto;

    void Start()
    {
        Cursor.SetCursor(mouseCursor, new Vector2(mouseCursor.width / 2, mouseCursor.height / 2), cursorMode);
        Cursor.lockState = CursorLockMode.Confined;
    }
}
