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
        public int Count => Pool.Count;
        public int Capacity => m_capacity;
        protected int m_capacity;
        protected Queue<IPoolable<T2>> Pool;

        public bool IsActive => m_isActive;
        protected bool m_isActive = false;
        public bool IsFixed => m_isFixed;
        protected bool m_isFixed;


        protected void Awake() {
            CheckInstance();
        }


        public void CreatePool(int capacity, bool isFixed) {
            if (m_isActive) {
                Debug.LogWarning("Poolは既に生成されています！");
                return;
            }

            if (prefab == null) {
                Debug.LogError("Prefabがセットされていません！");
                return;
            }

            m_capacity = capacity;
            Pool = new Queue<IPoolable<T2>>(capacity);

            for (int i = 0; i < capacity; i++) {
                T2 obj = Instantiate(prefab, transform).GetComponent<T2>();
                obj.gameObject.SetActive(false);
                Pool.Enqueue(obj);
            }

            m_isActive = true;
            m_isFixed = isFixed;
        }


        public void DestroyPool() {
            if (!PoolIsAvailable()) return;

            foreach (IPoolable<T2> obj in Pool) {
                Destroy(obj.Entity.gameObject);
            }

            Pool.Clear();
            m_capacity = 0;
            m_isActive = false;
            m_isFixed = true;
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
            if (!CanDequeue()) return;

            obj.OnCatched();
            obj.Entity.gameObject.SetActive(false);
            Pool.Enqueue(obj);
        }


        bool PoolIsAvailable() {
            if (!m_isActive) {
                Debug.LogError("PoolがActiveではありません！");
            }

            return m_isActive;
        }


        bool CanDequeue() {
            bool canDequeue = m_isFixed && Pool.Count < m_capacity;
            if (!canDequeue) {
                Debug.LogWarning("Poolの許容値を超えています！");
            }

            return canDequeue;
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
