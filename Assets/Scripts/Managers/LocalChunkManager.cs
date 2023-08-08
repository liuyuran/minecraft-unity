using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Base.Components;
using Base.Const;
using Base.Utils;
using Utils;
using Vector3 = UnityEngine.Vector3;

namespace Managers {
    /// <summary>
    /// 用于快速索引归属于一个Chunk的方块
    /// 由于这里的数据和游戏世界高度关联，所以不需要引入WorldId的概念
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class LocalChunkManager {
        public static LocalChunkManager Instance { get; } = new();
        private readonly ConcurrentDictionary<Vector3, long> _chunkVersionCache = new();
        private readonly ConcurrentDictionary<Vector3, Chunk> _chunkCache = new();
        private readonly ConcurrentDictionary<Vector3, Dictionary<Vector3, List<DroppedItem>>> _itemCache = new();

        private LocalChunkManager() { }
        
        public void AddChunkVersion(Vector3 pos, Chunk chunk) {
            _chunkVersionCache[pos] = chunk.Version;
            _chunkCache[pos] = DeepCopyHelper.DeepCopy(chunk);
        }
        
        public long GetChunkVersion(Vector3 pos) {
            if (!_chunkVersionCache.TryGetValue(pos, out _)) {
                return -1;
            }
            return _chunkVersionCache[pos];
        }
        
        public Chunk GetChunk(Vector3 pos) {
            return !_chunkCache.TryGetValue(pos, out _) ? null : _chunkCache[pos];
        }
        
        public void AddItem(Vector3 pos, Dictionary<Vector3, List<DroppedItem>> item) {
            _itemCache[pos] = item;
        }
        
        public Dictionary<Vector3, List<DroppedItem>> GetItem(Vector3 pos) {
            return !_itemCache.TryGetValue(pos, out _) ? null : _itemCache[pos];
        }
        
        public HashSet<Vector3> AutoUnloadChunk(Vector3 playerPos) {
            var position = playerPos + new Vector3();
            position.x = (float)Math.Round(position.x / ParamConst.ChunkSize);
            position.y = (float)Math.Round(position.y / ParamConst.ChunkSize);
            position.z = (float)Math.Round(position.z / ParamConst.ChunkSize);
            var allChunks = new HashSet<Vector3>(_chunkVersionCache.Keys);
            for (var x = -ParamConst.DisplayDistance - 1; x <= ParamConst.DisplayDistance + 1; x++) {
                for (var y = -ParamConst.DisplayDistance - 1; y <= ParamConst.DisplayDistance + 1; y++) {
                    for (var z = -ParamConst.DisplayDistance - 1; z <= ParamConst.DisplayDistance + 1; z++) {
                        allChunks.Remove(position + new Vector3(x, y, z));
                    }
                }
            }
            return allChunks;
        }
        
        public void RemoveChunks(HashSet<Vector3> chunks) {
            foreach (var chunk in chunks) {
                _chunkVersionCache.Remove(chunk, out _);
                _chunkCache.Remove(chunk, out _);
                _itemCache.Remove(chunk, out _);
            }
        }
        
        public void Clear() {
            _chunkVersionCache.Clear();
            _chunkCache.Clear();
            _itemCache.Clear();
        }
    }
}