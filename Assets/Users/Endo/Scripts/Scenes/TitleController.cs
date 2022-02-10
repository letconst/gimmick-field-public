using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

public class TitleController : MonoBehaviour
{
    private bool _isPressed;

    private SwitchInputController _inputController;

    private async void Start()
    {
        _inputController = SwitchInputController.Instance;

        // それぞれボタン押下時にチュートリアルシーンへ遷移
        _inputController.OnClickABXYButtonSubject.Subscribe(OnButtonPressed).AddTo(this);
        _inputController.OnClickGrabButtonSubject.Subscribe(OnButtonPressed).AddTo(this);

        // SoundManagerが初期化されるまで待機
        await UniTask.WaitUntil(() => SoundManager.IsInitialized);

        SoundManager.StopAll();
        SoundManager.PlayMusic(MusicDef.Title_BGM, isLoop: true);
    }

    /// <summary>
    /// チュートリアルシーンへ遷移する
    /// </summary>
    /// <param name="_"></param>
    /// <typeparam name="T"></typeparam>
    private async void OnButtonPressed<T>(T _)
    {
        // タイトルへ遷移された際、フェードを待機
        if (!_isPressed && SystemSceneManager.IsLoading)
        {
            await UniTask.WaitWhile(() => SystemSceneManager.IsLoading);
        }

        // まだボタンが押されてないときのみ処理
        if (_isPressed) return;

        _isPressed = true;

        SystemSceneManager.LoadNextScene("TutorialV2", SceneTransition.Fade);
    }
}
