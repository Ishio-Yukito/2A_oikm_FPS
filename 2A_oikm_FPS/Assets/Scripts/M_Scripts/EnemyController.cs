using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyController : MonoBehaviour
{
    #region 変数宣言

    [Header("敵の設定")]
    [SerializeField] GameObject expPrefab; // 爆発エフェクト
    [SerializeField] int hp;               // 敵の初期HP

    [Header("アイテムドロップ設定")]
    [SerializeField] GameObject dropItemPrefab;   // ドロップするアイテム
    [SerializeField] float dropItemHeight = 0.5f; // アイテムの出現する高さ
    bool isDead = false;                          // 敵が倒されたか

    [Header("HPバー設定")]
    [SerializeField] UnityEngine.UI.Slider hpSlider; // HPバー
    [SerializeField] GameObject hpBarObject;         // HPバーの表示・非表示

    [Header("発見・見失い表示設定")]
    [SerializeField] UnityEngine.UI.Text alertText; // 発見・見失いを表示するText
    [SerializeField] GameObject alertObject;        // 表示・非表示を切り替えるCanvas
    [SerializeField] float alertDisplayTime;        // 表示する時間
    float alertTimer = 0f;                          // 表示時間を計測するタイマー

    [Header("巡回設定")]
    [SerializeField] RoadNode startNode;   // 巡回を開始するポイント
    [SerializeField] float patrolSpeed;    // 巡回中の移動速度
    
    [Header("プレイヤー発見設定")]
    [SerializeField] Transform player;     // 発見対象のプレイヤー
    [SerializeField] float viewDistance ;  // プレイヤーを発見できる距離
    [SerializeField] float viewAngle;      // 敵が前方を見渡せる角度
    [SerializeField] float loseSightTime;  // 見失ってから追跡をやめるまでの時間

    [Header("追跡設定")]
    [SerializeField] float chaseSpeed;     // プレイヤーを追いかける速度
    [SerializeField] float rotationSpeed;  // 敵が向きを変える速さ

    [Header("射撃設定")]
    [SerializeField] GameObject bulletPrefab; // 弾のPrefab
    [SerializeField] Transform firePoint;     // 弾の発射位置
    [SerializeField] float shootInterval;     // 発射間隔
    [SerializeField] AudioClip shootSE;       // 弾の発射SE
    AudioSource audioSource;                  // AudioSource
    float shootTimer = 0f;                    // 発射までの時間

    [Header("障害物設定")]
    [SerializeField] float obstacleOffset;        // 壁から離す距離
    [SerializeField] float eyeHeight;             // 敵の目の高さ
    [SerializeField] float obstacleCheckDistance; // 障害物を先読みする距離
    CapsuleCollider capsuleCollider;              // 敵のCapsule Collider

    // 現在移動している巡回ポイント
    RoadNode currentNode;

    // 敵の行動状態
    bool isChasing = false;   // プレイヤーを追跡しているか
    bool isReturning = false; // 巡回ポイントへ戻っているか

    // プレイヤーを見失ってからの経過時間
    float lostTimer = 0f;

    // 敵の現在の行動状態
    string currentState = "";

    #endregion

    #region Unityイベント

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 最初の巡回ポイントを設定する
        currentNode = startNode;

        // Inspectorでプレイヤーが設定されていない場合
        if (player == null)
        {
            // TagがPlayerのオブジェクトを探す
            GameObject obj = GameObject.FindGameObjectWithTag("Player");

            // プレイヤーが見つかった場合はTransformを取得
            if (obj != null)
            {
                player = obj.transform;
            }
        }

        // HPバーの初期設定
        if (hpSlider != null)
        {
            // 最大HPを設定
            hpSlider.maxValue = hp;

            // 現在のHPを設定
            hpSlider.value = hp;
        }

        // 最初はHPバーを非表示にする
        if (hpBarObject != null)
        {
            hpBarObject.SetActive(false);
        }

        // 発見・見失い表示を非表示にする
        if (alertObject != null)
        {
            alertObject.SetActive(false);
        }

        // AudioSourceを取得
        audioSource = GetComponent<AudioSource>();

        // Capsule Colliderを取得
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        // デバッグ：RキーでHP減少
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Damage(1);
        }

        // Alertの表示時間を計測
        if (alertObject != null && alertObject.activeSelf)
        {
            alertTimer += Time.deltaTime;

            // 設定した時間が経過した場合
            if (alertTimer >= alertDisplayTime)
            {
                // Alertを非表示にする
                alertObject.SetActive(false);

                // タイマーをリセット
                alertTimer = 0f;
            }
        }

        // プレイヤーまたは巡回ポイントがない場合は処理を行わない
        if (player == null || currentNode == null)
        {
            return;
        }

        // 敵がプレイヤーを視界内に捉えているか調べる
        bool canSeePlayer = CanSeePlayer();

        // プレイヤーを発見している場合
        if (canSeePlayer)
        {
            // 新しく発見した瞬間だけ表示する
            if (!isChasing || isReturning)
            {
                // プレイヤーを発見した表示
                if (alertText != null)
                {
                    alertText.text = "！";
                }

                // 表示時間をリセット
                alertTimer = 0f;

                // 表示を有効にする
                if (alertObject != null)
                {
                    alertObject.SetActive(true);
                }
            }

            isChasing = true;    // 追跡状態にする
            isReturning = false; // 戻る処理を解除する
            lostTimer = 0f;      // 見失った時間をリセットする

            // 発見したことをConsoleに表示
            ChangeState("プレイヤーを発見！");
        }
        // プレイヤーが見えなくなり、追跡中の場合
        else if (isChasing)
        {
            // 見失ってからの時間を加算する
            lostTimer += Time.deltaTime;

            // 一定時間見つけられなかった場合
            if (lostTimer >= loseSightTime)
            {
                isChasing = false;   // 追跡を終了する
                isReturning = true;  // 巡回ポイントへ戻る

                // プレイヤーを見失った表示
                if (alertText != null)
                {
                    alertText.text = "？";
                }

                // 表示時間をリセット
                alertTimer = 0f;

                // 表示を有効にする
                if (alertObject != null)
                {
                    alertObject.SetActive(true);
                }

                // 戻る状態になったことを表示
                ChangeState("巡回ポイントへ戻る");
            }
        }

        // 現在の行動状態に応じて処理を切り替える
        if (isChasing)
        {
            // 追跡中の表示
            ChangeState("プレイヤー追跡");

            // プレイヤーを追いかける
            ChasePlayer();

            // プレイヤーが視界内にいる場合は撃つ
            if (canSeePlayer)
            {
                ShootAtPlayer();
            }
        }
        else if (isReturning)
        {
            // 元の巡回ポイントへ戻る
            ReturnToNode();
        }
        else
        {
            // 巡回中の表示
            ChangeState("巡回中");

            // 巡回ルートに沿って移動する
            Patrol();
        }
    }

    #endregion

    #region プレイヤーを発見できるか判定

    // プレイヤーを発見できるか判定
    bool CanSeePlayer()
    {
        // 敵からプレイヤーへ向かう方向を求める（水平方向で計算）
        Vector3 direction = player.position - transform.position;
        direction.y = 0; // 高さを揃えて平面上の方向と距離を計算

        // 敵とプレイヤーの距離を求める
        float distance = direction.magnitude;

        // 発見できる距離より遠い場合は見えない
        if (distance > viewDistance)
        {
            return false;
        }

        // 敵の正面（水平）とプレイヤーの方向との角度を求める
        Vector3 forward = transform.forward;
        forward.y = 0;
        float angle = Vector3.Angle(forward, direction);

        // 視界の角度から外れている場合は見えない
        if (angle > viewAngle * 0.5f)
        {
            return false;
        }

        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;

        // プレイヤーの上半身の位置
        Vector3 targetPosition = player.position + Vector3.up * 1f;

        // 敵からプレイヤーへ向かう方向
        Vector3 rayDirection = targetPosition - eyePosition;

        // 最初に当たったオブジェクトのみを検出
        if (Physics.Raycast(eyePosition, rayDirection.normalized, out RaycastHit hit, rayDirection.magnitude))
        {
            // 最初に当たったのが Objector であれば視界が遮られている
            if (hit.collider.CompareTag("Objector"))
            {
                return false;
            }
        }

        // 距離・角度ともに範囲内であれば発見とみなす
        return true;
    }

    #endregion

    #region 弾を撃つ処理

    // プレイヤーに向かって弾を撃つ
    void ShootAtPlayer()
    {
        // 発射間隔を計測する
        shootTimer += Time.deltaTime;

        // 発射間隔に達していなければ撃たない
        if (shootTimer < shootInterval)
        {
            return;
        }

        // 発射時間をリセットする
        shootTimer = 0f;

        // 弾のPrefabと発射位置が設定されているか確認
        if (bulletPrefab == null || firePoint == null)
        {
            return;
        }

        // プレイヤーの方向を計算する
        Vector3 direction = player.position - firePoint.position;
        direction.y = 0f; // 水平方向に固定

        // ほぼ同位置でなければ向きを判定
        if (direction.sqrMagnitude > 0.001f)
        {
            direction.Normalize();

            // 水平方向にまっすぐ向けた角度で弾を生成
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
        }

        // 弾の発射SEを再生
        if (audioSource != null && shootSE != null)
        {
            audioSource.PlayOneShot(shootSE);
        }
    }

    #endregion

    #region 巡回処理

    // 巡回処理
    void Patrol()
    {
        // 現在の巡回ポイントへ移動する
        MoveTo(currentNode.transform.position, patrolSpeed);

        // 敵と巡回ポイントの距離を求める
        // 高さ（Y軸）を除外した距離を求める
        Vector3 enemyPos = transform.position;
        Vector3 nodePos = currentNode.transform.position;
        enemyPos.y = 0;
        nodePos.y = 0;

        float distance = Vector3.Distance(enemyPos, nodePos);

        // ポイントに十分近づいた場合
        if (distance < 0.3f)
        {
            // 次の巡回ポイントが設定されている場合
            if (currentNode.next != null)
            {
                // 次のポイントに切り替える
                currentNode = currentNode.next;
            }
        }
    }

    #endregion

    #region プレイヤー追跡処理

    // プレイヤー追跡処理
    void ChasePlayer()
    {
        // プレイヤーの現在位置へ向かって移動する
        MoveTo(player.position, chaseSpeed);
    }

    #endregion

    #region 巡回ポイントへ戻る処理

    // 巡回ポイントへ戻る処理
    void ReturnToNode()
    {
        // 現在の巡回ポイントへ戻る
        MoveTo(currentNode.transform.position, patrolSpeed);

        // 高さ（Y軸）を除外して巡回ポイントとの距離を求める
        Vector3 enemyPos = transform.position;
        Vector3 nodePos = currentNode.transform.position;
        enemyPos.y = 0;
        nodePos.y = 0;

        float distance = Vector3.Distance(enemyPos, nodePos);

        // 元の巡回ポイントに到着した場合
        if (distance < 0.3f)
        {
            isReturning = false; // 戻る処理を終了する（通常巡回モードに戻る）
            isChasing = false;   // 追跡フラグを確実にクリアする
            lostTimer = 0f;      // 見失った時間をリセットする

            // Consoleに巡回再開を表示
            ChangeState("巡回中");
        }
    }

    #endregion

    #region デバック処理

    // 行動状態が変わったときにConsoleへ表示する
    void ChangeState(string newState)
    {
        // 前回と同じ状態なら表示しない
        if (currentState == newState)
        {
            return;
        }

        // 現在の状態を更新
        currentState = newState;

        // Consoleに敵の行動状態を表示
        Debug.Log(gameObject.name + "：" + currentState);
    }

    // Sceneビューで敵の視界範囲を描画する
    private void OnDrawGizmos()
    {
        // 敵の視界範囲を青色で表示
        Gizmos.color = Color.cyan;

        // 視界の届く距離を表示
        // 敵の正面方向を取得
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        // 視界の左右の境界角度を計算
        Vector3 leftDirection = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * forward;
        Vector3 rightDirection = Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * forward;

        // 地面と平行な円を描画する
        int segments = 40;
        Vector3 previousPoint = Vector3.zero;

        // 円を構成する点を順番に計算する
        // iを0からsegmentsまで繰り返す
        for (int i = 0; i <= segments; i++)
        {
            // 円周上の角度を計算する（0度から360度）
            float angle = i * 360f / segments;

            // 度数法の角度をラジアンに変換する
            // SinとCosで座標を計算するために使用
            float radian = angle * Mathf.Deg2Rad;

            // 円周上の点の位置を計算する
            // X軸とZ軸で円を描き、Y軸は0にして地面と平行にする
            Vector3 point = transform.position + new Vector3(Mathf.Sin(radian) * viewDistance,0f, Mathf.Cos(radian) * viewDistance);

            // 最初の点では線を引かない
            // 2点目以降は前回の点と現在の点を線でつなぐ
            if (i > 0)
            {
                Gizmos.DrawLine(previousPoint, point);
            }

            // 現在の点を保存し、次回の線の始点として使用する
            previousPoint = point;
        }

        // 視界の左右の境界線を描画
        Gizmos.DrawRay(transform.position,leftDirection * viewDistance);
        Gizmos.DrawRay(transform.position, rightDirection * viewDistance);
    }

    #endregion

    #region 指定した位置へ移動する共通処理

    // 指定した位置へ移動する共通処理
    void MoveTo(Vector3 destination, float speed)
    {
        // 移動先の高さを敵の現在の高さに合わせる
        // 水平方向だけ移動させるため
        destination.y = transform.position.y;

        // 敵から移動先へ向かう方向を求める
        Vector3 direction = destination - transform.position;
        direction.y = 0f;

        // 移動先がほぼ同じ位置ではない場合
        if (direction.sqrMagnitude > 0.01f)
        {
            // 移動方向を正規化
            direction.Normalize();

            // 1フレームで移動する距離
            float moveDistance = speed * Time.deltaTime;

            // 障害物を避けて移動できる方向を探す
            Vector3 moveDirection = FindMoveDirection(direction, moveDistance);

            // 移動できる方向が見つかった場合
            if (moveDirection != Vector3.zero)
            {
                // 移動方向に向くための回転
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

                // 滑らかに回転させる
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                // 移動
                transform.position += moveDirection * moveDistance;
            }
        }
    }

    #endregion

    #region 障害物を避けて移動できる方向を探す処理

    // 障害物を避けて移動できる方向を探す処理
    Vector3 FindMoveDirection(Vector3 direction, float distance)
    {
        // 移動方向の候補
        float[] angles =
        {
             0f,
            15f,  -15f,
            30f,  -30f,
            45f,  -45f,
            60f,  -60f,
            75f,  -75f,
            90f,  -90f,
           105f,  -105f,
           120f,  -120f,
           135f,  -135f,
           150f,  -150f,
           165f,  -165f
        };

        Vector3 bestDirection = Vector3.zero;
        float bestScore = -float.MaxValue;

        // 1フレーム分より長い距離で障害物を確認
        float checkDistance = Mathf.Max(obstacleCheckDistance, distance + obstacleOffset);

        // すべての方向を確認
        foreach (float angle in angles)
        {
            // 元の進行方向から角度を変更
            Vector3 candidateDirection = Quaternion.Euler(0f, angle, 0f) * direction;

            candidateDirection.Normalize();

            // 障害物がある場合は次の方向を確認
            if (CheckObstacle(candidateDirection, checkDistance, out _))
            {
                continue;
            }

            // 元の進行方向に近いほど高得点
            float score = Vector3.Dot(direction, candidateDirection);

            // 最も進行方向に近い候補を保存
            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = candidateDirection;
            }
        }

        return bestDirection;
    }

    #endregion

    #region 移動先に障害物があるか調べる処理

    // 移動先に障害物があるか調べる処理
    bool CheckObstacle(Vector3 direction, float distance, out RaycastHit obstacleHit)
    {
        // out変数を初期化
        obstacleHit = default;

        // Capsule Colliderがない場合
        if (capsuleCollider == null)
        {
            return false;
        }

        // Capsule Colliderの中心位置
        Vector3 center = transform.TransformPoint(capsuleCollider.center);

        // Capsule Colliderの半径と高さ
        float scale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));

        // 判定用カプセルの半径を少し小さくする
        float radius = capsuleCollider.radius * scale * 0.85f;
        float height = Mathf.Max(capsuleCollider.height * Mathf.Abs(transform.lossyScale.y), radius * 2f);

        // CapsuleCastの始点と終点
        Vector3 bottom = center + Vector3.down * (height * 0.5f - radius);
        Vector3 top = center + Vector3.up * (height * 0.5f - radius);

        // 障害物を検出
        RaycastHit[] hits = Physics.CapsuleCastAll(bottom, top, radius, direction, distance + 
                                                   obstacleOffset, Physics.DefaultRaycastLayers, 
                                                   QueryTriggerInteraction.Ignore);

        float nearestDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            // 自分自身は無視
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                continue;
            }

            // Objector以外は無視
            if (!hit.collider.CompareTag("Objector"))
            {
                continue;
            }

            // 最も近い障害物を取得
            if (hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                obstacleHit = hit;
            }
        }

        // 障害物が見つかった場合はtrue
        return nearestDistance != float.MaxValue;
    }

    #endregion

    #region ダメージを受ける処理

    // ダメージを受ける処理
    void Damage(int damage)
    {
        // すでに倒されている場合は処理しない
        if (isDead) return;

        // HPを減らす
        hp -= damage;

        // HPが最大値を超えないようにする
        hp = Mathf.Max(0, hp);

        // 現在のHPをConsoleに表示
        Debug.Log(gameObject.name + "：残りHP " + hp);

        // HPバーを表示する
        if (hpBarObject != null)
        {
            hpBarObject.SetActive(true);
        }

        // HPバーの数値を更新する
        if (hpSlider != null)
        {
            hpSlider.value = hp;
        }

        // HPが0以下になった場合
        if (hp <= 0)
        {
            // 倒された状態にする
            isDead = true;

            // 爆発エフェクトが設定されている場合
            if (expPrefab != null)
            {
                // 爆発エフェクトを生成
                Instantiate(expPrefab, transform.position, Quaternion.identity);
            }

            // アイテムが設定されている場合
            if (dropItemPrefab != null)
            {
                // 敵の少し上にアイテムを生成
                Vector3 dropPosition = transform.position + Vector3.up * dropItemHeight;

                Instantiate(dropItemPrefab, dropPosition, Quaternion.identity);
            }

            // 敵を削除
            Destroy(gameObject);
        }
    }

    #endregion

    #region 衝突処理

    // 衝突処理
    void OnTriggerEnter(Collider c)
    {
        // 敵同士の衝突を無視
        if (c.CompareTag("Enemy")) return;

        // 弾に当たったら
        if (c.CompareTag("Bullet"))
        {
            // HPを1減らす
            Damage(1);

            // 敵を削除
            Destroy(c.gameObject);
        }
    }

    #endregion
}
