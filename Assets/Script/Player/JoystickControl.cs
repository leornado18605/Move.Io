using UnityEngine;

public class JoystickControl : MonoBehaviour
{
    public static Vector3 direct;

    [Header("UI References")]
    [SerializeField] private RectTransform joystickBG;
    [SerializeField] private RectTransform joystickControl;
    [SerializeField] private GameObject joystickPanel;

    [Header("Settings")]
    [SerializeField] private float magnitude = 100f;

    // Cache variables
    private Vector3 screenCenter;
    private Vector3 startPoint;
    private Vector3 updatePoint;

    private Vector3 MousePosition => Input.mousePosition - screenCenter;

    private void Awake()
    {
        InitializeScreen();
        direct = Vector3.zero;
        joystickPanel.SetActive(false);
    }

    private void InitializeScreen()
    {
        screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
    }

    private void Update()
    {
        HandleMouseInput();
    }

    private void OnDisable()
    {
        direct = Vector3.zero;
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartJoystick();
        }

        if (Input.GetMouseButton(0))
        {
            UpdateJoystick();
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndJoystick();
        }
    }

    private void StartJoystick()
    {
        startPoint = MousePosition;
        joystickBG.anchoredPosition = startPoint;
        joystickPanel.SetActive(true);
    }

    private void UpdateJoystick()
    {
        updatePoint = MousePosition;
        Vector3 offset = updatePoint - startPoint;
        joystickControl.anchoredPosition = Vector3.ClampMagnitude(offset, magnitude) + startPoint;
        direct = new Vector3(offset.x, 0, offset.y).normalized;
    }

    private void EndJoystick()
    {
        joystickPanel.SetActive(false);
        direct = Vector3.zero;
    }
}