using UnityEngine;

public class Bullet : MonoBehaviour
{
    //Å@ïœêî
    Rigidbody rb;
    float lifeTime = 2.0f;
    float speed = 2f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(this.gameObject, lifeTime);

    }

    void FixedUpdate()
    {
        rb.MovePosition(this.transform.position + this.transform.forward * speed);

    }
}
