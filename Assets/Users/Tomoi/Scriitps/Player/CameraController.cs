using UnityEngine;
using nn.hid;
using UniRx;

public class CameraController : MonoBehaviour
{
    //_RotSpeedでカメラの回転速度を調整する
    [SerializeField] [Range(0.1f, 5.0f)] private float _RotSpeed = 2.0f;

    //_ViewingAngleで視野角の調整をする
    [SerializeField] [Range(0, 1f)] private float _ViewingAngle = 0.8f;

    [SerializeField] private bool isYAxisReversed = false;
    [SerializeField] private bool isXAxisReversed = false;

    void Start()
    {
        //右コントローラーのジョイスティックの取得
        SwitchInputController.Instance.OnInputRstickResiveed.Subscribe(Vector2 =>
        {
            var _RotX = (isXAxisReversed ? -Vector2.x : Vector2.x) * _RotSpeed;
            var _RotY = (isYAxisReversed ? Vector2.y : -Vector2.y) * _RotSpeed;

            this.transform.RotateAround(this.transform.position, Vector3.up, _RotX);
            this.transform.RotateAround(this.transform.position, this.transform.right, _RotY);
            //水平を0とした際のY軸の角度を-1~1で生成する
            var _CameraRotRestriction = Vector3.Dot(this.transform.forward, Vector3.up);
            //Y軸の視野角を制限する　_ViewingAngleを超す値になった際はカメラを回転しない
            if (_CameraRotRestriction > _ViewingAngle || _CameraRotRestriction < -_ViewingAngle)
            {
                //カメラの回転
                this.transform.RotateAround(this.transform.position, this.transform.right, -_RotY);
            }
        });
    }
}