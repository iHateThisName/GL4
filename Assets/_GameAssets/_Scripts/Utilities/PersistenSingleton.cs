using UnityEngine;
namespace Assets.Scripts.Singleton {
    /// <summary>
    /// Defines a persistent singleton in Unity. That will not be destroyed when changing scenes
    /// Ensures that only one instance of this class exists in the application.
    /// </summary>
    /// <typeparam name="T">The generic type parameter. Must be a subclass of Component.</typeparam>
    public class PersistenSingleton<T> : MonoBehaviour where T : Component {
        public bool AutoUnparentOnAwake = true;
        protected static T instance;

        // Prevent creating new instances while the application/Editor is shutting down or the singleton is being destroyed.
        private static bool isShuttingDown = false;

        public static bool HasInstance => instance != null;
        public static T TryGetInstance() => HasInstance ? instance : null;

        public static T Instance {
            get {
                // If the application is quitting or the singleton is shutting down, do NOT create a new GameObject.
                if (isShuttingDown || !Application.isPlaying) {
                    return instance;
                }

                if (instance == null) {
                    instance = FindAnyObjectByType<T>();
                    if (instance == null) {
                        GameObject go = new GameObject(typeof(T).Name + " Auto-Generated");
                        instance = go.AddComponent<T>();
                    }
                }
                return instance;
            }
        }

        protected virtual void Awake() { InitializeSingletion(); }

        protected virtual void InitializeSingletion() {
            if (!Application.isPlaying) return;
            if (this.AutoUnparentOnAwake) transform.SetParent(null);

            if (instance == null) {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            } else if (instance != this) {
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit() {
            isShuttingDown = true;
        }

        protected virtual void OnDestroy() {
            if (instance == this) {
                isShuttingDown = true;
            }
        }
    }
}
