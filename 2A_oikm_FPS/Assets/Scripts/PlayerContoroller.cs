using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerContoroller : MonoBehaviour
{
    #region - 変数宣言
    Vector3 moveVelocity;                   //　キー入力による移動ベクトル
    [SerializeField]float speed = 10f;       //  プレイヤーの速度
    [SerializeField]float jump  = 5f;       //  ジャンプの高さ
    Rigidbody rb;                           //  Rigidbodyを取得するよう
    float rayDistance = 1.0f;               //  rayを飛ばす距離
    bool isGround = true;                   //　接地判定
    bool isSprint = false;                  //  ダッシュ判定
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
    }

    void FixedUpdate()
    {
        rb.MovePosition(this.transform.position + moveVelocity);
    }
    #endregion

    #region - 初期化関数

    void Init()
    {
        rb = GetComponent<Rigidbody>(); //Rigidbody取得

    }

    #endregion

    #region - インプットシステム
    //移動用
    void OnMove(InputValue val)
    {
        Vector2 Input = val.Get<Vector2>();
        moveVelocity = new Vector3(Input.x, 0, Input.y) * speed * Time.deltaTime;
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
            speed = 15f;
        }
        else
        {
            speed = 10f;
        }
    }
    #endregion

}
