using UnityEngine;
using nn.hid;
using UniRx;
using UniRx.Triggers;
using System;
using System.ComponentModel.Design;
using nn.util;
using UnityEngine.InputSystem;

public class SwitchInputController : SingletonMonoBehaviour<SwitchInputController>
{
    //Npad関係
    private NpadId npadId = NpadId.Invalid;
    private NpadStyle npadStyle = NpadStyle.Invalid;
    private NpadState npadState = new NpadState();


    //6軸センサーの情報
    public SixAxisSensorState LeftJoyConRotaion { get; private set; }
    public SixAxisSensorState RightJoyConRotaion { get; private set; }

    //コントローラーの軸を補正するための変数
    private SixAxisSensorState CorrectLeftControllerAxis = default;
    private SixAxisSensorState CorrectRightControllerAxis = default;


    //ボタンの入力状態を保持、譲渡するためのenum
    public enum Status
    {
        None,
        GetButtonDown,
        GetButton,
        GetButtonUp
    }
    public struct Button
    {
        public NpadButton GetButton;
        public Status Status;
    }
    //ZRとZLの値がGrabButtonにより返ってくる
    public struct GrabButton
    {
        public Button ZR;
        public Button ZL;
    }

    private Subject<GrabButton> _GrabButtonSubject = new Subject<GrabButton>();
    public IObservable<GrabButton> OnClickGrabButtonSubject => _GrabButtonSubject;
    //ABXYの値が返ってくる
    public struct ABXYButton
    {
        public Button A;
        public Button B;
        public Button X;
        public Button Y;
    }

    private Subject<ABXYButton> _ABXYButtonSubject = new Subject<ABXYButton>();
    public IObservable<ABXYButton> OnClickABXYButtonSubject => _ABXYButtonSubject;

    private Subject<Vector2> _LJoyStickSubject = new Subject<Vector2>();
    public IObservable<Vector2> OnInputLstickResiveed => _LJoyStickSubject;

    private Subject<Vector2> _RJoyStickSubject = new Subject<Vector2>();
    public IObservable<Vector2> OnInputRstickResiveed => _RJoyStickSubject;

    private Vector2 LJoyStic()
    {
        AnalogStickState lStick = npadState.analogStickL;

        //0^1の値が欲しいのでfloatでキャストして割合を出す
        Vector2 axis;
        axis.x =(float) lStick.x / (float) AnalogStickState.Max;
        axis.y =(float) lStick.y / (float) AnalogStickState.Max;
        return axis;
    }
    private Vector2 RJoyStic()
    {
        AnalogStickState rStick = npadState.analogStickR;
        //0^1の値が欲しいのでfloatでキャストして割合を出す
        Vector2 axis;
        axis.x =(float) rStick.x / (float) AnalogStickState.Max;
        axis.y =(float) rStick.y / (float) AnalogStickState.Max;
        return axis;
    }

    //右コントローラーの加速度の取得
    private Subject<Float3> _RcontllolerAccelerometer = new Subject<Float3>();
    public IObservable<Float3> GetRcontllolerAccelerometer => _RcontllolerAccelerometer;

    //左コントローラーの加速度の取得
    private Subject<Float3> _LcontllolerAccelerometer = new Subject<Float3>();
    public IObservable<Float3> GetLcontllolerAccelerometer => _LcontllolerAccelerometer;

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        //Npadの初期化
        Npad.Initialize();
        Npad.SetSupportedIdType(new NpadId[] {NpadId.Handheld, NpadId.No1});
        Npad.SetSupportedStyleSet(NpadStyle.FullKey | NpadStyle.JoyDual);

        GrabButton _grabButton = default;
        _grabButton.ZL.GetButton = NpadButton.ZL;
        _grabButton.ZR.GetButton = NpadButton.ZR;

