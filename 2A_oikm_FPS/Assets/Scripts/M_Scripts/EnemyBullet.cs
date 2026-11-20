using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    #region 変数宣言

    [Header("弾の設定")]
    [SerializeField] float speed;     // 弾の移動速度
    [SerializeField] float lifeTime;  // 弾を削除する時間
    [SerializeField] int bulletDamage;// プレイヤーに与えるダメージ

    #endregion

    #region Unityイベント

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 一定時間経過したら弾を削除する
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        // 弾の正面方向へ移動する
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    #endregion

    #region 衝突処理

    void OnTriggerEnter(Collider c)
    {
        // 敵自身や他の弾に当たった場合は無視
        if (c.CompareTag("Enemy") || c.CompareTag("Bullet")) return;

        // Playerに当たったらダメージを与えて消える
        if (c.CompareTag("Player"))
        {
            //// Player_KariのDamageメソッドを呼び出す
            //Player p = c.GetComponent<Player>();
            //if (p != null)
            //{
            //    p.Damage(bulletDamage);
            //}

            Destroy(gameObject);
        }
        // 壁や障害物（Objector）に当たったら消える
        else if (c.CompareTag("Objector"))
        {
            Destroy(gameObject);
        }
    }

    #endregion
}
