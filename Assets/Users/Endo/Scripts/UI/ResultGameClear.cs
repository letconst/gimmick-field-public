using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ResultGameClear : SingletonMonoBehaviour<ResultGameClear>
{
    [SerializeField, Header("背景画像のマスク用マテリアル")]
    private Material backgroundMaterial;

    [SerializeField, Header("背景画像の連番画像")]
    private Sprite[] backgroundSprites;

    [SerializeField, Min(.1f), Header("背景画像が切り替わる間隔 (秒)")]
    private float backgroundUpdateInterval;

    [SerializeField, Min(.1f), Header("背景画像のマスクアニメーションが完了するまでの時間 (秒)")]
    private float backgroundDisplayTime;

    private int   _backgroundIndex;
    private Image _backgroundImage;

    private static readonly int MaskSize = Shader.PropertyToID("_MaskSize");

    private void Start()
    {
        _backgroundImage          = GetComponent<Image>();
        _backgroundImage.material = backgroundMaterial;

        backgroundMaterial.SetFloat(MaskSize, 0);
    }

#if UNITY_EDITOR
    private void OnApplicationQuit()
    {
        // ゲーム終了時に値が残るため、リセット (Editorのみ)
        backgroundMaterial.SetFloat(MaskSize, 0);
    }
#endif

    /// <summary>
    /// 次の背景画像のインデックスを取得する。配列のインデックスを超える場合は0となる
    /// </summary>
    /// <returns></returns>
    private int GetNextBackgroundIndex()
    {
        _backgroundIndex++;

        if (_backgroundIndex >= backgroundSprites.Length)
        {
            _backgroundIndex = 0;
        }

        return _backgroundIndex;
    }

    /// <summary>
    /// 背景画像のコマ送り表示を開始する
    /// </summary>
    public async void AnimateBackground()
    {
        while (true)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(backgroundUpdateInterval));

            Sprite background = backgroundSprites[GetNextBackgroundIndex()];
            _backgroundImage.sprite = background;
        }
    }

    /// <summary>
    /// 背景画像を非同期で表示する
    /// </summary>
    public async UniTask ShowBackground()
    {
        // シェーダー内での最大値
        // TODO: ハードコードどうにかしたい
        const int maxValue  = 2;
        float     maskValue = 0;

        while (backgroundMaterial.GetFloat(MaskSize) < maxValue)
        {
            maskValue += Time.deltaTime / backgroundDisplayTime;
            backgroundMaterial.SetFloat(MaskSize, maskValue);

            await UniTask.Yield(PlayerLoopTiming.Update);
        }
    }
}
