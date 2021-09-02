using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 15f;
    [SerializeField] float destroyTime = 7f;
    void Start()
    {
        Destroy(gameObject, destroyTime);
    }
    void Update()
    {
        transform.Translate(speed * Time.deltaTime * transform.forward, Space.World);
    }
}
