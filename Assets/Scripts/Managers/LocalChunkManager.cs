﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Base.Const;
using Unity.Entities;
using UnityEngine;

namespace Managers {
    /// <summary>
    /// 用于快速索引归属于一个Chunk的方块
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class LocalChunkManager {
        public static LocalChunkManager Instance { get; } = new();
        private readonly ConcurrentDictionary<Vector3, List<Entity>> _chunkCache = new();

        private LocalChunkManager() { }

        public void AddChunk(Vector3 pos, List<Entity> entities) {
            if (_chunkCache.TryGetValue(pos, out var value)) {
                value.AddRange(entities);
                return;
            }
            _chunkCache[pos] = entities;
        }
        
        public void AddChunk(Vector3 pos, Entity entity) {
            if (_chunkCache.TryGetValue(pos, out var value)) {
                value.Add(entity);
                return;
            }
            _chunkCache[pos] = new List<Entity> {entity};
        }
        
        public List<Entity> GetChunk(Vector3 pos) {
            return _chunkCache[pos];
        }

        public void RemoveChunk(Vector3 pos) {
            _chunkCache.TryRemove(pos, out _);
        }
        
        private void UnloadChunk(EntityManager manager, Vector3 pos) {
            var chunk = GetChunk(pos);
            foreach (var entity in chunk) {
                manager.DestroyEntity(entity);
            }
            RemoveChunk(pos);
        }
        
        public void AutoUnloadChunk(EntityManager manager, Vector3 playerPos) {
            var allChunks = new HashSet<Vector3>(_chunkCache.Keys);
            for (var x = -ParamConst.DisplayDistance; x <= ParamConst.DisplayDistance; x++) {
                for (var y = -ParamConst.DisplayDistance; y <= ParamConst.DisplayDistance; y++) {
                    for (var z = -ParamConst.DisplayDistance; z <= ParamConst.DisplayDistance; z++) {
                        allChunks.Remove(playerPos + new Vector3(x, y, z));
                    }
                }
            }
            foreach (var pos in allChunks) {
                UnloadChunk(manager, pos);
            }
        }
    }
}