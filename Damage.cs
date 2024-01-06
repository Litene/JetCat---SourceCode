using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public interface IDamageable {
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    void TakeDamage(int amount);
}

public interface IDamager {
    public int Damage { get; set; }
}
