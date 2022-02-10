using UniRx.Toolkit;
using UnityEngine;

public class SoundObjectPool : ObjectPool<AudioSource>
{
    private readonly AudioSource _original;

    public SoundObjectPool(AudioSource original)
    {
        _original = original;
    }

    protected override AudioSource CreateInstance()
    {
        // SoundManagerの子として再生用オブジェクト生成
        AudioSource source = Object.Instantiate(_original, SoundManager.Instance.transform);

        return source;
    }
}
