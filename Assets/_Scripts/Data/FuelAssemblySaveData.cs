using UnityEngine;

/// <summary>
/// This class holds the structure of fuel assembly save data.
/// </summary>
[System.Serializable]
public class FuelAssemblySaveData
{
    #region Variables

    public int fuelRows;            // Number of rows in the fuel assembly
    public int fuelColumns;         // Number of columns in the fuel assembly
    public Vector2 minimumPosition; // Minimum position in 2D space within the fuel assembly (bottom-left).
    public Vector2 maximumPosition; // Maximum position in 2D space within the fuel assembly (top-right).

    #endregion

    #region CustomMethods

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

    #endregion
}
