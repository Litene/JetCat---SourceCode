using System;
using System.Collections;
using System.Collections.Generic;
using Car;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Level {
    public class GameManager : MonoBehaviour, ICameraListener //very temporary
    {
        private static GameManager _instance;

        public static GameManager Instance {
            get => _instance;
        }

        public float _currentFade;
        private Fade _fade;

        private ThirdPersonController _playerController;
        private Transform _cameraTransform;
        private const float _zOffset = 30f; 
        private void Awake() {
            if (_instance != null) {
                gameObject.SetActive(false);
                return;
            }

            _instance = this;

            FindAllLevels();
            _arrow = FindObjectOfType<ArrowScript>();
            /*_rectMask = FindObjectOfType<RectMask2D>();
            if (_rectMask != null) {
                _currentFade = _rectMask.softness.y;
            }*/

            _fade = FindObjectOfType<Fade>();

            if (FindObjectOfType<ThirdPersonController>()) {
                _playerController = FindObjectOfType<ThirdPersonController>();
            }

            _cameraTransform = Camera.main.transform;
            
            //Controller = FindObjectOfType<CameraController>();
            //Controller.AddListener(this);
        }

        public List<CarBehaviour> AvailableCars;


        private void OnDestroy() {
            if (_instance != this) return;
            _instance = null;
        }

        private LevelManager[] _availableLevels;

        private void FindAllLevels() {
            _availableLevels = FindObjectsOfType<LevelManager>();

            foreach (var level in _availableLevels) //very lazy approach
            {
                level.gameObject.SetActive(false);
            }

            /*int length = _availableLevels.Length;

            for (int i = 1; i < length; ++i) //just insertion sort
            {
                int index = _availableLevels[i].GetLevelIndex();
                int j = i - 1;

                while (j >= 0 && _availableLevels[j].GetLevelIndex() > index)
                {
                    _availableLevels[j + 1] = _availableLevels[j];
                    j -= 1;
                }
                
                _availableLevels[j + 1] = _availableLevels[i];
            }*/
        }

        private int _currentLevel;
        private LevelManager _currentLevelManager;
        private LevelRules _currentRules;

        private int _currentCarIndex = 0;
        private List<CarBehaviour> _cars = new List<CarBehaviour>();
        private CarBehaviour _currentCar;
        private ArrowScript _arrow;

        public CarBehaviour CurrentCar {
            get => _currentCar;
            set {
                if (value != _currentCar) {
                    _arrow.SetCurrentCar(value);
                }

                _currentCar = value;
            }
        }

        private void Start() {
            if (_fade != null)
                _currentFade = _fade.GetFade();

            StartCoroutine(Fade(1));

            SetLevel(0);

            _spawnTimer = _currentRules.GetNextSpawnTime();

            InvokeRepeating("UpdateAvailableCars", 1, 0.2f);
        }

        private void UpdateAvailableCars() {
            var normalCars = FindObjectsOfType<CarBehaviour>().ToList();
            List<CarBehaviour> carsToRemove = new List<CarBehaviour>();
            foreach (var car in normalCars) {
                if (car.GetRenderer() == null)
                    continue;
                if (!car.GetRenderer().isVisible) {
                    carsToRemove.Add(car);
                }

                if (Mathf.Abs(car.transform.position.z - _playerController.transform.position.z) > _zOffset) {
                    carsToRemove.Add(car);
                }
                
                
                // var projection = Vector3.Project(Vector3.up, _cameraTransform.forward).normalized;
                // var fordwardDistance = Vector3.Dot(projection, car.transform.position - _playerController.transform.position);
                // if (fordwardDistance > _zOffset) {
                //     carsToRemove.Add(car);
                // }
                //car.transform.position

            }
            
            
            
            
            foreach (var car in carsToRemove) {
                normalCars.Remove(car);
            }

            AvailableCars = normalCars.OrderByDescending(car => car.GetTimeAlive()).ToList();
        }

        private float _spawnTimer = 0;

        private void Update() {
            _spawnTimer -= Time.deltaTime;

            if (_spawnTimer < 0) {
                CreateCar();
                _spawnTimer = _currentRules.GetNextSpawnTime();
            }
        }

        private int _lastSpawn = 0;

        void CreateCar() {
            CarSpawn spawn;
            if (_currentRules.CarSpawns.Length > 1) {
                int spawnIndex;
                do {
                    MathHelper.GetRandomIndex(_currentRules.CarSpawns, out spawnIndex);
                    if (spawnIndex == _lastSpawn) continue;
                    _lastSpawn = spawnIndex;
                    break;
                } while (true);

                spawn = _currentRules.CarSpawns[spawnIndex];
            }
            else {
                spawn = _currentRules.GetRandomSpawn();
            }

            CarBehaviour newCar = spawn.SpawnCar(_currentRules.GetRandomCar());
            _cars.Add(newCar);

            if (CurrentCar == null) {
                CurrentCar = newCar;
                InputManager.Instance.CurrentCar = _currentCar;
            }
        }

        public void SetRules(LevelRules rules) {
            _currentRules = rules;
        }

        public void RemoveCar(CarBehaviour car) {
            CarBehaviour currentCar = _currentCar;

            _cars.Remove(car);

            if (currentCar == car)
                SetCurrentCar(); // automatic, should not pick att random
        }

        void SetCurrentCar(int index = -1) {
            int newIndex = index;

            if (index == -1)
                MathHelper.GetRandomIndex(_cars.ToArray(), out newIndex);

            if (AvailableCars.Count == 0) {
                return;
            }

            if (AvailableCars[0] != null) {
                CurrentCar = AvailableCars[0]; // check if it selects the first one each time
                InputManager.Instance.CurrentCar = _currentCar;
            }
        }

        public void EndLevel() {
            // change to camerapan next milestone.
            //Controller.PanCamera();
            GameOver();
        }

        void NextLevel() {
            _currentLevelManager.gameObject.SetActive(false);

            _currentLevel = (_currentLevel + 1) % _availableLevels.Length;

            SetLevel(_currentLevel);
        }

        void SetLevel(int index) {
            _currentLevelManager = _availableLevels.FirstOrDefault(level => level.GetLevelIndex() == index);
            _currentLevel = index;
            if (_currentLevelManager == null) return;
            _currentRules = _currentLevelManager.GetRules();

            _currentLevelManager.gameObject.SetActive(true);
        }

        public void UpdateLevel() {
            NextLevel();
        }

        [SerializeField] private UnityEvent OnGameOver;

        private bool _gameOver = false;

        public void GameOver() {
            // timer och gameoverscene
            OnGameOver.Invoke();
            _gameOver = true;
            FadeSequence();
        }

        private void OnEnable() {
        }

        public void Pause() {
            // refactor make nicer till next milestone. 

            if (_gameOver) {
                SceneManager.LoadScene("GameOver");
                return;
            }

            Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        }

        public void FadeSequence(float currentFade = -1) {
            if (currentFade == -1) {
                StartCoroutine(Fade(_currentFade));
            }
            else {
                StartCoroutine(Fade(currentFade));
            }
        }

        private IEnumerator Fade(float currentFade) {
            Fading = true;
            WaitForEndOfFrame frameskip = new WaitForEndOfFrame();
            var Target = currentFade == 0 ? 1 : 0;

            float timer = 0;
            while (timer < 1) {
                timer += .5f * Time.deltaTime;
                currentFade = Mathf.Lerp(currentFade, Target, timer);
                _fade.SetFade(currentFade);
                //_rectMask.softness = new Vector2Int(0, currentFade);
                yield return frameskip;
            }

            currentFade = Target;
            /*if(Target == FadeIn)
                _rectMask.softness = new Vector2Int(0, 20000);
            else
                _rectMask.softness = new Vector2Int(0, currentFade);*/
            _fade.SetFade(currentFade);

            Fading = false;

            if (_gameOver) SceneManager.LoadScene("GameOver");
        }

        public bool Fading { get; set; }


        public UnityEvent OnGameOver1 => OnGameOver;

        public CameraController Controller { get; set; }
    }
}