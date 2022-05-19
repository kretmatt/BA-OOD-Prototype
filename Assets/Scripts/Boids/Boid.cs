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
/// Component script for boids. Is responsible for the instantiating process and detecting directions, which the boid can use to avoid obstacles.
/// </summary>
public class Boid : MonoBehaviour {

    #region Variables

      ////////////////////////////////////////////////////////////////////
     /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Settings for the boid
    /// </summary>
    BoidSettings settings;

    /// <summary>
    /// Material of the boid
    /// </summary>
    Material material;
    
    /// <summary>
    /// Current velocity of the boid
    /// </summary>
    public Vector3 velocity;

    /// <summary>
    /// Current acceleration of the boid
    /// </summary>
    [HideInInspector]
    public Vector3 acceleration;
    
    /// <summary>
    /// Flag that determines, whether the boid is going to collide with something or not
    /// </summary>
    public bool headingForCollision;

    #endregion

    #region Unity Messages

      ////////////////////////////////////////////////////////////////////
     /////////////////////////    Unity Messages   //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Gets called once in the lifetime of this script, whent the script is first enabled.
    /// "Prepares" the boid by setting some values
    /// </summary>
    void Awake()
    {
        material = transform.GetComponentInChildren<MeshRenderer>().material;
        acceleration = Vector3.zero;
        headingForCollision = false;
    }

    #endregion

    #region Methods

      ////////////////////////////////////////////////////////////////////
     /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////
   
    /// <summary>
    /// Method for initializig spawned boids
    /// </summary>
    /// <param name="settings">Settings for the boid</param>
    public void Initialize(BoidSettings settings)
    {
        this.settings = settings;
        float startSpeed = (settings.minimumSpeed + settings.maximumSpeed) / 2;
        velocity = transform.forward * startSpeed;
    }

    /// <summary>
    /// Method for setting the color of the boid
    /// </summary>
    /// <param name="col">New color for the boid</param>
    public void SetColour(Color col)
    {
        if (material != null)
            material.color = col;
    }

    /// <summary>
    /// Updates the boid data by checking whether there is a collision. If so, it adds a avoid force to the acceleration to prevent collisions.
    /// </summary>
    public void UpdateBoid()
    {
        if (headingForCollision)
        {
            Vector3 collisionAvoidDir = ObstacleRays();
            Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * settings.collisionAvoidanceWeight;
            acceleration += collisionAvoidForce;
        }
    }

    /// <summary>
    /// Method for detecting an unoccupied direction to avoid collisions
    /// </summary>
    /// <returns>The direction to fly towards to prevent a collision</returns>
    Vector3 ObstacleRays()
    {
        Vector3[] rayDirections = CollisionAvoidanceDirectionCalculator.directions;

        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 dir = transform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(transform.position, dir);
            if (!Physics.SphereCast(ray, settings.sphereCastRadius, settings.collisionAvoidanceDistance, settings.obstacleLayerMask))
            {
                return dir;
            }
        }

        return transform.forward;
    }

    /// <summary>
    /// Method for steering towards a certain direction, whilst also considering the maximum speed and the current velocity.
    /// </summary>
    /// <param name="vector">The direction to steer towards</param>
    /// <returns>The actual possible vector for avoiding the obstacles</returns>
    Vector3 SteerTowards(Vector3 vector)
    {
        Vector3 v = vector.normalized * settings.maximumSpeed - velocity;
        return Vector3.ClampMagnitude(v, settings.maxSteerForce);
    }

    #endregion
}