﻿using UnityEngine;
using UnityEngine.AI;
using _Camera;
using _Characters.Enemies;

namespace _Characters.Player
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CameraRaycaster))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    public class CharacterMovement : MonoBehaviour
    {
        [SerializeField] private float _stoppingDistance;
        [SerializeField] private float _moveSpeedMultiplier;
        [SerializeField] private float _movingTurnSpeed = 360;
        [SerializeField] private float _stationaryTurnSpeed = 180;
        [SerializeField] private float _moveThreshold = 1f;
        [SerializeField] private float _animationSpeedMultiplier = 1.5f;

        private NavMeshAgent _navMeshAgent;
        private Animator _animator;
        private Rigidbody _rigidbody;

        private float _turnAmount;
        private float _forwardAmount;

        private void Start ()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            SetCameraRaycaster();
            SetNavMeshAgent();
        }

        private void Update()
        {
            Move(_navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance
                    ? _navMeshAgent.desiredVelocity
                    : Vector3.zero);
        }

        private void SetCameraRaycaster()
        {
            var cameraRaycaster = Camera.main.GetComponent<CameraRaycaster>();
            cameraRaycaster.onMouseOverTerrain += ProcessMouseOverTerrain;
            cameraRaycaster.onMouseOverEnemy += MoveToEnemy;
        }

        private void SetNavMeshAgent()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _navMeshAgent.updatePosition = true;
            _navMeshAgent.updateRotation = false;
            _navMeshAgent.stoppingDistance = _stoppingDistance;
        }

        private void ProcessMouseOverTerrain(Vector3 destination)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _navMeshAgent.SetDestination(destination);
            }
        }

        private void MoveToEnemy(Enemy enemy)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(1))
            {
                _navMeshAgent.SetDestination(enemy.transform.position);
            }
        }
        public void OnAnimatorMove()
        {
            // we implement this function to override the default root motion.
            // this allows us to modify the positional speed before it's applied.
            if (Time.deltaTime > 0)
            {
                var velocity = (_animator.deltaPosition * _moveSpeedMultiplier) / Time.deltaTime;

                // we preserve the existing y part of the current velocity.
                velocity.y = _rigidbody.velocity.y;
                _rigidbody.velocity = velocity;
            }
        }

        public void Move(Vector3 movement)
        {
            SetForwardAndTurn(movement);
            ApplyExtraTurnRotation();
            UpdateAnimator();
        }

        private void SetForwardAndTurn(Vector3 movement)
        {
            // convert the world relative moveInput vector into a local-relative
            // turn amount and forward amount required to head in the desired direction.
            if (movement.magnitude > _moveThreshold)
            {
                movement.Normalize();
            }
            var localMovement = transform.InverseTransformDirection(movement);
            _turnAmount = Mathf.Atan2(localMovement.x, localMovement.z);
            _forwardAmount = localMovement.z;
        }

        private void ApplyExtraTurnRotation()
        {
            // help the character turn faster (this is in addition to root rotation in the animation)
            float turnSpeed = Mathf.Lerp(_stationaryTurnSpeed, _movingTurnSpeed, _forwardAmount);
            transform.Rotate(0, _turnAmount * turnSpeed * Time.deltaTime, 0);
        }

        private void UpdateAnimator()
        {
            _animator.SetFloat("Forward", _forwardAmount, 0.1f, Time.deltaTime);
            _animator.SetFloat("Turn", _turnAmount, 0.1f, Time.deltaTime);
            _animator.speed = _animationSpeedMultiplier;
        }
    }
}