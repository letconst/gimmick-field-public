using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

public static class AnimatorExtension
{
    /// <summary>
    /// 1フレーム後にResetTriggerされるSetTrigger
    /// </summary>
    /// <param name="self"></param>
    /// <param name="name">トリガー名</param>
    public static void SetTriggerOneFrame(this Animator self, string name)
    {
        self.SetTrigger(name);

        Observable.NextFrame()
                  .Subscribe(_ => { }, () =>
                  {
                      if (self) self.ResetTrigger(name);
                  });
    }

    /// <summary>
    /// 1フレーム後にResetTriggerされるSetTrigger
    /// </summary>
    /// <param name="self"></param>
    /// <param name="id">トリガーのID</param>
    public static void SetTriggerOneFrame(this Animator self, int id)
    {
        self.SetTrigger(id);

        Observable.NextFrame()
                  .Subscribe(_ => { }, () =>
                  {
                      if (self) self.ResetTrigger(id);
                  });
    }

    /// <summary>
    /// 指定の名前のアニメーションが再生されるまで待機する
    /// </summary>
    /// <param name="self"></param>
    /// <param name="name">アニメーション名</param>
    /// <returns></returns>
    public static UniTask WaitUntilAnimationNameIs(this Animator self, string name)
    {
        return UniTask.WaitUntil(() => self.GetCurrentAnimatorStateInfo(0).IsName(name));
    }

    /// <summary>
    /// 指定の名前のアニメーションが再生されている間待機する
    /// </summary>
    /// <param name="self"></param>
    /// <param name="name">アニメーション名</param>
    /// <returns></returns>
    public static UniTask WaitForAnimationNameIs(this Animator self, string name)
    {
        return UniTask.WaitWhile(() => self.GetCurrentAnimatorStateInfo(0).IsName(name));
    }

    /// <summary>
    /// 現在再生されているアニメーションが終了するまで待機する
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static UniTask WaitForCurrentAnimation(this Animator self)
    {
        return UniTask.WaitWhile(() => self.GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
    }
}
