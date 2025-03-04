using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class RodArrayController : MonoBehaviour
{
    [SerializeField]
    private GameObject rodContainer;

    private List<RodMovementController> controlRods = new List<RodMovementController>();
    private List<RodMovementController> moderatorRods = new List<RodMovementController>();

    public bool enableControlRods = false;
    public bool enableModeratorRods = false;

    public bool controlRodMovementState { get; private set; }       // Are control rods moving (true) or idle (false)?
    public bool moderatorRodMovementState { get; private set; }     // Are moderator rods moving (true) or idle (false)?

    public bool controlRodMovementDirection { get; private set; }   // Are control rods rising (true) or lowering (false)?
    public bool moderatorRodMovementDirection { get; private set; } // Are control rods rising (true) or lowering (false)?

    #region UnityMethods

    private void Start()
    {
        // Find all enabled control and moderator rod components
        for (int i = 0; i < rodContainer.gameObject.transform.childCount; i++)
        {
            // Get the child object
            GameObject _child = rodContainer.gameObject.transform.GetChild(i).gameObject;

            // Verify the child is active in the scene
            if (_child.gameObject.activeInHierarchy)
            {
                // Child is active, get the rod movement components in its children
                RodMovementController[] _movementControllers = _child.transform.GetComponentsInChildren<RodMovementController>(false);
    
                // Iterate through each movement controller
                foreach (RodMovementController _controller in _movementControllers)
                {
                    // Insert the rod controller into the respective list based on which type of rod it is on
                    if (_controller.gameObject.CompareTag("Control Rod"))
                    {
                        // Insert into the control rod list
                        controlRods.Add(_controller);
                    }
                    else if (_controller.gameObject.CompareTag("Moderator Rod"))
                    {
                        // Insert into the moderator rod list
                        moderatorRods.Add(_controller);
                    }
                }
            }
        }

        // Disable movement of either type of rod if there aren't any present
        if (controlRods.Count == 0)
        {
            // No control rods present, disable their movement
            enableControlRods = false;
        }

        if (moderatorRods.Count == 0)
        {
            // No moderator rods present, disable their movement
            enableModeratorRods = false;
        }
    }

    #endregion

    #region RodMovementMethods

    public void RaiseControlRods()
    {
        // Verify that control rod movement is enabled
        if (enableControlRods)
        {
            // Begin raising the control rods
            BeginMoveRods(controlRods, true);

            // Update the control rod movement state and direction values 
            controlRodMovementState = true;
            controlRodMovementDirection = true;
        }
    }

    public void LowerControlRods()
    {
        // Verify that control rod movement is enabled
        if (enableControlRods)
        {
            // Begin lowering the control rods
            BeginMoveRods(controlRods, false);

            // Update the control rod movement state and direction values
            controlRodMovementState = true;
            controlRodMovementDirection = false;
        }
    }

    public void RaiseModeratorRods()
    {
        // Verify that moderator rod movement is enabled
        if (enableModeratorRods)
        {
            // Begin raising the control rods
            BeginMoveRods(moderatorRods, true);
        }

        // Update the moderator rod movement state and direction values
        moderatorRodMovementState = true;
        moderatorRodMovementDirection = true;
    }

    public void LowerModeratorRods()
    {
        // Verify that moderator rod movement is enabled
        if (enableModeratorRods)
        {
            // Begin lowering the moderator rods
            BeginMoveRods(moderatorRods, false);
        }

        // Update the moderator rod movement state and direction values
        moderatorRodMovementState = true;
        moderatorRodMovementDirection = false;
    }

    private void BeginMoveRods(List<RodMovementController> _rods, bool _raise)
    {
        // Verify the array is not null or empty
        if (_rods != null && _rods.Count > 0)
        {
            // Iterate over each rod
            foreach (RodMovementController _rod in _rods)
            {
                _rod.BeginControlRodMovement(_raise);
            }
        }
    }

    public void HaltControlRods()
    {
        // Verify that control rod movement is enabled
        if (enableControlRods)
        {
            // Halt the control rods
            HaltRods(controlRods);

            // Reset the control rod movement state and direction
            controlRodMovementState = false;
            controlRodMovementDirection = false;
        }
    }

    public void HaltModeratorRods()
    {
        // Verify that moderator rod movement is enabled
        if (enableModeratorRods)
        {
            // Halt the moderator rods
            HaltRods(moderatorRods);
        }

        // Reset the moderator rod movement state and direction
        moderatorRodMovementState = false;
        moderatorRodMovementDirection = false;
    }

    public void HaltRods(List<RodMovementController> _rods)
    {
        // Verify the array is not null or empty
        if (_rods != null && _rods.Count > 0)
        {
            // Stop the movement of the control rods
            foreach (RodMovementController _rod in _rods)
            {
                _rod.StopControlRodMovement();
            }
        }
    }

    #endregion
}