        //ZRかZLが押されたときにGrabButtonSubjectから通知する処理
        this.UpdateAsObservable()
            .Where(_ => npadState.GetButtonUp(NpadButton.ZR | NpadButton.ZL)||
                        npadState.GetButton(NpadButton.ZR | NpadButton.ZL)||
                        npadState.GetButtonDown(NpadButton.ZR | NpadButton.ZL))
            .Subscribe(_ =>
                {
                    _grabButton.ZR.Status = npadState.GetButtonDown(NpadButton.ZR) ? Status.GetButtonDown :
                                            npadState.GetButton(NpadButton.ZR)     ? Status.GetButton :
                                            npadState.GetButtonUp(NpadButton.ZR)   ? Status.GetButtonUp : Status.None;

                    _grabButton.ZL.Status = npadState.GetButtonDown(NpadButton.ZL) ? Status.GetButtonDown :
                                            npadState.GetButton(NpadButton.ZL)     ? Status.GetButton :
                                            npadState.GetButtonUp(NpadButton.ZL)   ? Status.GetButtonUp : Status.None;

                    _GrabButtonSubject.OnNext(_grabButton);
                }
            );


        ABXYButton _abxyButton = default;
        _abxyButton.A.GetButton = NpadButton.A;
        _abxyButton.B.GetButton = NpadButton.B;
        _abxyButton.X.GetButton = NpadButton.X;
        _abxyButton.Y.GetButton = NpadButton.Y;

