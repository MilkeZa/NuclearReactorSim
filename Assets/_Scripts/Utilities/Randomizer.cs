using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This class handles the randomness within the application.
/// </summary>
public static class Randomizer
{
    #region Variables

    // Static random object to be used by the whole project using todays date in ticks as a seed
    private static System.Random Randy = new System.Random((int)DateTime.Now.Ticks);

    #endregion

    #region CustomMethods

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

    public static float ClampFloat(float _min, float _max, float _value)
    {
        // Clamp the value between the min and max values
        float _clampedValue = _value < _min ? _min : _value;
        return _clampedValue > _max ? _max : _value;
    }

    public static float MapRange(float _val, float _inMin, float _inMax, float _outMin, float _outMax)
    {
        // Map the value to the new range
        return _outMin + ((_val - _inMin) / (_inMax - _inMin) * (_outMax - _outMin));
    }

#nullable enable

    public static int GetRandomInt(int _min, int _max, System.Random? _random = null)
    {
        // Determine which random instance to use
        System.Random _rnd = _random != null ? _random : Randy;

        // Generate a random number between the minimum and maximum values inclusive
        return _rnd.Next(_min, _max + 1);
    }

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

    public static float GenerateRandomValueClamped(float _min, float _max, System.Random? _random = null)
    {
        // Generate a random value
        float _unclampedValue = GenerateRandomValue(_random);

        // Clamp the value and return it
        return ClampFloat(_min, _max, _unclampedValue);
    }

    #endregion
}
