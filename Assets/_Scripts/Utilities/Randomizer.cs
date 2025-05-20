using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Randomizer
{
    #region Variables

    // Static random object to be used by the whole project using todays date in ticks as a seed
    private static System.Random Randy = new System.Random((int)DateTime.Now.Ticks);

    #endregion

    #region CustomMethods

    /// <summary>
    /// Randomize a list of items.
    /// </summary>
    /// <typeparam name="T">List of items whose items should be mixed up.</typeparam>
    /// <param name="_list">List whose elements indices have been randomized.</param>
    public static void RandomizeList<T>(List<T> _list)
    {
        // Algorithm taken from https://stackoverflow.com/questions/273313/randomize-a-listt and is based on the Fisher-Yates shuffle
        int _n = _list.Count;
        while (_n > 1)
        {
            _n--;
            int _k = Randomizer.Randy.Next(_n + 1);
            T _value = _list[_k];
            _list[_k] = _list[_n];
            _list[_n] = _value;
        }
    }

    /// <summary>
    /// Clamp a float between two values.
    /// </summary>
    /// <param name="_min">Minimum value allowed.</param>
    /// <param name="_max">Maximum value allowed.</param>
    /// <param name="_value">Value to clamp.</param>
    /// <returns></returns>
    public static float ClampFloat(float _min, float _max, float _value)
    {
        // Clamp the value between the min and max values
        float _clampedValue = _value < _min ? _min : _value;
        return _clampedValue > _max ? _max : _value;
    }

    /// <summary>
    /// Map a value from one range to another.
    /// </summary>
    /// <param name="_val">Value to be mapped to the new range.</param>
    /// <param name="_inMin">Minimum of the input range.</param>
    /// <param name="_inMax">Maximum of the input range.</param>
    /// <param name="_outMin">Minimum of the output range.</param>
    /// <param name="_outMax">Maximum of the output range.</param>
    /// <returns></returns>
    public static float MapRange(float _val, float _inMin, float _inMax, float _outMin, float _outMax)
    {
        // Map the value to the new range
        return _outMin + ((_val - _inMin) / (_inMax - _inMin) * (_outMax - _outMin));
    }

#nullable enable

    /// <summary>
    /// Generate a random integer.
    /// </summary>
    /// <param name="_min">Minimum value of the output range.</param>
    /// <param name="_max">Maximum value of the output range.</param>
    /// <param name="_random">Random object to generate the number with.</param>
    /// <returns>A random integer value.</returns>
    public static int GetRandomInt(int _min, int _max, System.Random? _random = null)
    {
        // Determine which random instance to use
        System.Random _rnd = _random != null ? _random : Randy;

        // Generate a random number between the minimum and maximum values inclusive
        return _rnd.Next(_min, _max + 1);
    }

    /// <summary>
    /// Generate a random float value.
    /// </summary>
    /// <param name="_random">Random object used to generate the value.</param>
    /// <returns>A random float value.</returns>
    public static float GenerateRandomValue(System.Random? _random = null)
    {
        // Determine which random instance to use
        System.Random _rnd = _random == null ? Randy : _random;

        // Generate a random value between 0.0 and 1.0
        float _randomValue = (float)_rnd.NextDouble();

        // Map the value to an arbitrary range where abs(minValue) = maxValue, e.g., -100, 100
        float _mappedValue = MapRange(_randomValue, 0.0f, 1.0f, -100000f, 100000f);

        // Return the random mapped value
        return _mappedValue;
    }

    /// <summary>
    /// Generate a random value clamped within a range.
    /// </summary>
    /// <param name="_min">Minimum value of the output range.</param>
    /// <param name="_max">Maximum value of the output range.</param>
    /// <param name="_random">Random object used to generate the value.</param>
    /// <returns>Random value clamped within the range.</returns>
    public static float GenerateRandomValueClamped(float _min, float _max, System.Random? _random = null)
    {
        // Generate a random value
        float _unclampedValue = GenerateRandomValue(_random);

        // Clamp the value and return it
        return ClampFloat(_min, _max, _unclampedValue);
    }

    #endregion
}
