using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ShipController is a script for controllig the player and shooting projectiles
/// </summary>
public class ShipController : MonoBehaviour
{
    #region Variables

      ////////////////////////////////////////////////////////////////////
     /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Current destination of the projectiles
    /// </summary>
    private Vector3 destination;

    /// <summary>
    /// Positions where the projectiles get spawned
    /// </summary>
    public Transform leftFirePoint, rightFirePoint;

    /// <summary>
    /// Prefab of the projectile
    /// </summary>
    public GameObject projectile;

    /// <summary>
    /// Movement speed of the projectiles
    /// </summary>
    public float projectileSpeed = 120f;

    /// <summary>
    /// Movement speed of the player
    /// </summary>
    [SerializeField]
    private float forwardMoveSpeed = 60f;

    /// <summary>
    /// Speed of the rotation
    /// </summary>
    [SerializeField]
    private float turnSpeed = 60f;

    /// <summary>
    /// Flag that determines whether the user can control the ship at the moment
    /// </summary>
    bool active;

    /// <summary>
    /// Spawn point of the player character
    /// </summary>
    [SerializeField]
    Transform spawnPoint;

    #endregion

    #region Unity Messages

      ////////////////////////////////////////////////////////////////////
     /////////////////////////    Unity Messages   //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Gets called when the object becomes active in the hierarchy.
    /// Makes the following subscriptions:
    /// 1. GAME_START event:
    ///    ToggleShip - Toggles the user controls
    ///    ResetPosition - Moves the ship to the spawn point.
    /// 
    /// 3. GAME_END event:
    ///    ToggleShip - Toggles the user controls
    /// </summary>
    private void OnEnable()
    {
        EventManager.SubscribeMethodToEvent(EEventType.GAME_START, ToggleShip);
        EventManager.SubscribeMethodToEvent(EEventType.GAME_START, ResetPosition);
        EventManager.SubscribeMethodToEvent(EEventType.GAME_END, ToggleShip);
    }

    /// <summary>
    /// Gets called when the object becomes inactive / gets disabled in the hierarchy.
    /// Resets the following subscriptions:
    /// 1. GAME_START event:
    ///    ToggleShip
    ///    ResetPosition
    /// 
    /// 3. GAME_END event:
    ///    ToggleShip
    /// </summary>
    private void OnDisable()
    {
        EventManager.UnsubscribeMethodFromEvent(EEventType.GAME_START, ToggleShip);
        EventManager.UnsubscribeMethodFromEvent(EEventType.GAME_START, ResetPosition);
        EventManager.UnsubscribeMethodFromEvent(EEventType.GAME_END, ToggleShip);
    }

    /// <summary>
    /// Gets called once in the lifetime of this script, when the script is first enabled.
    /// Prepares projectiles for the game and disables the user controls
    /// </summary>
    private void Awake()
    {
        active = false;
        PoolManager.PreparePool(projectile, 100, new Vector3(1000, 1000, 1000));
    }

    /// <summary>
    /// Update is a method that gets called every frame. It is used for reading the user input and moving / rotating the ship
    /// </summary>
    void Update()
    {
        if (Input.GetButtonDown("Close"))
        {
            Application.Quit();
        }

        if (active)
        {
            TurnShip();
            MoveShip();

            if (Input.GetButtonDown("Fire1"))
            {
                ShootProjectile();
            }

            if (Input.GetButtonDown("StartMenu"))
            {
                EventManager.TriggerEvent(EEventType.GAME_END, null);
            }
        }
    }

    #endregion

    #region Methods

      ////////////////////////////////////////////////////////////////////
     /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method that toggles the user controls
    /// </summary>
    /// <param name="message">Message from the EventManager class</param>
    void ToggleShip(Dictionary<string, object> message)
    {
        active = !active;
    }

    /// <summary>
    /// Method that spawns the ship at its spawn point and resets the rotation
    /// </summary>
    /// <param name="message">Message from the EventManager class</param>
    void ResetPosition(Dictionary<string, object> message)
    {
        transform.position = spawnPoint.position;
        transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// Method that reads the user input and moves the ship depending on the value
    /// </summary>
    private void MoveShip()
    {
        var verticalInput = Input.GetAxis("Vertical");

        if (verticalInput > 0)
            transform.position += transform.forward * Time.deltaTime * verticalInput * forwardMoveSpeed;
    }

    /// <summary>
    /// Method for rotating the ship on three different axes
    /// </summary>
    private void TurnShip()
    {
        var yawInput = Input.GetAxis("Horizontal");
        var pitchInput = Input.GetAxis("Pitch");
        var rollInput = Input.GetAxis("Roll");

        float yaw = turnSpeed * Time.deltaTime * yawInput;
        float pitch = turnSpeed * Time.deltaTime * pitchInput;
        float roll = turnSpeed * Time.deltaTime * rollInput;

        transform.Rotate(-pitch, yaw, -roll);
    }

    /// <summary>
    /// Method that determines the direction the two projectiles are flying towards and instantiates the projectiles
    /// </summary>
    void ShootProjectile()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            destination = hit.point;
        }
        else
        {
            destination = ray.GetPoint(1000);
        }

        InstantiateProjectile(leftFirePoint);
        InstantiateProjectile(rightFirePoint);
    }

    /// <summary>
    /// Spawns a projectile at the specified point and sets its velocity
    /// </summary>
    /// <param name="firePoint">Spawn point of the projectile</param>
    private void InstantiateProjectile(Transform firePoint)
    {
        var projectileObj = PoolManager.SpawnObject(projectile, firePoint.position, Quaternion.identity);
        var rbody = projectileObj.AddComponent<Rigidbody>();
        rbody.useGravity = false;
        rbody.velocity = (destination - firePoint.position).normalized * projectileSpeed;
    }

    #endregion
}
