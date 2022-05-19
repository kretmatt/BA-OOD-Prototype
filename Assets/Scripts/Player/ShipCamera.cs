using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script that ensures that the camera follows the player
/// </summary>
public class ShipCamera : MonoBehaviour
{
    #region Variables

      ////////////////////////////////////////////////////////////////////
     /////////////////////////      Variables      //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Position and rotation of the player character
    /// </summary>
    [SerializeField] Transform shipTarget;
    
    /// <summary>
    /// Default distance to the player character
    /// </summary>
    [SerializeField] Vector3 defaultDistance = new Vector3(0f, 4f, -10f);
    
    /// <summary>
    /// Value that ensures that the camera doesn't snap to the new player position
    /// </summary>
    [SerializeField] float distanceDamp = 10f;
    
    /// <summary>
    /// Value that ensures that the rotation of the camera is not to snappy
    /// </summary>
    [SerializeField] float rotationDamp = 10f;

    #endregion

    #region Unity Messages

      ////////////////////////////////////////////////////////////////////
     /////////////////////////    Unity Messages   //////////////////////
    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Method that gets called every frame after the Update method. Ensures that the camera smoothly follows the player
    /// </summary>
    private void LateUpdate()
    {
        Vector3 newPosition = shipTarget.position + (shipTarget.rotation * defaultDistance);
        Vector3 currentPosition = Vector3.Lerp(transform.position, newPosition, distanceDamp * Time.deltaTime);
        
        transform.position = currentPosition;

        Quaternion newRotation = Quaternion.LookRotation(shipTarget.position - transform.position, shipTarget.rotation * Vector3.up);
        Quaternion currentRotation = Quaternion.Slerp(transform.rotation, newRotation, rotationDamp * Time.deltaTime);

        transform.rotation = currentRotation;
    }

    #endregion
}
