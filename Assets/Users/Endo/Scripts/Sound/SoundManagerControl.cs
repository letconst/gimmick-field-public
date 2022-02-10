using UnityEngine;

public partial class SoundManager
{
    /// <summary>
    /// BGMを再生する
    /// </summary>
    /// <param name="target">再生するBGM</param>
    /// <param name="volume">音量</param>
    /// <param name="isLoop">ループするか</param>
    /// <param name="position">再生する位置</param>
    /// <returns>サウンドID</returns>
    public static int PlayMusic(MusicDef target, float volume = 1, bool isLoop = false, Vector3? position = null)
    {
        return PlayAudio(Audio.AudioType.Music, (int) target, volume, isLoop, position);
    }

    /// <summary>
    /// SEを再生する
    /// </summary>
    /// <param name="target">再生するSE</param>
    /// <param name="volume">音量</param>
    /// <param name="isLoop">ループするか</param>
    /// <param name="position">再生する位置</param>
    /// <returns>サウンドID</returns>
    public static int PlaySound(SoundDef target, float volume = 1, bool isLoop = false, Vector3? position = null)
    {
        return PlayAudio(Audio.AudioType.Sound, (int) target, volume, isLoop, position);
    }

    /// <summary>
    /// UI用のSEを再生する
    /// </summary>
    /// <param name="target">再生するUI SE</param>
    /// <param name="volume">音量</param>
    /// <returns>サウンドID</returns>
    public static int PlayUISound(UISoundDef target, float volume = 1)
    {
        return PlayAudio(Audio.AudioType.UISound, (int) target, volume, false, null);
    }

    /// <summary>
    /// サウンドを再生する
    /// </summary>
    /// <param name="type">再生するサウンドの種類</param>
    /// <param name="target">再生するサウンドのenumインデックス</param>
    /// <param name="volume">音量</param>
    /// <param name="isLoop">ループするか</param>
    /// <param name="position">再生する位置</param>
    /// <returns>サウンドID</returns>
    public static int PlayAudio(Audio.AudioType type, int target, float volume, bool isLoop, Vector3? position)
    {
        int id = -1;

        switch (type)
        {
            case Audio.AudioType.Music:
            {
                id = PrepareMusic(target, volume, isLoop, position);
                GetMusicAudio(id).Play();

                break;
            }

            case Audio.AudioType.Sound:
            {
                id = PrepareSound(target, volume, isLoop, position);
                GetSoundAudio(id).Play();

                break;
            }

            case Audio.AudioType.UISound:
            {
                id = PrepareUISound(target, volume);
                GetUISoundAudio(id).Play();

                break;
            }
        }

        return id;
    }

    /// <summary>
    /// すべてのサウンドを停止する
    /// </summary>
    public static void StopAll()
    {
        _musicSource.Stop();
    }
}
