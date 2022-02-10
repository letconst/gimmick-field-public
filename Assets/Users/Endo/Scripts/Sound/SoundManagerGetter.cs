using System.Collections.Generic;
using UnityEngine;

public partial class SoundManager
{
    /// <summary>
    /// 指定した種類のサウンドのDictionaryを取得する
    /// </summary>
    /// <param name="type">サウンドの種類</param>
    /// <returns>サウンドDictionary</returns>
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

    /// <summary>
    /// 未使用のSEのAudioSourceを取得する
    /// </summary>
    /// <returns></returns>
    private static AudioSource GetFreeSoundSource()
    {
        foreach (AudioSource source in _soundSources)
        {
            if (source.clip != null) continue;

            return source;
        }

        return null;
    }

    /// <summary>
    /// 未使用のUI用SEのAudioSourceを取得する
    /// </summary>
    /// <returns></returns>
    private static AudioSource GetFreeUISoundSource()
    {
        foreach (AudioSource source in _UISoundSources)
        {
            if (source.clip != null) continue;

            return source;
        }

        return null;
    }

    /// <summary>
    /// サウンドをIDから取得する。どの種類のものか (BGMなのか等) がわかっている場合は専用メソッドの利用を推奨。
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
    /// BGMをサウンドIDから取得する
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Audio GetMusicAudio(int id)
    {
        return GetAudio(Audio.AudioType.Music, id);
    }

    /// <summary>
    /// SEをサウンドIDから取得する
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Audio GetSoundAudio(int id)
    {
        return GetAudio(Audio.AudioType.Sound, id);
    }

    /// <summary>
    /// UI用SEをサウンドIDから取得する
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
    /// <param name="type">取得するサウンドの種類</param>
    /// <param name="id">取得するサウンドのID</param>
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

    /// <summary>
    /// 指定のAudioSourceがSoundManager自身にアタッチされているものかを確認する
    /// </summary>
    /// <param name="target">確認したいAudioSource</param>
    /// <returns>アタッチされているか</returns>
    private static bool IsSelfAudioSource(AudioSource target)
    {
        if (target == _musicSource) return true;

        foreach (AudioSource source in _soundSources)
        {
            if (target == source) return true;
        }

        foreach (AudioSource source in _UISoundSources)
        {
            if (target == source) return true;
        }

        return false;
    }
}
