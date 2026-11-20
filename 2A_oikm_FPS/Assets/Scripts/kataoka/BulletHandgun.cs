using System.ComponentModel;
using UnityEngine;

public class BulletHandgun : MonoBehaviour
{
    Rigidbody rb;
    float lifetime = 2f;
    float speed = 0.3f;
    [SerializeField] float shotPow;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(this.transform.forward * shotPow, ForceMode.Impulse);
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        //rb.MovePosition(this.transform.position + this.transform.forward * speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") ||
            collision.gameObject.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
