using Cysharp.Threading.Tasks;

public class MainGameController : SceneControllerBase
{
    protected override async void Start()
    {
        base.Start();

        // SoundManagerが初期化されるまで待機
        await UniTask.WaitUntil(() => SoundManager.IsInitialized);

        SoundManager.StopAll();
        SoundManager.PlayMusic(MusicDef.Main2_BGM, isLoop: true);
    }
}
