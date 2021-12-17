using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
public class Spider : EnemyBase
{
    [SerializeField, Header("一辺を移動する時間 (秒)")]
    private float moveSpeed;

    [SerializeField, Header("壁と垂直な法線を決める際の試行ごとの角度"), Range(0, 90)]
    private int degPerInitTrial;

    [SerializeField, Header("クモの巣")]
    private Spiderweb web;

    [SerializeField]
    private Shader shader;

    [SerializeField]
    private Texture webTex;

    private Animator     _animator;
    private MeshRenderer _renderer;
    private Material     _webMaterial;

    private SpiderState _state;

    private float _totalWeaveDistance;

    private readonly List<Vector3> _passagePoints = new List<Vector3>();

    private const int WallLayer = 1 << 7;

    private static readonly int IsMove     = Animator.StringToHash("IsMove");
    private static readonly int SPropAlpha = Shader.PropertyToID("Vector1_a3aa0b61c05943b686b6d48e9f569c75");
    private static readonly int SPropTex   = Shader.PropertyToID("Texture2D_ab101d69e96444f3a869ddd9318ad100");

    public enum SpiderState
    {
        Idle,
        RandomMoving,
        Weaving,
        Dead,
    }

    private void Start()
    {
        _animator = GetComponent<Animator>();

        if (web)
        {
            _renderer = web.gameObject.GetComponent<MeshRenderer>();

            // アルファ値を個別に設定したいため、マテリアルを個別で作成・適用
            var mat = new Material(shader);
            _webMaterial       = mat;
            _renderer.material = mat;
            _renderer.material.SetTexture(SPropTex, webTex);
        }

        Init();

        this.ObserveEveryValueChanged(x => x._state)
            .Subscribe(OnStateChanged)
            .AddTo(this);
    }

    private void OnStateChanged(SpiderState state)
    {
        if (state == SpiderState.Weaving)
        {
            Weave();
        }
    }

    private void Update()
    {
        switch (_state)
        {
            case SpiderState.Idle:
                break;

            case SpiderState.RandomMoving:
                break;

            case SpiderState.Weaving:
            {
                break;
            }

            case SpiderState.Dead:
                break;
        }
    }

    /// <summary>
    /// クモが巣を構築するパターン
    /// </summary>
    private enum WeavePattern
    {
        /// <summary>水平方向への移動から構築を始めるパターン</summary>
        Horizon,

        /// <summary>斜め方向への移動から構築を始めるパターン</summary>
        Slash,
    }

    #region 初期化系メソッド

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Init()
    {
        int patternCount = Enum.GetNames(typeof(WeavePattern)).Length;
        // var pattern      = (WeavePattern) Random.Range(0, patternCount);
        const WeavePattern pattern = WeavePattern.Slash;

        CalculatePassagePoints(pattern);

        _state = SpiderState.Weaving;
    }

    /// <summary>
    /// 巣の構築時に移動する経路地点を算出する
    /// </summary>
    /// <param name="pattern">移動パターン</param>
    private void CalculatePassagePoints(WeavePattern pattern)
    {
        Vector3 startPos = default;
        Vector3 endPos   = default;

        // 通路に対する法線取得
        Vector3 norm = GetPerpendicularDirection(ref startPos, ref endPos);

        // 壁から壁までの中心座標
        Vector3 centerPos = Vector3.Lerp(startPos, endPos, .5f);

        var ray = new Ray
        {
            origin = centerPos
        };

        switch (pattern)
        {
            // TODO
            case WeavePattern.Horizon:
            {
                break;
            }

            case WeavePattern.Slash:
            {
                // 中心から右上45°へのベクトル作成し、ray地点を次の目的地に
                Vector3 dir = Quaternion.AngleAxis(60, Vector3.forward) * norm;
                ray.direction = dir;

                if (Physics.Raycast(ray, out RaycastHit hit1, Mathf.Infinity, WallLayer))
                {
                    _passagePoints.Add(hit1.point);
                    _totalWeaveDistance += Vector3.Distance(hit1.point, transform.position);
                }

                // そこから法線の逆方向（開始地点真上）にrayを飛ばし、接点を次の目的地に
                ray.origin    = hit1.point;
                ray.direction = -norm;

                if (Physics.Raycast(ray, out RaycastHit hit2, Mathf.Infinity, WallLayer))
                {
                    _passagePoints.Add(hit2.point);
                    _totalWeaveDistance += Vector3.Distance(hit2.point, hit1.point);
                }

                _passagePoints.Add(startPos);
                _passagePoints.Add(endPos);
                _totalWeaveDistance += Vector3.Distance(startPos, hit2.point) + Vector3.Distance(endPos, startPos);

                web.SetMesh(endPos, startPos, hit2.point, hit1.point);

                break;
            }
        }
    }

