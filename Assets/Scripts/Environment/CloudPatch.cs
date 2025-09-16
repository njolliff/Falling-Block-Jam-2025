using UnityEngine;

public class CloudPatch : MonoBehaviour
{
    public float moveSpeed = 1;
    public Vector3 startPos, endPos;

    void Awake()
    {
        // Get the object's position relative to the camera's viewport
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);

        // Set startPos and endPos as half a screen off the left and right sides of the screen at the current Y and Z
        startPos = Camera.main.ViewportToWorldPoint(new Vector3(-0.5f, viewportPos.y, viewportPos.z));
        endPos = Camera.main.ViewportToWorldPoint(new Vector3(1.5f, viewportPos.y, viewportPos.z));
    }

    void Update()
    {
        transform.SetPositionAndRotation(new Vector3(transform.position.x + (moveSpeed * Time.deltaTime), transform.position.y, transform.position.z), transform.rotation);
        if (transform.position.x >= endPos.x) transform.position = startPos;
    }
}