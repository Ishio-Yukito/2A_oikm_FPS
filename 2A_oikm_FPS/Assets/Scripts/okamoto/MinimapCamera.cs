using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [SerializeField] private Transform player; // 追従対象のプレイヤー
    [SerializeField] private float height = 20f; // プレイヤーからの高さ（3D用）

    // プレイヤーが移動した後にカメラを動かすため LateUpdate を使用
    void LateUpdate()
    {
        if (player == null) return;

        // プレイヤーのX, Z座標に合わせ、高さ(Y)だけ固定する
        Vector3 newPosition = player.position;
        newPosition.y = player.position.y + height;
        transform.position = newPosition;
    }
}