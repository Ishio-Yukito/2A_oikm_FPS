using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    #region Unityイベント

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #endregion

    #region 衝突処理

    // プレイヤーがアイテムに触れたとき
    void OnTriggerEnter(Collider c)
    {
        // Playerタグのオブジェクトに触れた場合
        if (c.CompareTag("Player"))
        {
            // アイテムを削除
            Destroy(gameObject);
        }
    }

    #endregion
}