    /// <summary>
    /// 現在地から壁への法線ベクトルを取得する
    /// </summary>
    /// <param name="startPoint">開始地点</param>
    /// <param name="endPoint">終了地点</param>
    /// <returns></returns>
    private Vector3 GetPerpendicularDirection(ref Vector3 startPoint, ref Vector3 endPoint)
    {
        Vector3 resultDir = default;
        float   minDist   = default;
        Vector3 baseDir   = transform.right;
        int     rotateDeg = 0;

        // 試行ごとに回転する角度から試行回数を算出
        int trialCount = (int) Mathf.Ceil((float) 180 / degPerInitTrial);

        // 試行回数分レイを飛ばして法線となるベクトルを求める
        for (int i = 0; i < trialCount; i++)
        {
            // 試行ごとに指定度回転
            baseDir = Quaternion.AngleAxis(degPerInitTrial, Vector3.up) * baseDir;
            var ray = new Ray(transform.position, baseDir);

            Physics.Raycast(ray, out RaycastHit firstHit, Mathf.Infinity, WallLayer);

            // 壁から壁への距離を出すため、逆方向にもレイを飛ばす
            ray.direction = -baseDir;

            Physics.Raycast(ray, out RaycastHit secondHit, Mathf.Infinity, WallLayer);

            if (!firstHit.transform || !secondHit.transform)
            {
                rotateDeg += degPerInitTrial;

                continue;
            }

            float dist = Vector3.Distance(firstHit.point, transform.position) +
                         Vector3.Distance(secondHit.point, transform.position);

            // 計算した壁から壁への距離が最短だったら更新
            if (minDist == 0 || minDist > dist)
            {
                resultDir  = baseDir;
                minDist    = dist;
                startPoint = firstHit.point;
                endPoint   = secondHit.point;
            }

            rotateDeg += degPerInitTrial;
        }

        return resultDir;
    }

    #endregion

    #region 動作系メソッド

    /// <summary>
    /// クモの巣を張る処理
    /// </summary>
    private async void Weave()
    {
        if (_passagePoints.Count == 0) return;

        // 移動距離
        float movedDistance = 0;

        _animator.SetBool(IsMove, true);

        // 通過点を順番に回らせる
        foreach (Vector3 point in _passagePoints)
        {
            Vector3 basePos = transform.position;
            Vector3 oldPos  = basePos;

            float elapsedTime = 0; // 次の目的地へ移動時の経過時間
            float movedRate   = 0; // 次の目的地までの進行率 (0-1)

            Turn(point);

            while (movedRate < 1)
            {
                elapsedTime += Time.deltaTime;
                movedRate   =  Mathf.Clamp01(elapsedTime / moveSpeed);

                transform.position =  Vector3.Lerp(basePos, point, movedRate);
                movedDistance      += Vector3.Distance(oldPos, transform.position);
                oldPos             =  transform.position;

                // 巣のマテリアルのアルファ値に透明度を進捗率で反映
                if (_renderer)
                {
                    _renderer.material.SetFloat(SPropAlpha, movedDistance / _totalWeaveDistance);
                }

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        // 仮で少し内側に移動させる
        {
            Vector3 basePos = transform.position;
            Vector3 targetPos = Vector3.Lerp(_passagePoints[_passagePoints.Count - 1],
                                             _passagePoints[_passagePoints.Count - 2], .25f);

            float elapsedTime = 0;
            float moveRate    = 0;

            Turn(targetPos);

            while (moveRate < 1)
            {
                elapsedTime        += Time.deltaTime;
                moveRate           =  Mathf.Clamp01(elapsedTime / moveSpeed);
                transform.position =  Vector3.Lerp(basePos, targetPos, moveRate);

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        _animator.SetBool(IsMove, false);
        web.canCatch = true;

        // 移動完了後はランダム移動に設定
        _state = SpiderState.RandomMoving;
    }

    /// <summary>
    /// 後ろに回転する
    /// TODO: アニメーションへの対応
    /// </summary>
    private void Turn(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    #endregion
}
