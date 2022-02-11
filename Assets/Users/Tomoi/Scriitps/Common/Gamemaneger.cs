using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
using UnityEngine.UI;

public class Gamemaneger : SingletonMonoBehaviour<Gamemaneger>
{
    [Header("制限時間"), SerializeField]
    private float _countTime = 60;

    private float _maxTime;

    [SerializeField]
    private Text timerText;

    [SerializeField]
    private Image gameClearImg;

    [SerializeField]
    private Image gameOverImg;

    [SerializeField]
    private Image _hourGlassImage;

    [SerializeField]
    private Sprite[] _hourGlassSprites = new Sprite[5];

    [SerializeField]
    private Player player;

    [SerializeField, Range(0, 4), Header("小数点以下の値を何桁表示するか")]
    private int _displayDigitsDecimalPointUndervalue = 0;

    [SerializeField]
    private Image gameOverBackground;

    private bool _timeStart = true;
    private bool _isReachedGameOver;

    private Subject<Unit>     _onGameOver = new Subject<Unit>();
    public  IObservable<Unit> OnGameOver => _onGameOver;

    public GameState State { get; set; }

    private void Start()
    {
        _maxTime = _countTime;
        CountStart();

        SwitchInputController.Instance
                             .OnClickABXYButtonSubject
                             .Where(_ => State == GameState.Result && !_isReachedGameOver)
                             .Subscribe(button =>
                             {
                                 // FIXME: 暫定
                                 if (SystemSceneManager.GetCurrentSceneName().Equals("TutorialV2"))
                                 {
                                     SystemSceneManager.LoadNextScene("MainGameV2", SceneTransition.Fade).Forget();
                                 }
                                 else
                                 {
                                     SystemSceneManager.LoadNextScene("Title", SceneTransition.Fade).Forget();
                                 }
                             });

        this.ObserveEveryValueChanged(x => x.State).Subscribe(OnGameStateChanged).AddTo(this);

        State = GameState.Main;
    }

    private void Update()
    {
        if (_timeStart)
        {
            _countTime -= Time.deltaTime;

            if (_countTime >= _maxTime - 10) //ゲームスタートから10秒//一段回目の画像変更
            {
                _hourGlassImage.sprite = _hourGlassSprites[0];
                timerText.text         = Mathf.Floor(_countTime).ToString("F0");
            }
            else if (_countTime >= _maxTime * 0.5) //スタートから10秒 ~ 50％まで //二段回目の画像変更
            {
                _hourGlassImage.sprite = _hourGlassSprites[1];
                timerText.text         = Mathf.Floor(_countTime).ToString("F0");
            }
            else if (_countTime >= 10f) //50% ~ 残り10秒まで //三段回目の画像変更
            {
                _hourGlassImage.sprite = _hourGlassSprites[2];
                timerText.text         = Mathf.Floor(_countTime).ToString("F0");
            }
            else if (_countTime >= 0) //ゲームオーバーまで10秒//四段回目の画像変更
            {
                _hourGlassImage.sprite = _hourGlassSprites[3];
                timerText.text         = _countTime.ToString("F" + _displayDigitsDecimalPointUndervalue);
            }
            else if (0 > _countTime)
            {
                //五段回目の画像変更
                _hourGlassImage.sprite = _hourGlassSprites[4];
                timerText.text         = 0.ToString("F" + _displayDigitsDecimalPointUndervalue);
                _timeStart             = false;
                SetGameStateToResult(false);
            }
        }
    }

    public void CountStart() => _timeStart = true;

    private async void OnGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Main:
                break;

            case GameState.Pause:
                break;

            case GameState.Result:
            {
                // カウントダウン停止
                _timeStart = false;

                // ゲームクリア表示
                if (!_isReachedGameOver)
                {
                    if (ResultGameClear.Instance)
                    {
                        ResultGameClear.Instance.AnimateBackground();
                        await ResultGameClear.Instance.ShowBackground();

                        gameClearImg.gameObject.SetActive(true);
                        await FadeTransition.FadeOut(gameClearImg, 0);
                        await FadeTransition.FadeIn(gameClearImg);
                    }
                }
                // ゲームオーバー表示
                else
                {
                    gameOverImg.gameObject.SetActive(true);
                    UniTask a = FadeTransition.FadeIn(gameOverImg);
                    UniTask b = FadeTransition.FadeIn(gameOverBackground, 1, 0, 0.7058823529411765f);

                    await UniTask.WhenAll(a, b);
                }

                break;
            }
        }
    }

    /// <summary>
    /// GameStateをリザルトに遷移する
    /// </summary>
    /// <param name="isGameClear">ゲームクリアかどうか。falseならゲームオーバー</param>
    public void SetGameStateToResult(bool isGameClear)
    {
        // ゲームオーバーならイベント発行
        if (!isGameClear)
        {
            _onGameOver.OnNext(Unit.Default);
        }

        _isReachedGameOver = !isGameClear;
        State              = GameState.Result;
    }
}

public enum GameState
{
    Main,
    Pause,
    Result
}
