using System;
using Level;
using UnityEngine;

namespace Car {
    [RequireComponent(typeof(Rigidbody))]
    public abstract class CarBehaviour : MonoBehaviour, IDamager {
        protected Rigidbody Rb;

        private WayPoint[] _wayPoints;

        protected Vector3 MoveDirection;

        [SerializeField] protected float turnSpeed, baseSpeed, addedSpeed, acceleration;

        public int Damage { get; set; } = 100;
        private byte _currentGear = 0;
        private const byte MaxGear = 3;
        public int UpwardsModifier = 10;
        public int RagdollMultiplier = 4;

        private float _currentMaxSpeed;

        private float _timeAlive;

        public float GetTimeAlive() {
            return _timeAlive;
        }

        public Renderer GetRenderer() {
            if (gameObject.GetComponentInChildren<Renderer>()) {
                return gameObject.GetComponentInChildren<Renderer>();
            }

            return null;
        }


        protected virtual void Start() {
            SetGear(MaxGear / 2);
            _currentWayPoint = 1;
            Rb = GetComponent<Rigidbody>();
        }

        protected virtual void Update() {
            if (_moveOverrideTimer > 0) {
                _moveOverrideTimer -= Time.deltaTime;
                if (_moveOverrideTimer < 0) {
                    _moveOverrideTimer = 0;
                }
            }

            int wayPoint = GetCurrentWayPoint();

            MoveDirection = CalculateDriveDirection(wayPoint);

            DriveLoop();
        }

        public void SetWayPointData(WayPoint[] newData) {
            _wayPoints = newData;

            _moveSign = Mathf.Sign((_wayPoints[0].Position - _wayPoints[^1].Position).x) > 0;
        }

        public void ReplaceCurrentWayPoint(WayPoint newPoint) {
            _wayPoints[_currentWayPoint] = newPoint;
        }

        protected Vector3 CalculateDriveDirection(int waypointIndex, bool flattenVector = true) {
            Vector3 direction = (_wayPoints[waypointIndex].Position - Rb.position);

            if (flattenVector)
                direction.y = 0;

            return direction.normalized;
        }

        private Quaternion _playerMove;
        private float _moveOverrideTimer = 0;

        public void MoveOverride(float input) //add invertcontrol 
        {
            float dir = MathHelper.ZeroSign(input);

            _moveOverrideTimer = dir == 0 ? 1 : -1;

            _playerMove = Quaternion.Euler(0, dir * turnSpeed * (_moveSign ? 1 : -1), 0);
        }

        public void ShiftGear(float input) => SetGear(Mathf.Sign(input) < 0 ? _currentGear - 1 : _currentGear + 1);

        private void SetGear(int index) {
            index = Math.Clamp(index, 0, MaxGear);

            _currentGear = (byte)index;

            _currentMaxSpeed = baseSpeed + addedSpeed * _currentGear;
        }

        private int _currentWayPoint = 0;

        private bool _moveSign;

        protected int GetCurrentWayPoint() {
            var i = _currentWayPoint;

            float xDiff = (_wayPoints[i].Position - Rb.position).x;

            bool passed = _moveSign ? xDiff > 0 : xDiff < 0;

            if (passed && i + 1 < _wayPoints.Length)
                _currentWayPoint++;
            else if (i + 1 >= _wayPoints.Length)
                DeSpawn();

            return _currentWayPoint;
        }

        private Vector3 _currentMoveDir;
        private float _currentMoveSpeed;

        protected virtual void DriveLoop() {
            if (_moveOverrideTimer == 0)
                _currentMoveDir = Vector3.MoveTowards(_currentMoveDir, MoveDirection, turnSpeed * Time.deltaTime);
            else
                _currentMoveDir = Vector3.MoveTowards(_currentMoveDir, _playerMove * _currentMoveDir, Time.deltaTime);

            if (_currentMoveSpeed > _currentMaxSpeed) {
                _currentMoveSpeed = Mathf.MoveTowards(_currentMoveSpeed, _currentMaxSpeed, Time.deltaTime);
            }
            else _currentMoveSpeed += _currentMaxSpeed * acceleration * (Time.deltaTime);

            transform.forward = _currentMoveDir;

            Rb.velocity = _currentMoveDir * _currentMoveSpeed;
        }
        //todo avoid Objects????

        private void DeSpawn() {
            GameManager.Instance.RemoveCar(this);
            Destroy(gameObject);
        }


        private void OnCollisionEnter(Collision collision) {
            if (collision.gameObject.CompareTag("Player")) {
                var contact = collision.GetContact(0);
                var playerRB = collision.gameObject.GetComponent<Rigidbody>();
                playerRB.constraints = RigidbodyConstraints.None;
                playerRB.AddForceAtPosition(
                    (Rb.velocity + playerRB.velocity + (Vector3.up * UpwardsModifier)).normalized * RagdollMultiplier *
                    Rb.mass, contact.point, ForceMode.Impulse);
                collision.gameObject.GetComponent<ThirdPersonController>().TakeDamage(Damage);
            }
        }
    }
}