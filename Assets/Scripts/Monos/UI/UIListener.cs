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

        private void OptionListener(ref TemplateContainer ui) {
            //
        }

        private void AchievementListener(ref TemplateContainer ui) {
            //
        }
    }
}