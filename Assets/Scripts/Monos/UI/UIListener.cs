using System.Threading;
using Base;
using Base.Events.ClientEvent;
using Base.Manager;
using Const;
using Managers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Monos.UI {
    public partial class UIManager {
        private void MainMenuListener(ref TemplateContainer ui) {
            ui.Query<Button>("single-player").First().clicked += () => {
                // 开始游戏，跳转界面将在Mono脚本的Update回调中完成
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
            ui.Query<Button>("mods").First().clicked += () => {
                // 关于
                OpenUI("mods");
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

        private void OptionListener(ref TemplateContainer ui) {
            ui.Query<Button>("close").First().clicked += () => {
                // 关于
                CloseUI("option");
            };
        }

        private void ModsListener(ref TemplateContainer ui) {
            //
        }
    }
}