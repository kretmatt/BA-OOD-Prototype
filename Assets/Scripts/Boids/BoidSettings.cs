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
/// Settings for the boid enemies
/// </summary>
[CreateAssetMenu]
public class BoidSettings : ScriptableObject
{
      ////////////////////////////////////////////////////////////////////
     /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Minimum speed of the boids
    /// </summary>
    public float minimumSpeed = 2;
    
    /// <summary>
    /// Maximum speed of the boids
    /// </summary>
    public float maximumSpeed = 5;
    
    /// <summary>
    /// Perception radius of the boids (for flock checks etc)
    /// </summary>
    public float boidPerceptionRadius = 2.5f;
    
    /// <summary>
    /// Avoidance radius of the boid (distance to other boids to avoid them)
    /// </summary>
    public float boidAvoidanceRadius = 1;
    
    /// <summary>
    /// Maximum steer force of the boids
    /// </summary>
    public float maxSteerForce = 3;

    /// <summary>
    /// Weight of the alignment of boids
    /// </summary>
    public float alignmentWeight = 1;
    
    /// <summary>
    /// Weight of the cohesion of boids
    /// </summary>
    public float cohesionWeight = 1;
    
    /// <summary>
    /// Weight of the separation of boids
    /// </summary>
    public float seperationWeight = 1;

    /// <summary>
    /// Weight of the targetting aspect of the boids
    /// </summary>
    public float targetWeight = 1;

    /// <summary>
    /// LayerMask of the obstacle layer
    /// </summary>
    [Header("Collisions")]
    public LayerMask obstacleLayerMask;
    
    /// <summary>
    /// Radius for spherecasts
    /// </summary>
    public float sphereCastRadius = .27f;
    
    /// <summary>
    /// Weight of the avoidance of obstacles
    /// </summary>
    public float collisionAvoidanceWeight = 10;
    
    /// <summary>
    /// Distance of the raycasts / closest possible distance to obstacles
    /// </summary>
    public float collisionAvoidanceDistance = 5;

}