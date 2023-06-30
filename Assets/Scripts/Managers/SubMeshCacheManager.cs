using System.Collections.Generic;
using Base.Utils;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using Utils;

namespace Managers {
    public class SubMeshCacheManager {
        public static SubMeshCacheManager Instance { get; } = new();
        public readonly AABB RenderEdge;
        private readonly Dictionary<int, string> _meshCache = new();
        private readonly Dictionary<string, int> _meshCacheRev = new();
        private readonly Dictionary<string, MaterialMeshInfo> _meshPrefabs = new();

        private SubMeshCacheManager() {
            var hybridRenderer = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntitiesGraphicsSystem>();
            const int max = Chunk.Up | Chunk.Down | Chunk.Left | Chunk.Right | Chunk.Front | Chunk.Back;
            var blocks = BlockTypeManager.Instance.GetBlockIds();
            var materialID = hybridRenderer.RegisterMaterial(new Material(Shader.Find("Universal Render Pipeline/Lit")) {
                mainTexture = BlockTypeManager.Instance.GetMergedTexture(),
                enableInstancing = true
            });
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
                    if (i == max && index == 0) {
                        RenderEdge = mesh.bounds.ToAABB();
                    }
                }
            }
        }

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
        
        public MaterialMeshInfo GetCubeMesh(int blockId, int renderFlag) {
            return _meshPrefabs[$"cube:{_meshCache[blockId]}:{renderFlag}"];
        }
        
        public int GetMeshId(string blockId) {
            return _meshCacheRev[blockId];
        }
    }
}