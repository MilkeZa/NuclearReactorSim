using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RodMovementController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Speed at which control rods move")]
    private float speed = 0.5f;                   // A value of 4 is used to speed up testing, 0.25 or 0.5 is likely a more realistic speed

    [SerializeField]
    [Tooltip("Maximum distance control rods may be raised")]
    private float maxRaiseDistance = 8f;        // Maximum offset from the original height rods may travel

    private float originHeight;                 // Height (y-value)
    private float maxOffsetFromOriginHeight;    

    private bool moveControlRod = false;        // No movement when false, movement when true
    private int raiseDirection = -1;            // Lower when -1, raise when 1

    #region UnityMethods

    private void Awake()
    {
        // Minimum position is where the object begins
        originHeight = transform.position.y;

        // Maximum position is the minimum position plus the max raise distance along the y-axis
        maxOffsetFromOriginHeight = originHeight + maxRaiseDistance;
    }

    private void Update()
    {
        // Check if the control is allowed to and should be moving
        //if (enableMovement && moveControlRod)
        if (moveControlRod)
        {
            // Calculate the new position offset from user input
            float _heightOffset = raiseDirection * speed * Time.fixedDeltaTime;

            // Add the height offset to the current positions y value (height)
            float _offsetFromOrigin = _heightOffset + transform.position.y;

            // Clamp the value between the minimum (lowest) and maximum (highest) position values
            float _finalOffset = Mathf.Clamp(_offsetFromOrigin, originHeight, maxOffsetFromOriginHeight);

            // Create a new V3 with the final offset value in the y position
            Vector3 _position = new Vector3(transform.position.x, _finalOffset, transform.position.z);

            // Apply this position to the transform
            transform.position = _position;
        }

        //// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
        ////  User input for testing
        ////
        ////        Uncomment this section to manually control the rod

        //// Check for the down press of the the W/S or Up/Down arrow keys
        //bool _startUp = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
        //bool _startDown = Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);

        //// Start the movement in the given direction, prioritising the lowering movement over raising
        //if (_startDown)
        //{
        //    // Lower control rod
        //    BeginControlRodMovement(false);
        //    Debug.Log("Begin lowering control rod");
        //}
        //else if (_startUp)
        //{
        //    // Raise the control rod
        //    BeginControlRodMovement(true);
        //    Debug.Log("Begin raising control rod");
        //}

        //// Check for the release of the W/S or Up/Down arrow keys
        //bool _stopUp = Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W);
        //bool _stopDown = Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S);

        //// Stop the movement

        //// Prevent the release of the W/Up arrow keys from stopping down movement
        //if (moveControlRod && raiseDirection == -1 && _stopDown)
        //{
        //    // Stop lowering the control rod
        //    StopControlRodMovement();
        //    Debug.Log("Stop lowering control rod");
        //}

        //// Prevent the release of the S/Down arrow keys from stopping the control rod from raising
        //if (moveControlRod && raiseDirection == 1 && _stopUp)
        //{
        //    // Stop raising the control rod
        //    StopControlRodMovement();
        //    Debug.Log("Stop raiding control rod");
        //}
        //// ----------------------------------------------------------------------------------------------------------------------------------------------------------------
    }

    #endregion

    #region MovementMethods

    public void BeginControlRodMovement(bool _raiseControlRod)
    {
        // Set the move control rod flag high
        moveControlRod = true;

        // Set the movement direction flag
        raiseDirection = _raiseControlRod ? 1 : -1;
    }

    public void StopControlRodMovement()
    {
        // Set the move control rod flag low
        moveControlRod = false;

        // Reset the isRaising flag to false (this seems like it would be a good idea in a real reactor as default would always be to lower control rods, slowing reactions down - except in those old RBMK reactors, if you catch my drift)
        raiseDirection = -1;
    }

    #endregion
}
