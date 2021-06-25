using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    VehicleBase PlayerVehicle;
    OxideInput _oxideInput;
    public AudioSource Music;

    VehicleInput GetInput()
    {
        if (PlayerVehicle != null)
            return PlayerVehicle.Input;

        return default;
    }

    void SetInput(VehicleInput input)
    {
        if (PlayerVehicle == null)
            return;

        PlayerVehicle.Input = input;
    }

    bool GetCanUseInput()
    {
        return PlayerVehicle != null && PlayerVehicle.gameObject.activeInHierarchy;
    }

    void Awake()
    {
        _oxideInput = new OxideInput();

        _oxideInput.Player.Prrr.performed += c =>
        {
            if (!GetCanUseInput())
                return;

            var vInput = GetInput();
           //    vInput.WantsToPurr = true;
            SetInput(vInput);
        };
    }

    void OnEnable()
    {
        _oxideInput.Enable();

        if (Music != null)
        {
            Music.loop = true;
            Music.Play();
        }
    }

    void OnDisable()
    {
        if (_oxideInput != null)
        {
            _oxideInput.Disable();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        PlayerVehicle = FindObjectOfType<VehicleBase>();
    }

    void Update()
    {
        if (!GetCanUseInput())
            return;

        var playerInput = _oxideInput.Player;
        var vInput = GetInput();

        vInput.Steering = playerInput.Move.ReadValue<Vector2>().x;
        vInput.Gas = playerInput.Gas.ReadValue<float>();
        vInput.Brake = playerInput.Brake.ReadValue<float>();

        SetInput(vInput);
    }
}
