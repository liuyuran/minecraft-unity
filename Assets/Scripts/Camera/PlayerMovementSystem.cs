using Base.Const;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Camera {
    [BurstCompile]
    public partial class PlayerMovementSystem : SystemBase {
        private KeyActionSettings _keyActionSettings;
        private KeyActionSettings.StandardActions _standardActions;
        protected override void OnCreate() {
            _keyActionSettings = new KeyActionSettings();
            _keyActionSettings.Enable();
            _standardActions = _keyActionSettings.standard;
        }

        protected override void OnDestroy() {
            _keyActionSettings.Dispose();
        }

        [BurstCompile]
        protected override void OnUpdate() {
            Entities.WithAll<Player, Self>()
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
            var leftStick = _standardActions.Move.ReadValue<Vector2>();
            var jump = _standardActions.Jump.triggered;
            Entities.WithAll<Player, Self>().ForEach(
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

                    if (jump) {
                        vel.Linear.y = localToWorld.Up.y * 2 * speed;
                    }

                    vel.Linear.xz = playerPos;
                }
            ).WithoutBurst().Run();
            cameraTransform.position = cameraPos;
            Entities.WithAll<Player, Self>()
                .ForEach((ref Player player) => {
                    player.Pos = cameraPos;
                }).Run();
        }
    }
}