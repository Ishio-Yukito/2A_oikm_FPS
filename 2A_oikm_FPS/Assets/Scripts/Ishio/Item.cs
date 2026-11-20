using UnityEngine;

public class Item : MonoBehaviour
{
    #region
    //[SerializeField] GameObject ItemPanel;
    bool nearItem = false; //アイテムが近くにあるかどうか
    #endregion
    #region Unityイベント

    void Start()
    {

    }

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
            nearItem = true;
            //ItemPanel.SetActive(true);
        }
    }

    void OnTriggerExit(Collider c)
    {
        if (c.CompareTag("Player"))
        {
            nearItem = false;
            //ItemPanel.SetActive(false);
        }
    }
    #endregion
}

   
