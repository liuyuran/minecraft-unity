using System.Collections.Generic;
using Base.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using UnityEngine;
using Material = UnityEngine.Material;
using Vector3 = UnityEngine.Vector3;

namespace Managers {
    /// <summary>
    /// 用于预先生成虚拟方块模型及其配套的贴图、渲染边界、碰撞体等数据的管理器
    /// </summary>
    public class SubMeshCacheManager {
        public static SubMeshCacheManager Instance { get; } = new();
        public readonly AABB RenderEdge; // 通用方块渲染边界，决定Camera在特定视角下的剔除范围
        public readonly PhysicsCollider Collider; // 通用方块碰撞体，用于正确激活Raycast
        private readonly Dictionary<int, string> _meshCache = new();
        private readonly Dictionary<string, int> _meshCacheRev = new();
        private readonly Dictionary<string, MaterialMeshInfo> _meshPrefabs = new();

        private SubMeshCacheManager() {
            var hybridRenderer =
                World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntitiesGraphicsSystem>();
            const int max = Chunk.Up | Chunk.Down | Chunk.Left | Chunk.Right | Chunk.Front | Chunk.Back;
            var blocks = BlockTypeManager.Instance.GetBlockIds();
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) {
                mainTexture = BlockTypeManager.Instance.GetMergedTexture(),
                enableInstancing = true,
            };
            var materialID = hybridRenderer.RegisterMaterial(material);
            for (var i = 0; i < max + 1; i++) {
                for (var index = 0; index < blocks.Length; index++) {
                    var blockId = blocks[index];
                    _meshCache[index] = blockId;
                    _meshCacheRev[blockId] = index;
                    var mesh = GenerateCubeMeshInfo(i);
                    ApplyUV(mesh, blockId, i);
                    var meshID = hybridRenderer.RegisterMesh(mesh);
                    _meshPrefabs.Add($"cube:{blockId}:{i}", new MaterialMeshInfo {
                        Material = (int)materialID.value,
                        Mesh = (int)meshID.value
                    });
                    if (i != max || index != 0) continue;
                    RenderEdge = mesh.bounds.ToAABB();
                    Collider = GenerateCollider(mesh);
                }
            }
        }

        /// <summary>
        /// 根据已生成的Mesh生成Collider
        /// 其实有更高效的unsafe内存拷贝方法，但是unsafe关键字的污染性太强了，还是不要用
        /// </summary>
        /// <param name="mesh">Mesh对象</param>
        /// <returns>Collider对象</returns>
        private static PhysicsCollider GenerateCollider(Mesh mesh) {
            NativeArray<float3> meshVertices;
            NativeArray<int3> meshTris;
            using (meshTris = new NativeArray<int3>(mesh.triangles.Length / 3, Allocator.Temp))
            using (meshVertices = new NativeArray<float3>(mesh.vertices.Length, Allocator.Temp)) {
                var index = 0;
                foreach (var v in mesh.vertices)
                    meshVertices[index++] = v;
                index = 0;
                for (var i = 0; i < mesh.triangles.Length; i += 3)
                    meshTris[index++] = new int3(mesh.triangles[i], mesh.triangles[i + 1],
                        mesh.triangles[i + 2]);

                var meshColliderReference =
                    Unity.Physics.MeshCollider.Create(meshVertices, meshTris,
                        CollisionFilter.Default, Unity.Physics.Material.Default);
                return new PhysicsCollider { Value = meshColliderReference };
            }
        }

