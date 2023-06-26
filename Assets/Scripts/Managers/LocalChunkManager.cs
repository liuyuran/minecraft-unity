using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Base.Const;
using UnityEngine;

namespace Managers {
    /// <summary>
    /// 用于快速索引归属于一个Chunk的方块
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class LocalChunkManager {
        public static LocalChunkManager Instance { get; } = new();
        private readonly ConcurrentDictionary<Vector3, long> _chunkVersionCache = new();

        private LocalChunkManager() { }
        
        public void AddChunkVersion(Vector3 pos, long version) {
            _chunkVersionCache[pos] = version;
        }
        
        public long GetChunkVersion(Vector3 pos) {
            if (!_chunkVersionCache.TryGetValue(pos, out _)) {
                return -1;
            }
            return _chunkVersionCache[pos];
        }
        
        public IEnumerable<Vector3> AutoUnloadChunk(Vector3 playerPos) {
            var position = playerPos + new Vector3();
            position.x = (float)Math.Round(position.x / ParamConst.ChunkSize);
            position.y = (float)Math.Round(position.y / ParamConst.ChunkSize);
            position.z = (float)Math.Round(position.z / ParamConst.ChunkSize);
            var allChunks = new HashSet<Vector3>(_chunkVersionCache.Keys);
            for (var x = -ParamConst.DisplayDistance; x <= ParamConst.DisplayDistance; x++) {
                for (var y = -ParamConst.DisplayDistance; y <= ParamConst.DisplayDistance; y++) {
                    for (var z = -ParamConst.DisplayDistance; z <= ParamConst.DisplayDistance; z++) {
                        allChunks.Remove(position + new Vector3(x, y, z));
                    }
                }
            }
            return allChunks;
        }
        
        public void RemoveChunks(HashSet<Vector3> chunks) {
            foreach (var chunk in chunks) {
                _chunkVersionCache.Remove(chunk, out _);
            }
        }
    }
}