using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
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
/// Job for calculating the vectors necessary for the movement of boids. It follows the three behavioral rules of boids:
/// 1. Cohesion
/// 2. Separation
/// 3. Alignment
/// </summary>
[BurstCompile]
public struct BoidAccelerationJob : IJobParallelFor
{
    /// <summary>
    /// Deltatime from the main thread
    /// </summary>
    [ReadOnly]
    public float deltaTime;

    /// <summary>
    /// Boid settings
    /// </summary>
    [ReadOnly]
    public BoidSettingsStruct settings;

    /// <summary>
    /// Position of the target (player)
    /// </summary>
    [ReadOnly]
    public Vector3 targetPos;

    /// <summary>
    /// Boid data
    /// </summary>
    public NativeArray<BoidData> boidData;

    /// <summary>
    /// Executes the job
    /// </summary>
    /// <param name="index">Index of the current boid, used for accessing the current boids data</param>
    public void Execute(int index)
    {
        BoidData currData = boidData[index];
        // The acceleration is calculated every frame and does not take previous acceleration values into account
        Vector3 acceleration = Vector3.zero;
        // Steer towards target - Additional "force" that is not basic boid behavior
        Vector3 offsetToTarget = targetPos - currData.position;
        acceleration = SteerTowards(offsetToTarget, currData) * settings.targetWeight;

        if (currData.numFlockmates != 0)
        {
            currData.flockCentre /= currData.numFlockmates;

            Vector3 offsetToFlockmatesCentre = currData.flockCentre - currData.position;

            // Calculate the three forces to abide the boid behavioral rules
            Vector3 alignmentForce = SteerTowards(currData.flockHeading, currData) * settings.alignmentWeight;
            Vector3 cohesionForce = SteerTowards(offsetToFlockmatesCentre, currData) * settings.cohesionWeight;
            Vector3 seperationForce = SteerTowards(currData.avoidanceHeading, currData) * settings.seperationWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }

        currData.acceleration = acceleration;
        boidData[index] = currData;
    }

    Vector3 SteerTowards(Vector3 vector, BoidData bd)
    {
        Vector3 v = vector.normalized * settings.maximumSpeed - bd.velocity;
        return Vector3.ClampMagnitude(v, settings.maxSteerForce);
    }
}