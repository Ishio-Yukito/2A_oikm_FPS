using UnityEngine;

public class PlayerSatus : MonoBehaviour
{
    public static PlayerSatus Instance { get; private set; }

    public int CurrentHP;

    private void Awake()
    {
        // ƒVƒ“ƒOƒ‹ƒgƒ“‚ÌŽÀ‘Ì‚ðŠ„‚è“–‚Ä‚é
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        CurrentHP = 100;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            CurrentHP -= 10;
        }
    }
}