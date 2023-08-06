using Const;
using Managers;
using UnityEngine;

namespace Camera {
    public class CameraControl : MonoBehaviour {
        public float lookSpeedH = 2f;
        public float lookSpeedV = 2f;

        private float _yaw;
        private float _pitch;

        private KeyActionSettings _keyActionSettings;
        private KeyActionSettings.StandardActions _standardActions;

        private void Start() {
            var eulerAngles = transform.eulerAngles;
            _yaw = eulerAngles.y;
            _pitch = eulerAngles.x;
        }

        private void OnEnable() {
            _keyActionSettings = new KeyActionSettings();
            _keyActionSettings.Enable();
            _standardActions = _keyActionSettings.standard;
        }

        private void OnDisable() {
            _keyActionSettings.Dispose();
        }

        private void Update() {
            switch (GameManager.Instance.State) {
                case GameState.Menu:
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true; // 隐藏鼠标 
                    return;
                case GameState.Playing:
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false; // 隐藏鼠标
                    var rightStick = _standardActions.Look.ReadValue<Vector2>();
                    _yaw += lookSpeedH * rightStick.x;
                    _pitch -= lookSpeedV * rightStick.y;
                    _yaw = Mathf.Repeat(_yaw, 360f);
                    _pitch = Mathf.Clamp(_pitch, -90, 90);
                    transform.eulerAngles = new Vector3(_pitch, _yaw, 0f);
                    break;
                case GameState.Loading:
                default:
                    return;
            }
        }
    }
}