using UnityEngine;

public class EscapePoint : MonoBehaviour
{
    #region 変数宣言

    [Header("脱出ポイントの範囲")]
    [SerializeField] float escapeRange; // 脱出範囲

    [Header("プレイヤー")]
    [SerializeField] Transform player; // プレイヤー

    #endregion

    #region Unityイベント

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // プレイヤーを取得
        FindPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        // 脱出範囲を確認
        CheckEscapeDistance();
    }

    #endregion

    #region プレイヤー取得

    // プレイヤーを自動取得する関数
    void FindPlayer()
    {
        // Inspectorで設定されていない場合
        if (player == null)
        {
            // Playerタグのオブジェクトを取得
            GameObject obj = GameObject.FindGameObjectWithTag("Player");

            // オブジェクトが存在する場合
            if (obj != null)
            {
                player = obj.transform;
            }
        }
    }

    #endregion

    #region 脱出範囲の確認

    // プレイヤーとの距離を確認する関数
    void CheckEscapeDistance()
    {
        // プレイヤーが見つからなければ終了
        if (player == null)
        {
            return;
        }

        // 脱出ポイントとプレイヤーの距離を取得
        float distance = Vector3.Distance(transform.position, player.position);

        // 脱出範囲内に入った場合
        if (distance <= escapeRange)
        {
            // 脱出処理を実行
            Escape();
        }
    }

    #endregion

    #region 脱出処理

    // 脱出を完了する関数
    void Escape()
    {
        // Player_Kariを取得
        Player_Kari playerScript = player.GetComponent<Player_Kari>();

        // プレイヤーが存在する場合
        if (playerScript != null)
        {
            // 脱出完了
            playerScript.EscapeCompleted();
        }
    }

    #endregion

    #region Sceneビューで脱出範囲を描画する

    // Sceneビューで脱出範囲を描画する
    private void OnDrawGizmos()
    {
        // 脱出範囲の色を赤色にする
        Gizmos.color = Color.red;

        // 円を構成する線の数
        int segments = 40;

        // 前回の点を保存する
        Vector3 previousPoint = Vector3.zero;

        // 円周上の点を順番に計算する
        for (int i = 0; i <= segments; i++)
        {
            // 円周上の角度を計算
            float angle = i * 360f / segments;

            // 度数法からラジアンに変換
            float radian = angle * Mathf.Deg2Rad;

            // 円周上の点の位置を計算
            Vector3 point = transform.position + new Vector3(Mathf.Sin(radian) * escapeRange, 0f, Mathf.Cos(radian) * escapeRange);

            // 2点目以降は前回の点と線でつなぐ
            if (i > 0)
            {
                Gizmos.DrawLine(previousPoint, point);
            }

            // 現在の点を保存
            previousPoint = point;
        }
    }

    #endregion

    #region プレイヤーとの接触判定(使わない)
    /*
     // プレイヤーが脱出ポイントに触れたとき
    void OnTriggerEnter(Collider c)
    {
        // Playerタグを確認
        if (c.CompareTag("Player"))
        {
            // Player_Kariを取得
            Player_Kari player = c.GetComponent<Player_Kari>();

            // プレイヤーが存在する場合
            if (player != null)
            {
                // 脱出完了
                player.EscapeCompleted();
            }
        }
    }
     */
    #endregion
}
