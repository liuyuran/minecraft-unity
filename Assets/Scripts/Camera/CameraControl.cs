using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Camera {
    public class CameraControl : MonoBehaviour {
        public float lookSpeedH = 2f;
        public float lookSpeedV = 2f;

        private float _yaw;
        private float _pitch;

        private KeyActionSettings _keyActionSettings;
        private KeyActionSettings.StandardActions _standardActions;

        private void Start() {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false; // 隐藏鼠标
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
            var rightStick = _standardActions.Look.ReadValue<Vector2>();
            _yaw += lookSpeedH * rightStick.x;
            _pitch -= lookSpeedV * rightStick.y;
            transform.eulerAngles = new Vector3(_pitch, _yaw, 0f);
        }
    }
}