using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

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
/// Job for moving the individual boids and calculating their new velocities.
/// </summary>
[BurstCompile]
public struct BoidMovementJob : IJobParallelForTransform
{
    /// <summary>
    /// Velocities of the boids
    /// </summary>
    public NativeArray<Vector3> velocities;

    /// <summary>
    /// Important movement related data for the boids
    /// </summary>
    [ReadOnly]
    public NativeArray<BoidData> boidData;

    /// <summary>
    /// Deltatime from the main thread because Time.deltaTime can not be called from another thread
    /// </summary>
    [ReadOnly]
    public float deltaTime;

    /// <summary>
    /// Boid settings
    /// </summary>
    [ReadOnly]
    public BoidSettingsStruct settings;

    /// <summary>
    /// Executes the job, updating the TransformAccess parameter
    /// </summary>
    /// <param name="index">Current index of the boid</param>
    /// <param name="transform">Transform data of the boid</param>
    public void Execute(int index, TransformAccess transform)
    {
        Vector3 velocity = velocities[index];
        velocity += boidData[index].acceleration * deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp(speed, settings.minimumSpeed, settings.maximumSpeed);
        velocity = dir * speed;
        transform.position += velocity * deltaTime;
        transform.rotation = Quaternion.LookRotation(dir, boidData[index].up);
        velocities[index] = velocity;
    }
}