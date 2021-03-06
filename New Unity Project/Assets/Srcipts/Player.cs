using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : Actor
{
    [SerializeField] float speed = 5;
    CharacterController controller;
    void Start()
    {
        camTransform = Camera.main.transform;
        controller = GetComponent<CharacterController>();
        InitCrossHair();
        InitAnimationClips();
        InitFire();
        InitGravity();
        State = StateType.Idle;
    }


    #region InitCrossHair
    float originFieldOfView;
    Canvas aimCanvas;
    Canvas normalCanvas;
    void InitCrossHair()
    {
        originFieldOfView = Camera.main.fieldOfView;
        aimCanvas = transform.Find("AimCanvas").GetComponent<Canvas>();
        normalCanvas = transform.Find("NormalCanvas").GetComponent<Canvas>();
        aimCanvas.enabled = false;
        normalCanvas.enabled = true;
    }
    #endregion InitCrossHair
    #region InitAnimationClips
    int hashIdle;
    int hashWalk;
    int hashRoll;
    int hashDash;
    int hashFire;
    int hashReload;
    int hashJump;
    int hashHit;
    int hashDie;
    void InitAnimationClips()
    {
        hashIdle = Animator.StringToHash("Idle");
        hashWalk = Animator.StringToHash("Walk");
        hashRoll = Animator.StringToHash("Roll");
        //hashDash = Animator.StringToHash("Dash");
        hashFire = Animator.StringToHash("Fire");
        hashReload = Animator.StringToHash("Reload");
        //hashJump = Animator.StringToHash("Jump");
        hashHit = Animator.StringToHash("Hit");
        hashDie = Animator.StringToHash("Die");
    }
    #endregion InitAnimationClips
    #region InitFire
    GameObject bullet;
    readonly string bulletString = "Bullet/Bullet";
    Transform bulletPoint;
    Light bulletPointLight;
    Vector3 screenCenter;
    void InitFire()
    {
        bullet = (GameObject)Resources.Load(bulletString);
        bulletPoint = transform.Find("BulletPoint");
        bulletPointLight = bulletPoint.GetComponentInChildren<Light>();
        screenCenter.x = Camera.main.pixelWidth * 0.5f;
        screenCenter.y = Camera.main.pixelHeight * 0.5f;
    }
    #endregion InitFire
    #region InitGravity
    float gravityAcceleration = 9.81f;
    float gravityVelocity;
    float s;
    void InitGravity()
    {
        gravityAcceleration = 9.81f;
        gravityVelocity = 0;
        s = 0;
    }
    #endregion InitGravity

    void Update()
    {
        Move();
        UseGravity();
        Fire();
        Zoom();
        CameraRotate();
    }

    #region Zoom
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
    #endregion Zoom

    #region Move
    Vector3 move;
    Vector3 relateMove;
    void Move()
    {
        move = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) move.z = 1;
        if (Input.GetKey(KeyCode.S)) move.z = -1;
        if (Input.GetKey(KeyCode.A)) move.x = -1;
        if (Input.GetKey(KeyCode.D)) move.x = 1;

        if (move != Vector3.zero)
        {
            move.Normalize();

            relateMove = Vector3.zero;
            relateMove = transform.forward * move.z;
            relateMove += transform.right * move.x;
            relateMove.y = 0;
            controller.Move(speed * Time.deltaTime * relateMove);

            State = StateType.Walk;
        }
        else
            State = StateType.Idle;
    }
    #endregion Move

    #region UseGravity
    void UseGravity()
    {
        if (IsGround() == true)
        {
            print("??");
            InitGravity(); //???? ????????
        }
        else
        {
            print("????");
            gravityAccelerationMove(); // ???? ??????????
        }
    }
    float t;
    void gravityAccelerationMove()
    {
        t = Time.deltaTime;

        s = gravityVelocity + (0.5f * gravityAcceleration * Mathf.Pow(t, 2));

        controller.Move(new Vector3(0, -s * t, 0));

        gravityVelocity += gravityAcceleration * t;
    }
    bool IsGround()
    { // true = ???? ????, false = ???? ??????
        return controller.isGrounded;
    }
    #endregion UseGravity

    #region Fire
    float fireDelay = 0.05f;
    float fireableTime;
    Coroutine BulletLightCoHandle;
    void Fire()
    {
        if (Input.GetMouseButton(0))
        {
            if (Time.time > fireableTime)
            {
                Ray centerRay = Camera.main.ScreenPointToRay(screenCenter);
                fireableTime = Time.time + fireDelay;
                State = StateType.Fire;
                var newBullet = Instantiate(bullet, bulletPoint.position, Quaternion.identity);
                newBullet.transform.forward = centerRay.direction;
                BulletLightCoHandle = StopAndStartCo(BulletLightCoHandle, BulletLightCo());
            }
        }
    }
    float bulletLightTime = 0.05f;
    IEnumerator BulletLightCo()
    {
        bulletPointLight.enabled = true;
        yield return new WaitForSeconds(bulletLightTime);
        bulletPointLight.enabled = false;
    }
    #endregion Fire

    #region CameraRotate
    Transform camTransform;
    [SerializeField] float mouseSensitivity = 1f;
    void CameraRotate()
    {
        // ???????? ?????? ?????????? ?????? - ?????? ???????? ????
        float mouseMoveX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseMoveY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        // camTransforms
        var worldUp = camTransform.InverseTransformDirection(Vector3.up);
        var rotation = camTransform.rotation *
                       Quaternion.AngleAxis(mouseMoveX, worldUp) *
                       Quaternion.AngleAxis(mouseMoveY, Vector3.left);

        transform.eulerAngles = new Vector3(0f, rotation.eulerAngles.y, 0f);
        var eulerRotation = rotation.eulerAngles;
        if (eulerRotation.x >= 310)
            eulerRotation.x = Mathf.Clamp(eulerRotation.x, 320, 360);
        else if (eulerRotation.x >= 40)
            eulerRotation.x = Mathf.Clamp(eulerRotation.x, 0, 40);

        camTransform.rotation = Quaternion.Euler(eulerRotation);
    }
    #endregion CameraRotate

    #region PlayAnimation
    void PlayAnimation()
    {
        switch (State)
        {
            case StateType.Idle:
                animator.Play(hashIdle);
                break;
            case StateType.Walk:
                animator.Play(hashWalk);
                break;
            case StateType.Roll:
                animator.Play(hashRoll);
                break;
            case StateType.Dash:
                animator.Play(hashDash);
                break;
            case StateType.Fire:
                animator.Play(hashFire);
                break;
            case StateType.Reload:
                animator.Play(hashReload);
                break;
            case StateType.Jump:
                animator.Play(hashJump);
                break;
            case StateType.Hit:
                animator.Play(hashHit);
                break;
            case StateType.Die:
                animator.Play(hashDie);
                break;
        }
    }
    #endregion PlayAnimation

    #region State
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

            PlayAnimation();
        }
    }
    #endregion State

    #region Methods
    void StopCo(Coroutine handle)
    {
        if (handle != null)
            StopCoroutine(handle);
    }
    Coroutine StopAndStartCo(Coroutine handle, IEnumerator Fn)
    {
        StopCo(handle);
        return StartCoroutine(Fn);
    }
    #endregion Methods
}
