using UnityEngine;

public class RoadNode : MonoBehaviour
{
    #region 変数宣言

    [Header("次に進むノード")]
    public RoadNode next;   // 次に進むノード

    #endregion

    #region Sceneビューで巡回ポイントを描画する

    // Sceneビューで巡回ポイントを描画する
    private void OnDrawGizmos()
    {
        // 巡回ポイントの色を黄色にする
        Gizmos.color = Color.yellow;

        // このオブジェクトの位置に球体を描画する
        Gizmos.DrawSphere(transform.position, 0.3f);

        // 次の巡回ポイントが設定されている場合
        if (next != null)
        {
            // 次のポイントまでの線を緑色で描画する
            Gizmos.color = Color.green;

            Gizmos.DrawLine(transform.position, next.transform.position);

            // 次のポイントの位置を小さな球体で表示する
            Gizmos.DrawWireSphere(next.transform.position, 0.2f);
        }
    }

    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
