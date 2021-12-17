using UnityEngine;

public class Audio
{
    /// <summary>
    /// 音の固有ID
    /// </summary>
    public int ID { get; private set; }

    /// <summary>
    /// 音の種類
    /// </summary>
    public AudioType Type { get; private set; }

    /// <summary>
    /// 音が再生されているか
    /// </summary>
    public bool IsPlaying { get; private set; }

    /// <summary>
    /// 音がリジュームしているか
    /// </summary>
    public bool IsResumed { get; private set; }

    /// <summary>
    /// 音の音量
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
    /// 音が再生される位置
    /// </summary>
    public Vector3? Position
    {
        get => _position;
        set => _position = value ?? SoundManager.Instance.transform.position;
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

    private static int _instanceID;

    private Vector3? _position;

    private float     _volume;
    private bool      _isLoop;
    private AudioClip _clip;

    public enum AudioType
    {
        Music,
        Sound,
        UISound,
    }

    public Audio(AudioType type, AudioSource source, AudioClip clip, float volume, bool isLoop, Vector3 position)
    {
        // ユニークIDを設定
        ID          = _instanceID++;
        AudioSource = source;
        Type        = type;
        Clip        = clip;
        Volume      = volume;
        IsLoop      = isLoop;
        Position    = position;
    }

    public void Play()
    {
        AudioSource.Play();

        IsPlaying = true;
    }
}
