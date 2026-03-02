using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public string playerTag = "Player";

    [Header("Horizontal Follow")]
    public float smoothSpeed = 6f;

    [Header("Look Ahead")]
    public float lookAheadDistance = 1.2f;
    public float lookAheadSpeed = 4f;
    public float moveThreshold = 0.1f;

    [Header("Vertical Platformer Lock")]
    public float baseY = 0f;

    [Header("Camera X Locks")]
    public Transform cameraLeftLock;
    public Transform cameraRightLock;

    private Transform target;
    private Rigidbody2D targetRb;

    private float currentLookAhead = 0f;
    private float targetLookAhead = 0f;
    private Vector3 lastTargetPos;

    void Start()
    {
        FindTarget();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }

        // -------------------------
        // HORIZONTAL LOOK AHEAD
        // -------------------------
        float horizontalVelocity;

        if (targetRb != null)
            horizontalVelocity = targetRb.linearVelocity.x;
        else
            horizontalVelocity = (target.position.x - lastTargetPos.x) / Time.deltaTime;

        if (Mathf.Abs(horizontalVelocity) > moveThreshold)
            targetLookAhead = Mathf.Sign(horizontalVelocity) * lookAheadDistance;
        else
            targetLookAhead = 0f;

        currentLookAhead = Mathf.Lerp(
            currentLookAhead,
            targetLookAhead,
            lookAheadSpeed * Time.deltaTime
        );

        float desiredX = target.position.x + currentLookAhead;

        // -------------------------
        // APPLY CAMERA X LOCKS
        // -------------------------
        if (cameraLeftLock != null || cameraRightLock != null)
        {
            Camera cam = GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
            
            if (cam != null)
            {
                float halfHeight = cam.orthographicSize;
                float halfWidth = halfHeight * cam.aspect;

                if (cameraLeftLock != null)
                    desiredX = Mathf.Max(desiredX, cameraLeftLock.position.x + halfWidth);

                if (cameraRightLock != null)
                    desiredX = Mathf.Min(desiredX, cameraRightLock.position.x - halfWidth);
            }
            else
            {
                // Fallback to center locking if no camera found
                if (cameraLeftLock != null) desiredX = Mathf.Max(desiredX, cameraLeftLock.position.x);
                if (cameraRightLock != null) desiredX = Mathf.Min(desiredX, cameraRightLock.position.x);
            }
        }

        // -------------------------
        // HARD LOCK VERTICAL AXIS
        // -------------------------
        float desiredY = baseY;

        // -------------------------
        // APPLY CAMERA POSITION
        // -------------------------
        Vector3 desiredPosition = new Vector3(
            desiredX,
            desiredY,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        lastTargetPos = target.position;
    }

    void FindTarget()
    {
        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null)
        {
            target = p.transform;
            targetRb = p.GetComponent<Rigidbody2D>();
            lastTargetPos = target.position;
        }
    }

    public void SnapToTarget()
    {
        if (target == null) return;

        float x = target.position.x;

        if (cameraLeftLock != null)
            x = Mathf.Max(x, cameraLeftLock.position.x);

        if (cameraRightLock != null)
            x = Mathf.Min(x, cameraRightLock.position.x);

        transform.position = new Vector3(
            x,
            baseY,
            transform.position.z
        );

        currentLookAhead = 0f;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            lastTargetPos = target.position;
            // Optionally snap or smooth? Smooth is better for cinematic.
        }
    }
}
