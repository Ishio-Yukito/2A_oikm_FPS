using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    //　変数
    public GameObject player;
    float my;
    void Start()
    {
        
    }

    void Update()
    {
        my = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(my) > 0.01f)
        {
            transform.RotateAround(transform.position, Vector3.right, -my); // 回転軸はplayerオブジェクトのワールド座標Y軸
        }
    }
}
