using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
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
/// Job that calculates the different vectors for boid movements. Specifically, the following three values are calculated by iterating over other boids:
/// 1. Alignment (Flock Heading)
/// 2. Separation (Avoidance Heading)
/// 3. Cohesion (Flock Centre)
/// </summary>
[BurstCompile]
public struct BoidCalcJob : IJobParallelFor
{
    /// <summary>
    /// Write buffer of this job. dataArray can not be updated during this job because this would lead to inconsistencies. Instead the results are saved to the goalArray variable
    /// </summary>
    [WriteOnly]
    public NativeArray<BoidData> goalArray;

    /// <summary>
    /// Boid data to be updated / worked on
    /// </summary>
    [ReadOnly]
    public NativeArray<BoidData> dataArray;

    /// <summary>
    /// Boid settings
    /// </summary>
    [ReadOnly]
    public BoidSettingsStruct settings;

    /// <summary>
    /// Amount of boids
    /// </summary>
    [ReadOnly]
    public float numBoids;

    /// <summary>
    /// Executes the job for a boid with the specified index
    /// </summary>
    /// <param name="index">Index of the current boid</param>
    public void Execute(int index)
    {
        BoidData bd = new BoidData()
        {
            position = dataArray[index].position,
            direction = dataArray[index].position,
            index = index
        };
        for (int i = 0; i < dataArray.Length; i++)
        {
            if (dataArray[i].index != index)
            {

                float3 offset = dataArray[i].position - dataArray[index].position;
                float sqrDst = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;
                // Check if the boid is within the perceptionRadius
                if (sqrDst < settings.boidPerceptionRadius * settings.boidPerceptionRadius)
                {
                    bd.numFlockmates += 1;
                    bd.flockHeading += dataArray[i].direction;
                    bd.flockCentre += dataArray[i].position;

                    // Check if the boid is within the avoidanceRadius
                    if (sqrDst < settings.boidAvoidanceRadius * settings.boidAvoidanceRadius)
                    {
                        float3 avValue = offset / sqrDst;
                        Vector3 avector = new Vector3(avValue.x, avValue.y, avValue.z);
                        bd.avoidanceHeading -= avector;
                    }
                }
            }
        }
        goalArray[index] = bd;
    }
}