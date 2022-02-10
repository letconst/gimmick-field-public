using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostEffectController : SingletonMonoBehaviour<PostEffectController>
{
    [SerializeField]
    private Volume globalVolume;

    private Vignette _vignette;

    private bool _isPlayingDamageEffect;

    private CancellationTokenSource _damageEffectCts;

    private void Start()
    {
        if (globalVolume)
        {
            globalVolume.profile.TryGet(out _vignette);
        }

        _damageEffectCts = new CancellationTokenSource();
    }

    /// <summary>
    /// 画面にダメージエフェクトを表示する
    /// </summary>
    /// <param name="showSeconds">エフェクトを表示し続ける秒数</param>
    /// <param name="fadeSeconds">エフェクトが消える際のフェード秒数</param>
    public async void PlayDamageEffect(float showSeconds, float fadeSeconds)
    {
        // 再生中なら、過去のエフェクト再生を中断
        if (_isPlayingDamageEffect)
        {
            _damageEffectCts.Cancel();
            _damageEffectCts = new CancellationTokenSource();
        }

        _isPlayingDamageEffect    = true;
        _vignette.intensity.value = .5f;

        // 表示秒数分待機
        await UniTask.Delay(System.TimeSpan.FromSeconds(showSeconds));

        // 徐々に消す
        while (_vignette.intensity.value > 0)
        {
            // 中断されたら終了
            if (_damageEffectCts.IsCancellationRequested) return;

            _vignette.intensity.value -= Time.unscaledDeltaTime / fadeSeconds;

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        _isPlayingDamageEffect = false;
    }
}
