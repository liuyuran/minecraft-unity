using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Camera {
    public partial class PlayerMovementSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.WithAll<Player>()
                .ForEach((ref PhysicsMass mass) => {
                    mass.InverseInertia.x = 0;
                    mass.InverseInertia.z = 0;
                }).Run();
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            const float speed = 3f;
            var cameraTransform = CameraLink.Instance.transform;
            var cameraPos = cameraTransform.position;
            var playerPos = new float2(0, 0);
            var cameraRotation = cameraTransform.rotation.eulerAngles;
            var currentQuaternion = Quaternion.Euler(0, cameraRotation.y, 0);
            var gamepad = InputSystem.GetDevice<Gamepad>();
            if (gamepad == null) return;
            var leftStick = gamepad.leftStick.ReadValue();
            Entities.WithAll<Player>()
                .ForEach(
                    (Entity entity, ref LocalToWorld localToWorld, ref PhysicsVelocity vel, ref PhysicsMass mass) => {
                        mass.InverseInertia.x = 0;
                        mass.InverseInertia.z = 0;
                        cameraPos = localToWorld.Position + new float3(0, 1.5f, 0);
                        entityManager.SetComponentData(entity, new LocalToWorld {
                            Value = float4x4.TRS(
                                localToWorld.Position,
                                currentQuaternion,
                                new float3(1, 1, 1)
                            )
                        });
                        if (leftStick.y != 0) {
                            playerPos = localToWorld.Forward.xz * leftStick.y * speed;
                        }

                        if (leftStick.x != 0) {
                            playerPos += localToWorld.Right.xz * leftStick.x * speed;
                        }

                        if (gamepad.yButton.isPressed) {
                            vel.Linear.y = localToWorld.Up.y * speed;
                        }

                        vel.Linear.xz = playerPos;
                    }).WithoutBurst().Run();
            cameraTransform.position = cameraPos;
        }
    }
}