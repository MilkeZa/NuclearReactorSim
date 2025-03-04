using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles the reading and writing of json data.
/// </summary>
public static class SaveJsonData
{
    #region Variables

    private static string fuelAssemblyDataFilePath = Application.persistentDataPath + "/FuelAssemblySaveData.json";

    #endregion

    #region CustomMethods

    public static void WriteFuelAssemblySaveData(int _fuelRows, int _fuelColumns, Vector2 _minimumPosition, Vector2 _maximumPosition)
    {
        // Create a new fuel assembly save data instance
        FuelAssemblySaveData _fuelAssemblySaveData = new FuelAssemblySaveData(_fuelRows, _fuelColumns, _minimumPosition, _maximumPosition);

        // Write the fuel assembly data to the data file
        WriteJsonData<FuelAssemblySaveData>(fuelAssemblyDataFilePath, _fuelAssemblySaveData);
        Debug.Log($"Writing fuel assembly data to file\n{JsonUtility.ToJson(_fuelAssemblySaveData)}");
    }

    public static FuelAssemblySaveData ReadFuelAssemblySaveData()
    {
        // Read the json data from the data file
        string _jsonData = System.IO.File.ReadAllText(fuelAssemblyDataFilePath);

        // Parse the json data into an instance
        FuelAssemblySaveData _fuelAssemblySaveData = JsonUtility.FromJson<FuelAssemblySaveData>(_jsonData);
        Debug.Log($"Loaded fuel assembly save data {_fuelAssemblySaveData}\n{_fuelAssemblySaveData}");

        // Return the fuel assembly data
        return _fuelAssemblySaveData;
    }

    private static void WriteJsonData<T>(string _filePath, T _data)
    {
        // If the incoming data is already a string, set the json data variable, otherwise, convert the data to a json string
        string _jsonData = _data.GetType() == typeof(string) ? _data as string : JsonUtility.ToJson(_data);

        // Write the data to the file path
        System.IO.File.WriteAllText(_filePath, _jsonData);
    }

    #endregion
}
