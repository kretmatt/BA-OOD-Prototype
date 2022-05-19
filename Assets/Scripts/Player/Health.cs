using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that represents the health of a player
/// </summary>
public class Health : MonoBehaviour
{
    #region Variables

      ////////////////////////////////////////////////////////////////////
     /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Maximum health of the player
    /// </summary>
    [SerializeField] float maxHealth = 100;

    /// <summary>
    /// Current health of the player
    /// </summary>
    [SerializeField] float curHealth;

    /// <summary>
    /// Key of the damage value in the message sent from the EventManager class
    /// </summary>
    private static string damageKey = "damage";

    #endregion

    #region Unity Messages

      ////////////////////////////////////////////////////////////////////
     /////////////////////////    Unity Messages   //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Gets called when the object becomes active in the hierarchy.
    /// Makes the following subscriptions:
    /// 1. HEALTH_DECREASE event:
    ///    TakeDamage - Method that reduces the health of the player by the amount passed in from the EventManager class
    /// 
    /// 2. GAME_END event:
    ///    SetHealth - Resets the health of the player to the maximum value
    /// </summary>
    private void OnEnable()
    {
        EventManager.SubscribeMethodToEvent(EEventType.HEALTH_DECREASE, TakeDamage);
        EventManager.SubscribeMethodToEvent(EEventType.GAME_END, SetHealth);
    }

    /// <summary>
    /// Gets called when the object becomes inactive / gets disabled in the hierarchy.
    /// Resets the following subscriptions:
    /// 1. HEALTH_DECREASE event:
    ///    TakeDamage
    /// 
    /// 2. GAME_END event:
    ///    SetHealth 
    /// </summary>
    private void OnDisable()
    {
        EventManager.UnsubscribeMethodFromEvent(EEventType.HEALTH_DECREASE, TakeDamage);
        EventManager.UnsubscribeMethodFromEvent(EEventType.GAME_END, SetHealth);
    }

    /// <summary>
    /// Method that gets called during initialization. Sets the current health to the maximum health value
    /// </summary>
    void Start()
    {
        curHealth = maxHealth;
    }

    #endregion

    #region Methods

      ////////////////////////////////////////////////////////////////////
     /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method for resetting the current health to the maximum health
    /// </summary>
    /// <param name="message">Message from the EventManager class</param>
    void SetHealth(Dictionary<string, object> message)
    {
        curHealth = maxHealth;
    }

    /// <summary>
    /// Method for simulating taking damage from asteroids or boids
    /// </summary>
    /// <param name="message">Message from the EventManager class. Should contain the amount of damage taken</param>
    public void TakeDamage(Dictionary<string, object> message)
    {
        if (message.ContainsKey(damageKey))
        {
            float damage = (float)message[damageKey];

            curHealth -= damage;
            if (curHealth < 0)
            {
                EventManager.TriggerEvent(EEventType.GAME_END, null);
            }
            else
            {
                EventManager.TriggerEvent(EEventType.HEALTH_DISPLAY, new Dictionary<string, object>() { { "percentage", curHealth / maxHealth } });
            }
        }
    }

    #endregion
}
