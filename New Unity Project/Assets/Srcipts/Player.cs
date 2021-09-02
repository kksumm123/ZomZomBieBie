using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor
{
    [SerializeField] float speed = 5;
    float originFieldOfView;
    Canvas aimCanvas;
    Canvas normalCanvas;
    void Start()
    {
        camTransform = Camera.main.transform;
        originFieldOfView = Camera.main.fieldOfView;
        aimCanvas = transform.Find("AimCanvas").GetComponent<Canvas>();
        normalCanvas = transform.Find("NormalCanvas").GetComponent<Canvas>();
        aimCanvas.enabled = false;
        normalCanvas.enabled = true;
        State = StateType.Idle;
    }

    void Update()
    {
        Move();
        Zoom();
        CamaraRotate();
    }

    bool isZoomMode = false;
    float zoomValue = 20f;
    float zoomDuration = 0.2f;
    TweenerCore<float, float, FloatOptions> tween;
    void Zoom()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (isZoomMode == false)
            {
                isZoomMode = true;
                aimCanvas.enabled = true;
                normalCanvas.enabled = false;
                tween.Kill();
                tween = DOTween.To(() => originFieldOfView
                                , x => Camera.main.fieldOfView = x
                                , zoomValue, zoomDuration).SetLink(gameObject);
            }
        }
        else if (Input.GetMouseButtonUp(1))
        {
            if (isZoomMode == true)
            {
                isZoomMode = false;
                aimCanvas.enabled = false;
                normalCanvas.enabled = true;
                tween.Kill();
                tween = DOTween.To(() => zoomValue
                                , x => Camera.main.fieldOfView = x
                                , originFieldOfView, zoomDuration).SetLink(gameObject);
            }
        }
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

    enum StateType
    {
        Idle,
        Walk,
        Roll,
        Dash,
        Fire,
        Reload,
        Jump,
        Hit,
        Die,
    }
    StateType m_state;

    private StateType State
    {
        get => m_state;
        set
        {
            if (m_state == value)
                return;

            print($"Player State : {m_state} -> {value}");
            m_state = value;
        }
    }
}
