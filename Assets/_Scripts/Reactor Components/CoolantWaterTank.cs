using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoolantWaterTank : MonoBehaviour
{
    public CoolantWater[] water;

    public bool enablePumps { get; private set; } = true;
    public float minTemperature { get; private set; }
    public float maxTemperature { get; private set; }
    public float averageTemperature { get; private set; }
    public float currentWaterLevel { get; private set; }
    public float maxWaterLevel { get; private set; }

    #region MeasurementMethods

    public void InsertWater(CoolantWater[] _water)
    {
        // Update the water array value
        water = _water;

        // Calculate the max water level and the current water level
        maxWaterLevel = _water.Length;
    }

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

    public void EnablePumps()
    {
        // Set the enable pumps flag
        enablePumps = true;
    }

    public void DisablePumps()
    {
        // Set the enable pumps flag
        enablePumps = false;
    }

    #endregion
}
