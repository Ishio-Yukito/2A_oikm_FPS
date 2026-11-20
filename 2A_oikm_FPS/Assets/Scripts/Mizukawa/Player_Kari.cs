using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Kari : MonoBehaviour
{
    #region 変数宣言

    [Header("プレイヤー移動")]
    [SerializeField] float moveSpeed; // 通常の移動速度

    [Header("プレイヤーHP")]
    [SerializeField] int hp; // プレイヤーのHP

    [Header("コンポーネント")]
    CharacterController controller;
    Camera mainCamera; // メインカメラ

    float verticalVelocity;

    #endregion

    #region Unityイベント

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // CharacterController取得
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;

        // 設定漏れを確認
        if (controller == null)
        {
            Debug.LogError("CharacterControllerがありません！");
        }

        if (mainCamera == null)
        {
            Debug.LogError("MainCameraが見つかりません！");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 必要なコンポーネントがなければ終了
        if (controller == null || mainCamera == null)
        {
            return;
        }

        Vector3 input = Vector3.zero;

        // キーボード(WASD / 矢印キー)
        if (Keyboard.current != null)
        {
            // 前後（W/S または ↑↓）
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                input.z += 1;
            }
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                input.z -= 1;
            }

            // 左右（A/D または ←→）
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                input.x -= 1;
            }
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                input.x += 1;
            }
        }

        // ゲームコントローラー
        if (Gamepad.current != null)
        {
            // 左スティック
            Vector2 stick = Gamepad.current.leftStick.ReadValue();

            input.x += stick.x;
            input.z += stick.y;

            // 十字キー
            Vector2 dpad = Gamepad.current.dpad.ReadValue();

            input.x += dpad.x;
            input.z += dpad.y;
        }

        // 斜め移動の速度を調整
        input = Vector3.ClampMagnitude(input, 1f);

        // カメラ基準の移動方向を取得
        Transform cam = mainCamera.transform;

        Vector3 forward = cam.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cam.right;
        right.y = 0f;
        right.Normalize();

        Vector3 move = forward * input.z + right * input.x;

        // 水平方向と垂直方向を合わせて移動
        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        // 移動方向にプレイヤーを向ける
        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(move);

            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 10f * Time.deltaTime);
        }
    }

    #endregion

    #region ダメージを受ける処理（EnemyBullet から呼ばれる）

    // ダメージを受ける処理（EnemyBullet から呼ばれる）
    public void Damage(int damage)
    {
        hp -= damage;
        hp = Mathf.Max(0, hp); // 0未満にならないように固定

        Debug.Log("プレイヤーがダメージを受けた！ 残りHP: " + hp);

        if (hp <= 0)
        {
            Debug.Log("プレイヤー死亡！");
            // 死亡時の処理（ゲームオーバー画面表示など）をここに記述
        }
    }

    #endregion
}
