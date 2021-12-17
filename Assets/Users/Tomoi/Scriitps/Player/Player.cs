using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UniRx;
using UnityEngine;

public class Player : MonoBehaviour
{
    //コードを読みやすくするため
    GameObject  PlayerGameObject;
    Rigidbody   PlayerRb;
    [SerializeField]
    float SpeedParameter = 1;
    [SerializeField]
    GameObject camera;

    // FIXME: 暫定
    public bool canMovement = true;

    void Start()
    {
        PlayerGameObject = this.gameObject;
        PlayerRb = PlayerGameObject.GetComponent<Rigidbody>();
        SwitchInputController.Instance.OnClickGrabButtonSubject.Subscribe( _GrabButton =>
        {
            //Debug.Log(_GrabButton);
        });
        SwitchInputController.Instance.OnClickABXYButtonSubject.Subscribe( _ABXYButton =>
        {
            //Debug.Log(_ABXYButton);
        });
        // FIXME: Where暫定
        SwitchInputController.Instance.OnInputLstickResiveed.Where(_ => canMovement).Subscribe( Vector2 =>
        {
            PlayerAddForce(camera.transform.right * Vector2.x,GenerateZVector3(Vector2.y) );
        });
        Gamemaneger.Instance.GameOver.Subscribe(_ =>
        {
            Debug.Log("GameOver");
        });
    }
    //PlayerAddForceに渡すz軸用ののVector3を生成する
    private Vector3 GenerateZVector3(float y)
    {
        Vector3 vector3 = camera.transform.forward * y;
        //カメラの角度でy座標に値が入る可能性があるので0fで上書き
        vector3.y = 0f;
        return vector3;
    }
    public void PlayerAddForce(Vector3 x, Vector3 z)
    {
        PlayerRb.velocity = x * SpeedParameter + z * SpeedParameter;
    }
}