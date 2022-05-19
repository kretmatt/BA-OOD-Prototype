using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

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
/// The BoidManager class is a script for managing all boids in the scene. 
/// The script uses mostly jobs to simulate the behavior of boids to use the benefits of the BurstCompiler and the Job System
/// </summary>
public class BoidManager : MonoBehaviour
{
    #region Variables

      ////////////////////////////////////////////////////////////////////
     /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Settings for the boids
    /// </summary>
    [SerializeField]
    BoidSettings settings;
    
    /// <summary>
    /// Settings for the boids in struct form
    /// </summary>
    [SerializeField]
    BoidSettingsStruct settingsStruct;
    
    /// <summary>
    /// Target of the boids
    /// </summary>
    [SerializeField]
    Transform target;
    
    /// <summary>
    /// Boids currently managed by the BoidManager script
    /// </summary>
    [SerializeField]
    List<Boid> boids = new List<Boid>();

    /// <summary>
    /// Boid data array
    /// </summary>
    NativeArray<BoidData> boidData;
    
    /// <summary>
    /// "Temporary" place the boid data is stored. Used as a buffer for output in BoidCalcJob
    /// </summary>
    NativeArray<BoidData> tempdata;
    
    /// <summary>
    /// Hit results from the spherecasts of every single boid ()
    /// </summary>
    NativeArray<RaycastHit> hitResults;
    
    /// <summary>
    /// Sphercast commands of the boids
    /// </summary>
    NativeArray<SpherecastCommand> spherecastCommands;
    
    /// <summary>
    /// Transforms of the boids used in the jobs
    /// </summary>
    TransformAccessArray transformsA;

    /// <summary>
    /// Velocities of the boids
    /// </summary>
    NativeArray<Vector3> velocities;

    /// <summary>
    /// JobHandle for the boid movement job
    /// </summary>
    JobHandle? moveHandle;

    /// <summary>
    /// Flag that determines whether the boids should be updated or not
    /// </summary>
    bool started = false;

    #endregion

    #region Unity Messages

      ////////////////////////////////////////////////////////////////////
     /////////////////////////    Unity Messages   //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Gets called when the object becomes active in the hierarchy.
    /// Makes the following subscriptions:
    /// 1. GAME_END event:
    ///    DespawnBoids - Despawns all boids in the scene once the game ends.  
    /// 
    /// 2. ENEMIES_SPAWNED event:
    ///    InitializeBoids - Initializes all boids spawned from the Spawner script and gives them some values to work with
    /// 
    /// 3. ENEMY_DEATH event
    ///    RemoveBoid - Removes a boid when it collides with the player
    /// </summary>
    private void OnEnable()
    {
        EventManager.SubscribeMethodToEvent(EEventType.ENEMIES_SPAWNED, InitializeBoids);
        EventManager.SubscribeMethodToEvent(EEventType.ENEMY_DEATH, RemoveBoid);
        EventManager.SubscribeMethodToEvent(EEventType.GAME_END, DespawnBoids);
    }

    /// <summary>
    /// Gets called when the object becomes inactive / gets disabled in the hierarchy.
    /// Resets the following subscriptions:
    /// 1. GAME_END event:
    ///    DespawnBoids
    /// 
    /// 2. ENEMIES_SPAWNED event:
    ///    InitializeBoids 
    /// 
    /// 3. ENEMY_DEATH event
    ///    RemoveBoid
    /// </summary>
    private void OnDisable()
    {
        EventManager.UnsubscribeMethodFromEvent(EEventType.ENEMIES_SPAWNED, InitializeBoids);
        EventManager.UnsubscribeMethodFromEvent(EEventType.GAME_END, DespawnBoids);
        EventManager.UnsubscribeMethodFromEvent(EEventType.ENEMY_DEATH, RemoveBoid);
    }

    /// <summary>
    /// Gets called once in the lifetime of this script, when the script is first enabled.
    /// Takes the boid settings object and turns it into a struct, which can then be used in jobs.
    /// </summary>
    private void Awake()
    {
        settingsStruct = new BoidSettingsStruct(settings);
    }

