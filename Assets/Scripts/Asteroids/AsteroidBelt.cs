using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

/// <summary>
/// The AsteroidBelt class is a component for creating an asteroid belt inside the scene. Asteroids are randomly spawned between an inner and outer radius.
/// The movement of the individual asteroids, the spawning and despawning processes are all managed by the AsteroidBelt class.
/// </summary>
public class AsteroidBelt : MonoBehaviour
{
    #region Variables

      ////////////////////////////////////////////////////////////////////
     /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Array of all asteroid prefabs to be used in the asteroid belt
    /// </summary>
    [SerializeField]
    GameObject[] asteroidPrefabs;

    /// <summary>
    /// Amount of asteroids in the asteroid belt
    /// </summary>
    [SerializeField]
    int numberOfAsteroids;

    /// <summary>
    /// Seed of the asteroid belt
    /// </summary>
    [SerializeField]
    int asteroidBeltSeed;

    /// <summary>
    /// Inner radius of the asteroid belt
    /// </summary>
    [SerializeField]
    float beltInnerRadius;

    /// <summary>
    /// Outer radius of the asteroid belt
    /// </summary>
    [SerializeField]
    float beltOuterRadius;

    /// <summary>
    /// Height of the asteroid belt
    /// </summary>
    [SerializeField]
    float beltHeight;

    /// <summary>
    /// Speed of the belt objects
    /// </summary>
    [SerializeField]
    float beltObjectOrbitSpeed;

    /// <summary>
    /// Flag that determines whether the asteroids move clockwise (true) or counter-clockwise (false)
    /// </summary>
    [SerializeField]
    bool beltRotationDirection;

    /// <summary>
    /// List of all objects managed by the asteroid belt
    /// </summary>
    List<BeltObject> beltObjects = new List<BeltObject>();

    /// <summary>
    /// Job handle for the movement of the asteroids
    /// </summary>
    private JobHandle? beltObjectsMoveJob;
    
    /// <summary>
    /// Asteroid data used for rotating the asteroid around the center of the asteroid belt
    /// </summary>
    NativeArray<BeltObject.Data> bobjects;
    
    /// <summary>
    /// Transforms of the objects managed by the asteroid belt
    /// </summary>
    TransformAccessArray transformsA;

    #endregion

    #region Unity Messages

      ////////////////////////////////////////////////////////////////////
     /////////////////////////    Unity Messages   //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Gets called once in the lifetime of this script, when the script is first enabled
    /// Prepares an object pool, from which asteroids can be taken without needing to be instantiated during the execution of the game. The asteroids get created in the beginning.
    /// </summary>
    private void Awake()
    {
        for (int i = 0; i < asteroidPrefabs.Length; i++)
        {
            PoolManager.PreparePool(asteroidPrefabs[i], 500, Vector3.zero);
        }
    }

    /// <summary>
    /// Gets called when the object becomes active in the hierarchy.
    /// Makes the following subscriptions:
    /// 1. GAME_START event:
    ///    PlaceInitialAsteroids - Places the asteroids once the games is started. If the object pool does not have enough asteroids of one type, it automatically creates more.
    /// 
    /// 2. GAME_END event:
    ///    DespawnAsteroids - Removes the asteroids from the scene once the game ends (timer ends or the player has no health left)
    /// 
    /// 3. SET_ASTEROIDS event
    ///    SetDensity - Sets the amount of asteroids that will be spawned once the game starts.
    /// </summary>
    private void OnEnable()
    {
        EventManager.SubscribeMethodToEvent(EEventType.GAME_START, PlaceInitialAsteroids);
        EventManager.SubscribeMethodToEvent(EEventType.GAME_END, DespawnAsteroids);
        EventManager.SubscribeMethodToEvent(EEventType.SET_ASTEROIDS, SetDensity);
    }

    /// <summary>
    /// Gets called when the object becomes inactive / gets disabled in the hierarchy.
    /// Resets the following subscriptions:
    /// 1. GAME_START event:
    ///    PlaceInitialAsteroids
    ///    
    /// 2. GAME_END event:
    ///    DespawnAsteroids
    ///    
    /// 3. SET_ASTEROIDS event
    ///    SetDensity
    /// </summary>
    private void OnDisable()
    {
        EventManager.UnsubscribeMethodFromEvent(EEventType.GAME_START, PlaceInitialAsteroids);
        EventManager.UnsubscribeMethodFromEvent(EEventType.GAME_END, DespawnAsteroids);
        EventManager.UnsubscribeMethodFromEvent(EEventType.SET_ASTEROIDS, SetDensity);
    }

    /// <summary>
    /// Gets called when the game object, this script is attached to, gets destroyed.
    /// In this case, two native arrays are disposed of to prevent memory leaks and to release memory.
    /// </summary>
    void OnDestroy()
    {
        if (transformsA.isCreated)
            transformsA.Dispose();
        if (bobjects != null && bobjects.IsCreated)
            bobjects.Dispose();
    }

