using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    #region 変数宣言

    [Header("追従対象")]
    [SerializeField] Transform target;                  // プレイヤー

    [Header("カメラ設定")]
    [SerializeField] Vector3 offset = new Vector3(0f, 0f, 0f); // プレイヤーからの距離
    [SerializeField] float followSpeed;                        // カメラの追従速度

    [Header("視点操作")]
    [SerializeField] float mouseSpeed;                  // マウス感度
    [SerializeField] float lookSpeed;                   // コントローラー右スティック感度

    [Header("上下角度制限")]
    [SerializeField] float minPitch;                    // 最小角度
    [SerializeField] float maxPitch ;                   // 最大角度

    float yaw = 0f;                                     // 左右回転角度
    float pitch = 20f;                                  // 上下回転角度

    #endregion

    #region Unityイベント

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // プレイヤーの向きに合わせて初期角度を設定
        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 視点入力
        Vector2 look = Vector2.zero;

        // マウス入力
        if (Mouse.current != null)
        {
            look += Mouse.current.delta.ReadValue() * mouseSpeed;
        }

        // コントローラー右スティック入力
        if (Gamepad.current != null)
        {
            look += Gamepad.current.rightStick.ReadValue() * lookSpeed * Time.deltaTime;
        }

        // 左右回転
        yaw += look.x;

        // 上下回転
        pitch -= look.y;

        // 上下の角度制限
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    #endregion

    #region Update終了後にカメラを追従させる

    // Update終了後にカメラを追従させる
    void LateUpdate()
    {
        // プレイヤーが存在しなければ終了
        if (target == null)
        {
            return;
        }

        // カメラの回転
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // プレイヤーの後方・上方へ配置
        Vector3 targetPos = target.position + rotation * offset;

        // 滑らかに追従
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        // プレイヤーの目の高さを見る
        Vector3 lookPos = target.position + Vector3.up * 1.5f;

        transform.LookAt(lookPos);
    }

    #endregion
}
