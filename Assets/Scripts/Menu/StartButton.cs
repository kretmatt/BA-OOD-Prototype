using UnityEngine;

/// <summary>
/// Simple script for starting the game
/// </summary>
public class StartButton : MonoBehaviour
{
      ////////////////////////////////////////////////////////////////////
     /////////////////////////        Methods      //////////////////////
    ////////////////////////////////////////////////////////////////////
    
    /// <summary>
    /// Method bound to a button. Once the button is clicked, the GAME_START event will be triggered.
    /// </summary>
    public void Click()
    {
        EventManager.TriggerEvent(EEventType.GAME_START, null);
    }
}
