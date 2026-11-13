using UnityEngine;

public class Bullet : MonoBehaviour
{
    //Å@ïœêî
    Rigidbody rb;
    float lifeTime = 2.0f;
    float speed = 0.3f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(this.gameObject, lifeTime);

        Debug.DrawRay(transform.position, transform.forward * 10f, Color.green, 5f);

    }

    void FixedUpdate()
    {
        rb.MovePosition(this.transform.position + this.transform.forward * speed);

        Debug.DrawRay(transform.position, transform.forward * 2f, Color.blue);
    }
}
