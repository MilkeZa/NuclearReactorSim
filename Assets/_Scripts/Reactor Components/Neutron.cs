using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// This class controls the neutrons that are spawned from the reactions with fissile fuel.
/// </summary>
public class Neutron : MonoBehaviour
{
    [Header("State based Travel Speed")]
    [SerializeField]
    [Tooltip("Speed of the object when in the slow state")]
    private float slowSpeed = 1f;

    [SerializeField]
    [Tooltip("Speed of the object when in the fast state")]
    private float fastSpeed = 2f;

    [Header("State based Sprite Coloring")]
    [SerializeField]
    [Tooltip("Color of the interior when in the slow state")]
    private Color slowColor = Color.black;

    [SerializeField]
    [Tooltip("Color of the interior when in the fast state")]
    private Color fastColor = Color.white;

    [SerializeField]
    [Tooltip("Sprite whose color will be updated when state is altered")]
    private SpriteRenderer interiorSprite;

    public bool isThermalNeutron { get; private set; }  // True when travelling too fast to cause a reaction, false, when slower than the max reaction speed

    public float speed { get; private set; }            // Slow when state is false, true, otherwise

    private float minimumSpeed;                 // Minimum speed that when reached, the neutron is destroyed. Used for water based moderation
    private float maxReactionSpeed;             // Maximum speed at which a neutron can cause a reaction with a fissile fuel rod

    private Vector3 travelDirection;            // Direction in which neutron will travel

    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Speed at which water slows down neutrons")]
    private float waterSlowSpeed = 0.1f;        // How quickly water slows neutrons down

    private CircleCollider2D circleCollider;

    #region UnityMethods

    private void Awake()
    {
        // Get the collider component
        circleCollider = GetComponent<CircleCollider2D>();

        // Neutrons are thermal at start
        SetState(true);

        // Create a random object using this game objects instance id
        System.Random _random = new System.Random(gameObject.GetInstanceID());

        // Generate random x and y values to create a travel direction
        float _x = Randomizer.MapRange((float)_random.NextDouble(), 0.0f, 1.0f, -1.0f, 1.0f);
        float _y = Randomizer.MapRange((float)_random.NextDouble(), 0.0f, 1.0f, -1.0f, 1.0f);

        // Combine the x and y values to create the travel direction
        travelDirection = new Vector3(_x, _y, 0f).normalized;

        // Calculate the minimum speed at 3/4 the slow speed
        minimumSpeed = 3f * (slowSpeed / 4f);

        // Calculate the max reaction speed as 1.5x the slow speed
        maxReactionSpeed = 1.5f * slowSpeed;
    }

    private void OnBecameInvisible()
    {
        // Destroy neutrons that are no longer visible on screen
        Destroy(gameObject);
    }

    private void Update()
    {
        // Move the gameobject
        transform.position += travelDirection * (speed * Time.fixedDeltaTime);

        // Check if the neutron is a thermal neutron or not
        if (speed <= maxReactionSpeed && isThermalNeutron)
        {
            // Neutron is now travelling below the maximum reaction speed, set the neutron state to false
            SetState(false);
        }

        // Check if the neutron is travelling at or below the minimum speed
        if (speed <= minimumSpeed)
        {
            // Neutron is travelling too slow (it got moderated, likely by water) and needs to be destroyed
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Neutrons current speed determines what it can and cannot interact with.
        if (isThermalNeutron)
        {
            // Check if the neutron has collided with a moderator rod
            if (collision.gameObject.CompareTag("Moderator Rod"))
            {
                // Reflect neutron from the moderator
                travelDirection = new Vector3(-travelDirection.x, travelDirection.y, travelDirection.z);

                // Neutron has hit a moderator rod, slow it down immediately
                SetState(false);
            }

            // Control rods are ignored while the neutron is moving fast thanks to the fast neutron layer.
        }
        else
        {
            // Neutron is travelling slow enough to react with control rods and fuel rods
            if (collision.gameObject.CompareTag("Control Rod"))
            {
                // Neutron is absorbed by the control rod, destroy it
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object collided with is a fuel rod and is NOT a thermal neutron
        if (collision.gameObject.CompareTag("Fuel Rod") && !isThermalNeutron)
        {
            // Neutron has collided with a fuel rod, get the fuel rod component
            FuelRod _fuelRod = collision.gameObject.GetComponent<FuelRod>();

            // Check if a reaction with this fuel rod can occur
            if (_fuelRod.NeutronReaction())
            {
                // A reaction can occur, destroy this neutron gameobject
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Check if the neutron is in water
        if (collision.gameObject.CompareTag("Moderator Water"))
        {
            // Get the coolant water component
            CoolantWater _water = collision.gameObject.GetComponent<CoolantWater>();

            // Heat up the water, where slower speed means less heat
            _water.HeatWater(speed);

            // Gradually slow the neutron down over time
            SlowNeutron();
        }
    }

    #endregion

    #region StateMethods

    private void SetState(bool _state)
    {
        // Update the state bool
        isThermalNeutron = _state;

        // Set the color of the interior sprite
        if (isThermalNeutron)
        {
            // Set the color to the fast color
            interiorSprite.color = fastColor;

            // Set the speed to the fast speed
            speed = fastSpeed;
        }
        else
        {
            // Set the color to the slow color
            interiorSprite.color = slowColor;

            // Set the speed to the slow speed
            speed = maxReactionSpeed;

            // Set the layer to a regular neutron
            gameObject.layer = 10;
        }
    }

    private void SlowNeutron()
    {
        // Calculate the loss of speed
        float _speedDelta = -(waterSlowSpeed * Time.fixedDeltaTime);

        // Subtract the speed delta from the speed
        speed -= _speedDelta;
    }

    #endregion
}
