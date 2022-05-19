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
/// BoidData struct used for storing important data to move boids in a scene
/// </summary>
public struct BoidData
{
      ////////////////////////////////////////////////////////////////////
     /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Position of the boid
    /// </summary>
    public Vector3 position;
    
    /// <summary>
    /// Up vector of the boid
    /// </summary>
    public Vector3 up;
    
    /// <summary>
    /// Direction of the boid
    /// </summary>
    public Vector3 direction;
    
    /// <summary>
    /// Current velocity of the boid
    /// </summary>
    public Vector3 velocity;

    /// <summary>
    /// Direction where the flock, the current boid belongs to, is heading
    /// </summary>
    public Vector3 flockHeading;
    
    /// <summary>
    /// Position of the flock centre
    /// </summary>
    public Vector3 flockCentre;
    
    /// <summary>
    /// Direction the boid needs to head to in order to avoid other boids
    /// </summary>
    public Vector3 avoidanceHeading;
    
    /// <summary>
    /// Current acceleration of the boid
    /// </summary>
    public Vector3 acceleration;
    
    /// <summary>
    /// Number of members of the flock
    /// </summary>
    public int numFlockmates;
    
    /// <summary>
    /// Index of the boid
    /// </summary>
    public int index;
}