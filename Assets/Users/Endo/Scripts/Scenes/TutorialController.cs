using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    private async void Start()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1));

        SoundManager.PlayMusic(MusicDef.Main_BGM, 1, true);
    }
}
