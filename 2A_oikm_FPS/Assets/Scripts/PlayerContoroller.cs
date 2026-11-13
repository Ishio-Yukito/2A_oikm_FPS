using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerContoroller : MonoBehaviour
{
    #region - 変数宣言
    Vector3 moveVelocity;                   //　キー入力による移動ベクトル
    [SerializeField]float speed = 0.2f;       //  プレイヤーの速度
    [SerializeField]float jump  = 5f;       //  ジャンプの高さ
    Rigidbody rb;                           //  Rigidbodyを取得するよう
    float rayDistance = 1.0f;               //  rayを飛ばす距離
    bool isGround = true;                   //　接地判定
    bool isSprint = false;                  //  ダッシュ判定

    float rotationSpeed = 800f;         /// 方向転換時の角度回転速度
    [SerializeField] GameObject Bullet1;    //  弾用
    Transform cam;      // カメラのトランスフォーム変数（前と右方向のベクトルを取得）
    Vector2 inputDir;   // インプットシステムからの方向入力情報
    Vector3 moveDir;    // カメラ基準の移動ベクトル変数

    #endregion

    #region - Unityイベント
    void Start()
    {
        Init();

    }

    void Update()
    {
        Hit();　// 接地判定
        Dush(); // ダッシュ判定
        //CameraOperation(); //カメラ処理
        if (Keyboard.current.leftShiftKey.wasReleasedThisFrame)
        {
            isSprint = false;
        }
    }

    void FixedUpdate()
    {
        moveDir_CAM_cnv();
        rb.MovePosition(this.transform.position + moveVelocity);
        Input_Dir_interp();
    }
    #endregion

    #region - 初期化関数

    void Init()
    {
        rb = GetComponent<Rigidbody>(); //Rigidbody取得
        cam = Camera.main.transform;      /// カメラのトランスフォーム情報を取得
    }

    #endregion

    #region - インプットシステム
    //移動用
    void OnMove(InputValue val)
    {
        inputDir = val.Get<Vector2>();    /// インプットシステムの方向入力値
    }

    //ジャンプ用
    void OnJump(InputValue val)
    {
        if(val.isPressed&&isGround)
        {
            rb.AddForce(Vector3.up * jump, ForceMode.Impulse);
        }
    }

    void OnSprint(InputValue val)
    {
        isSprint = val.isPressed;
    }

    //弾発射処理
    void OnAttack(InputValue val)
    {
        if (val.isPressed)
        {
            Vector3 launch_Pos = cam.transform.position + cam.transform.forward * 1.0f;
            Instantiate(Bullet1, launch_Pos, cam.transform.rotation);
        }
    }
    #endregion

    #region - 当たり判定用関数

    void Hit()
    {
        isGround= Physics.Raycast(this.transform.position,Vector3.down,rayDistance);
    }

    #endregion

    #region - ダッシュ用関数
    void Dush()
    {
        if(isSprint)
        {
            speed = 0.6f;
        }
        else
        {
            speed = 0.2f;
        }
    }
    #endregion

    /*
    #region - カメラ操作(マウス)
    void CameraOperation()
    {
        mx = Input.GetAxis("Mouse X");
        my = Input.GetAxis("Mouse Y");

        if(Mathf.Abs(mx)>0.01f)
        {
            transform.RotateAround(transform.position, Vector3.up, mx); // 回転軸はplayerオブジェクトのワールド座標Y軸
        }
    }

    #endregion
    */
    #region -FPSカメラ用の関数
    void moveDir_CAM_cnv()                                              /// キー入力ベクトルをカメラ（画面）基準ベクトルに変換する
    {
        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;    ///カメラ基準の前方ベクトルを取得
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;    ///カメラ基準の右方向ベクトルを取得

        moveDir = camForward * inputDir.y + camRight * inputDir.x;      /// キー入力とカメラベクトルを合成して進行ベクトルにする
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();             /// 進行ベクトルの斜め方向の長さが１を超えないよう正規化

        moveVelocity = moveDir * speed;                                 /// 進行ベクトルに速度をかけて移動ベクトルにする

    }
    void Input_Dir_interp()
    {
        if (inputDir.sqrMagnitude > 0.01f)                                          ///入力方向へのＹ軸回転補間
        {
            float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;  /// 目標Ｙ軸角度取得（K04でカメラ基準仕様に変更）
            float currentAngle = transform.eulerAngles.y;                           /// 現在のＹ軸角度を取得
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);  /// 角度補間
            transform.rotation = Quaternion.Euler(0, newAngle, 0);                  /// 実際の角度に補間結果を適用
        }
    }
    #endregion
}