    /// <summary>
    /// Method that gets called every frame.
    /// BoidManager uses Update to calculate boid data, make spherecasts and move the boids.
    /// </summary>
    void Update()
    {
        if (started==true)
        {
            if (moveHandle != null)
            {
                // 7. Finish move job on next frame and save the velocities
                moveHandle?.Complete();
                for (int i = 0; i < boidData.Length; i++)
                {
                    boids[i].velocity = velocities[i];
                }
            }

            for(int i = 0; i < boids.Count; i++)
            {
                var bdata = new BoidData()
                {
                    position = boids[i].transform.position,
                    direction = boids[i].transform.forward,
                    velocity = boids[i].velocity,
                    index = i,
                    up = boids[i].transform.up
                };
                boidData[i] = bdata;
            }
            // 1. Calculate the vectors for the boids by iterating over all boids
            var myJob = new BoidCalcJob()
            {
                dataArray = boidData,
                numBoids = boids.Count,
                settings = settingsStruct,
                goalArray = tempdata,
            };

            var boidCalcHandle = myJob.Schedule(boids.Count, 64);

            // 2. Check if they are heading for collision 
            for (int i = 0; i < boids.Count; i++)
            {
                spherecastCommands[i] = new SpherecastCommand(boids[i].transform.position, settings.sphereCastRadius, boids[i].transform.forward, settings.collisionAvoidanceDistance, settings.obstacleLayerMask);
            }

            var sphereHandle = SpherecastCommand.ScheduleBatch(spherecastCommands, hitResults, 1, boidCalcHandle);

            // 3. Calculate all forces (except avoid force)

            var calcAccelerationJob = new BoidAccelerationJob()
            {
                deltaTime = Time.deltaTime,
                settings = settingsStruct,
                boidData = tempdata,
                targetPos = target.position
            };

            var calcAccelerationHandle = calcAccelerationJob.Schedule(boidData.Length, 64, sphereHandle);

            calcAccelerationHandle.Complete();

            // 4. Update the boids (and their velocities)

            for (int i = 0; i < boids.Count; i++)
            {
                boidData[i] = tempdata[i];
                boids[i].acceleration = boidData[i].acceleration;
                boids[i].headingForCollision = hitResults[i].collider != null ? true : false;
                boids[i].UpdateBoid();
            }

            // 5. Retrieve the current velocities & accelerations

            for (int i = 0; i < boids.Count; i++)
            {
                velocities[i] = boids[i].velocity;
                BoidData tempBd = boidData[i];
                tempBd.acceleration = boids[i].acceleration;
                boidData[i] = tempBd; 
            }

            // 6. Move the boids

            var moveJob = new BoidMovementJob()
            {
                deltaTime = Time.deltaTime,
                settings = settingsStruct,
                velocities = velocities,
                boidData = boidData
            };

            moveHandle = moveJob.Schedule(transformsA);
        }
    }

    #endregion

    #region Methods

      ////////////////////////////////////////////////////////////////////
     /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// InitializeBoids finds all objects of the type Boid and initializes them
    /// </summary>
    /// <param name="message">Message from the event manager</param>
    void InitializeBoids(Dictionary<string, object> message)
    {
        started = true;
        boids = new List<Boid>();
        var foundBoids = FindObjectsOfType<Boid>(false);
        
        boidData = new NativeArray<BoidData>(foundBoids.Length, Allocator.Persistent);
        tempdata = new NativeArray<BoidData>(foundBoids.Length, Allocator.Persistent);
        hitResults = new NativeArray<RaycastHit>(foundBoids.Length, Allocator.Persistent);
        velocities = new NativeArray<Vector3>(foundBoids.Length, Allocator.Persistent);
        spherecastCommands = new NativeArray<SpherecastCommand>(foundBoids.Length, Allocator.Persistent);
        
        Transform[] transforms = new Transform[foundBoids.Length];
        
        for (int i = 0; i < foundBoids.Length; i++)
        {
            foundBoids[i].Initialize(settings);
            boids.Add(foundBoids[i]);
            transforms[i] = foundBoids[i].transform;
        }

        transformsA = new TransformAccessArray(transforms, JobsUtility.JobWorkerMaximumCount);
    }

    /// <summary>
    /// Despawns a Boid passed in from the event manager
    /// </summary>
    /// <param name="message">Message from the event manager. Contains the Boid to be removed from the BoidManager (e.g. Boid collides with Player, dealing damage and despawning afterwards)</param>
    void RemoveBoid(Dictionary<string, object> message)
    {
        if (message.ContainsKey("boid"))
        {
            Boid b = (Boid)message["boid"];
            boids.Remove(b);
        }
    }

    /// <summary>
    /// Removes the boids from the scene and cleans up the native arrays
    /// </summary>
    /// <param name="message">Message from the event manager</param>
    void DespawnBoids(Dictionary<string, object> message)
    {
        started = false;
        moveHandle?.Complete();

        foreach (Boid b in boids)
        {
            Destroy(b.gameObject);
        }

        boids.Clear();

        if (boidData.IsCreated)
            boidData.Dispose();
        if (tempdata.IsCreated)
            tempdata.Dispose();
        if (hitResults.IsCreated)
            hitResults.Dispose();
        if (spherecastCommands.IsCreated)
            spherecastCommands.Dispose();
        if (transformsA.isCreated)
            transformsA.Dispose();
        if (velocities.IsCreated)
            velocities.Dispose();
    }

    #endregion
}