        //A,B,X,Yのどれかが押されたときにABXYButtonSubjectから通知する処理
        this.UpdateAsObservable()
            .Where(_ => npadState.GetButtonUp(NpadButton.A | NpadButton.B | NpadButton.X | NpadButton.Y)||
                        npadState.GetButton(NpadButton.A | NpadButton.B | NpadButton.X | NpadButton.Y)||
                        npadState.GetButtonDown(NpadButton.A | NpadButton.B | NpadButton.X | NpadButton.Y))
            .Subscribe(_ =>
                {
                    _abxyButton.A.Status = npadState.GetButtonDown(NpadButton.A) ? Status.GetButtonDown :
                                           npadState.GetButton(NpadButton.A)     ? Status.GetButton :
                                           npadState.GetButtonUp(NpadButton.A)   ? Status.GetButtonUp : Status.None;

                    _abxyButton.B.Status = npadState.GetButtonDown(NpadButton.B) ? Status.GetButtonDown :
                                           npadState.GetButton(NpadButton.B)     ? Status.GetButton :
                                           npadState.GetButtonUp(NpadButton.B)   ? Status.GetButtonUp : Status.None;

                    _abxyButton.X.Status = npadState.GetButtonDown(NpadButton.X) ? Status.GetButtonDown :
                                           npadState.GetButton(NpadButton.X)     ? Status.GetButton :
                                           npadState.GetButtonUp(NpadButton.X)   ? Status.GetButtonUp : Status.None;

                    _abxyButton.Y.Status = npadState.GetButtonDown(NpadButton.Y) ? Status.GetButtonDown :
                                           npadState.GetButton(NpadButton.Y)     ? Status.GetButton :
                                           npadState.GetButtonUp(NpadButton.Y)   ? Status.GetButtonUp : Status.None;

                    _ABXYButtonSubject.OnNext(_abxyButton);
                }
            );
        //左コントローラーのジョイスティックに入力があったらOnInputLstickResiveedから通知する処理
        this.UpdateAsObservable()
            .Where(_ => LJoyStic() != Vector2.zero)
            .Subscribe(_ => { _LJoyStickSubject.OnNext(LJoyStic()); }
            );
        //右コントローラーのジョイスティックに入力があったらOnInputRstickResiveedから通知する処理
        this.UpdateAsObservable()
            .Where(_ => RJoyStic() != Vector2.zero)
            .Subscribe(_ => { _RJoyStickSubject.OnNext(RJoyStic()); }
            );
    }
    private SixAxisSensorState state = new SixAxisSensorState();

    private nn.util.Float4 npadQuaternion = new nn.util.Float4();
    private Quaternion quaternion = new Quaternion();


    public Vector3 RJoyConAccel    { get; private set; }
    public Vector3 LJoyConAccel    { get; private set; }
    public Vector3 RJoyConVelocity { get; private set; }
    public Vector3 LJoyConVelocity { get; private set; }

    private void Update()
    {
        if (UpdatePadState())
        {
            for (int i = 0; i < handleCount; i++)
            {
                SixAxisSensor.GetState(ref state, handle[i]);

                state.GetQuaternion(ref npadQuaternion);
                quaternion.Set(npadQuaternion.x, npadQuaternion.z, npadQuaternion.y, -npadQuaternion.w);

                // 重力を除いた加速度
                Vector3 accel = quaternion * new Vector3(state.acceleration.x, state.acceleration.z, state.acceleration.y);
                accel += new Vector3(0, .986f);

                // 重力を受けた状態の加速度（元の値）
                Vector3 origAccel;
                (origAccel.x, origAccel.y, origAccel.z) = (state.acceleration.x,
                                                           state.acceleration.y,
                                                           state.acceleration.z);

                // 速度
                Vector3 velocity = (origAccel - accel) * Time.deltaTime;

                //軸の補正
                switch (i)
                {
                    case 0://左0
                        LeftJoyConRotaion = state;
                        LJoyConAccel      = accel;
                        LJoyConVelocity   = velocity;
                        _LcontllolerAccelerometer.OnNext(state.acceleration);
                        break;
                    case 1://右1
                        RightJoyConRotaion = state;
                        RJoyConAccel       = accel;
                        RJoyConVelocity    = velocity;
                        _RcontllolerAccelerometer.OnNext(state.acceleration);
                    break;
                }
            }
        }
    }
    
    private bool UpdatePadState()
    {
        //コントローラーの持ち方の取得
        NpadStyle handheldStyle = Npad.GetStyleSet(NpadId.Handheld);
        NpadState handheldState = npadState;
        //初期値ではないとき
        if (handheldStyle != NpadStyle.None)
        {
            Npad.GetState(ref handheldState, NpadId.Handheld, handheldStyle);
            if (handheldState.buttons != NpadButton.None)
            {
                if ((npadId != NpadId.Handheld) || (npadStyle != handheldStyle))
                {
                    this.GetSixAxisSensor(NpadId.Handheld, handheldStyle);
                }
                npadId = NpadId.Handheld;
                npadStyle = handheldStyle;
                npadState = handheldState;
                return true;
            }
        }

        //NpadId.No1 は1Pのプレイヤーの指定をしている
        NpadStyle no1Style = Npad.GetStyleSet(NpadId.No1);
        NpadState no1State = npadState;
        if (no1Style != NpadStyle.None)
        {
            Npad.GetState(ref no1State, NpadId.No1, no1Style);
            if (no1State.buttons != NpadButton.None)
            {
                if ((npadId != NpadId.No1) || (npadStyle != no1Style))
                {
                    this.GetSixAxisSensor(NpadId.No1, no1Style);
                }
                npadId = NpadId.No1;
                npadStyle = no1Style;
                //npadState に入力情報が入っている
                npadState = no1State;
                return true;
            }
        }

        if ((npadId == NpadId.Handheld) && (handheldStyle != NpadStyle.None))
        {
            npadId = NpadId.Handheld;
            npadStyle = handheldStyle;
            npadState = handheldState;
        }
        else if ((npadId == NpadId.No1) && (no1Style != NpadStyle.None))
        {
            npadId = NpadId.No1;
            npadStyle = no1Style;
            npadState = no1State;
        }
        else
        {
            npadId = NpadId.Invalid;
            npadStyle = NpadStyle.Invalid;
            npadState.Clear();
            return false;
        }
        return true;
    }

    private SixAxisSensorHandle[] handle = new SixAxisSensorHandle[2];
    private int handleCount = 0;

    private void GetSixAxisSensor(NpadId id, NpadStyle style)
    {
        for (int i = 0; i < handleCount; i++)
        {
            SixAxisSensor.Stop(handle[i]);
        }

        handleCount = SixAxisSensor.GetHandles(handle, 2, id, style);

        for (int i = 0; i < handleCount; i++)
        {
            SixAxisSensor.Start(handle[i]);
        }
    }
}
