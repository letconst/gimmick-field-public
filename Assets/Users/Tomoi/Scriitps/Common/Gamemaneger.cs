using System;
using UnityEngine;
using UniRx;
using UnityEngine.UI;

public class Gamemaneger : SingletonMonoBehaviour<Gamemaneger>
{
    [Header("時間"), SerializeField]
    private float _time = 60;

    [SerializeField]
    private Text timerText;

    [SerializeField]
    private Image gameClearImg;

    [SerializeField]
    private Image gameOverImg;

    [SerializeField]
    private Player player;

    private bool _timeStart = true;
    private bool _isReachedGameOver;

    private Subject<Unit>     _GameOver = new Subject<Unit>();
    public  IObservable<Unit> GameOver => _GameOver;

    private void Start()
    {
        CountStart();

        GameOver.Subscribe(_ =>
                {
                    _isReachedGameOver = true;
                    player.canMovement = false;
                    gameOverImg.gameObject.SetActive(true);
                })
                .AddTo(this);

        SwitchInputController.Instance.OnClickABXYButtonSubject
                             .Where(_ => _isReachedGameOver)
                             .Subscribe(_ => { SystemSceneManager.LoadNextScene("Title", SceneTransition.Fade); })
                             .AddTo(this);
    }

    private void Update()
    {
        if (_timeStart)
        {
            _time          -= Time.deltaTime;
            timerText.text =  _time.ToString("0");

            if (_time < 0)
            {
                _GameOver.OnNext(Unit.Default);
                _timeStart     = false;
                timerText.text = "0";
            }
        }
    }

    public void CountStart() => _timeStart = true;

    public void OnClear()
    {
        _timeStart         = false;
        _isReachedGameOver = true;
        player.canMovement = false;
        gameClearImg.gameObject.SetActive(true);
    }
}
