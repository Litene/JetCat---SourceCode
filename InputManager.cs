using System;
using System.Collections;
using System.Collections.Generic;
using Car;
using Level;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour {
    private InputScheme _menuHandling;
    private InputScheme _playerControls;
    private InputScheme _carControls;
    private Vector2 _movementInput;
    private ThirdPersonController _characterController;
    private CarBehaviour _currentCarController;
    private Vector2 _cameraInput;
    private bool _rightButtonPressed;
    private bool _leftButtonPressed;
    private Vector2 _playerCharacterInput = Vector2.zero;

    private Vector2 PlayerCharacterInput {
        get => _playerCharacterInput;
        set {
            _playerCharacterInput = value;
            if (_rightButtonPressed && _leftButtonPressed) {
                // doesn't work
                return;
            }

            _characterController.SetMoveVector(_playerCharacterInput);
        }
    }

    public bool RightButtonPressed {
        set {
            if (_leftButtonPressed && value) {
                CharacterController.CharacterJump(value);
            }

            _rightButtonPressed = value;
        }
    }

    public bool LeftButtonPressed {
        set {
            if (_rightButtonPressed && value) {
                CharacterController.CharacterJump(value);
            }

            _leftButtonPressed = value;
        }
    }


    private static InputManager _instance;

    public static InputManager Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<InputManager>();
                if (_instance == null) {
                    GenerateSingleton();
                }
            }

            return _instance;
        }
    }

    private static void GenerateSingleton() {
        // remove singleton?
        GameObject inputManagerObject = new GameObject("InputManager");
        DontDestroyOnLoad(inputManagerObject);
        _instance = inputManagerObject.AddComponent<InputManager>();
    }

    private bool _carControllerCheck = false;

    public CarBehaviour CurrentCar {
        get => _currentCarController;
        set {
            var carControl = _carControls.CarControl;

            if (_carControllerCheck && value != _currentCarController) {
                carControl.Move.performed -= val => _currentCarController.MoveOverride(val.ReadValue<float>());
                carControl.Move.canceled -= _ => _currentCarController.MoveOverride(0);
                carControl.Speed.performed -= val => _currentCarController.ShiftGear(val.ReadValue<float>());
            }

            _carControllerCheck = true;

            _currentCarController = value;

            carControl.Move.performed += val => _currentCarController.MoveOverride(val.ReadValue<float>());
            carControl.Move.canceled += _ => _currentCarController.MoveOverride(0);
            carControl.Speed.performed += val => _currentCarController.ShiftGear(val.ReadValue<float>());
        }
    }

    private bool _gameManagerCheck = false;

    private GameManager LocalGameManager {
        get {
            if (!_gameManagerCheck)
                _gameManager = GameManager.Instance;

            return _gameManager;
        }
    }

    private GameManager _gameManager;

    private bool _characterControllerCheck = false;

    private ThirdPersonController CharacterController {
        get => _characterController;

        set {
            _characterControllerCheck = true;

            _characterController = value;
        }
    }

    private void RightPlayerButtonPressed(float pressed) {
        RightButtonPressed = pressed > 0.1f;
        PlayerCharacterInput = pressed > 0.1f ? new Vector2(1, 0) : Vector2.zero;

        Debug.Log(pressed > 0.1f);
    }

    private void LeftPlayerButtonPressed(float pressed) {
        LeftButtonPressed = pressed > 0.1f;
        PlayerCharacterInput = pressed > 0.1f ? new Vector2(-1, 0) : Vector2.zero;

        Debug.Log(pressed > 0.1f);
    }

    private void PlayerSpeedControl(float value) {
        
        CharacterController.ChangeMovementSpeed(value > 0);
    }

    private void PauseInput() {
        LocalGameManager.Pause();
    }

    private void ResetInput() {
        //reload scene here

        SceneManager.LoadScene("MainMenu");
    }

    private void Awake() {
        if (GameObject.Find("Player") != null) {
            CharacterController = GameObject.Find("Player").GetComponent<ThirdPersonController>();
        }
    }

    private void OnEnable() {
        InputSystem.FlushDisconnectedDevices();

        int gamepadCount = Gamepad.all.Count;
        

        _menuHandling ??= new InputScheme();

        _playerControls ??= new InputScheme();
        _playerControls.devices = new InputDevice[] { gamepadCount > 0 ? Gamepad.all[0] : Keyboard.current };

        _carControls ??= new InputScheme();
        _carControls.devices = new InputDevice[]
            { gamepadCount > 1 ? Gamepad.all[gamepadCount - 1] : Keyboard.current };
        //_carControls.devices = new InputDevice[] { Gamepad.all[0] }; // for testing purposes


        var navigation = _menuHandling.Navigation;

        navigation.Pause.performed += _ => PauseInput();
        navigation.Reset.performed += _ => ResetInput();

        if (_characterControllerCheck) {
            var playerMovement = _playerControls.PlayerMovement;

            /*_playerControls.PlayerMovement.RightButton.performed +=
                val => RightPlayerButtonPressed(val.ReadValue<float>());
            _playerControls.PlayerMovement.LeftButton.performed +=
                val => LeftPlayerButtonPressed(val.ReadValue<float>());
            _playerControls.PlayerMovement.SpeedUp.performed += _ => _characterController.ChangeMovementSpeed(true);
            _playerControls.PlayerMovement.SpeedDown.performed += _ => _characterController.ChangeMovementSpeed(false);*/

            playerMovement.RightButton.performed += _ => RightPlayerButtonPressed(1);
            playerMovement.RightButton.canceled += _ => RightPlayerButtonPressed(0);

            playerMovement.LeftButton.performed += _ => LeftPlayerButtonPressed(1);
            playerMovement.LeftButton.canceled += _ => LeftPlayerButtonPressed(0);

            playerMovement.Speed.performed += val => PlayerSpeedControl(val.ReadValue<float>());
        }

        _playerControls.PlayerMovement.Enable();
        _carControls.CarControl.Enable();

        _menuHandling.Navigation.Enable();
    }

    private void OnDisable() {
        _playerControls.PlayerMovement.Disable();
        _carControls.CarControl.Disable();
        _menuHandling.Navigation.Disable();
    }
}