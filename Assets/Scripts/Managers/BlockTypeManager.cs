using System;
using System.Collections.Generic;
using System.Linq;
using Base.Blocks;
using Exceptions;
using UnityEngine;
using UnityEngine.Windows;
using Utils;
using Block = Base.Blocks.Block;

namespace Managers {
    /// <summary>
    /// 方块类型映射管理器
    /// </summary>
    public class BlockTypeManager {
        public static BlockTypeManager Instance { get; } = new();
        private int _textureSize = 512;
        private int _totalWidth;
        private int _totalHeight;
        private readonly Dictionary<string, Type> _blockLink = new();
        private readonly Dictionary<string, Texture2D> _textureLink = new();
        private readonly Dictionary<string, Rect> _uvOffset = new();
        private bool _locked;
        private Texture2D _bigTexture;

        private BlockTypeManager() {
            RegisterBlock(new Air());
        }

        /// <summary>
        /// 注册方块类型
        /// </summary>
        /// <param name="obj">一个方块的实例</param>
        /// <exception cref="DuplicateBlockIdException">当重复注册同一个id时抛出此异常</exception>
        /// <exception cref="TextureLoadFailedException">方块贴图载入失败时抛出此异常</exception>
        /// <exception cref="BlockRegistryForbiddenException">游戏启动后注册方块会抛出此异常</exception>
        private void RegisterBlock(Block obj) {
            if (_locked) throw new BlockRegistryForbiddenException("出于性能考虑，游戏启动后禁止注册方块");
            var blockId = obj.ID;
            if (_blockLink.ContainsKey(blockId)) throw new DuplicateBlockIdException(blockId);
            _blockLink.Add(blockId, obj.GetType());
            var byteArray = File.ReadAllBytes($"{Application.dataPath}/Texture/{obj.Texture}");
            var texture = new Texture2D(_textureSize, _textureSize);
            var isLoaded = texture.LoadImage(byteArray);
            if (!isLoaded) throw new TextureLoadFailedException(blockId, obj.Texture);
            _textureLink.Add(blockId, texture);
        }
        
        private static Vector2 GenerateUVOffset(Rect baseVec, Vector2 offset) {
            var uvOffset = new Vector2 {
                x = baseVec.x + offset.x * baseVec.width,
                y = baseVec.y + offset.y * baseVec.height
            };
            return uvOffset;
        }

        /// <summary>
        /// 获取方块贴图在合成后大贴图中的UV坐标
        /// </summary>
        /// <param name="blockId">方块ID</param>
        /// <param name="direction">目标面</param>
        /// <returns>若该方块已注册，返回UV坐标</returns>
        public IEnumerable<Vector2> GetBlockTexture(string blockId, Direction direction) {
            if (!_uvOffset.ContainsKey(blockId)) throw new TextureNotFoundException(blockId);
            var value = _uvOffset[blockId];
            var uvs = new Vector2[4];
            switch (direction) {
                case Direction.north:
                    uvs[0] = new Vector2(0.667f, 0.0f);
                    uvs[1] = new Vector2(0.667f, 0.5f);
                    uvs[2] = new Vector2(1.0f, 0.5f);
                    uvs[3] = new Vector2(1.0f, 0.0f);
                    break;
                case Direction.south:
                    uvs[0] = new Vector2(0.0f, 0.0f);
                    uvs[1] = new Vector2(0.0f, 0.5f);
                    uvs[2] = new Vector2(0.333f, 0.5f);
                    uvs[3] = new Vector2(0.333f, 0.0f);
                    break;
                case Direction.east:
                    uvs[0] = new Vector2(0.667f, 0.5f);
                    uvs[1] = new Vector2(0.667f, 1.0f);
                    uvs[2] = new Vector2(1.0f, 1.0f);
                    uvs[3] = new Vector2(1.0f, 0.5f);
                    break;
                case Direction.west:
                    uvs[0] = new Vector2(0.334f, 0.5f);
                    uvs[1] = new Vector2(0.334f, 1.0f);
                    uvs[2] = new Vector2(0.666f, 1.0f);
                    uvs[3] = new Vector2(0.666f, 0.5f);
                    break;
                case Direction.up:
                    uvs[0] = new Vector2(0.334f, 0.0f);
                    uvs[1] = new Vector2(0.334f, 0.5f);
                    uvs[2] = new Vector2(0.666f, 0.5f);
                    uvs[3] = new Vector2(0.666f, 0.0f);
                    break;
                case Direction.down:
                    uvs[0] = new Vector2(0.0f, 0.5f);
                    uvs[1] = new Vector2(0.0f, 1.0f);
                    uvs[2] = new Vector2(0.333f, 1.0f);
                    uvs[3] = new Vector2(0.333f, 0.5f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
            return new [] {
                GenerateUVOffset(value, uvs[0]),
                GenerateUVOffset(value, uvs[1]),
                GenerateUVOffset(value, uvs[2]),
                GenerateUVOffset(value, uvs[3])
            };
        }
        
        /// <summary>
        /// 返回合成后的大贴图
        /// </summary>
        /// <returns>大贴图</returns>
        public Texture2D GetMergedTexture() {
            if (_bigTexture != null) return _bigTexture;
            // 锁定注册功能，不然大贴图就没有意义了
            _locked = true;
            // 计算新的大贴图所需要的面积，以及贴图列表
            Dictionary<string, int> textureIndex = new();
            const int width = 1;
            var height = (int) Math.Ceiling((double) _textureLink.Count / width);
            _totalWidth = width * _textureSize;
            _totalHeight = height * _textureSize;
            var texture2Ds = new Texture2D[_textureLink.Count];
            var index = 0;
            foreach (var (key, value) in _textureLink) {
                textureIndex[key] = index;
                texture2Ds[index] = value;
                index++;
            }

            // 清空贴图缓存，好歹释放一点点内存
            _textureLink.Clear();
            _bigTexture = new Texture2D(_totalWidth, _totalHeight);
            // 合成
            var rects = _bigTexture.PackTextures(texture2Ds, 0, width * _textureSize);
            // 下面的算法我猜的，在面积足够的前提下，unity不会进行贴图压缩，所以保留关键的x、y偏移量即可
            foreach (var (key, value) in textureIndex) {
                if (rects.Length <= value) continue;
                var rect = rects[value];
                _uvOffset.Add(key, rect);
            }
            return _bigTexture;
        }

        /// <summary>
        /// 设置贴图分辨率，需要在游戏开始前调用
        /// </summary>
        /// <param name="size">贴图尺寸，单位像素</param>
        public void SetTextureSize(int size) {
            _textureSize = size;
        }
        
        public string[] GetBlockIds() {
            return _uvOffset.Keys.ToArray();
        }
    }
}