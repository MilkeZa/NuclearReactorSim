using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

/// <summary>
/// This class handles the editor styling for the Fuel Assembly Generator components.
/// </summary>
[CustomEditor(typeof(ReactorGenerator))]
public class ReactorGeneratorEditor : Editor
{
    #region Variables

    public VisualTreeAsset VisualTree;

    private ReactorGenerator reactorGenerator;

    private Button saveSettingsButton;
    private Button resetSettingsButton;
    private Button generateAssemblyButton;
    private Button clearAssemblyButton;

    #endregion

    #region UnityMethods

    private void OnEnable()
    {
        // Grab the target
        reactorGenerator = (ReactorGenerator)target;
    }

    public override VisualElement CreateInspectorGUI()
    {
        // Create a root instance
        VisualElement root = new VisualElement();

        // Add in all the UI builder stuff
        VisualTree.CloneTree(root);

        // Find buttons using a root query
        saveSettingsButton = root.Q<Button>("SaveFuelAssemblySettingsButton");
        resetSettingsButton = root.Q<Button>("ResetFuelAssemblySettingsButton");
        generateAssemblyButton = root.Q<Button>("GenerateFuelAssemblyButton");
        clearAssemblyButton = root.Q<Button>("ClearFuelAssemblyButton");

        // Assign callbacks to the buttons
        saveSettingsButton.RegisterCallback<ClickEvent>(OnSaveButtonClicked);
        resetSettingsButton.RegisterCallback<ClickEvent>(OnResetButtonClicked);
        generateAssemblyButton.RegisterCallback<ClickEvent>(OnGenerateButtonClicked);
        clearAssemblyButton.RegisterCallback<ClickEvent>(OnClearButtonClicked);

        // Return the root instance
        return root;
    }

    #endregion

    #region Button Events

    /// <summary>
    /// Callback for when the save settings button is pressed.
    /// </summary>
    /// <param name="_event"></param>
    private void OnSaveButtonClicked(ClickEvent _event)
    {
        reactorGenerator.SaveFuelAssemblySettings();
    }

    /// <summary>
    /// Callback for when the reset settings button is clicked.
    /// </summary>
    /// <param name="_event"></param>
    private void OnResetButtonClicked(ClickEvent _event)
    {
        reactorGenerator.ResetFuelAssemblySettings();
    }

    /// <summary>
    /// Callback for when the generate fuel assembly button is clicked.
    /// </summary>
    /// <param name="_event"></param>
    private void OnGenerateButtonClicked(ClickEvent _event)
    {
        reactorGenerator.GenerateFuelAssembly();
    }

    /// <summary>
    /// Callback for when the clear fuel assembly button is clicked.
    /// </summary>
    /// <param name="_event"></param>
    private void OnClearButtonClicked(ClickEvent _event)
    {
        reactorGenerator.ClearFuelAssembly();
    }

    #endregion
}
