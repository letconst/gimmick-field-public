using UnityEngine;

public enum HandType
{
    /// <summary>未定義の状態。個々のアクション側スクリプトの初期化時に他の値を設定すること</summary>
    Undefined,

    /// <summary>左手</summary>
    Left,

    /// <summary>右手</summary>
    Right,

    /// <summary>左右どちらかの片手</summary>
    One,

    /// <summary>両手</summary>
    Both,
}

public interface IActionable
{
    /// <summary>
    /// アクション開始時の初期化処理
    /// </summary>
    /// <param name="handType">どの手でアクションが開始されたか</param>
    void Action(HandType handType);

    /// <summary>
    /// アクション終了時の処理
    /// </summary>
    /// <param name="handType">どの手でアクションが終了されたか</param>
    void DeAction(HandType handType);

    ///<summary>
    ///Outlineを表示するか
    /// </summary>
    bool _isOutline { get; }

    /// <summary>
    /// Outlineを表示する
    /// </summary>
    void ShowOutline();

    /// <summary>
    ///Outlineを非表示にする
    /// </summary>
    void HideOutline();

    /// <summary>
    /// どの手で入力されたときにアクションを実行するか
    /// </summary>
    HandType RequireHand { get; }

    /// <summary>
    /// 掴める常態かどうかの判定
    /// </summary>
    bool isGrab { get; }
}
