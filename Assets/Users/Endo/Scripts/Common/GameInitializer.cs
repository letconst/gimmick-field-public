using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

public static class GameInitializer
{
    [RuntimeInitializeOnLoadMethod]
    private static async void Init()
    {
        Application.targetFrameRate = 60;

        // Joy-Conの入力コントローラーオブジェクトを生成
        new GameObject("SwitchInputController").AddComponent<SwitchInputController>();

        // システム管理オブジェクト生成
        GameObject gameSystemObj = await Addressables.InstantiateAsync("GameSystem");
        gameSystemObj.name = gameSystemObj.name.Replace("(Clone)", "");
        Object.DontDestroyOnLoad(gameSystemObj);
    }
}
