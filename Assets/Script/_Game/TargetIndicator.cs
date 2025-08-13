using UnityEngine;

public class TargetIndicator : MonoBehaviour
{

    [SerializeField] private GameObject indicatorUI; // Gắn prefab UI vào đây trong Inspector
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private RectTransform indicator;
    [SerializeField] private Camera mainCamera;

    [Header("Display Settings")]
    [SerializeField] private float edgePadding = GameConstants.EDGE_PADDING;
    [SerializeField] private float minViewportDistance = GameConstants.MIN_VIEWPORT;
    [SerializeField] private float maxViewportDistance = GameConstants.MAX_VIEWPORT;
    [SerializeField] private bool rotateToTarget = true;
    [SerializeField] private bool scaleByDistance = true;
    [SerializeField] private float minScale = 0.7f;
    [SerializeField] private float maxScale = 1.2f;

    [Header("Fade Settings")]
    [SerializeField] private bool fadeWhenClose = true;
    [SerializeField] private float fadeStartDistance = 5f;
    [SerializeField] private float fadeEndDistance = 2f;

    // Cache components
    private CanvasGroup cachedCanvasGroup;
    private Transform cachedTransform;
    private Transform cachedCameraTransform;

    private void Awake()
    {
        // Cache components for performance
        cachedTransform = transform;
        cachedCanvasGroup = indicator.GetComponent<CanvasGroup>();
        if (cachedCanvasGroup == null)
            cachedCanvasGroup = indicator.gameObject.AddComponent<CanvasGroup>();

        if (mainCamera != null)
            cachedCameraTransform = mainCamera.transform;
    }

    private void Update()
    {
        if (ShouldHideIndicator())
        {
            indicator.gameObject.SetActive(false);
            return;
        }

        indicator.gameObject.SetActive(true);
        UpdateIndicatorPosition();
        UpdateIndicatorRotation();
        UpdateIndicatorScale();
        UpdateIndicatorAlpha();
    }

    private bool ShouldHideIndicator()
    {
        if (target == null || indicator == null || mainCamera == null)
            return true;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(target.position);

        return viewportPos.z > 0 &&
               viewportPos.x > minViewportDistance &&
               viewportPos.x < maxViewportDistance &&
               viewportPos.y > minViewportDistance &&
               viewportPos.y < maxViewportDistance;
    }

    private void UpdateIndicatorPosition()
    {
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(target.position);

        if (viewportPos.z < 0)
        {
            viewportPos.x = 1f - viewportPos.x;
            viewportPos.y = 1f - viewportPos.y;
            viewportPos.z = 0;
            viewportPos = Vector3Maximize(viewportPos - new Vector3(0.5f, 0.5f, 0)) + new Vector3(0.5f, 0.5f, 0);
        }

        viewportPos.x = Mathf.Clamp(viewportPos.x, minViewportDistance, maxViewportDistance);
        viewportPos.y = Mathf.Clamp(viewportPos.y, minViewportDistance, maxViewportDistance);

        indicator.position = new Vector2(
            viewportPos.x * Screen.width,
            viewportPos.y * Screen.height
        );
    }

    private Vector3 Vector3Maximize(Vector3 v)
    {
        float max = Mathf.Max(Mathf.Abs(v.x), Mathf.Abs(v.y));
        return v / max;
    }

    private void UpdateIndicatorRotation()
    {
        if (!rotateToTarget)
            return;

        Vector3 direction = (target.position - cachedCameraTransform.position).normalized;
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        indicator.rotation = Quaternion.Euler(0, 0, -angle);
    }

    private void UpdateIndicatorScale()
    {
        if (!scaleByDistance)
            return;

        float distance = Vector3.Distance(target.position, cachedCameraTransform.position);
        float t = Mathf.InverseLerp(fadeStartDistance, fadeEndDistance, distance);
        float scale = Mathf.Lerp(minScale, maxScale, t);
        indicator.localScale = Vector3.one * scale;
    }

    private void UpdateIndicatorAlpha()
    {
        if (!fadeWhenClose)
            return;

        float distance = Vector3.Distance(target.position, cachedCameraTransform.position);
        float alpha = Mathf.InverseLerp(fadeEndDistance, fadeStartDistance, distance);
        cachedCanvasGroup.alpha = alpha;
    }

    public void UpdateTarget(Transform newTarget)
    {
        target = newTarget;
        gameObject.SetActive(newTarget != null);
    }

}