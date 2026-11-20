using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class K04_Player : MonoBehaviour
{
    ///////// 定数・変数定義 /////////////////////////////////////////////////////////////////////////////////////////////////
    public static K04_Player Instance { get; private set; }
    Vector3 moveVelocity;
    float speed = 0.2f;
    float jumpForce = 8.0f;
    Rigidbody rb;

    bool isSprint;
    Vector2 inputDir;
    [SerializeField] float rotationSpeed = 800f;
    [SerializeField] GameObject Bullet_A;
    Transform cam;
    Vector3 moveDir;

    [SerializeField] public int bulletcount;
    private float waitTime = 3.0f; // 3秒後にリロード完了
    private bool isReloading = false; // リロード中フラグ

    ////// 初期化処理 ///////////////////////////////////////////////////////////////////////////////////////////////////////

    // ★★★ ここを追加（AwakeでInstanceを設定する） ★★★
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.transform;
        bulletcount = 30;
    }

    ////// 物理運動アップデート///////////////////////////////////////////////////////////////////////////////////////////////

    void FixedUpdate()
    {
        moveDir_CAM_cnv();
        rb.MovePosition(this.transform.position + moveVelocity);
        Input_Dir_interp();
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////// メソッドパート /////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    void moveDir_CAM_cnv()
    {
        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;

        moveDir = camForward * inputDir.y + camRight * inputDir.x;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        moveVelocity = moveDir * speed;
    }

    void OnMove(InputValue val)
    {
        inputDir = val.Get<Vector2>();
    }

    RaycastHit Ground_CHK()
    {
        Ray g_ray = new Ray(this.transform.position, Vector3.down);
        RaycastHit g_ray_hit;
        Physics.Raycast(g_ray, out g_ray_hit, 1.01f);
        return g_ray_hit;
    }

    void OnJump(InputValue val)
    {
        RaycastHit g_ray_hit = Ground_CHK();
        if (g_ray_hit.collider != null && val.isPressed)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void OnSprint(InputValue val)
    {
        isSprint = val.isPressed;
        if (isSprint) speed = 0.6f;
        else speed = 0.2f;
    }

    void Input_Dir_interp()
    {
        if (inputDir.sqrMagnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            float currentAngle = transform.eulerAngles.y;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Euler(0, newAngle, 0);
        }
    }

    void OnAttack(InputValue val)
    {
        // リロード中または残弾ゼロの場合は撃てない
        if (isReloading || bulletcount <= 0) return;

        if (val.isPressed)
        {
            Debug.Log("マウス左ボタン押しました");
            Vector3 launch_Pos = cam.position + cam.forward * 1.0f;
            Instantiate(Bullet_A, launch_Pos, cam.rotation);

            bulletcount--;
        }
    }

    /// Input System の "Reload" アクション（例: Rキー）に対応するメッセージ
    void OnReload(InputValue val)
    {
        if (val.isPressed && !isReloading && bulletcount < 30)
        {
            StartCoroutine(ReloadCoroutine());
        }
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        Debug.Log("リロード開始...");

        // 3秒待機
        yield return new WaitForSeconds(waitTime);

        bulletcount = 30;
        isReloading = false;
        Debug.Log("3秒経過！リロード完了");
    }
}