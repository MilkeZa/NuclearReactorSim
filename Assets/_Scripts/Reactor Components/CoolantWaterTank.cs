using UnityEngine;

/// <summary>
/// This class controls the behavior of the totality of coolant water within the reactor core.
/// </summary>
public class CoolantWaterTank : MonoBehaviour
{
    #region Variables

    public CoolantWater[] water;    // Array of coolant water objects

    public bool enablePumps { get; private set; } = true;   // Enable/disable cooling across all water objects
    public float minTemperature { get; private set; }       // Minimum water temperature present within the reactor core
    public float maxTemperature { get; private set; }       // Maximum water temperature present within the reactor core
    public float averageTemperature { get; private set; }   // Average water temperature across all water within the reactor core
    public float currentWaterLevel { get; private set; }    // Current water level (non-evaporated) within the reactor core
    public float maxWaterLevel { get; private set; }        // Maximum water level (all non-evaporated) within the reactor core

    #endregion

    #region MeasurementMethods

    /// <summary>
    /// "Pump" the water into the tank.
    /// </summary>
    /// <param name="_water"></param>
    public void InsertWater(CoolantWater[] _water)
    {
        // Update the water array value
        water = _water;

        // Calculate the max water level and the current water level
        maxWaterLevel = _water.Length;
    }

    /// <summary>
    /// "Pump" the water out of the tank.
    /// </summary>
    public void ClearWater()
    {
        // Update the water array value and reset each attribute
        water = null;

        minTemperature = float.MaxValue;
        maxTemperature = float.MinValue;
        averageTemperature = 0f;
        currentWaterLevel = 0f;
        maxWaterLevel = 0f;
    }

    /// <summary>
    /// Update the water measurements.
    /// </summary>
    public void UpdateMeasurements()
    {
        // Check if water is present
        if (water == null)
        {
            // No water in the tank, set values to float.MinValue and exit
            minTemperature = maxTemperature = averageTemperature = float.MinValue;
            return;
        }

        float _sum = 0f;
        float _average;
        float _minTemp = float.MaxValue;
        float _maxTemp = float.MinValue;
        float _currentWaterLevel = 0f;

        // Iterate over each water object
        foreach (CoolantWater _water in water)
        {
            float _temp = _water.currentTemperature;

            // Add the waters temperature to the sum
            _sum += _temp;

            // Check if a new min or max temperature has been found
            if (_temp < _minTemp)
            {
                // New minimum temperature found
                _minTemp = _temp;
            }
            else if (_maxTemp < _temp)
            {
                // New maximum temperature found
                _maxTemp = _temp;
            }

            // Update the current water level
            _currentWaterLevel += _water.capacityRemaining;
        }

        // Calculate the average temperature of the water cells
        _average = _sum / water.Length;

        // Assign each of the values to their respective attributes
        minTemperature = _minTemp;
        maxTemperature = _maxTemp;
        averageTemperature = _average;
        currentWaterLevel = Mathf.Round(((_currentWaterLevel / maxWaterLevel) * 10f) / 10f) * 100f;
    }

    /// <summary>
    /// Enable the pumps, allowing for water to cool.
    /// </summary>
    public void EnablePumps()
    {
        // Set the enable pumps flag
        enablePumps = true;
    }

    /// <summary>
    /// Disable the pumps, not allowing water to cool.
    /// </summary>
    public void DisablePumps()
    {
        // Set the enable pumps flag
        enablePumps = false;
    }

    #endregion
}
