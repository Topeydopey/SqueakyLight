using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSpeed = 10f;
    private float pitch = 0f;
    private float yaw = 0f;

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float upDown = 0;

        if (Input.GetKey(KeyCode.E)) upDown = 1;
        if (Input.GetKey(KeyCode.Q)) upDown = -1;

        Vector3 direction = new Vector3(horizontal, upDown, vertical);
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.Self);

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * lookSpeed;
        pitch -= mouseY * lookSpeed;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.localRotation = Quaternion.Euler(pitch, yaw, 0);
    }
}
