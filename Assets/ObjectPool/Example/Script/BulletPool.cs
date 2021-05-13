using System;
using System.Collections;
using System.Collections.Generic;
using Harumaro.ObjectPool;
using UnityEngine;

public class BulletPool : ObjectPool<BulletPool, Bullet> {
    private void Start() {
        CreatePool(100);
    }


    private void Update() {
        Release();
    }
}
