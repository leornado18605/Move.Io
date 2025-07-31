using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickControl : MonoBehaviour
{
    public static Vector3 direct;

    private Vector3 screen;
    private Vector3 startPoint;
    private Vector3 updatePoint;

    private Vector3 MousePosition => Input.mousePosition - screen / 2;

    public RectTransform joystickBG;
    public RectTransform joystickControl;
    public float magnitude;

    public GameObject joystickPanel;

    void Awake()
    {
        screen.x = Screen.width;
        screen.y = Screen.height;

        direct = Vector3.zero;
        joystickPanel.SetActive(false);
    }

    void Update()
    {
        MouseEvent();
    }

    private void OnDisable()
    {
        direct = Vector3.zero;
    }

    public void MouseEvent()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPoint = MousePosition;
            joystickBG.anchoredPosition = startPoint;
            joystickPanel.SetActive(true);
        }

        if (Input.GetMouseButton(0))
        {
            updatePoint = MousePosition;
            Vector3 offset = updatePoint - startPoint;

            joystickControl.anchoredPosition = Vector3.ClampMagnitude(offset, magnitude) + startPoint;

            direct = new Vector3(offset.x, 0, offset.y).normalized;
        }

        if (Input.GetMouseButtonUp(0))
        {
            joystickPanel.SetActive(false);
            direct = Vector3.zero;
        }
    }
}
