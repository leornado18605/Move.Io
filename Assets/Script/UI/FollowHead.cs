using UnityEngine;

public class UIFollowHead : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, GameConstants.UI_HEAD_OFFSET_Y, 0);

    // Cache components
    private Transform cachedTransform;
    private Camera cachedMainCamera;

    private void Awake()
    {
        cachedTransform = transform;
        cachedMainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        UpdatePosition();
        UpdateRotation();
    }

    private void UpdatePosition()
    {
        cachedTransform.position = target.position + offset;
    }

    private void UpdateRotation()
    {
        Vector3 camForward = cachedMainCamera.transform.forward;
        camForward.y = 0;
        cachedTransform.rotation = Quaternion.LookRotation(camForward);
    }
}