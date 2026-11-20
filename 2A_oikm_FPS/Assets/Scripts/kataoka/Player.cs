using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    #region 変数
    // 効果音
    [Header("効果音")]
    [SerializeField] AudioSource jump;                         // ジャンプ
    [SerializeField] AudioSource[] fall = new AudioSource[0];  // 落下
    [SerializeField] AudioSource sprint;                         // ダッシュ
    [SerializeField] AudioSource stomp;                        // ストンプ
    [SerializeField] AudioSource shot;                         // 発射

    Vector3 respawnPoint;

    [Header("移動速度関連")]
    Vector3 moveVelocity;                   // 移動速度ベクトル
    [SerializeField] float speed;           // 移動速度
    Vector2 inputDirect;                    // 入力方向
    Vector3 moveDirect;                     // 移動方向ベクトル

    [Header("壁登り関連")]
    [SerializeField] float climbSpeed;
    [SerializeField] float climbPow;
    bool isClimb = false;

    [Header("ジャンプ関連")]
    [SerializeField] float jumpPow;         // ジャンプ力
    [SerializeField] int jumpCount;         // ジャンプ回数

    [Header("RayCast関連")]
    [SerializeField] float isGroundCollision;
    [SerializeField] float isWallCollision;

    [Header("攻撃関連")]
    [SerializeField] GameObject BulletA;
    [SerializeField] Transform BulletFolder;
    bool isShooted = false;
    bool possibleShoot = true;

    [SerializeField] int weaponType;    // 武器タイプ
    // 武器種
    public enum WEAPON
    {
        NONE,        // 素手
        HANDGUN,     // 拳銃
        ASSULT,      // アサルトライフル
        SNIPER,      // スナイパーライフル
        SHOTGUN,     // ショットガン
    };
    private const int WEAPONTYPE_MAX = 4;
    // 弾オブジェクト
    [SerializeField] GameObject[] BulletObj = new GameObject[0];
    // 武器種毎の発射間隔
    [SerializeField] float[] weaponInterval = {
        0f,                 // 素手
        0.3f,               // 拳銃
        0.1f,               // アサルトライフル
        1f,                 // スナイパーライフル
        0.8f,               // ショットガン
    };
    // 武器種毎の弾数
    [SerializeField] int[] weaponBullet = {
        0,                  // 素手
        10,                 // 拳銃
        30,                 // アサルトライフル
        7,                  // スナイパーライフル
        9,                  // ショットガン
    };
    [SerializeField] int[] weaponBulletMax = {
        0,                  // 素手
        10,                 // 拳銃
        30,                 // アサルトライフル
        7,                  // スナイパーライフル
        9,                  // ショットガン
    };
    bool isReload = false;
    [SerializeField] float[] weaponReloadInterval = {
        0f,                 // 素手
        1.0f,               // 拳銃
        1.5f,               // アサルトライフル
        2f,                 // スナイパーライフル
        1.0f,               // ショットガン
    };
    IEnumerator currentReload;
    [SerializeField] Text weaponText;
    [SerializeField] Text currentBulletText;

    [Header("スプリント関連")]
    [SerializeField] float sprintPow;       // スプリント力
    bool isSprint = false;                  // スプリント中か
    bool dashDisabled = false;

    [Header("ストンピング関連")]
    [SerializeField] float stompPow;        // ストンピング力
    bool isStomp = false;                   // ストンピング落下中か

    [Header("ゲームオーバー関連")]
    [SerializeField] bool isGameover = false;

    [Header("ポーズ関連")]
    [SerializeField] bool isPaused = false;

    [Header("カメラ関連")]
    Transform cameraTrf;                       // カメラ位置
    [SerializeField] float cameraRotateSpeed;  // カメラ回転速度

    // コンポーネント
    Rigidbody rb;

    #endregion

    /// <summary>
    /// 開始時呼び出す関数
    /// </summary>
    void Init()
    {
#if UNITY_EDITOR
        // カーソル表示
        Cursor.visible = true;
#else
        // カーソル非表示
        Cursor.visible = false;
#endif

        // リスポーン初期設定
        RespawnSet();
        // コンポーネント取得
        GetComponentAll();
        // 変数リセット
        VariableReset();

        for (int i = 0; i < weaponBullet.Length; i++)
        {
            weaponBullet[i] = weaponBulletMax[i];
        }
    }

    /// <summary>
    /// 全使用コンポーネント取得
    /// </summary>
    void GetComponentAll()
    {
        rb = GetComponent<Rigidbody>();
        cameraTrf = Camera.main.transform;

    }

    /// <summary>
    /// 変数リセット
    /// </summary>
    void VariableReset()
    {
        jumpCount = 2;

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 初期設定
        Init();
        
    }

    void FixedUpdate()
    {
        // 方向移動
        MoveDirection();
        // moveVelocity分移動させる
        rb.MovePosition(this.transform.position + moveVelocity);
        // 方向転換
        InputAngleDirection();
        // スプリント
        Sprint();
        // 発射
        Shoot((WEAPON)weaponType);

        // 壁に引っ掛からなくするために線形減衰を0にしているため、
        // 線形減衰を別で実装
        ControlLinearVelocity();

        // 接地判定処理
        RaycastHit groundRayHit = GroundHit();
        if (groundRayHit.collider != null)
        {
            JumpHeal();
            Debug.Log("yes接地");
        }
        else
        {
            Debug.Log("not接地");
        }
        if (isStomp && groundRayHit.collider != null)
        {
            isStomp = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        }

        // 壁登り用の壁判定処理
        RaycastHit wallRayHit = WallHit();
        if (wallRayHit.collider != null &&
            wallRayHit.collider.CompareTag("Wall"))
        {
            dashDisabled = true;
            isClimb = true;
            WallClimb();
            Debug.Log("yes壁登り");
        }
        else
        {
            if (isClimb)
            {
                rb.AddForce(Vector3.up * climbPow, ForceMode.Impulse);
                //rb.AddForce((transform.forward * climbPow / 1.25f) * -1, ForceMode.Impulse);
                StartCoroutine(DashEnableTime(0.5f));
                Debug.Log("登った");
                isClimb = false;
            }
            Debug.Log("not壁登り");
        }


        if (transform.position.y <= -10f)
        {
            Respawn();
            //death.Play();
        }

        if (isGameover)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, rb.linearVelocity.z);
            transform.position = new Vector3(0, transform.position.y, 0);
        }

    }

    // Update is called once per frame
    void Update()
    {
        WeaponTextSet((WEAPON)weaponType);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 仕様面の処理

    /// <summary>
    /// 壁に引っ掛からなくするために線形減衰を0にしているため、
    /// 線形減衰を別で実装
    /// </summary>
    void ControlLinearVelocity()
    {
        // X方向のlinearVelocityが0よりも上になった場合
        if (rb.linearVelocity.x > 0)
        {
            // X方向のlinearVelocityを10ずつ下げる
            rb.linearVelocity -= new Vector3(10f * Time.fixedDeltaTime, 0, 0);
            // その際に0よりも下になった場合
            if (rb.linearVelocity.x <= 0)
            {
                // X方向のlinearVelocityを強制的に0にする
                // これで違和感の無いブレーキをかけられるようにしている
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, rb.linearVelocity.z);
            }
        }
        if (rb.linearVelocity.x < 0)
        {
            rb.linearVelocity += new Vector3(10f * Time.fixedDeltaTime, 0, 0);
            if (rb.linearVelocity.x >= 0)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, rb.linearVelocity.z);
            }
        }
        if (rb.linearVelocity.z > 0)
        {
            rb.linearVelocity -= new Vector3(0, 0, 10f * Time.fixedDeltaTime);
            if (rb.linearVelocity.z <= 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0);
            }
        }
        if (rb.linearVelocity.z < 0)
        {
            rb.linearVelocity += new Vector3(0, 0, 10f * Time.fixedDeltaTime);
            if (rb.linearVelocity.z >= 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0);
            }
        }
    }

    /// <summary>
    /// リスポーン地点再設定
    /// </summary>
    /// <param name="_timePlus">足す制限時間の量 0にすれば足さない</param>
    void RespawnSet()
    {
        respawnPoint = new Vector3(0, 2.5f, transform.position.z);
    }

    void Respawn()
    {
        if (currentReload != null)
        {
            StopCoroutine(currentReload);
            isReload = false;
        }

        rb.linearVelocity = Vector3.zero;
        transform.position = respawnPoint;

        for (int i = 0; i < weaponBullet.Length; i++)
        {
            weaponBullet[i] = weaponBulletMax[i];
        }
    }

    void GameOver()
    {
        isGameover = true;
        // カーソル表示
        Cursor.visible = true;
    }

    void OnPause(InputValue _val)
    {
        if (!isPaused && _val.isPressed)
        {
            Cursor.visible = true;
            isPaused = true;
            Time.timeScale = 0f;
        }
        else if (isPaused && _val.isPressed)
        {
            Cursor.visible = false;
            isPaused = false;
            Time.timeScale = 1f;
        }

    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 操作関連の処理

    void OnMove(InputValue _val)
    {
        if (!isGameover)
        {
            inputDirect = _val.Get<Vector2>();
            moveVelocity = (new Vector3(inputDirect.x, 0, inputDirect.y) * speed) * Time.deltaTime;
        }

    }

    void MoveDirection()
    {
        Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTrf.forward, Vector3.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(cameraTrf.right, Vector3.up).normalized;

        moveDirect = cameraForward * inputDirect.y + cameraRight * inputDirect.x;
        if (moveDirect.sqrMagnitude > 1f)
        {
            moveDirect.Normalize();
        }

        moveVelocity = (moveDirect * speed) * Time.deltaTime;
    }

    RaycastHit GroundHit()
    {
        Ray groundRay = new Ray(this.transform.position, Vector3.down);
        RaycastHit groundRayHit;
        Physics.Raycast(groundRay, out groundRayHit, isGroundCollision);
        return groundRayHit;
    }

    void InputAngleDirection()
    {
        if (!isGameover &&
            inputDirect.sqrMagnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDirect.x, moveDirect.z) * Mathf.Rad2Deg;
            float currentAngle = transform.eulerAngles.y;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, cameraRotateSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, newAngle, 0);
        }
    }

    RaycastHit WallHit()
    {
        Ray wallRay = new Ray(this.transform.position, transform.forward);
        RaycastHit wallRayHit;
        //Physics.BoxCast(transform.position, new Vector3(0.5f, 0.8f, 0.5f) * 0.5f, transform.forward, out wallRayHit, Quaternion.identity, isWallCollision);
        Physics.Raycast(wallRay, out wallRayHit, isWallCollision);
        return wallRayHit;
    }

    void WallClimb()
    {
        JumpHeal();
        rb.linearVelocity = Vector3.zero;
        transform.position += new Vector3(0f, climbSpeed * Time.fixedDeltaTime, 0f);

    }

    void OnJump(InputValue _val)
    {
        if (!isGameover &&
            jumpCount >= 1 && _val.isPressed)
        {
            jumpCount--;
            rb.AddForce(Vector3.up * jumpPow, ForceMode.Impulse);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        }
    }

    void JumpHeal()
    {
        jumpCount = 1;
    }

    void OnSprint(InputValue _val)
    {
        if (!isGameover &&
            !isSprint)
        {
            sprint.Play();
        }
        isSprint = _val.isPressed;
    }

    void Sprint()
    {
        if (!isGameover &&
            isSprint)
        {
            rb.AddForce((transform.forward * sprintPow), ForceMode.Impulse);
            // これだとスライド
            //rb.AddForce((transform.forward * sprintPow) * -1, ForceMode.Impulse);    ///リジッドボディで画面後ろに物理衝撃力を与える
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

    }

    IEnumerator DashEnableTime(float _time)
    {
        yield return new WaitForSeconds(_time);
        dashDisabled = false;
    }

    void OnStomp(InputValue _val)
    {
        if (!isGameover &&
            !isStomp && _val.isPressed)
        {
            isStomp = true;
            rb.AddForce(Vector3.down * stompPow, ForceMode.Impulse);    ///リジッドボディで画面上方向に物理衝撃力を与える
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 武器関連の処理

    void OnAttack(InputValue _val)
    {
        isShooted = _val.isPressed;

    }

    /// <summary>
    /// 武器テキスト更新
    /// </summary>
    /// <param name="_type">武器種</param>
    void WeaponTextSet(WEAPON _type)
    {
        // そのまま配列番号として使うためにWEAPON型変数を(int)でint化した物を用意
        int weapontype = (int)_type;
        switch (_type)
        {
            // 武器種毎の表示テキスト
            // それぞれの武器種番号に対応させたテキストを表示させる
            case WEAPON.NONE:
                weaponText.text = "武器 : 素手";
                currentBulletText.text = "弾数 : 無し";
                break;
            case WEAPON.HANDGUN:
                weaponText.text = "武器 : 拳銃";
                currentBulletText.text = "弾数 : " + weaponBullet[weapontype].ToString();
                break;
            case WEAPON.ASSULT:
                weaponText.text = "武器 : アサルトライフル";
                currentBulletText.text = "弾数 : " + weaponBullet[weapontype].ToString();
                break;
            case WEAPON.SNIPER:
                weaponText.text = "武器 : スナイパーライフル";
                currentBulletText.text = "弾数 : " + weaponBullet[weapontype].ToString();
                break;
            case WEAPON.SHOTGUN:
                weaponText.text = "武器 : ショットガン";
                currentBulletText.text = "弾数 : " + weaponBullet[weapontype].ToString();
                break;
        }

    }

    /// <summary>
    /// 発射
    /// </summary>
    /// <param name="_type">武器種</param>
    void Shoot(WEAPON _type)
    {
        // そのまま配列番号として使うためにWEAPON型変数を(int)でint化した物を用意
        int weapontype = (int)_type;
        // 発射中かつ発射可能かつ弾数が0でない場合
        if (isShooted && possibleShoot && weaponBullet[weaponType] > 0)
        {
            switch (_type)
            {
                // 武器種毎に別の弾を発射
                case WEAPON.NONE:
                    {
                        Debug.Log("武器を持っていないぜ！！！！！！！！！！！！！！！！！１！！！１！１！１！１！！！！！！！");
                        break;
                    }
                case WEAPON.HANDGUN:
                    {
                        if (!isReload)
                        {
                            Debug.Log("発射 : 拳銃");
                            // 発射可能をfalseにしてクールタイムを無視して発射を不可能にする
                            possibleShoot = false;
                            // 弾を減らす
                            weaponBullet[weaponType]--;
                            // 効果音
                            shot.Play();
                            // 発射可能でない場合
                            if (!possibleShoot)
                            {
                                // 発射クールタイムのコルーチンを実行する
                                StartCoroutine(ShootCooldown(weaponInterval[weapontype]));
                            }
                            // 発射場所指定
                            Vector3 launchPos = this.transform.position + this.transform.forward * 1.25f;
                            // 対応する弾を発射
                            Instantiate(BulletObj[weapontype],
                                    launchPos,
                                    this.transform.rotation,
                                    BulletFolder);
                        }
                        break;
                    }
                case WEAPON.ASSULT:
                    {
                        if (!isReload)
                        {
                            Debug.Log("発射 : アサルトライフル");
                            possibleShoot = false;
                            // 弾を減らす
                            weaponBullet[weaponType]--;
                            shot.Play();
                            if (!possibleShoot)
                            {
                                StartCoroutine(ShootCooldown(weaponInterval[weapontype]));
                            }
                            Vector3 launchPos = this.transform.position + this.transform.forward * 1.25f;
                            Instantiate(BulletObj[weapontype],
                                    launchPos,
                                    this.transform.rotation,
                                    BulletFolder);
                        }
                        break;
                    }
                case WEAPON.SNIPER:
                    {
                        if (!isReload)
                        {
                            Debug.Log("発射 : スナイパーライフル");
                            possibleShoot = false;
                            // 弾を減らす
                            weaponBullet[weaponType]--;
                            shot.Play();
                            if (!possibleShoot)
                            {
                                StartCoroutine(ShootCooldown(weaponInterval[weapontype]));
                            }
                            Vector3 launchPos = this.transform.position + this.transform.forward * 1.25f;
                            Instantiate(BulletObj[weapontype],
                                    launchPos,
                                    this.transform.rotation,
                                    BulletFolder);
                        }
                        break;
                    }
                // ショットガンは弾が一つでも装填されていれば撃てる
                case WEAPON.SHOTGUN:
                    {
                        Debug.Log("発射 : ショットガン");
                        // リロードを強制中断
                        if (currentReload != null)
                        {
                            isReload = false;
                            StopCoroutine(currentReload);
                        }
                        possibleShoot = false;
                        // 弾を減らす
                        weaponBullet[weaponType]--;
                        shot.Play();
                        if (!possibleShoot)
                        {
                            StartCoroutine(ShootCooldown(weaponInterval[weapontype]));
                        }
                        Vector3 launchPos = this.transform.position + this.transform.forward * 1.25f;
                        Instantiate(BulletObj[weapontype],
                                launchPos,
                                this.transform.rotation,
                                BulletFolder);
                        break;
                    }
                default:
                    {
                        Debug.Log("武器を持っていないぜ！！！！！！！！！！！！！！！！！１！！！１！１！１！１！！！！！！！");
                        break;
                    }
            }
        }
        else
        {
            Debug.Log("発射不可");
        }
    }

    void OnReload(InputValue _val)
    {
        if (_val.isPressed)
        {
            Reload((WEAPON)weaponType);
        }
    }

    /// <summary>
    /// リロード
    /// </summary>
    /// <param name="_val">キー押したか</param>
    /// <param name="_type">武器種</param>
    void Reload(WEAPON _type)
    {
        // そのまま配列番号として使うためにWEAPON型変数を(int)でint化した物を用意
        int weapontype = (int)_type;

        // リロード中でないかつキーが押された場合
        if (!isReload && weaponBullet[weapontype] < weaponBulletMax[weapontype])
        {

            switch (_type)
            {
                case WEAPON.NONE:
                    {
                        Debug.Log("武器を持っていないぜ！！！！！！！！！！！！！！！！！１！！！１！１！１！１！！！！！！！");
                        break;
                    }
                case WEAPON.HANDGUN:
                    {
                        // リロード中にする事で重複されないようにする
                        isReload = true;
                        // リロード処理
                        Debug.Log("リロード : 拳銃");
                        if (weaponBullet[weapontype] < weaponBulletMax[weapontype])
                        {
                            // リロード時間コルーチンに
                            // それぞれのリロード時間と再度WEAPON型に戻したweapontypeを渡した現在リロードのコルーチン起動
                            currentReload = ReloadTime(weaponReloadInterval[weapontype], (WEAPON)weapontype);
                            StartCoroutine(currentReload);
                        }
                        break;
                    }
                case WEAPON.ASSULT:
                    {
                        // リロード中にする事で重複されないようにする
                        isReload = true;
                        // リロード処理
                        Debug.Log("リロード : アサルトライフル");
                        if (weaponBullet[weapontype] < weaponBulletMax[weapontype])
                        {
                            // リロード時間コルーチンに
                            // それぞれのリロード時間と再度WEAPON型に戻したweapontypeを渡した現在リロードのコルーチン起動
                            currentReload = ReloadTime(weaponReloadInterval[weapontype], (WEAPON)weapontype);
                            StartCoroutine(currentReload);
                        }
                        break;
                    }
                case WEAPON.SNIPER:
                    {
                        // リロード中にする事で重複されないようにする
                        isReload = true;
                        // リロード処理
                        Debug.Log("リロード : スナイパーライフル");
                        if (weaponBullet[weapontype] < weaponBulletMax[weapontype])
                        {
                            // リロード時間コルーチンに
                            // それぞれのリロード時間と再度WEAPON型に戻したweapontypeを渡した現在リロードのコルーチン起動
                            currentReload = ReloadTime(weaponReloadInterval[weapontype], (WEAPON)weapontype);
                            StartCoroutine(currentReload);
                        }
                        break;
                    }
                case WEAPON.SHOTGUN:
                    {
                        // リロード中にする事で重複されないようにする
                        isReload = true;
                        // リロード処理
                        Debug.Log("リロード : ショットガン");
                        if (weaponBullet[weapontype] < weaponBulletMax[weapontype])
                        {
                            // リロード時間コルーチンに
                            // それぞれのリロード時間と再度WEAPON型に戻したweapontypeを渡した現在リロードのコルーチン起動
                            currentReload = ReloadTime(weaponReloadInterval[weapontype], (WEAPON)weapontype);
                            StartCoroutine(currentReload);
                        }
                        break;
                    }
                default:
                    {
                        Debug.Log("武器を持っていないぜ！！！！！！！！！！！！！！！！！１！！！１！１！１！１！！！！！！！");
                        break;
                    }
            }
        }
    }

    /// <summary>
    /// 発射クールタイム
    /// </summary>
    /// <param name="_sec">秒数</param>
    /// <returns></returns>
    IEnumerator ShootCooldown(float _sec)
    {
        // _secに指定した秒数待ってから発射可能にする
        yield return new WaitForSeconds(_sec);
        possibleShoot = true;
    }

    /// <summary>
    /// リロード時間
    /// </summary>
    /// <param name="_sec">秒数</param>
    /// <param name="_type">武器種</param>
    /// <returns></returns>
    IEnumerator ReloadTime(float _sec, WEAPON _type)
    {
        // そのまま配列番号として使うためにWEAPON型変数を(int)でint化した物を用意
        int weapontype = (int)_type;
        if (weapontype == (int)WEAPON.SHOTGUN)
        {
            // 弾数が弾の最大数でない場合
            while (weaponBullet[weapontype] < weaponBulletMax[weapontype])
            {
                // _secに指定した秒数待ってから弾を一つずつリロードする
                yield return new WaitForSeconds(_sec);
                weaponBullet[weapontype]++;
            }
        }
        else
        {
            // _secに指定した秒数待ってから武器種毎の最大弾数をリロードする
            yield return new WaitForSeconds(_sec);
            weaponBullet[weapontype] = weaponBulletMax[weapontype];
        }
        isReload = false;
    }

    void OnWeaponChange(InputValue _val)
    {
        if (!isReload && _val.isPressed)
        {
            WeaponChange();
        }
    }

    void WeaponChange()
    {
        // リロード中断して切り替える場合
        //if (currentReload != null)
        //{
        //    StopCoroutine(currentReload);
        //    isReload = false;
        //}

        weaponType++;
        if (weaponType > WEAPONTYPE_MAX)
        {
            weaponType = 0;
        }
        if (weaponType < 0)
        {
            weaponType = WEAPONTYPE_MAX;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // コリジョン/コライダー関連の処理

    private void OnCollisionEnter(Collision collision)
    {
        if (!isGameover &&
            collision.gameObject.CompareTag("Death"))
        {
            Respawn();
            //death.Play();
        }

    }

    private void OnTriggerEnter(Collider collision)
    {
        
    }

}
