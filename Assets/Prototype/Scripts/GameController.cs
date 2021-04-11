using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    VehicleBase PlayerVehicle;

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

    // Start is called before the first frame update
    void Start()
    {
        PlayerVehicle = FindObjectOfType<VehicleBase>();
    }

    public void OnMove(InputValue input)
    {
        if (!GetCanUseInput())
            return;

        var vInput = GetInput();
        vInput.Steering = input.Get<Vector2>().x;
        SetInput(vInput);
    }

    public void OnGas(InputValue input)
    {
        if (!GetCanUseInput())
            return;

        var vInput = GetInput();
        vInput.Gas = input.Get<float>();
        SetInput(vInput);
    }

    public void OnPrrr(InputValue Input)
    {
        if (!GetCanUseInput())
            return;

        var vInput = GetInput();
        vInput.WantsToPurr = true;
        SetInput(vInput);
    }
}
