using UnityEngine;

public class CoolantWater : MonoBehaviour
{
    #region Variables

    private SpriteRenderer waterSprite;
    private BoxCollider2D waterCollider;

    [SerializeField]
    [Tooltip("Color displayed when water is cool")]
    private Color coolColor = new Color(176f / 255f, 237f / 255f, 255f / 255f, 1f); // Sprite color when water is "cool"

    [SerializeField]
    [Tooltip("Color displayed when water is hot")]
    private Color hotColor = new Color(255f / 255f, 76f / 255f, 76f / 255f, 1f);    // Sprite color when water is "hot"

    private Color evaporationColor;                                                 // Sprite color when water is "evaporated"

    public bool isLightWater { get; private set; } = true;                          // Lightwater (true) evaporates quicker than heavier water (false).

    [SerializeField]
    [Tooltip("Temperature of water that has absorbed no neutrons")]
    private float coolTemperature = 32f;

    [SerializeField]
    [Tooltip("Temperature moves towards this value as it absorbs neutrons")]
    private float baseHotTemperature = 100f;

    private float hotTemperature;

    [SerializeField]
    [Tooltip("Temperature at which the water will no longer be able to absorb neutrons")]
    private float baseEvaporationTemperature = 112f;

    private float evaporationTemperature;    // Final evaporation temperature used when heating the water

    private float maxTemperature;           // Maximum temperature water is allowed to reach. This is just to keep it from rising forever

    [SerializeField]
    [Tooltip("Multiplier for the base evaporation temperature used in heavy water")]
    private float heavyWaterHeatFactor = 1.1f;


    public float currentTemperature { get; private set; }   // Current temperature of the water
    private Color currentColor;         // Linearly interpolated value between cool and hot temperature colors

    [SerializeField]
    [Tooltip("Enables the water to cool when heated")]
    private bool enableCooling;

    [SerializeField]
    [Tooltip("Speed at which the water cools down")]
    private float coolFactor = 1f;       // Speed at which water cools down

    [SerializeField]
    [Tooltip("Speed at which the water heats up")]
    private float heatFactor = 1.1f;    // Speed at which water heats up

    private bool hasEvaporated = false; // Has the water evaporated into a gas?
    private bool hasCondensed = true;   // Has the water condensed into a liquid? Water starts condensed be default.

    /* 
     * 0, 1 clamped value indicating how much cooling this water cell is capable of. 
     *  Calculated as follows
     *      1f - MapRange(currentTemperature, coolTemperature, evaporationTemperature, 0f, 1f)
     */
    public float capacityRemaining { get; private set; }

    #endregion

    #region UnityMethods

    private void Awake()
    {
        // Get the sprite renderer component to change water color
        waterSprite = GetComponent<SpriteRenderer>();

        // Get the box collider component to manage collisions/triggers
        waterCollider = GetComponent<BoxCollider2D>();

        // Initialize the water variables
        InitializeWater();
    }

    private void Update()
    {
        // Check if the water is at or above cool temperature
        if (currentTemperature >= coolTemperature)
        {
            // Check if cooling is enabled
            if (enableCooling)
            {
                // Water is currently heated, attempt to dissipate the heat
                CoolWater();
            }

            // Check if the temperature is above the evaporation point and hasn't already evaporated
            if (currentTemperature >= evaporationTemperature && !hasEvaporated)
            {
                // Temperature has exceeded the evaporation point, meaning it can no longer moderate the reaction
                EvaporateWater();
            }

            // Check if the temperature is below the condensation point and hasn't already condensed
            if (currentTemperature < evaporationTemperature && !hasCondensed)
            {
                // Temperature is back within liquid range, meaning it can once again moderate the reaction
                CondenseWater();
            }
        }
    }

    #endregion

    #region InitializationMethods

    /// <summary>
    /// Initialize the state of the coolant water.
    /// </summary>
    private void InitializeWater()
    {
        // Calculate and assign the hot and evaporation temperatures
        hotTemperature = isLightWater ? baseHotTemperature : baseHotTemperature * heavyWaterHeatFactor;
        evaporationTemperature = isLightWater ? baseEvaporationTemperature : baseEvaporationTemperature * heavyWaterHeatFactor;

        // Calculate the evaporation colors using the hot color with full transparency
        evaporationColor = new Color(hotColor.r, hotColor.g, hotColor.b, 0f);

        // Calculate the maximum temperature
        maxTemperature = 2f * evaporationTemperature;

        // All water starts cooled
        currentTemperature = coolTemperature;

        // Set the state of the water
        ChangeTemperature(0f);
    }

    #endregion

    #region TemperatureMethods

