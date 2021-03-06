using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    VehicleBase PlayerVehicle;
    OxideInput _oxideInput;
    public AudioSource Music;
    public Texture TitleScreenTex;
    public Texture BlackBordersTex;

    public enum GameState
    {
        TitleScreen = 0,
        Playing
    };
    GameState _currentState = GameState.TitleScreen;
    public GameState GetGameState() { return _currentState; }

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
        if (_currentState == GameState.TitleScreen)
        {
            PlayerVehicle.GetComponent<CapsuleCollider>().enabled = false;
            PlayerVehicle.GetComponent<Rigidbody>().useGravity = false;

            var playerInput = _oxideInput.Player;
            if (playerInput.Gas.ReadValue<float>() > 0)
            {
                _currentState = GameState.Playing;
                if (Music != null)
                {
                    Music.loop = true;
                    Music.Play();
                }
            }
        }
        else if (_currentState == GameState.Playing)
        {
            PlayerVehicle.GetComponent<CapsuleCollider>().enabled = true;
            PlayerVehicle.GetComponent<Rigidbody>().useGravity = true;

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

    private void OnGUI()
    {
        if (_currentState == GameState.TitleScreen)
        {
            float textureAspect = 1920.0f / 1080.0f;
            float oneOverTextureAspect = 1.0f / textureAspect;
            float screenAspect = Screen.width / (float) Screen.height;
            float textureX = 0;
            float textureY = 0;
            float textureWidth = Screen.width;
            float textureHeight = Screen.height;
            Debug.Log("asdasdasd");
            if (screenAspect < textureAspect)
            {
                textureWidth = Screen.width;
                textureHeight = textureWidth * oneOverTextureAspect;
                textureY = Mathf.Abs(Screen.height - textureHeight) / 2.0f;
            }
            else
            {
                textureHeight = Screen.height;
                textureWidth = textureHeight * textureAspect;
                textureX = Mathf.Abs(Screen.width - textureWidth) / 2.0f;
            }
           
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), BlackBordersTex);
            GUI.DrawTexture(new Rect(textureX, textureY, textureWidth, textureHeight), TitleScreenTex);
        }
        else if (_currentState == GameState.Playing)
        {

        }
    }
}
