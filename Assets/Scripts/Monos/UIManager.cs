using System;
using System.Collections.Generic;
using System.Threading;
using Base;
using Base.Events.ClientEvent;
using Base.Manager;
using Const;
using Exceptions;
using Managers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Monos {
    public delegate void UIListenerDelegate(ref TemplateContainer ui);
    /// <summary>
    /// UI管理器
    /// 由于Unity DOTS并未提供原生的UI解决方案，所以只好用UI Toolkit来实现
    /// 不可改成UGUI，那玩意性能消耗太大，而且不符合完全迎合Unity官方建议的初衷
    /// </summary>
    public class UIManager: MonoBehaviour {
        private UIDocument _uiDocument;
        private GameState? _nowState;
        private readonly IDictionary<string, TemplateContainer> _uxmlLink = new Dictionary<string, TemplateContainer>();

        /// <summary>
        /// 注册UI界面
        /// </summary>
        /// <param name="uiName">UI名字</param>
        /// <param name="path">UI的uxml描述文件地址</param>
        /// <param name="listener">UI后处理器</param>
        private void RegistryUI(string uiName, string path, UIListenerDelegate listener) {
            if (_uxmlLink.ContainsKey(uiName)) throw new DuplicateUIException(uiName);
            var uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            var ui = uiAsset.Instantiate();
            listener?.Invoke(ref ui);
            _uxmlLink.Add(uiName, ui);
        }
        
        /// <summary>
        /// 覆盖目前已有的UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        private void JumpUI(string uiName) {
            if (!_uxmlLink.ContainsKey(uiName)) throw new UINotFoundException(uiName);
            _uiDocument.rootVisualElement.Clear();
            _uiDocument.rootVisualElement.Add(_uxmlLink[uiName]);
        }
        
        /// <summary>
        /// 额外打开新的ui
        /// </summary>
        /// <param name="uiName">UI名字</param>
        private void OpenUI(string uiName) {
            if (!_uxmlLink.ContainsKey(uiName)) throw new UINotFoundException(uiName);
            _uiDocument.rootVisualElement.Add(_uxmlLink[uiName]);
        }
        
        /// <summary>
        /// 关闭指定UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        private void CloseUI(string uiName) {
            if (!_uxmlLink.ContainsKey(uiName)) throw new UINotFoundException(uiName);
            _uiDocument.rootVisualElement.Remove(_uxmlLink[uiName]);
        }
        
        private void Awake() {
            _uxmlLink.Clear();
            RegistryUI("MainMenu", "UI Toolkit/StartMenu.uxml", MainMenuListener);
            RegistryUI("Loading", "UI Toolkit/StartMenu.uxml", null);
            RegistryUI("Playing", "UI Toolkit/StartMenu.uxml", PlayingListener);
        }

        private void MainMenuListener(ref TemplateContainer ui) {
            ui.Query<Button>("start").First().clicked += () => {
                // 开始游戏，但按理说这里还应该跳转到另一个界面，但暂时忽略掉
                GameManager.Instance.SetState(GameState.Loading);
                new Thread(() => { Game.Start(""); }).Start();
                Thread.Sleep(1000);
                CommandTransferManager.NetworkAdapter?.SendToServer(new PlayerJoinEvent {
                    Nickname = "Kamoeth"
                });
            };
            ui.Query<Button>("option").First().clicked += () => {
                // 选项
                OpenUI("option");
            };
            ui.Query<Button>("achievement").First().clicked += () => {
                // 成就
                OpenUI("achievement");
            };
            ui.Query<Button>("exit").First().clicked += () => {
                // 退出
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
                Application.Quit();
            };
        }
        
        private void PlayingListener(ref TemplateContainer ui) {
            //
        }

        /// <summary>
        /// 获取并更新相对于屏幕的UI
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">当State出现意外值的时候</exception>
        private void UpdateUIDocument() {
            GameState targetState = GameManager.Instance.State;
            if (_nowState == targetState) return;
            _uiDocument.rootVisualElement.Clear();
            switch (targetState) {
                case GameState.Menu:
                    JumpUI("MainMenu");
                    break;
                case GameState.Loading:
                    JumpUI("Loading");
                    break;
                case GameState.Playing:
                    JumpUI("Playing");
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