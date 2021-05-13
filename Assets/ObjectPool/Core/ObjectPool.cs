using System;
using System.Collections.Generic;
using UnityEngine;

namespace Harumaro.ObjectPool {

    /// <summary>
    /// オブジェクトプールを作成するための基底クラスです。
    /// シングルトンなため、単一インスタンスのみ生成可能です。
    /// </summary>
    /// <typeparam name="T1">プールとなるクラス（継承先）</typeparam>
    /// <typeparam name="T2">プールされるオブジェクトのクラス</typeparam>
    public class ObjectPool<T1, T2> : MonoBehaviour
        where T1 : ObjectPool<T1, T2> where T2 : MonoBehaviour, IPoolable<T2> {

        [SerializeField] protected GameObject prefab;
        protected int m_entityCount;
        protected Queue<IPoolable<T2>> Pool;
        protected bool IsActive = false;


        protected void Awake() {
            CheckInstance();
        }


        public void CreatePool(int entityCount) {
            if (IsActive) {
                Debug.LogWarning("Poolは既に生成されています！");
                return;
            }

            if (prefab == null) {
                Debug.LogError("Prefabがセットされていません！");
            }

            m_entityCount = entityCount;
            Pool = new Queue<IPoolable<T2>>(entityCount);

            for (int i = 0; i < entityCount; i++) {
                T2 obj = Instantiate(prefab, transform).GetComponent<T2>();
                obj.gameObject.SetActive(false);
                Pool.Enqueue(obj);
            }

            IsActive = true;
        }


        public void DestroyPool() {
            if (!PoolIsAvailable()) return;

            foreach (IPoolable<T2> obj in Pool) {
                Destroy(obj.Entity.gameObject);
            }

            Pool.Clear();
            m_entityCount = 0;
            IsActive = false;
        }


        public T2 Release() {
            if (!PoolIsAvailable()) return null;

            try {
                IPoolable<T2> obj = Pool.Dequeue();
                T2 entity = obj.Entity;

                entity.gameObject.SetActive(true);
                obj.OnReleased();

                return entity;
            } catch {
                Debug.LogError("Release可能なPoolEntityが存在しません！");
                return null;
            }
        }


        public void Catch(IPoolable<T2> obj) {
            if (!PoolIsAvailable()) return;

            obj.OnCatched();
            obj.Entity.gameObject.SetActive(false);
            Pool.Enqueue(obj);
        }


        bool PoolIsAvailable() {
            if (!IsActive) {
                Debug.LogError("PoolがActiveではありません！");
            }

            return IsActive;
        }


        public static T1 Instance {
            get {
                if (_instance == null) {
                    Type t = typeof(T1);

                    _instance = (T1) FindObjectOfType(t);
                    if (_instance == null) {
                        Debug.LogError(t + " をアタッチしているGameObjectはありません");
                    }
                }

                return _instance;
            }
        }

        static T1 _instance;


        protected bool CheckInstance() {
            if (_instance == null) {
                _instance = this as T1;
                return true;
            } else if (_instance == this) {
                return true;
            }

            Destroy(this);
            return false;
        }
    }

}
