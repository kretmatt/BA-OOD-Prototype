using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The asteroid class is a component for marking a game object as an asteroid. Asteroids are elements in the scene that can be destroyed using projectiles.
/// Once an asteroid gets destroyed, the current score gets increased by the maximum health of the asteroid.
/// </summary>
public class Asteroid : MonoBehaviour
{
    #region Variables

      ////////////////////////////////////////////////////////////////////
     /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Maximum health of the asteroid
    /// </summary>
    [SerializeField]
    int maxHealth;

    /// <summary>
    /// Current health of the asteroid
    /// </summary>
    int currentHealth;

    #endregion

    #region Unity Messages

      ////////////////////////////////////////////////////////////////////
     /////////////////////////    Unity Messages   //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Gets called when the object becomes active in the hierarchy.
    /// Makes the following subscriptions:
    /// 1. GAME_END event:
    ///     1.1 ResetHealth - Resets the current health of an asteroid to the maximum
    /// </summary>
    private void OnEnable()
    {
        EventManager.SubscribeMethodToEvent(EEventType.GAME_END, ResetHealth);
    }

    /// <summary>
    /// Gets called when the object becomes inactive / gets disabled in the hierarchy.
    /// Resets the following subscriptions:
    /// 1. GAME_END event:
    ///     1.1 ResetHealth
    /// </summary>
    private void OnDisable()
    {
        EventManager.UnsubscribeMethodFromEvent(EEventType.GAME_END, ResetHealth);
    }

    /// <summary>
    /// Gets called once in the lifetime of this script, when the script is first enabled
    /// Sets the current health to the maximum health.
    /// </summary>
    private void Start()
    {
        ResetHealth(null);
    }

    /// <summary>
    /// Method that gets called when the asteroid collides with another collider / rigid body.
    /// </summary>
    /// <param name="collision">The other game object which collides with the asteroid.</param>
    private void OnCollisionEnter(Collision collision)
    {
        // If the colliding game object is a projectile, reduce the health of the asteroid. Once destroyed (current health reaches 0), increase the score by the maximum health of this asteroid.
        if (collision.gameObject.CompareTag("Projectile"))
        {
            currentHealth--;

            if (currentHealth <= 0)
            {
                PoolManager.ReleaseObject(this.gameObject);
                EventManager.TriggerEvent(EEventType.SCORE_INCREASE, new Dictionary<string, object>() { { "amount", maxHealth } });
            }
        }
        // If the colliding game object is the player, reduce their health.
        else if (collision.gameObject.CompareTag("Player"))
        {
            EventManager.TriggerEvent(EEventType.HEALTH_DECREASE, new Dictionary<string, object>() { { "damage", (float)currentHealth } });
        }
    }

    #endregion

    #region Methods

      ////////////////////////////////////////////////////////////////////
     /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method for resetting the health of an asteroid.
    /// </summary>
    /// <param name="message">Message passed in from the event manager</param>
    void ResetHealth(Dictionary<string, object> message)
    {
        currentHealth = maxHealth;
    }

    #endregion
}
