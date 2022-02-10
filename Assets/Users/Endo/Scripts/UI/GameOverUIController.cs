using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameOverUIController : MonoBehaviour
{
    [SerializeField, Header("リトライボタンのインスタンス")]
    private Button retryBtn;

    [SerializeField, Header("タイトルへ戻るボタンのインスタンス")]
    private Button toTitleBtn;

    private bool _isClickedAny;

    private void Start()
    {
        // 各種ボタン初期化
        InitButton(retryBtn, OnClickedRetry);
        InitButton(toTitleBtn, OnClickedToTitle);

        // ゲームオーバー時の処理登録
        Gamemaneger.Instance.OnGameOver.Subscribe(OnGameStateChanged).AddTo(this);
    }

    /// <summary>
    /// ボタンの初期化処理
    /// </summary>
    /// <param name="btn">対象のボタン</param>
    /// <param name="callback">ボタン押下時に実行するメソッド</param>
    private static void InitButton(Button btn, UnityAction callback)
    {
        if (!btn) return;

        btn.interactable = false;

        if (callback != null)
        {
            btn.onClick.AddListener(callback);
        }
    }

    /// <summary>
    /// リトライボタン押下時の処理
    /// </summary>
    private async void OnClickedRetry()
    {
        if (_isClickedAny) return;

        _isClickedAny           = true;
        toTitleBtn.interactable = false; // 反対側のボタンを無効化

        await UniTask.Delay(System.TimeSpan.FromSeconds(1));

        SystemSceneManager.LoadNextScene(SystemSceneManager.GetCurrentSceneName(), SceneTransition.Fade).Forget();
    }

    /// <summary>
    /// タイトルへ戻るボタン押下時の処理
    /// </summary>
    private async void OnClickedToTitle()
    {
        if (_isClickedAny) return;

        _isClickedAny         = true;
        retryBtn.interactable = false; // 反対側のボタンを無効化

        await UniTask.Delay(System.TimeSpan.FromSeconds(1));

        SystemSceneManager.LoadNextScene("Title", SceneTransition.Fade).Forget();
    }

    /// <summary>
    /// ゲームオーバー時の処理
    /// </summary>
    /// <param name="_"></param>
    private void OnGameStateChanged(Unit _)
    {
        // ボタン操作可能に
        if (retryBtn) retryBtn.interactable     = true;
        if (toTitleBtn) toTitleBtn.interactable = true;

        // リトライボタンにフォーカス設定
        retryBtn.Select();
        retryBtn.OnSelect(null); // ハイライトされないためnull実行
    }
}
