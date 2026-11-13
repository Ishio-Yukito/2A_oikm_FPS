using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class Bullet_A : MonoBehaviour
{
    //’è”E•Ï”

    Rigidbody rb;
    float lifeTime = 2.0f;
    float speed = 0.3f;

    //‰Šú‰»ˆ—
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(this.gameObject, lifeTime);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.MovePosition(this.transform.position + this.transform.forward* speed);
    }
}
