using UniRx;
using UnityEngine;

public class TitleController : MonoBehaviour
{
    private bool _isPressed;

    private SwitchInputController _inputController;

    private void Start()
    {
        _inputController = SwitchInputController.Instance;

        // それぞれボタン押下時にチュートリアルシーンへ遷移
        _inputController.OnClickABXYButtonSubject.Subscribe(OnButtonPressed);
        _inputController.OnClickGrabButtonSubject.Subscribe(OnButtonPressed);
    }

    /// <summary>
    /// チュートリアルシーンへ遷移する
    /// </summary>
    /// <param name="_"></param>
    /// <typeparam name="T"></typeparam>
    private void OnButtonPressed<T>(T _)
    {
        // まだボタンが押されてないときのみ処理
        if (_isPressed) return;

        _isPressed = true;

        SystemSceneManager.LoadNextScene("TutorialAlpha", SceneTransition.Fade);
    }
}
