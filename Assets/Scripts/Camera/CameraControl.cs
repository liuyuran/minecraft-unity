using UnityEngine;
using UnityEngine.InputSystem;

namespace Camera {
    public class CameraControl : MonoBehaviour {
        public float lookSpeedH = 2f;
        public float lookSpeedV = 2f;

        private float _yaw;
        private float _pitch;


        private void Start() {
            var eulerAngles = transform.eulerAngles;
            _yaw = eulerAngles.y;
            _pitch = eulerAngles.x;
        }

        private void Update() {
            var gamepad = InputSystem.GetDevice<Gamepad>();
            if (gamepad == null) return;
            var rightStick = gamepad.rightStick.ReadValue();
            _yaw += lookSpeedH * rightStick.x;
            _pitch -= lookSpeedV * rightStick.y;
            transform.eulerAngles = new Vector3(_pitch, _yaw, 0f);
        }
    }
}