using System;
using System.Collections;
using System.Collections.Generic;
using Harumaro.ObjectPool;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bullet : MonoBehaviour, IPoolable<Bullet> {
    public Bullet Entity => this;

    public float lifeTime;
    float _time;


    void Update() {
        transform.localPosition += transform.right;
        _time += Time.deltaTime;

        if (_time > lifeTime) {
            BulletPool.Instance.Catch(this);
        }
    }


    float GetRandomAngle() => Random.Range(0f, 30f);


    public void OnReleased() {
        transform.localRotation = Quaternion.Euler(GetRandomAngle(), GetRandomAngle(), GetRandomAngle());
    }


    public void OnCatched() {
        transform.localPosition = Vector3.zero;
        _time = 0f;
    }
}
