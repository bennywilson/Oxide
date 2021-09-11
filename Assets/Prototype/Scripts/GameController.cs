using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    VehicleBase _playerVehicle;
    OxideInput _oxideInput;
    public AudioSource[] _music;
    int _musicChannelIndex = 0;
    public Texture _titleScreenTex;
    public Texture _blackBordersTex;

    VehicleAIManager _vehicleAIManager;

    public enum GameState
    {
        TitleScreen = 0,
        Playing
    };
    GameState _currentState = GameState.TitleScreen;
    public GameState GetGameState() { return _currentState; }

    VehicleInput GetInput()
    {
        if (_playerVehicle != null)
        {
            return _playerVehicle.Input;
        }

        return default;
    }

    void SetInput(VehicleInput input)
    {
        if (_playerVehicle == null)
            return;

        _playerVehicle.Input = input;
    }

    bool GetCanUseInput()
    {
        return _playerVehicle != null && _playerVehicle.gameObject.activeInHierarchy;
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
    public VehicleBase GetPlayer()
    {
        return _playerVehicle;
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
        _playerVehicle = FindObjectOfType<VehicleBase>();
        _vehicleAIManager = gameObject.GetComponent<VehicleAIManager>();
        _vehicleAIManager.SetGameController(this);
        _vehicleAIManager.enabled = false;
    }

    float lastSwitch = 0;
    void FixedUpdate()
    {
        var playerInput = _oxideInput.Player;

        if (_currentState == GameState.TitleScreen)
        {
            _playerVehicle.GetComponent<CapsuleCollider>().enabled = false;
            _playerVehicle.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionY;

            if (playerInput.Gas.ReadValue<float>() > 0)
            {
                _currentState = GameState.Playing;
                if (_music != null)
                {
                    _music[_musicChannelIndex].Play();
                }
                _vehicleAIManager.enabled = true;
                _vehicleAIManager.OnRaceStart();
            }
#if UNITY_EDITOR
            else if (playerInput.Prrr.ReadValue<float>() > 0)
            {
                _currentState = GameState.Playing;
                if (_music != null)
                {
                    _music[_musicChannelIndex].loop = true;
                    _music[_musicChannelIndex].Play();
                }
                ((CarPhysicsObject)_playerVehicle).CheatWarp();
                _vehicleAIManager.enabled = true;
                _vehicleAIManager.OnRaceStart();
            }
#endif
            //   Debug.Log(Time.time + " playerInput.Prrr.ReadValue<bool>() = " + playerInput.Prrr.ReadValue<bool>());
        }
        else if (_currentState == GameState.Playing)
        {
            _playerVehicle.GetComponent<CapsuleCollider>().enabled = true;
            _playerVehicle.GetComponent<Rigidbody>().constraints &= ~RigidbodyConstraints.FreezePositionY;

            if (playerInput.Music.ReadValue<float>() > 0.5f && Time.time > lastSwitch + 2.0f)
            {
                lastSwitch = Time.time;

                if (_musicChannelIndex > -1)
                {
                    _music[_musicChannelIndex].Pause();
                }

                _musicChannelIndex++;
                if (_musicChannelIndex < _music.Length)
                {
                    _music[_musicChannelIndex].Play();
                }
                else
                {
                    _musicChannelIndex = -1;
                }
                

               /* if (_music.isPlaying)
                {
                    _music.Pause();
                }
                else
                {
                    _music.Play();
                }*/
            }

            if (!GetCanUseInput())
                return;

            var vInput = GetInput();

            vInput.Steering = playerInput.Move.ReadValue<Vector2>().x;
            vInput.Gas = playerInput.Gas.ReadValue<float>();
            vInput.WantsToPurr = playerInput.Prrr.ReadValue<float>() > 0;
          //  Debug.Log(Time.time + " " + vInput.WantsToPurr);
          //  vInput.Brake = playerInput.Brake.ReadValue<float>();

            SetInput(vInput);

            _vehicleAIManager.UpdateController();
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
           
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _blackBordersTex);
            GUI.DrawTexture(new Rect(textureX, textureY, textureWidth, textureHeight), _titleScreenTex);
        }
        else if (_currentState == GameState.Playing)
        {

        }
    }
}
