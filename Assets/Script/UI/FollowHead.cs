using UnityEngine;

public class UIFollowHead : MonoBehaviour
{
    public Transform target; // Gắn object Head
    public Vector3 offset = new Vector3(0, 0.3f, 0);

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + offset;

        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0;
        transform.rotation = Quaternion.LookRotation(camForward);
    }
}
