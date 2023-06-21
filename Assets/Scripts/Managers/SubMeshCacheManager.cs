using System.Collections.Generic;
using Base.Utils;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using Utils;

namespace Managers {
    public class SubMeshCacheManager {
        public static SubMeshCacheManager Instance { get; } = new();
        private readonly Dictionary<string, RenderMeshArray> _meshPrefabs = new();
        private readonly Dictionary<string, RenderBounds> _meshRenderPrefabs = new();

        private readonly Material _material = new(Shader.Find("Universal Render Pipeline/Lit")) {
            mainTexture = BlockTypeManager.Instance.GetMergedTexture()
        };

        private SubMeshCacheManager() {
            const int max = Chunk.Up | Chunk.Down | Chunk.Left | Chunk.Right | Chunk.Front | Chunk.Back;
            var blocks = BlockTypeManager.Instance.GetBlockIds();
            for (var i = 0; i < max + 1; i++) {
                foreach (var blockId in blocks) {
                    var mesh = GenerateCubeMeshInfo(i);
                    ApplyUV(mesh, blockId, i);
                    var arr = new RenderMeshArray(
                        new[] { _material },
                        new[] { mesh }
                    );
                    _meshPrefabs.Add($"cube:{blockId}:{i}", arr);
                    _meshRenderPrefabs.Add($"cube:{blockId}:{i}", new RenderBounds {
                        Value = arr.Meshes[0].bounds.ToAABB()
                    });
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
        
        public RenderMeshArray GetCubeMesh(string blockId, int renderFlag) {
            return _meshPrefabs[$"cube:{blockId}:{renderFlag}"];
        }
        
        public RenderBounds GetCubeMeshRender(string blockId, int renderFlag) {
            return _meshRenderPrefabs[$"cube:{blockId}:{renderFlag}"];
        }
    }
}