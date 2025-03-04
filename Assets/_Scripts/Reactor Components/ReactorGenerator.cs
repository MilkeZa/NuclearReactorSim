using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles the generation of a fuel assembly.
/// </summary>
public class ReactorGenerator : MonoBehaviour
{
    #region Variables

    // These are gameobjects that need to be assigned before running
    [SerializeField]
    [Tooltip("Prefab to be used as the fuel rod object")]
    private GameObject fuelRodPrefab;

    [SerializeField]
    private GameObject waterPrefab;

    [SerializeField]
    private GameObject waterTankParent;
    private CoolantWaterTank waterTank;


    [SerializeField]
    [Tooltip("Parent object to put fuel rods in")]
    private FuelAssembly fuelAssembly;

    // These settings control the number of fuel rods
    [SerializeField]
    [Tooltip("Number of rows of fuel rods")]
    private int fuelRows = 20;

    [SerializeField]
    [Tooltip("Number of columns of fuel rods")]
    private int fuelColumns = 10;

    // These settings control where on screen fuel rods are placed
    [SerializeField]
    [Tooltip("Minimum position on screen a fuel rod will be placed")]
    private Vector2 minimumPosition;

    [SerializeField]
    [Tooltip("Maximum position on screen a fuel rod will be placed")]
    private Vector2 maximumPosition;

    #endregion

    #region UnityMethods

    private void Awake()
    {
        // Load the saved fuel assembly settings
        ResetFuelAssemblySettings();

        // Get the coolant water tank component
        waterTank = waterTankParent.GetComponent<CoolantWaterTank>();
    }

    private void Start()
    {
        // Clear any existing fuel assembly and generate a new one
        ClearFuelAssembly();
        ClearCoolantWater();

        GenerateFuelAssembly();
        GenerateCoolantWater();
    }

    #endregion

    #region FuelMethods

    public void SaveFuelAssemblySettings()
    {
        // Pass the current values to the json handler to write them to persistent storage
        SaveJsonData.WriteFuelAssemblySaveData(fuelRows, fuelColumns, minimumPosition, maximumPosition);
    }

    public void ResetFuelAssemblySettings()
    {
        // Load the values saved in the persistent data file
        FuelAssemblySaveData _defaultFuelAssemblyData = SaveJsonData.ReadFuelAssemblySaveData();

        // Copy saved values over to current values
        fuelRows = _defaultFuelAssemblyData.fuelRows;
        fuelColumns = _defaultFuelAssemblyData.fuelColumns;
        minimumPosition = _defaultFuelAssemblyData.minimumPosition;
        maximumPosition = _defaultFuelAssemblyData.maximumPosition;
    }

    public void GenerateFuelAssembly()
    {
        // Verify that no fuel rods currently exist
        if (fuelAssembly.fuelRods != null)
        {
            // Fuel rods currently exist, erase them
            fuelAssembly.DestroyFuelRods();
        }

        // Generate the fuel rods
        FuelRod[,] _fuelRods = GenerateFuelRods();

        // Insert the fuel rods into the fuel assembly
        fuelAssembly.InsertFuelRods(_fuelRods);

        // Generate the coolant water
        GenerateCoolantWater();
    }

    private FuelRod[,] GenerateFuelRods()
    {
        // Generate the fuel rod gameobjects
        FuelRod[,] _fuelRods = new FuelRod[fuelRows, fuelColumns];

        // Iterate through each row in the array
        for (int x = 0; x < _fuelRods.GetLength(0); x++)
        {
            // Iterate through each column in the array
            for (int y = 0; y < _fuelRods.GetLength(1); y++)
            {
                // Instantiate a fuel rod object using the position offsets
                GameObject _fuelRodObject = Instantiate(fuelRodPrefab);

                // Set the gameobjects parent transform to the fuel assembly transform
                _fuelRodObject.transform.parent = fuelAssembly.transform;

                // Get the fuel rod component
                FuelRod _fuelRod = _fuelRodObject.GetComponent<FuelRod>();

                // Insert the fuel rod into the array
                _fuelRods[x, y] = _fuelRod;
            }
        }

        // Set the fuel rods positions
        SetFuelRodPositions(_fuelRods);

        Debug.Log($"Generated {_fuelRods.Length} fuel rods");

        // Return the fuel rod array
        return _fuelRods;
    }

    private void SetFuelRodPositions(FuelRod[,] _fuelRods)
    {
        /* Position Notes:
         *  - Most bottom left fuel rod is at index 0
         *  - index increases moving towards the right until the right most element is reached
         *  - next index is one row above all the way to the left
         *  
         * Example of an assembly with nine (9) elements:
         * 
         *          Column 0:   Column 1:   Column 2:
         *  Row 2:      6           7           8
         *  Row 1:      3           4           5
         *  Row 0:      0           1           2
         */

        // Get the number of rows and columns
        int _rowCount = _fuelRods.GetLength(0);
        int _columnCount = _fuelRods.GetLength(1);

        // Iterate through each row
        for (int x = 0; x < _rowCount; x++)
        {
            // Calculate this columns x position here to avoid redoing the same calculation for each element in the column
            float _progressX = x / (_rowCount - 1f);
            float _posX = Mathf.Lerp(minimumPosition.x, maximumPosition.x, _progressX);
            
            // Iterate through each column
            for (int y = 0; y < _columnCount; y++)
            {
                // Calculate the y value
                float _progressY = y / (_columnCount - 1f);
                float _posY = Mathf.Lerp(minimumPosition.y, maximumPosition.y, _progressY);

                // Create a new v3 using the x and y position values
                Vector3 _pos = new Vector3(_posX, _posY, 0f);

                // Set the fuel rod game objects transforms position
                _fuelRods[x, y].transform.position = _pos;
            }
        }
    }

    public void ClearFuelAssembly()
    {
        // Instruct the fuel assembly to destroy all fuel rods
        fuelAssembly.DestroyFuelRods();

        ClearCoolantWater();
    }

    #endregion

    #region CoolantWaterMethods

    private void GenerateCoolantWater()
    {
        CoolantWater[] _waterTank = new CoolantWater[fuelAssembly.rowCount * fuelAssembly.columnCount];

        // Verify the fuel assembly has fuel in it
        if (fuelAssembly != null && fuelAssembly.fuelRodCount > 0)
        {
            int _counter = 0;
            foreach (FuelRod _rod in fuelAssembly.FlattenedFuelRods())
            {
                // Get the position of the rod
                Vector3 _pos = _rod.transform.position;

                // Instantiate a new water prefab at this location
                GameObject _water = Instantiate(waterPrefab, _pos, Quaternion.identity);

                // Set the water objects parent transform
                _water.gameObject.transform.parent = waterTankParent.transform;

                // Insert the coolant water component into the array
                _waterTank[_counter] = _water.GetComponent<CoolantWater>();

                // Increment the counter variable
                _counter++;
            }
        }

        // Assign the water tanks water
        waterTank.InsertWater(_waterTank);
    }

    private void ClearCoolantWater()
    {
        foreach (CoolantWater _obj in FindObjectsOfType<CoolantWater>())
        {
            // Detroy their gameobjects
            DestroyImmediate(_obj.gameObject);
        }

        // Clear the water tanks water
        waterTank.ClearWater();
    }

    #endregion
}
