using UnityEngine;

public class EnemySimple : MonoBehaviour
{
    public float maxHealth = 100f;
    float hp;

    void Start()
    {
        hp = maxHealth;
    }

    public void TakeDamage(float d)
    {
        hp -= d;
        if (hp <= 0) Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