        /// <summary>
        /// 根据渲染标记生成特殊Mesh
        /// </summary>
        /// <param name="renderFlag">按位运算出来的渲染标记</param>
        /// <returns>Mesh对象</returns>
        private static Mesh GenerateCubeMeshInfo(int renderFlag) {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            if ((renderFlag & Chunk.Up) > 0) {
                vertices.AddRange(new Vector3[] {
                    new(0, 1, 0),
                    new(0, 1, 1),
                    new(1, 1, 1),
                    new(1, 1, 0)
                });
                triangles.AddRange(new[] {
                    vertices.Count - 4,
                    vertices.Count - 3,
                    vertices.Count - 2,
                    vertices.Count - 2,
                    vertices.Count - 1,
                    vertices.Count - 4
                });
            }

            if ((renderFlag & Chunk.Down) > 0) {
                vertices.AddRange(new Vector3[] {
                    new(0, 0, 1),
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(1, 0, 1)
                });
                triangles.AddRange(new[] {
                    vertices.Count - 4,
                    vertices.Count - 3,
                    vertices.Count - 2,
                    vertices.Count - 2,
                    vertices.Count - 1,
                    vertices.Count - 4
                });
            }

            if ((renderFlag & Chunk.Left) > 0) {
                vertices.AddRange(new Vector3[] {
                    new(0, 0, 1),
                    new(0, 1, 1),
                    new(0, 1, 0),
                    new(0, 0, 0)
                });
                triangles.AddRange(new[] {
                    vertices.Count - 4,
                    vertices.Count - 3,
                    vertices.Count - 2,
                    vertices.Count - 2,
                    vertices.Count - 1,
                    vertices.Count - 4
                });
            }

            if ((renderFlag & Chunk.Right) > 0) {
                vertices.AddRange(new Vector3[] {
                    new(1, 0, 0),
                    new(1, 1, 0),
                    new(1, 1, 1),
                    new(1, 0, 1)
                });
                triangles.AddRange(new[] {
                    vertices.Count - 4,
                    vertices.Count - 3,
                    vertices.Count - 2,
                    vertices.Count - 2,
                    vertices.Count - 1,
                    vertices.Count - 4
                });
            }

            if ((renderFlag & Chunk.Front) > 0) {
                vertices.AddRange(new Vector3[] {
                    new(0, 0, 0),
                    new(0, 1, 0),
                    new(1, 1, 0),
                    new(1, 0, 0)
                });
                triangles.AddRange(new[] {
                    vertices.Count - 4,
                    vertices.Count - 3,
                    vertices.Count - 2,
                    vertices.Count - 2,
                    vertices.Count - 1,
                    vertices.Count - 4
                });
            }

            if ((renderFlag & Chunk.Back) > 0) {
                vertices.AddRange(new Vector3[] {
                    new(1, 0, 1),
                    new(1, 1, 1),
                    new(0, 1, 1),
                    new(0, 0, 1)
                });
                triangles.AddRange(new[] {
                    vertices.Count - 4,
                    vertices.Count - 3,
                    vertices.Count - 2,
                    vertices.Count - 2,
                    vertices.Count - 1,
                    vertices.Count - 4
                });
            }

            var mesh = new Mesh {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray()
            };
            return mesh;
        }

        /// <summary>
        /// 将UV信息应用到Mesh上
        /// </summary>
        /// <param name="mesh">Mesh对象</param>
        /// <param name="blockId">方块ID</param>
        /// <param name="renderFlag">渲染标记</param>
        private static void ApplyUV(Mesh mesh, string blockId, int renderFlag) {
            var uvs = new List<Vector2>();
            if ((renderFlag & Chunk.Up) > 0) {
                uvs.AddRange(BlockTypeManager.Instance.GetBlockTexture(blockId, Direction.up));
            }

            if ((renderFlag & Chunk.Down) > 0) {
                uvs.AddRange(BlockTypeManager.Instance.GetBlockTexture(blockId, Direction.down));
            }

            if ((renderFlag & Chunk.Left) > 0) {
                uvs.AddRange(BlockTypeManager.Instance.GetBlockTexture(blockId, Direction.west));
            }

            if ((renderFlag & Chunk.Right) > 0) {
                uvs.AddRange(BlockTypeManager.Instance.GetBlockTexture(blockId, Direction.east));
            }

            if ((renderFlag & Chunk.Front) > 0) {
                uvs.AddRange(BlockTypeManager.Instance.GetBlockTexture(blockId, Direction.south));
            }

            if ((renderFlag & Chunk.Back) > 0) {
                uvs.AddRange(BlockTypeManager.Instance.GetBlockTexture(blockId, Direction.north));
            }

            mesh.SetUVs(0, uvs.ToArray());
            mesh.RecalculateNormals();
        }

        /// <summary>
        /// 获取覆写材质和顶点信息所需的Component对象
        /// </summary>
        /// <param name="blockId">方块ID</param>
        /// <param name="renderFlag">渲染标记</param>
        /// <returns>记载模型顶点信息和材质信息的Component</returns>
        public MaterialMeshInfo GetCubeMesh(int blockId, int renderFlag) {
            return _meshPrefabs[$"cube:{_meshCache[blockId]}:{renderFlag}"];
        }

        /// <summary>
        /// 用于转换字符串版本和数字版本的方块ID
        /// </summary>
        /// <param name="blockId">字符串版本方块ID</param>
        /// <returns>数字版本的方块ID</returns>
        public int GetMeshId(string blockId) {
            return _meshCacheRev[blockId];
        }
    }
}