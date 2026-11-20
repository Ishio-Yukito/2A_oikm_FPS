using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    GameObject player;
    [SerializeField] GameObject enemy;
    float timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 1.0f)
        {
            timer = 0;
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        float rad = Random.Range(0f, Mathf.PI * 2f);
        Vector2 rad_vec2 = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
        rad_vec2 *= 15f;
        Vector3 spawnPos = player.transform.position + new Vector3(rad_vec2.x, 0, rad_vec2.y);
        Instantiate(enemy, spawnPos, this.transform.rotation);

    }

}
