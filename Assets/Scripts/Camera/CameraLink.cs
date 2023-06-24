namespace Camera {
    public class CameraLink : UnityEngine.MonoBehaviour {
        public static UnityEngine.Camera Instance;

        private void Awake() {
            Instance = GetComponent<UnityEngine.Camera>();
        }
    }
}