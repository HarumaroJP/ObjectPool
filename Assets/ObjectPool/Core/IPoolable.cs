using UnityEngine;

namespace Harumaro.ObjectPool {

    public interface IPoolable<T> where T : MonoBehaviour {
        T Entity { get; }

        void OnReleased();
        void OnCatched();
    }

}
