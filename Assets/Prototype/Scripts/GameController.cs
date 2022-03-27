#define DEBUG_BANTER 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
public class GameController : MonoBehaviour
{
    VehicleBase _playerVehicle;
    OxideInput _oxideInput;

    public AudioSource[] _music;
    public AudioSource[] _staticSounds;
    int _musicChannelIndex = 0;
    public float _musicTalkVolume = 0.4f;

    public Texture _titleScreenTex;
    public Texture _blackBordersTex;

    [SerializeField] private float _TrackEndDist = 865.0f;

    VehicleAIManager _vehicleAIManager;

    float _endGameTime = 0.0f;

    [SerializeField]
    bool _disableBanter;

    [System.Serializable]
    public class BanterInfo
    {
        [System.Serializable]
        public class BanterScenario
        {
            [System.Serializable]
            public class BanterAction
            {
                public GameObject _banterTarget;
                public string _animationToPlay;
                public int _animationLoopCount;
                public AudioSource _audioToPlay;
                public float _animationSpeed;
                public float _secsDelay;
                public GameObject _gameObjectToActivate;
                public GameObject _gameObjectToDeactivate;
            }

            public string _scenarioName;
            public bool _duckAudio;
            public BanterAction[] Actions;
        }

        public string _banterName;
        public bool _playOverMusic;
        public float _distanceToPlayAt;
        public BanterScenario[] _scenarios;
    }

   // [SerializeField]
    [FormerlySerializedAs("_banterScenarios")]
    [SerializeField] private BanterInfo[] _historicalBanter;

    [SerializeField] private BanterInfo[] _drivingBanter;


    float _timeOfLastBanter = -10;
    int _banterIndex;
    bool _isBanterRunning;

    public enum GameState
    {
        TitleScreen = 0,
        Playing,
        EndScreen
    };
    GameState _currentState = GameState.TitleScreen;
    public GameState GetGameState() { return _currentState; }

    static GameController s_GameController;
    public static GameController GetGameController()
    {
        return s_GameController;
    }
    public float GetNormalizedDistanceTravelled()
    {
        return Mathf.Clamp01(_playerVehicle.DistanceAlongSpline / _TrackEndDist);
    }

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
        s_GameController = this;
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

    private void Update()
    {
        if (_disableBanter == false && _isBanterRunning == false && _timeOfLastBanter < Time.time - 10.0f && _banterIndex < _historicalBanter.Length)
        {
            if (_playerVehicle.DistanceAlongSpline > _historicalBanter[_banterIndex]._distanceToPlayAt)
            {
#if DEBUG_BANTER
                Debug.Log(Time.time + "Should be playing banner!" + _musicChannelIndex);
#endif
                int scenarioIndex = Random.Range(0, _historicalBanter[_banterIndex]._scenarios.Length - 1);

                if (_musicChannelIndex == -1 || _historicalBanter[_banterIndex]._playOverMusic)
                {
                    _isBanterRunning = true;
                    StartCoroutine(PlayBanterScript(_historicalBanter[_banterIndex]._scenarios[scenarioIndex]));
                }
                _banterIndex++;
            }
        }    
    }

    public void PlayDrivingBanter(string name)
    {
#if DEBUG_BANTER
        Debug.Log(Time.time + "PlayDrivingBanter!");
#endif
        if (_isBanterRunning == true)
        {
            return;
        }

        for (int i = 0; i < _drivingBanter.Length; i++)
        {
            if (_drivingBanter[i]._banterName == name)
            {
#if DEBUG_BANTER
                Debug.Log(Time.time + "     Found " + name);

#endif

                int scenarioIndex = Random.Range(0, _drivingBanter[i]._scenarios.Length - 1);
                _isBanterRunning = true;

                StartCoroutine(PlayBanterScript(_drivingBanter[i]._scenarios[scenarioIndex]));
                return;
            }
        }
    }

    private IEnumerator PlayBanterScript(BanterInfo.BanterScenario scenario)
    {
        int currentAction = 0;
        int duckedChannel = _musicChannelIndex;

#if DEBUG_BANTER
        Debug.Log(Time.time + " PLAYBANTERSCRIPT!");
#endif
        if (scenario._duckAudio && duckedChannel >= 0)
        {
            _music[duckedChannel].volume = _musicTalkVolume;
            ((CarPhysicsObject)_playerVehicle).SetEngineVolume(0.05f);
        }

        while (true && currentAction < scenario.Actions.Length)
        {
            float nextYield = UpdateBanter(scenario.Actions[currentAction]);
            currentAction++;
            if (nextYield < 0)
            {
                break;
            }

            yield return new WaitForSeconds(nextYield);
        }

        _timeOfLastBanter = Time.time;

        _isBanterRunning = false;

        if (scenario._duckAudio && duckedChannel >= 0)
        {
            _music[duckedChannel].volume = 1.0f;
            ((CarPhysicsObject)_playerVehicle).SetEngineVolume(0.13f);
        }

#if DEBUG_BANTER
        Debug.Log(Time.time + "Finished banter");
#endif
    }

