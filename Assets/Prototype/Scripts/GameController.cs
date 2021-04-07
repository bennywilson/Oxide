using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    Vehicle PlayerVehicle;

    // Start is called before the first frame update
    void Start()
    {
        PlayerVehicle = FindObjectOfType<Vehicle>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnMove(InputValue input)
    {
        Debug.Log("Here!");
        if (PlayerVehicle == null)
        {
            return;
        }

        PlayerVehicle.OnMoveInput(input);

    }

    public void OnGas(InputValue input)
    {
        Debug.Log("Woah!");

        if (PlayerVehicle == null)
        {
            return;
        }

        PlayerVehicle.OnGas(input);

        Debug.Log(Time.time + " : " + input.Get<float>());
    }

    public void OnPrrr(InputValue Input)
    {
        if (PlayerVehicle == null)
        {
            return;
        }

        PlayerVehicle.OnPrrr(Input);
    }
}
