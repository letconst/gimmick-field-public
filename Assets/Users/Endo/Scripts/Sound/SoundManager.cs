using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
    [SerializeField, Header("UI用SEのチャンネル数（同時に鳴らせる数）")]
    private int UISoundChannel;

    private static bool _isInitialized;

    private static Vector3 _selfPos;

    private static AudioSource   _musicSource;
    private static AudioSource[] _UISoundSources;

    private static Dictionary<int, Audio> _musicsAudio;
    private static Dictionary<int, Audio> _soundsAudio;
    private static Dictionary<int, Audio> _UISoundsAudio;

    private static Dictionary<int, AudioClip> _musicClips;

    protected override async void Awake()
    {
        base.Awake();

        _selfPos = transform.position;

        await Init();
    }

    /// <summary>
    /// 初期化処理を行う
    /// </summary>
    private static async UniTask Init()
    {
        if (_isInitialized) return;

        _musicsAudio   = new Dictionary<int, Audio>();
        _soundsAudio   = new Dictionary<int, Audio>();
        _UISoundsAudio = new Dictionary<int, Audio>();

        // 各種AudioSource生成
        _musicSource    = AddAudioSourceComponent();
        _UISoundSources = new AudioSource[Instance.UISoundChannel];

        for (int i = 0; i < Instance.UISoundChannel; i++)
        {
            _UISoundSources[i] = AddAudioSourceComponent();
        }

        DontDestroyOnLoad(Instance);

        _musicClips = new Dictionary<int, AudioClip>();

        // サウンドファイル読み込み
        // TODO: SEも読み込む
        Array musicDef = Enum.GetValues(typeof(MusicDef));

        for (int i = 0; i < musicDef.Length; i++)
        {
            object def  = musicDef.GetValue(i);
            var    clip = await Addressables.LoadAssetAsync<AudioClip>(def.ToString());

            _musicClips.Add((int) def, clip);
        }

        _isInitialized = true;

        // 自身にAudioSourceを新規アタッチする
        AudioSource AddAudioSourceComponent()
        {
            return Instance.gameObject.AddComponent<AudioSource>();
        }
    }

    private static Dictionary<int, Audio> GetAudioDict(Audio.AudioType type)
    {
        return type switch
        {
            Audio.AudioType.Music   => _musicsAudio,
            Audio.AudioType.Sound   => _soundsAudio,
            Audio.AudioType.UISound => _UISoundsAudio,
            _                       => null
        };
    }

    #region 取得系メソッド

    /// <summary>
    /// 音をIDから取得する。どの種類のものか (BGMなのか等) がわかっている場合は専用メソッドの利用を推奨。
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Audio GetAudio(int id)
    {
        Audio audio = GetMusicAudio(id);

        if (audio != null)
        {
            return audio;
        }

        audio = GetSoundAudio(id);

        if (audio != null)
        {
            return audio;
        }

        audio = GetUISoundAudio(id);

        return audio;
    }

    /// <summary>
    /// BGMをIDから取得する
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Audio GetMusicAudio(int id)
    {
        return GetAudio(Audio.AudioType.Music, id);
    }

    /// <summary>
    /// SEをIDから取得する
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Audio GetSoundAudio(int id)
    {
        return GetAudio(Audio.AudioType.Sound, id);
    }

    /// <summary>
    /// UI用SEをIDから取得する
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Audio GetUISoundAudio(int id)
    {
        return GetAudio(Audio.AudioType.UISound, id);
    }

    /// <summary>
    /// Audioを取得する
    /// </summary>
    /// <param name="type">取得する音の種類</param>
    /// <param name="id">取得する音のID</param>
    /// <returns>Audio</returns>
    public static Audio GetAudio(Audio.AudioType type, int id)
    {
        Dictionary<int, Audio> audioDict = GetAudioDict(type);

        // 辞書がなければnullを返す（念の為）
        if (audioDict == null) return null;

        if (audioDict.ContainsKey(id))
        {
            return audioDict[id];
        }

        return null;
    }

    #endregion

    private static int PrepareMusic(int target, float volume, bool isLoop)
    {
        return PrepareAudio(Audio.AudioType.Music, target, volume, isLoop, _selfPos);
    }

    private static int PrepareAudio(Audio.AudioType type, int target, float volume, bool isLoop, Vector3 position)
    {
        AudioClip clip = type switch
        {
            Audio.AudioType.Music   => _musicClips[target],
            Audio.AudioType.Sound   => null,
            Audio.AudioType.UISound => null
        };

        Audio audio = type switch
        {
            Audio.AudioType.Music   => new Audio(type, _musicSource, clip, volume, isLoop, position),
            Audio.AudioType.Sound   => null,
            Audio.AudioType.UISound => null
        };

        _musicsAudio.Add(audio.ID, audio);

        return audio.ID;
    }

    #region 再生系メソッド

    /// <summary>
    /// BGMを再生する
    /// </summary>
    /// <param name="target">再生するBGM</param>
    public static void PlayMusic(MusicDef target)
    {
    }

    /// <summary>
    /// BGMを再生する
    /// </summary>
    /// <param name="target">再生するBGM</param>
    /// <param name="volume">音量</param>
    public static int PlayMusic(MusicDef target, float volume)
    {
        return PlayAudio(Audio.AudioType.Music, (int) target, volume, false, _selfPos);
    }

    /// <summary>
    /// BGMを再生する
    /// </summary>
    /// <param name="target">再生するBGM</param>
    /// <param name="volume">音量</param>
    /// <param name="isLoop">ループするか</param>
    public static int PlayMusic(MusicDef target, float volume, bool isLoop)
    {
        return PlayAudio(Audio.AudioType.Music, (int) target, volume, isLoop, _selfPos);
    }

    /// <summary>
    /// BGMを再生する
    /// </summary>
    /// <param name="target">再生するBGM</param>
    /// <param name="volume">音量</param>
    /// <param name="isLoop">ループするか</param>
    /// <param name="position">再生する位置</param>
    public static void PlayMusic(MusicDef target, float volume, bool isLoop, Vector3 position)
    {
    }

    /// <summary>
    /// SEを再生する
    /// </summary>
    /// <param name="target">再生するSE</param>
    public static void PlaySound(SoundDef target)
    {
    }

    /// <summary>
    /// SEを再生する
    /// </summary>
    /// <param name="target">再生するSE</param>
    /// <param name="volume">音量</param>
    public static void PlaySound(SoundDef target, float volume)
    {
    }

    /// <summary>
    /// SEを再生する
    /// </summary>
    /// <param name="target">再生するSE</param>
    /// <param name="volume">音量</param>
    /// <param name="isLoop">ループするか</param>
    public static void PlaySound(SoundDef target, float volume, bool isLoop)
    {
    }

    /// <summary>
    /// SEを再生する
    /// </summary>
    /// <param name="target">再生するSE</param>
    /// <param name="volume">音量</param>
    /// <param name="isLoop">ループするか</param>
    /// <param name="position">再生する位置</param>
    public static void PlaySound(SoundDef target, float volume, bool isLoop, Vector3 position)
    {
    }

    /// <summary>
    /// UI用のSEを再生する
    /// </summary>
    /// <param name="target">再生するUI SE</param>
    public static void PlayUISound(UISoundDef target)
    {
    }

    /// <summary>
    /// UI用のSEを再生する
    /// </summary>
    /// <param name="target">再生するUI SE</param>
    /// <param name="volume">音量</param>
    public static void PlayUISound(UISoundDef target, float volume)
    {
    }

    public static int PlayAudio(Audio.AudioType type, int target, float volume, bool isLoop, Vector3 position)
    {
        int id = -1;

        switch (type)
        {
            case Audio.AudioType.Music:
            {
                id = PrepareMusic(target, volume, isLoop);
                GetMusicAudio(id).Play();

                break;
            }

            case Audio.AudioType.Sound:
                break;

            case Audio.AudioType.UISound:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        return id;
    }

    /// <summary>
    /// すべての音を停止する
    /// </summary>
    public static void StopAll()
    {
        _musicSource.Stop();
    }

    #endregion
}