    float UpdateBanter(BanterInfo.BanterScenario.BanterAction action)
    {
        if (action._animationToPlay.Length > 0 && action._banterTarget != null)
        {
#if DEBUG_BANTER
            Debug.Log(Time.time + "Playing anim " + action._animationToPlay);
#endif
            var anim = action._banterTarget.GetComponentInChildren<Animation>();
            anim[action._animationToPlay].speed = action._animationSpeed;
            anim[action._animationToPlay].blendMode = AnimationBlendMode.Blend;
            anim.Blend(action._animationToPlay, 0.2f, 1.3f);

            for (int i = 1; i < action._animationLoopCount; i++)
            {
                var animState = anim.PlayQueued(action._animationToPlay, QueueMode.CompleteOthers);
                animState.speed = action._animationSpeed;
            }
        }
        if (action._gameObjectToActivate != null)
        {
            action._gameObjectToActivate.SetActive(true);
        }

        if (action._gameObjectToDeactivate != null)
        {
            action._gameObjectToDeactivate.SetActive(false);
        }

        if (action._audioToPlay != null)
        {
#if DEBUG_BANTER
            Debug.Log(Time.time + "Playing audio " + action._audioToPlay);
#endif
            action._audioToPlay.Play();
        }
        return action._secsDelay;
    }

    float lastSwitch = 0;
    void FixedUpdate()
    {
        var playerInput = _oxideInput.Player;

        if (_currentState == GameState.TitleScreen)
        {
            _playerVehicle.GetComponent<CapsuleCollider>().enabled = false;
            if (_playerVehicle.GetComponent<Rigidbody>() != null)
            {
                _playerVehicle.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionY;
            }
            if (playerInput.Gas.ReadValue<float>() > 0)
            {

                ((CarPhysicsObject)_playerVehicle).StartCar();

                _currentState = GameState.Playing;
                if (_music != null)
                {
                    _music[_musicChannelIndex].Play();
                }
                _vehicleAIManager.enabled = true;
                _vehicleAIManager.OnRaceStart();
                _banterIndex = 0;
            }
#if UNITY_EDITOR
            else if (playerInput.Prrr.ReadValue<float>() > 0)
            {
                ((CarPhysicsObject)_playerVehicle).StartCar();

                _currentState = GameState.Playing;
                if (_music != null)
                {
                    _music[_musicChannelIndex].loop = true;
                    _music[_musicChannelIndex].Play();
                }
                ((CarPhysicsObject)_playerVehicle).CheatWarp(328);
                _vehicleAIManager.enabled = true;
                _vehicleAIManager.OnRaceStart();
                _banterIndex = 1;
            }
            else if (playerInput.Music.ReadValue<float>() > 0)
            {
                ((CarPhysicsObject)_playerVehicle).StartCar();
                _currentState = GameState.Playing;
                if (_music != null)
                {
                    _music[_musicChannelIndex].loop = true;
                    _music[_musicChannelIndex].Play();
                }
                ((CarPhysicsObject)_playerVehicle).CheatWarp(750);
                _vehicleAIManager.enabled = true;
                _vehicleAIManager.OnRaceStart();
                _banterIndex = 2;
            }
#endif
            //   Debug.Log(Time.time + " playerInput.Prrr.ReadValue<bool>() = " + playerInput.Prrr.ReadValue<bool>());
        }
        else if (_currentState == GameState.Playing)
        {
            _playerVehicle.GetComponent<CapsuleCollider>().enabled = true;

            if (_playerVehicle.GetComponent<Rigidbody>() != null)
            {
                _playerVehicle.GetComponent<Rigidbody>().constraints &= ~RigidbodyConstraints.FreezePositionY;
            }

            if (playerInput.Music.ReadValue<float>() > 0.5f && Time.time > lastSwitch + 2.0f)
            {
                lastSwitch = Time.time;
                StartCoroutine("ChangeChannel");
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

            if (_playerVehicle.DistanceAlongSpline > 865.0f)
            {
                _currentState = GameState.EndScreen;

             //   var vInput = GetInput();

                vInput.Steering = 0;
                vInput.Gas = 0;
                vInput.WantsToPurr = false;
                SetInput(vInput);
                ((CarPhysicsObject)_playerVehicle).StopCar();

                _endGameTime = Time.time;
            }
        }
        else if (_currentState == GameState.EndScreen)
        {
            if (Time.time > _endGameTime + 5.0f)
            {
                _currentState = GameState.TitleScreen;
                _playerVehicle.enabled = true;
                _playerVehicle.transform.position = new Vector3(-0.66f, 0.0f, 9.24f);
            }
        }
    }

    private IEnumerator ChangeChannel()
    {
        if (_musicChannelIndex > -1)
        {
            _music[_musicChannelIndex].Pause();
        }

        _staticSounds[0].Play();
        yield return new WaitForSeconds(Random.Range(0.25f, 0.35f));
        _staticSounds[0].Pause();

        _musicChannelIndex++;
        if (_musicChannelIndex < _music.Length)
        {
            _music[_musicChannelIndex].Play();

            if (_isBanterRunning)
            {
                _music[_musicChannelIndex].volume = _musicTalkVolume;
            }
            else
            {
                _music[_musicChannelIndex].volume = 1.0f;
            }
        }
        else
        {
            _musicChannelIndex = -1;
        }
    }

    private void OnGUI()
    {
        if (_currentState == GameState.TitleScreen || _currentState == GameState.EndScreen)
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
