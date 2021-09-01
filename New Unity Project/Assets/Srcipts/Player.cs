using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor
{

    [SerializeField] float speed = 5;
    void Start()
    {
        camTransform = Camera.main.transform;
    }

    void Update()
    {
        Move();
        CamaraRotate();
    }

    Vector3 move;
    void Move()
    {
        move = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) move.z = 1;
        if (Input.GetKey(KeyCode.S)) move.z = -1;
        if (Input.GetKey(KeyCode.A)) move.x = -1;
        if (Input.GetKey(KeyCode.D)) move.x = 1;

        move.Normalize();

        if (move != Vector3.zero)
        {
            transform.Translate(move * speed * Time.deltaTime);
        }
    }

    Transform camTransform;
    [SerializeField] float mouseSensitivity = 1f;
    void CamaraRotate()
    {
        // 카메라와 캐릭터 로테이션을 바꾸자 - 마우스 이동량에 따라
        float mouseMoveX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseMoveY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        // camTransforms
        var worldUp = camTransform.InverseTransformDirection(Vector3.up);
        var rotation = camTransform.rotation *
                       Quaternion.AngleAxis(mouseMoveX, worldUp) *
                       Quaternion.AngleAxis(mouseMoveY, Vector3.left);
        transform.eulerAngles = new Vector3(0f, rotation.eulerAngles.y, 0f);
        camTransform.rotation = rotation;
    }
}