    /// <summary>
    /// Gets called every frame, if the script is enabled
    /// In this case, the positions and the rotations of the asteroid get updated to make the asteroids rotate around the center of the asteroid belt.
    /// </summary>
    void Update()
    {
        if (bobjects != null && transformsA.isCreated)
        {
            var asteroidBeltJob = new BeltObjectMovementJob
            {
                dataArray = bobjects,
                deltaTime = Time.deltaTime,
            };

            beltObjectsMoveJob = asteroidBeltJob.Schedule(transformsA);
        }
    }

    /// <summary>
    /// Gets called once all Update functions have been called.
    /// In this case, the script ensures that all asteroids have been moved before moving to the next frame.
    /// </summary>
    private void LateUpdate()
    {
        beltObjectsMoveJob?.Complete();
    }

    #endregion

    #region Methods

      ////////////////////////////////////////////////////////////////////
     /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method for setting the density of the asteroid belt (amount of asteroids)
    /// </summary>
    /// <param name="message">Message from the event manager, containing the new amount of asteroids</param>
    void SetDensity(Dictionary<string, object> message)
    {
        if (message.ContainsKey("amount"))
        {
            numberOfAsteroids = (int)message["amount"];
        }
    }

    /// <summary>
    /// Method for despawning the asteroids once the game ends
    /// </summary>
    /// <param name="message">Message from the event manager</param>
    void DespawnAsteroids(Dictionary<string, object> message)
    {
        beltObjectsMoveJob?.Complete();

        if (transformsA.isCreated)
            transformsA.Dispose();
        if (bobjects != null && bobjects.IsCreated)
            bobjects.Dispose();

        foreach (BeltObject bo in beltObjects)
        {
            Destroy(bo.GetComponent<Rigidbody>());
            PoolManager.ReleaseObject(bo.gameObject);
        }

        beltObjects.Clear();
    }

    /// <summary>
    /// Method for spawning in the asteroids once the game starts
    /// </summary>
    /// <param name="message">Message from the event manager</param>
    void PlaceInitialAsteroids(Dictionary<string, object> message)
    {
        float distanceToBeltCenter, angle, x, y, z;
        // Set the state of Random so the performance comparison is fair (ensure that the comparisons have the same circumstances)
        Random.InitState(asteroidBeltSeed);
        Transform[] transforms = new Transform[numberOfAsteroids];

        // Spawn density amount belt objects for the belt
        for (int i = 0; i < numberOfAsteroids; i++)
        {
            // Retrieve a random angle and radius / distance value (Angle is in radians because Mathf
            // only takes radians values)
            angle = Random.Range(0, (2 * Mathf.PI));
            distanceToBeltCenter = Random.Range(beltInnerRadius, beltOuterRadius);

            // Calculate the x, y and z coordinates. X and Z are calculated with the unit circle
            // and multiplied with the distance to the asteroid belt center. Y is the height of the asteroid
            y = Random.Range(-(beltHeight / 2), (beltHeight / 2));
            x = distanceToBeltCenter * Mathf.Cos(angle);
            z = distanceToBeltCenter * Mathf.Sin(angle);

            // Select an asteroid, generate a random rotation and retrieve an instance of the selected asteroid from the Object Pool
            int asteroidPos = Random.Range(0, asteroidPrefabs.Length);
            var chosenAsteroid = asteroidPrefabs[asteroidPos];
            var randomRotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
            GameObject asteroid = PoolManager.SpawnObject(chosenAsteroid, transform.position + transform.rotation * new Vector3(x,y,z), randomRotation);

            // Initialize the asteroid and add a RigidBody component to it
            asteroid.GetComponent<BeltObject>().InitAsteroidBeltObject(beltObjectOrbitSpeed, gameObject, beltRotationDirection);
            var rbody = asteroid.AddComponent<Rigidbody>();
            rbody.useGravity = false;
            rbody.mass = asteroidPos*12+12;

            beltObjects.Add(asteroid.GetComponent<BeltObject>());
            transforms[i] = asteroid.transform;
        }

        // Make a new NativeArray with BeltObject.Data. Make it persistent so it can be used in several jobs without needing to be recreated
        bobjects = new NativeArray<BeltObject.Data>(beltObjects.Count, Allocator.Persistent);

        for (var i = 0; i < beltObjects.Count; i++)
        {
            var bobjectdata = new BeltObject.Data(beltObjects[i]);
            bobjects[i] = bobjectdata;
        }

        // Make a TransformAccessArray for the move job, because transforms will be used.
        transformsA = new TransformAccessArray(transforms, JobsUtility.JobWorkerMaximumCount);
    }

    #endregion
}
