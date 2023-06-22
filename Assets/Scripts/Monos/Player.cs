using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Monos {
    public class Player: MonoBehaviour {
        private float _mXRotation;
        private const float MoveSpeedScale = 0.05f;
        private const float ScaleScale = 1.5f;
        private const float MaxControlDistance = 5.0f;
        public Camera firstPerson;
        public Camera thirdPerson;

        private void Start() {
            firstPerson.enabled = true;
            thirdPerson.enabled = false;
        }

        /// <summary>
        /// 获取镜头指向的物体
        /// </summary>
        /// <returns>指向的物体，若无法获取，返回null</returns>
        private GameObject GetPointToSomeone() {
            var ray = firstPerson.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, MaxControlDistance)) return null;
            var objectHit = hit.transform.gameObject;
            return objectHit;
        }

        private void OnCollisionEnter(Collision other) {
            Debug.Log("debug");
        }

        private void Update() {
            var gamepad = InputSystem.GetDevice<Gamepad>();
            if (gamepad == null) return;
            var playerTransform = transform;
            var transformF = firstPerson.transform;
            var transformT = thirdPerson.transform;
            // 通过左摇杆控制移动
            var leftStick = gamepad.leftStick.ReadValue();
            var move = new Vector3(leftStick.x, 0, leftStick.y);
            playerTransform.position += playerTransform.rotation * move * MoveSpeedScale + 
                                        transformF.rotation * move * MoveSpeedScale;
            // 通过右摇杆控制上下左右的面向，似乎左右需要操作玩家对象，而上下则需要操作镜头
            var rightStick = gamepad.rightStick.ReadValue() * ScaleScale;
            _mXRotation -= rightStick.y;
            _mXRotation = Mathf.Clamp(_mXRotation, -90f, 90f);
            transformF.localRotation = Quaternion.Euler(_mXRotation, 0f, 0f);
            transformT.localRotation = Quaternion.Euler(_mXRotation, 0f, 0f);
            transform.Rotate(Vector3.up * rightStick.x);
            if (gamepad.yButton.isPressed) {
                playerTransform.position += Vector3.up * MoveSpeedScale;
            }
            if (gamepad.bButton.isPressed) {
                playerTransform.position -= Vector3.up * MoveSpeedScale;
            }
            // 通过左右扳机进行交互
            if (gamepad.rightTrigger.isPressed) {
                var target = GetPointToSomeone();
                if (target != null) {
                    Debug.Log(target.GetInstanceID());
                }
            }
        }
    }
}