    /// <summary>
    /// Convert a temperature to color.
    /// </summary>
    /// <param name="_temperature">Temperature of the water.</param>
    /// <returns>Color result of the input temperature.</returns>
    private Color TemperatureToColor(float _temperature)
    {
        float _tempFactor;  // Value in the range [0, 1] used to interpolate the colors
        float _colorFactor; // Factor used to determine 

        // Colors whose RGBA values will be interpolated between
        Color _lowerColor, _upperColor;

        // Check what temperature range the water is currently within
        if (coolTemperature <= currentTemperature && currentTemperature < hotTemperature)
        {
            // Temperature is between cool and hot
            _tempFactor = Randomizer.MapRange(currentTemperature, coolTemperature, hotTemperature, 0f, 1f);
            _colorFactor = Mathf.Lerp(coolTemperature, hotTemperature, _tempFactor);
            _lowerColor = coolColor;
            _upperColor = hotColor;
        }
        else if (hotTemperature <= currentTemperature && currentTemperature < evaporationTemperature)
        {
            // Temperature is between hot and evaporated
            _tempFactor = Randomizer.MapRange(currentTemperature, hotTemperature, evaporationTemperature, 0f, 1f);
            _colorFactor = Mathf.Lerp(hotTemperature, evaporationTemperature, _tempFactor);
            _lowerColor = hotColor;
            _upperColor = evaporationColor;
        }
        else
        {
            // Temperature is greater than evaporated, which means it will always be "invisible" (alpha = 1)
            _tempFactor = 1f;
            _colorFactor = 1f;
            _lowerColor = evaporationColor;
            _upperColor = evaporationColor;
        }

        // Calculate the average of each of the lower and upper colors r, g, b, and a values 
        float _r = Mathf.Lerp(_lowerColor.r, _upperColor.r, _tempFactor);
        float _g = Mathf.Lerp(_lowerColor.g, _upperColor.g, _tempFactor);
        float _b = Mathf.Lerp(_lowerColor.b, _upperColor.b, _tempFactor);
        float _a = Mathf.Lerp(_lowerColor.a, _upperColor.a, _tempFactor);

        // Create a new color using the previously calculated rgba values
        Color _color = new Color(_r, _g, _b, _a);

        // Return the color
        return _color;
    }

    /// <summary>
    /// Update the temperature of the water.
    /// </summary>
    /// <param name="_temperatureDelta">Difference in temperature to apply to the water.</param>
    private void ChangeTemperature(float _temperatureDelta)
    {
        // Apply the change in temperature to the current temperature, clamping to the range [coolTemperature, maxTemperature]
        currentTemperature = Mathf.Clamp(currentTemperature + _temperatureDelta, coolTemperature, maxTemperature);

        // Calculate the current color using the current temperature
        currentColor = TemperatureToColor(currentTemperature);

        // Calculate the capacity remaining
        capacityRemaining = 1f - Randomizer.MapRange(currentTemperature, coolTemperature, evaporationTemperature, 0f, 1f);

        // Apply the color to the sprite
        waterSprite.color = currentColor;
    }

    /// <summary>
    /// Lower the temperature of the water.
    /// </summary>
    public void CoolWater()
    {
        // Calculate the negative change in temperature as water is cooling down
        float _temperatureDelta = -(coolFactor * Time.fixedDeltaTime);

        // Apply the temperature delta
        ChangeTemperature(_temperatureDelta);
    }

    /// <summary>
    /// Raise the temperature of the water.
    /// </summary>
    /// <param name="_heatAmount">Quantity with which the temperature should be raised.</param>
    public void HeatWater(float _heatAmount)
    {
        // Calculate the positive change in temperature
        float _temperatureDelta = _heatAmount * heatFactor * Time.fixedDeltaTime;

        // Apply the temperature delta
        ChangeTemperature(_temperatureDelta);
    }

    /// <summary>
    /// Set the state of the pumps.
    /// </summary>
    /// <param name="_state">Pumps are enabled (true), or disable (false), controlling if water can be cooled or not.</param>
    public void SetPumpState(bool _state)
    {
        // Enable or disable the water to cool depending on state
        enableCooling = _state;
    }

    /// <summary>
    /// Trigger an evaporation with the water, disabling its ability to moderate the reaction.
    /// </summary>
    private void EvaporateWater()
    {
        // Current temperature has exceeded the evaporation point. Set the has evaporated and has condensed flags
        hasEvaporated = true;
        hasCondensed = false;

        // Disable the collider component to keep it from interacting with more neutrons
        waterCollider.enabled = false;
    }

    /// <summary>
    /// Condense back into water, allowing for moderation of the reaction.
    /// </summary>
    private void CondenseWater()
    {
        // Water has cooled enough to condense into a liquid. Set the has evaporated and has condensed flags
        hasEvaporated = false;
        hasCondensed = true;

        // Enable the collider component to allow the water to interact with neutrons
        waterCollider.enabled = true;
    }

    #endregion
}
