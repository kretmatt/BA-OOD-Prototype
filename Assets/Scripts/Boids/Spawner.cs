using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/***********************************************
 * Inspired by and adapted from:
 * Title: Boids
 * Author: S., Lague
 * Date: August 26, 2019
 * Availability: https://github.com/SebLague/Boids
 
    MIT License

    Copyright (c) 2019 Sebastian Lague

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

 * ********************************************/

/// <summary>
/// Script for spawning the enemies (boids)
/// </summary>
public class Spawner : MonoBehaviour
{
    #region Variables

      ////////////////////////////////////////////////////////////////////
     /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Enemy prefab
    /// </summary>
    [SerializeField]
    Boid prefab;
    
    /// <summary>
    /// Radius of the spawn zone
    /// </summary>
    [SerializeField]
    float spawnRadius = 10;
    
    /// <summary>
    /// Amount of enemies to spawn
    /// </summary>
    [SerializeField]
    int spawnCount = 100;
    
    /// <summary>
    /// Color of the enemies
    /// </summary>
    [SerializeField]
    Color colour;

    #endregion

    #region Unity Messages

      ////////////////////////////////////////////////////////////////////
     /////////////////////////    Unity Messages   //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Gets called once in the lifetime of this script, when the script is first enabled.
    /// In this case, the object pool for the enemies is created.
    /// </summary>
    private void Awake()
    {
        PoolManager.PreparePool(prefab.gameObject, 100, Vector3.zero);
    }

    /// <summary>
    /// Gets called when the object becomes active in the hierarchy.
    /// Makes the following subscriptions:
    /// 1. GAME_START event:
    ///    SpawnEnemies - Spawns the enemies and triggers the ENEMIES_SPAWNED event
    /// 
    /// 2. SET_ENEMIES event:
    ///    SetEnemies - Sets the amount of enemies to spawn to the value selected by the user
    /// </summary>
    private void OnEnable()
    {
        EventManager.SubscribeMethodToEvent(EEventType.SET_ENEMIES, SetEnemies);
        EventManager.SubscribeMethodToEvent(EEventType.GAME_START, SpawnEnemies);
    }

    /// <summary>
    /// Gets called when the object becomes inactive / gets disabled in the hierarchy.
    /// Resets the following subscriptions:
    /// 1. GAME_START event:
    ///    SpawnEnemies
    /// 
    /// 2. SET_ENEMIES event:
    ///    SetEnemies
    /// </summary>
    private void OnDisable()
    {
        EventManager.UnsubscribeMethodFromEvent(EEventType.GAME_START, SpawnEnemies);
        EventManager.UnsubscribeMethodFromEvent(EEventType.SET_ENEMIES, SetEnemies);
    }

    #endregion

    #region Methods

      ////////////////////////////////////////////////////////////////////
     /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Sets the amount of enemies to the value sent by the event manager
    /// </summary>
    /// <param name="message">Message from the event manager. Contains the new amount of enemies to spawn (key = enemies)</param>
    void SetEnemies(Dictionary<string, object> message)
    {
        if (message.ContainsKey("enemies"))
        {
            spawnCount = (int)message["enemies"];
        }
    }

    /// <summary>
    /// Method for spawning the enemies once the game begins
    /// </summary>
    /// <param name="message">Message from the event manager</param>
    void SpawnEnemies(Dictionary<string, object> message)
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            Boid boid = PoolManager.SpawnObject(prefab.gameObject, pos, Quaternion.identity).GetComponent<Boid>();
            // Select a random starting direction for the boid
            boid.transform.forward = Random.insideUnitSphere;
            boid.SetColour(colour);
        }

        EventManager.TriggerEvent(EEventType.ENEMIES_SPAWNED, null);
    }

    #endregion
}