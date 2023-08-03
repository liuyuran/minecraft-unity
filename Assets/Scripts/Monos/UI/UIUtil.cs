using Exceptions;
using UnityEditor;
using UnityEngine.UIElements;

namespace Monos.UI {
    public partial class UIManager {
        /// <summary>
        /// 注册UI界面
        /// </summary>
        /// <param name="uiName">UI名字</param>
        /// <param name="path">UI的uxml描述文件地址</param>
        /// <param name="listener">UI后处理器</param>
        private void RegistryUI(string uiName, string path, UIListenerDelegate listener) {
            if (_uxmlLink.ContainsKey(uiName)) throw new DuplicateUIException(uiName);
            var uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Assets/{path}");
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
    }
}