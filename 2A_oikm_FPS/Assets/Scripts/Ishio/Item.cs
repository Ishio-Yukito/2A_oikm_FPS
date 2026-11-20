using UnityEngine;

public class Item : MonoBehaviour
{
    #region
    //[SerializeField] GameObject ItemPanel;
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
            //ItemPanel.SetActive(true);
        }
    }

    void OnTriggerExit(Collider c)
    {
        if (c.CompareTag("Player"))
        {
            //ItemPanel.SetActive(false);
        }
    }
    #endregion
}

   
