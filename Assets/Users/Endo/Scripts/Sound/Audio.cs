using UnityEngine;

public class Audio
{
    /// <summary>
    /// サウンドの固有ID
    /// </summary>
    public int ID { get; private set; }

    /// <summary>
    /// サウンドの種類
    /// </summary>
    public AudioType Type { get; private set; }

    /// <summary>
    /// サウンドが再生されているか
    /// </summary>
    public bool IsPlaying { get; private set; }

    /// <summary>
    /// サウンドが一時停止しているか
    /// </summary>
    public bool IsPaused { get; private set; }

    /// <summary>
    /// サウンドの音量
    /// </summary>
    public float Volume
    {
        get => _volume;
        private set
        {
            _volume = value;

            if (AudioSource)
            {
                AudioSource.volume = value;
            }
        }
    }

    /// <summary>
    /// ループするか
    /// </summary>
    public bool IsLoop
    {
        get => _isLoop;
        private set
        {
            _isLoop = value;

            if (AudioSource)
            {
                AudioSource.loop = value;
            }
        }
    }

    /// <summary>
    /// サウンドが再生される位置
    /// </summary>
    public Vector3? Position
    {
        get => _position;
        set
        {
            _position = value;

            if (AudioSource && _position != null)
            {
                AudioSource.transform.position = _position.Value;
            }
        }
    }

    public AudioSource AudioSource { get; private set; }

    public AudioClip Clip
    {
        get => _clip;
        private set
        {
            _clip = value;

            if (AudioSource)
            {
                AudioSource.clip = value;
            }
        }
    }

    public float SpatialBlend
    {
        get => _spatialBlend;
        set
        {
            _spatialBlend = value;

            if (AudioSource)
            {
                AudioSource.spatialBlend = value;
            }
        }
    }

    private static int _instanceID;

    private Vector3? _position;

    private float     _volume;
    private bool      _isLoop;
    private AudioClip _clip;
    private float     _spatialBlend;

    public enum AudioType
    {
        Music,
        Sound,
        UISound,
    }

    public Audio(AudioType type, AudioSource source, AudioClip clip, float volume, bool isLoop, Vector3? position)
    {
        // ユニークIDを設定
        ID = _instanceID++;

        // 位置指定があればオブジェクト生成
        if (position != null && !source)
        {
            AudioSource = SoundManager.SoundPool.Rent();
        }
        else
        {
            AudioSource = source;
        }

        // 位置指定があれば3Dサウンドに設定
        if (position != null)
        {
            SpatialBlend = 1;
        }
        else
        {
            SpatialBlend = 0;
        }

        Type     = type;
        Clip     = clip;
        Volume   = volume;
        IsLoop   = isLoop;
        Position = position;
    }

    /// <summary>
    /// サウンドを再生する
    /// </summary>
    public void Play()
    {
        // TODO: nullの場合は2Dサウンドで空きがなかった場合なので、古いやつから再利用する形にする
        // 最悪、2Dでもプーリングさせる
        if (AudioSource == null) return;

        AudioSource.Play();

        IsPlaying = true;
    }

    /// <summary>
    /// 毎フレーム実行する処理
    /// </summary>
    public void Update()
    {
        if (!AudioSource) return;

        // 再生状態更新
        if (AudioSource.isPlaying != IsPlaying)
        {
            IsPlaying = AudioSource.isPlaying;
        }
    }

    /// <summary>
    /// AudioSourceの情報をリセットする
    /// </summary>
    public void Reset()
    {
        Volume       = 1;
        IsLoop       = false;
        Position     = null;
        Clip         = null;
        SpatialBlend = 0;
    }
}
