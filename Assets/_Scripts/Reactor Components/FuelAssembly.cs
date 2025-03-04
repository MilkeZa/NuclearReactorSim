using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class holds the fuel rods used in the reactor.
/// </summary>
public class FuelAssembly : MonoBehaviour
{
    public int rowCount { get; private set; } = 0;                  // Number of rows of fuel rods in the assembly
    public int columnCount { get; private set; } = 0;               // Number of columns of fuel rods in the assembly
    public int fuelRodCount { get; private set; } = 0;              // Number of fuel rods in the assembly
    public bool isEmpty { get; private set; } = false;                  // Indicates whether the assembly has fuel or not

    public FuelRod[,] fuelRods { get; private set; }            // Individual fuel rods making up the assembly

    public Action<int> OnFuelRodsInserted;  // Called when the fuel rods are inserted
    public Action OnFuelRodsDestroyed;      // Called when the fuel rods are destroyed

    public void InsertFuelRods(FuelRod[,] _fuelRods)
    {
        // Set the fuel rod variable
        fuelRods = _fuelRods;

        // Set the row and column count variables
        rowCount = fuelRods.GetLength(0);
        columnCount = fuelRods.GetLength(1);

        // Calculate the number of fuel rods that will make up the assembly
        fuelRodCount = rowCount * columnCount;

        // Set the isEmpty flag
        isEmpty = false;

        // Invoke the OnFuelRodsInserted action
        OnFuelRodsInserted?.Invoke(fuelRodCount);
    }

    public void DestroyFuelRods()
    {
        // List of gameobjects to destroy
        List<GameObject> _objectsToDestroy = new List<GameObject>();

        // Check if this transform has children as it is the only place where fuel rods should exist
        if (transform.childCount > 0)
        {
            // Iterate through each child object
            for (int i = 0; i < transform.childCount; i++)
            {
                // Grab the current childs gameobject
                GameObject _child = transform.GetChild(i).gameObject;

                // Check if the child object is tagged as a fuel rod
                if (_child.CompareTag("Fuel Rod"))
                {
                    // Fuel rod found, append the gameobject to the list to destroy
                    _objectsToDestroy.Add(transform.GetChild(i).gameObject);
                }
            }
        }

        // Check if the function is being called in edit or run mode
        if (Application.IsPlaying(this))
        {
            // Application is running, use Destroy()
            foreach (GameObject _object in _objectsToDestroy)
            {
                Destroy(_object);
            }
        }
        else
        {
            // Application is not running, use DestroyImmediate()
            foreach (GameObject _object in _objectsToDestroy)
            {
                DestroyImmediate(_object);
            }
        }


        // Reset the row, column, total fuel rod counts, and the isEmpty flag
        rowCount = 0;
        columnCount = 0;
        fuelRodCount = 0;
        isEmpty = true;

        // Invoke the OnFuelRodsDestroyed action
        OnFuelRodsDestroyed?.Invoke();
    }

    /// <summary>
    /// Returns a 1 dimensional array filled with the fuel rod components.
    /// </summary>
    /// <returns></returns>
    public FuelRod[] FlattenedFuelRods()
    {
        // Create an array to hold the fuel rod elements
        FuelRod[] _fuelRods = new FuelRod[rowCount * columnCount];

        // Iterate through each row
        for (int x = 0, i = 0; x < rowCount; x++)
        {
            // Iterate through each column
            for (int y = 0; y < columnCount; y++, i++)
            {
                // Insert the fuel rod
                _fuelRods[i] = fuelRods[x, y];
            }
        }

        // Return the fuel rod array
        return _fuelRods;
    }
}
