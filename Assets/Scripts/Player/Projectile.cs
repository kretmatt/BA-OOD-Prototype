using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script that is responsible for the behavior of projectiles
/// </summary>
public class Projectile : MonoBehaviour
{
    #region Variables

      ////////////////////////////////////////////////////////////////////
     /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Impact effect that gets spawned once the projectile collides with an asteroid
    /// </summary>
    public GameObject impactVFX;

    /// <summary>
    /// Flag that determines whether the projectile collided already
    /// </summary>
    private bool collided = false;
    
    /// <summary>
    /// Maximum time the projectile is active for
    /// </summary>
    public float TimeToLive = 2.5f;
    #endregion

    #region Unity Messages

      ////////////////////////////////////////////////////////////////////
     /////////////////////////    Unity Messages   //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method that gets executed once the projectile game object becomes active in the scene. Ensures that the Remove method gets executed after a certain amount of time
    /// </summary>
    private void OnEnable()
    {
        StartCoroutine(Remove());
    }

    /// <summary>
    /// Method that gets executed when the projectile collides with another object
    /// </summary>
    /// <param name="collision">Collision object that contains information about the object the projectile collided with</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Projectile" && collision.gameObject.tag != "Player" && !collided)
        {
            collided = true;

            var impact = Instantiate(impactVFX, collision.contacts[0].point, Quaternion.identity) as GameObject;

            Destroy(impact, 2);
            Destroy(gameObject.GetComponent<Rigidbody>());
            PoolManager.ReleaseObject(this.gameObject);
        }
    }

    #endregion

    #region Methods

      ////////////////////////////////////////////////////////////////////
     /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method that removes the projectile if if has not collided after a certain amount of time
    /// </summary>
    /// <returns></returns>
    IEnumerator Remove()
    {
        yield return new WaitForSeconds(TimeToLive);
        if (!collided)
        {
            Destroy(gameObject.GetComponent<Rigidbody>());
            PoolManager.ReleaseObject(this.gameObject);
        }
    }

    #endregion
}
