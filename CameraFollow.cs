using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothing = 5f;
    public float currentZ, currentY;

    Vector3 offset;
    void Start()
    {
        currentZ = transform.position.z;
        currentY = transform.position.y;
        offset = transform.position - target.position;
    }

    void FixedUpdate()
    {
        Vector3 targetCamPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, new Vector3(targetCamPos.x, currentY, targetCamPos.z), smoothing * Time.deltaTime);
    }
        
    void Update()
    {
        if (transform.position.y <= 20)
        {
            currentY = 20;
            transform.position = new Vector3(transform.position.x, 20, transform.position.z);
        }
        if (transform.position.y >= 42)
        {
            currentY = 42;
            transform.position = new Vector3(transform.position.x, 42, transform.position.z);
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0f && currentY >= 20)
        {
            currentY -= 2;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f && currentY <= 42)
        {
            currentY += 2;
        }
        
    }
}
