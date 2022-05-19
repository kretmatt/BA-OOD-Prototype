using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script that removes the enemy (boid) colliding with the player, damaging them
/// </summary>
public class EnemyTriggerScript : MonoBehaviour
{
    #region Variables

    ////////////////////////////////////////////////////////////////////
    /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////
    
    /// <summary>
    /// The amount of damage the enemy deals to the player on collision
    /// </summary>
    [SerializeField]
    float damage = 5f;

    #endregion

    #region Unity Messages

      ////////////////////////////////////////////////////////////////////
     /////////////////////////    Unity Messages   //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method that gets called once the enemy (boid) collides with the player
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && gameObject.activeInHierarchy)
        {
            EventManager.TriggerEvent(EEventType.HEALTH_DECREASE, new Dictionary<string, object> { { "damage", damage } });
            EventManager.TriggerEvent(EEventType.ENEMY_DEATH, new Dictionary<string, object>() { { "boid", gameObject.GetComponent<Boid>() } });
            PoolManager.ReleaseObject(gameObject);
        }
    }

    #endregion


}
