using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;

[RequireComponent(typeof(MeshFilter))]
public class Spiderweb : MonoBehaviour
{
     [SerializeField]
     private Spider spider;

     [SerializeField, Header("炎の煙のパーティクル")]
     private ParticlePlayer flameSmokeParticle;

     private ParticlePool _flameSmokeParticlePool;

     //今プレイヤーを捕まえているか
     private bool _Catch;
     //振り払った回数内側
     private int _ShakeOffCountInside;
     //振り払った回数外側
     private int _ShakeOffCountOutside;

     [Header("蜘蛛の巣からの開放値"),SerializeField]
     private float success_value = 20;

     //プレイヤーのオブジェクト
     private GameObject _PlayerObject;
     //固定しておくプレイヤーの座標
     private Vector3 _RestraintPoint;

     private ShowSpiderwebUI _UIinstance;

     private IObservable<SpiderwebChecker.ColliderPosition> _subject;

     private MeshFilter   _meshFilter;
     private MeshCollider _collider;

     // プレイヤーを捕まえるか
     public bool canCatch;

     private void Awake()
     {
          _meshFilter = GetComponent<MeshFilter>();
          _collider   = GetComponent<MeshCollider>();
     }

     void Start()
     {
          _UIinstance = ShowSpiderwebUI.Instance;

          _flameSmokeParticlePool = new ParticlePool(flameSmokeParticle);

          _subject = SpiderwebChecker.Instance.OnColliderEnterHand;

          _subject.Where(_=>_Catch).Subscribe(CheckHandStatus).AddTo(this);
     }

     private void CheckHandStatus(SpiderwebChecker.ColliderPosition colliderPosition)
     {
          if (colliderPosition == SpiderwebChecker.ColliderPosition.Inside)
          {
               _ShakeOffCountInside++;
          }else if (colliderPosition == SpiderwebChecker.ColliderPosition.Outside)
          {
               _ShakeOffCountOutside++;
          }
     }
     void Update()
     {
          //Playerを捕まえた際
          if(_Catch)
          {
               _UIinstance.ValueSet((_ShakeOffCountOutside + _ShakeOffCountInside) /(success_value * 2));

               //Playerの座標を固定
               _PlayerObject.transform.position = _RestraintPoint;
               //振った回数が一定値を越したら
               if (success_value <= _ShakeOffCountOutside && success_value <= _ShakeOffCountInside)
               {
                    _Catch = false;

                    _ShakeOffCountInside  = 0;
                    _ShakeOffCountOutside = 0;
                    ShowSpiderwebUI.Instance.ValueSet(0);
                    Destroy();
               }
          }
     }

     private async void Destroy()
     {
          //UIを非表示にする処理
          _UIinstance.ShowSpiderwebSlider(false);
          //処理
          PlayerHandController.SetSpiderwebCheckHandActive(false,HandType.Both,true);
          // Destroy(this.gameObject);

          canCatch                                         = false;
          PlayerHandController.Instance.playerCaughtSpider = false;

          // 巣を透明に
          spider.SetWebAlpha(0);

          await UniTask.Delay(TimeSpan.FromSeconds(3));

          spider.state = Spider.SpiderState.Idle;
     }

     public static float GetAfterDecimalPoint(float self )
     {
          return self % 1;
     }
     //このスクリプトがアタッチされているオブジェクトのコライダーの範囲にPlayerのtagを持つオブジェクトが入ったら発火
     private void OnTriggerEnter(Collider obj)
     {
          if (canCatch && obj.CompareTag("Player"))
          {
               PlayerActionController _playerAction = obj.GetComponent<PlayerActionController>();

               if (_playerAction._rightHoldGameObject && _playerAction._rightHoldGameObject.CompareTag("Torch") ||
                   _playerAction._leftHoldGameObject  && _playerAction._leftHoldGameObject.CompareTag("Torch"))
               {
                    RemoveSpiderweb();

                    SoundManager.PlaySound(SoundDef.PutOutFire_SE);
               }
               else
               {
                    PlayerHandController.Instance.playerCaughtSpider = true;
                    PlayerHandController.SetSpiderwebCheckHandActive(true, HandType.Both, true);

                    // 手で持っているものは落とさせる
                    _playerAction.LeftHoldObj?.DeAction(HandType.Left);
                    _playerAction.RightHoldObj?.DeAction(HandType.Right);

                    _Catch = true;
                    _UIinstance.ShowSpiderwebSlider(true);
                    _UIinstance.ValueSet(0);
                    _PlayerObject = obj.gameObject;
                    _RestraintPoint = obj.transform.position;
               }
          }
     }


     //コピペです
     /// <summary>
     /// 蜘蛛の巣を消す
     /// </summary>
     private void RemoveSpiderweb()
     {
          SpawnParticle(() => Destroy(gameObject), .5f);
     }

     /// <summary>
     /// 煙パーティクルを出現させる
     /// </summary>
     /// <param name="callback">出現中に実行するコールバック</param>
     /// <param name="callbackInvokeRate">パーティクルがどのくらいの割合再生されたらコールバックが実行されるか (0-1)</param>
     private async void SpawnParticle(System.Action callback, float callbackInvokeRate)
     {
          ParticlePlayer particle1 = _flameSmokeParticlePool.Rent();
          ParticlePlayer particle2 = _flameSmokeParticlePool.Rent();
          particle1.transform.position = transform.position;
          particle2.transform.position = transform.position;
          particle1.transform.rotation = transform.rotation * Quaternion.Euler(0, 90, 90);
          particle2.transform.rotation = transform.rotation * Quaternion.Euler(0, 90, -90);

          particle1.PlayParticle();
          particle2.PlayParticle();

          float duration          = particle1.selfParticle.main.duration;
          bool  isCallbackInvoked = false;

          // パーティクルが停止するまで待機
          while (!particle1.selfParticle.isStopped)
          {
               // コールバックがあれば、指定割合で実行
               if (callback != null)
               {
                    float playRate = particle1.selfParticle.time / duration;

                    if (!isCallbackInvoked && callbackInvokeRate <= playRate)
                    {
                         isCallbackInvoked = true;
                         callback();
                    }
               }

               await UniTask.Yield(PlayerLoopTiming.Update);
          }

          _flameSmokeParticlePool.Return(particle1);
          _flameSmokeParticlePool.Return(particle2);
     }


     /// <summary>
     /// 巣オブジェクトのメッシュを設定する
     /// </summary>
     /// <param name="v1">左下の頂点となる座標</param>
     /// <param name="v2">右下の頂点となる座標</param>
     /// <param name="v3">左上の頂点となる座標</param>
     /// <param name="v4">右上の頂点となる座標</param>
     public void SetMesh(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
     {
          List<Vector3> vertices = new List<Vector3>
          {
               transform.InverseTransformPoint(v1),
               transform.InverseTransformPoint(v2),
               transform.InverseTransformPoint(v3),
               transform.InverseTransformPoint(v4),
          };

          List<int> indexes = new List<int>();
          indexes.AddRange(new []{0, 2, 1, 2, 3, 1});

          _meshFilter.mesh.SetVertices(vertices);
          _meshFilter.mesh.SetIndices(indexes.ToArray(), MeshTopology.Triangles, 0);
          _collider.sharedMesh = _meshFilter.mesh;
     }
}
