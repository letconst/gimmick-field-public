using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class SoundEnumGenerator : EditorWindow
{
    private const string BaseDir = "/Sounds";

    // ソースとなるサウンドファイルディレクトリ
    private static readonly string BGMSourceDir  = $"{BaseDir}/BGM";
    private static readonly string SESourceDir   = $"{BaseDir}/SE";
    private static readonly string UISESourceDir = $"{BaseDir}/UISE";

    // 各enumおよびそのファイル名
    private const string BGMFileName  = "MusicDef";
    private const string SEFileName   = "SoundDef";
    private const string UISEFileName = "UISoundDef";

    // enum保存先ディレクトリ
    private static readonly string SaveDir      = $"{BaseDir}/Definitions";
    private static readonly string BGMSavePath  = $"{SaveDir}/{BGMFileName}.cs";
    private static readonly string SESavePath   = $"{SaveDir}/{SEFileName}.cs";
    private static readonly string UISESavePath = $"{SaveDir}/{UISEFileName}.cs";

    private static readonly string Description = @$"所定のディレクトリにあるサウンドファイルを列挙し、それをenumに起こすツールです。
各サウンドファイルは、種類ごとに以下のディレクトリへ配置してください。

・BGM
　→ Assets/{BGMSourceDir}
・SE
　→ Assets/{SESourceDir}
・UI用SE
　→ Assets/{UISESourceDir}

enumは、以下のディレクトリに生成されます。

→ Assets/{SaveDir}
";

    private static readonly string Tab = new string(' ', 4);

    private bool _isCheckedBGM  = true;
    private bool _isCheckedSE   = true;
    private bool _isCheckedUISE = true;

    private System.Diagnostics.Stopwatch _sw = new System.Diagnostics.Stopwatch();

    [MenuItem("GF Tools/Sound enum generator")]
    private static void Init()
    {
        var parserWindow = GetWindowWithRect<SoundEnumGenerator>(new Rect(0, 0, 300, 400));
        parserWindow.Show();
    }

    private void OnGUI()
    {
        // 説明表示
        EditorGUILayout.HelpBox(Description, MessageType.None);

        GUILayout.Label("生成対象", EditorStyles.whiteLargeLabel);

        _isCheckedBGM  = GUILayout.Toggle(_isCheckedBGM, "BGM");
        _isCheckedSE   = GUILayout.Toggle(_isCheckedSE, "SE");
        _isCheckedUISE = GUILayout.Toggle(_isCheckedUISE, "UI用SE");

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("生成"))
        {
            _sw.Restart();

            if (_isCheckedBGM)
            {
                CreateEnumFile(Audio.AudioType.Music);
            }

            if (_isCheckedSE)
            {
                CreateEnumFile(Audio.AudioType.Sound);
            }

            if (_isCheckedUISE)
            {
                CreateEnumFile(Audio.AudioType.UISound);
            }

            _sw.Stop();

            Log($"生成完了 ({_sw.ElapsedMilliseconds}ms)");
        }
    }

    /// <summary>
    /// サウンドのenumファイルを生成する
    /// </summary>
    /// <param name="audioType">どの種類のサウンドか</param>
    private static void CreateEnumFile(Audio.AudioType audioType)
    {
        // サウンドファイル一覧を取得
        string[] files = audioType switch
        {
            Audio.AudioType.Music   => GetFilesName(ToAbsolutePath(BGMSourceDir)),
            Audio.AudioType.Sound   => GetFilesName(ToAbsolutePath(SESourceDir)),
            Audio.AudioType.UISound => GetFilesName(ToAbsolutePath(UISESourceDir))
        };

        // ファイルがなければ終了
        if (files == null || files.Length == 0) return;

        // 保存先ディレクトリがなければ作成
        if (!Directory.Exists(SaveDir))
        {
            Directory.CreateDirectory(SaveDir);
        }

        string enumName = audioType switch
        {
            Audio.AudioType.Music   => BGMFileName,
            Audio.AudioType.Sound   => SEFileName,
            Audio.AudioType.UISound => UISEFileName,
        };

        // 構文生成
        string syntax = GenerateEnumSyntax(enumName, files);

        string savePath = ToAbsolutePath(audioType switch
        {
            Audio.AudioType.Music   => BGMSavePath,
            Audio.AudioType.Sound   => SESavePath,
            Audio.AudioType.UISound => UISESavePath
        });

        // ファイルへ保存
        File.WriteAllText(savePath, syntax);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 指定ディレクトリ内の全ファイル名を取得する (.metaファイルは除く)
    /// </summary>
    /// <param name="sourceDir">対象のディレクトリ (絶対パス)</param>
    /// <returns>対象ディレクトリに存在した全ファイル名</returns>
    private static string[] GetFilesName(string sourceDir)
    {
        // 指定ディレクトリが存在しなければエラー出力および終了
        if (!Directory.Exists(sourceDir))
        {
            LogError($"以下のディレクトリは存在しません。ディレクトリを作成した上、サウンドファイルをそこへ配置してください。\n{sourceDir}");

            return null;
        }

        string[] files  = Directory.GetFiles(sourceDir);
        string[] result = files.Where(file => !file.EndsWith(".meta")).ToArray();

        return result;
    }

    /// <summary>
    /// enumファイルの構文を生成する
    /// </summary>
    /// <param name="enumName">enumの名前</param>
    /// <param name="items">enumの要素となる項目</param>
    /// <returns></returns>
    private static string GenerateEnumSyntax(string enumName, IEnumerable<string> items)
    {
        var sb = new StringBuilder();

        sb.Append("public enum ")
          .Append(enumName)
          .Append("\n{\n");

        foreach (string file in items)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);

            sb.Append(Tab)
              .Append(fileName)
              .Append(",\n");
        }

        sb.Append("}\n");

        return sb.ToString();
    }

    private static string ToAbsolutePath(string path)
    {
        return $"{Application.dataPath}/{path}";
    }

    private static void Log(string message)
    {
        Debug.Log($"[{nameof(SoundEnumGenerator)}] {message}");
    }

    private static void LogError(string message)
    {
        Debug.LogError($"[{nameof(SoundEnumGenerator)}] {message}");
    }
}
#endif
