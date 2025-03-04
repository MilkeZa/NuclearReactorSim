using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This class holds the structure of fuel assembly save data.
/// </summary>
[System.Serializable]
public class FuelAssemblySaveData
{
    public int fuelRows;
    public int fuelColumns;
    public Vector2 minimumPosition;
    public Vector2 maximumPosition;

    public FuelAssemblySaveData(int _fuelRows, int _fuelColumns, Vector2 _minimumPosition, Vector2 _maximumPosition)
    {
        fuelRows = _fuelRows;
        fuelColumns = _fuelColumns;
        minimumPosition = _minimumPosition;
        maximumPosition = _maximumPosition;
    }

    public override string ToString()
    {
        return $"Fuel Assembly Save Data\nFuel Rows: {fuelRows}\nFuel Columns: {fuelColumns}\nMinimum Position: {minimumPosition}\nMaximum Position: {maximumPosition}";
    }
}
