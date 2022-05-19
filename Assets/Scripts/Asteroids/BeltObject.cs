using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

/// <summary>
/// A single object inside an asteroid belt.
/// </summary>
public class BeltObject : MonoBehaviour
{
    #region Variables

      ////////////////////////////////////////////////////////////////////
     /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Speed of the asteroid while rotating around the asteroid belt center
    /// </summary>
    [SerializeField]
    private float _orbitSpeed;
    
    /// <summary>
    /// Transform object of the parent. The parent of the belt object is not set in transform.parent, because it would force the execution of the BeltObjectMovementJob on the main thread
    /// </summary>
    [SerializeField]
    private Transform _parentTransform;
    
    /// <summary>
    /// Flag that determines, whether the belt object rotates clockwise or counter-clockwise around the asteroid belt center
    /// </summary>
    [SerializeField]
    private bool _rotationDirection;

    #endregion

    #region Methods

      ////////////////////////////////////////////////////////////////////
     /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// "Constructor" method of the belt object. For setting up the belt object after the GetComponent method.
    /// </summary>
    /// <param name="speed">The speed of the belt object for rotating around the parent (and its axis)</param>
    /// <param name="parent">The transform of the parent of the belt object, namely the asteroind belt itself</param>
    /// <param name="rotateClockwise">Flag that determines whether the rotation around the parent is clockwise or counter clockwise</param>
    public void InitAsteroidBeltObject(float speed, GameObject parent, bool rotateClockwise)
    {
        this._orbitSpeed = speed;
        this._parentTransform = parent.transform;
        this._rotationDirection = rotateClockwise;
    }

    #endregion

    #region Structs

    /// <summary>
    /// Struct of essential data for the belt object. Used in jobs.
    /// </summary>
    public struct Data
    {
        #region Struct Variables

        /// <summary>
        /// Orbit speed of the belt object
        /// </summary>
        private float _orbitSpeed;

        /// <summary>
        /// Orbit direction of the belt object. True is clockwise, false is counter-clockwise
        /// </summary>
        private bool rotationDirection;

        /// <summary>
        /// Position of the asteroid belt center
        /// </summary>
        private Vector3 _parentPosition;

        /// <summary>
        /// Up vector of the asteroid belt center
        /// </summary>
        private Vector3 _parentUp;

        #endregion

        #region Struct constructors

        /// <summary>
        /// Constructor for the BeltObject.Data struct
        /// </summary>
        /// <param name="beltObject">The BeltObject from which the data is taken</param>
        public Data(BeltObject beltObject)
        {
            _orbitSpeed = beltObject._orbitSpeed;
            rotationDirection = beltObject._rotationDirection;
            _parentPosition = beltObject._parentTransform.position;
            _parentUp = beltObject._parentTransform.up;
        }

        #endregion

        #region Struct methods

        /// <summary>
        /// Updates the position and rotation of the _transform parameter
        /// </summary>
        /// <param name="_transform">Transform struct of a belt object</param>
        /// <param name="deltaTime">Current Time.deltaTime value passed from the caller</param>
        public void UpdateTransformAccess(TransformAccess _transform, float deltaTime)
        {
            // Clockwise rotation around the center of the asteroid belt
            if (rotationDirection)
            {
                _transform.position = RotateAround(_transform.position, _parentPosition, _parentUp, _orbitSpeed * deltaTime * 0.15f);
            }
            // Counter-clockwise rotation around the center of the asteroid belt
            else
            {
                _transform.position = RotateAround(_transform.position, _parentPosition, -_parentUp, _orbitSpeed * deltaTime * 0.15f);
            }
        }

        /// <summary>
        /// Method for rotating a belt object around the asteroid belt center
        /// </summary>
        /// <param name="pos">Position of the belt object</param>
        /// <param name="pivot">Position which the belt object should rotate around</param>
        /// <param name="axis">Axis around which the belt object rotates (up vector)</param>
        /// <param name="delta">Speed of the belt object</param>
        /// <returns>New position of belt object</returns>
        Vector3 RotateAround(Vector3 pos, Vector3 pivot, Vector3 axis, float delta)
        {
            var newPositionCoords = math.mul(Quaternion.AngleAxis(delta, axis), pos - pivot) + new float3(pivot.x, pivot.y, pivot.z);

            return new Vector3(newPositionCoords.x, newPositionCoords.y, newPositionCoords.z);
        }

        #endregion
    }

    #endregion
}