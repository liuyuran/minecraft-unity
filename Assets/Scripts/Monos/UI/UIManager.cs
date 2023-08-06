using System;
using System.Collections.Generic;
using Base.Manager;
using Const;
using Managers;
using UnityEngine;
using UnityEngine.UIElements;

namespace Monos.UI {
    public delegate void UIListenerDelegate(ref TemplateContainer ui);

    /// <summary>
    /// UI管理器
    /// 由于Unity DOTS并未提供原生的UI解决方案，所以只好用UI Toolkit来实现
    /// 不可改成UGUI，那玩意性能消耗太大，而且不符合完全迎合Unity官方建议的初衷
    /// </summary>
    public partial class UIManager : MonoBehaviour {
        [SerializeField] public UIDocument uiDocument;
        private GameState? _nowState;
        private readonly IDictionary<string, TemplateContainer> _uxmlLink = new Dictionary<string, TemplateContainer>();
        
        private void Awake() {
            _uxmlLink.Clear();
            // 初始化mod管理器
            ModManager.Instance.ScanAllMod();
            LogManager.Instance.Info("扩展模组扫描完成");
            // TODO 预期通过MODManager动态读取自定义UI
            RegistryUI("main-menu", "UI/menu/main-menu.uxml", MainMenuListener);
            RegistryUI("loading", "UI/public/loading.uxml", null);
            RegistryUI("playing", "UI/in-game/playing.uxml", PlayingListener);
            RegistryUI("option", "UI/menu/option.uxml", OptionListener);
            RegistryUI("mods", "UI/menu/mods.uxml", ModsListener);
        }

        /// <summary>
        /// 获取并更新相对于屏幕的UI
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">当State出现意外值的时候</exception>
        private void UpdateUIDocument() {
            var targetState = GameManager.Instance.State;
            if (_nowState == targetState || !uiDocument.isActiveAndEnabled) return;
            switch (targetState) {
                case GameState.Menu:
                    JumpUI("main-menu");
                    break;
                case GameState.Loading:
                    JumpUI("loading");
                    break;
                case GameState.Playing:
                    JumpUI("playing");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _nowState = targetState;
        }

        private void Update() {
            UpdateUIDocument();
        }
    }
}