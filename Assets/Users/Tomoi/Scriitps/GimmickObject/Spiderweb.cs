using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

[RequireComponent(typeof(MeshFilter))]
public class Spiderweb : MonoBehaviour
{
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

     private MeshFilter _meshFilter;

     // プレイヤーを捕まえるか
     public bool canCatch;

     private void Awake()
     {
          _meshFilter = GetComponent<MeshFilter>();
     }

     void Start()
     {
          _UIinstance = ShowSpiderwebUI.Instance;

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
                    ShowSpiderwebUI.Instance.ValueSet(0);
                    Destroy();
               }
          }
     }

     void Destroy()
     {
          //UIを非表示にする処理
          _UIinstance.ShowSpiderwebSlider(false);
          //処理
          PlayerHandController.SetSpiderwebCheckHandActive(false);
          Destroy(this.gameObject);
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
               PlayerHandController.Instance.playerCaughtSpider = true;
               PlayerHandController.SetSpiderwebCheckHandActive(true);

               _Catch                = true;
               _UIinstance.ShowSpiderwebSlider(true);
               _UIinstance.ValueSet(0);
               _PlayerObject = obj.gameObject;
               _RestraintPoint = obj.transform.position;
          }
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
     }
}
