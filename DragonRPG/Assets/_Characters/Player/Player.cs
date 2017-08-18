﻿using UnityEngine;
using UnityEngine.Assertions;
using _Camera;
using _Characters.SpecialAbilities;
using _Core;
// TODO: Consider rewiring
using _Levels;
using _Weapons;


namespace _Characters
{
    public class Player : MonoBehaviour, IDamageable
    {

        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _damage = 10f;
        [SerializeField] private Weapon _weaponInUse;
        [SerializeField] private AnimatorOverrideController _animatorOverrideController;
        [SerializeField] private SpecialAbilityConfig _specialAbility1;

        private float _currentHealth;
        private float _lastHitTime;
        private GameObject _currentTarget;
        private CameraRaycaster _cameraRaycaster;
        private Animator _animator;

        void Start()
        {
            RegisterForMouseClick();
            SetCurrentHealthToMax();
            PutWeaponInHand();
            SetAnimator();
            _specialAbility1.AddComponent(gameObject);
        }

        public float HealthAsPercentage
        {
            get { return _currentHealth / _maxHealth; }
        }

        public void TakeDamage(float damage)
        {
            _currentHealth = Mathf.Clamp(_currentHealth - damage, 0f, _maxHealth);
        }

        private void SetCurrentHealthToMax()
        {
            _currentHealth = _maxHealth;
        }

        private void SetAnimator()
        {
            _animator = GetComponent<Animator>();
            _animator.runtimeAnimatorController = _animatorOverrideController;
            _animatorOverrideController["DEAFAULT ATTACK"] = _weaponInUse.GetAttackAnimationClip();
        }

        private void PutWeaponInHand()
        {
            var weaponPrefab = _weaponInUse.GetWeaponPrefab();
            var dominantHand = RequestDominantHand();
            var spawnedWeapon = Instantiate(weaponPrefab, dominantHand.transform);

            spawnedWeapon.transform.localPosition = _weaponInUse.gripTransform.localPosition;
            spawnedWeapon.transform.localRotation = _weaponInUse.gripTransform.localRotation;
        }

        private GameObject RequestDominantHand()
        {
            var dominantHands = GetComponentsInChildren<DominantHand>();
            int dominantHandsCount = dominantHands.Length;

            Assert.AreNotEqual(0, dominantHandsCount, "No dominant hand for player.");
            Assert.IsFalse(dominantHandsCount > 1, "Multiple dominant hands for player.");

            return dominantHands[0].gameObject;
        }

        private void RegisterForMouseClick()
        {
            _cameraRaycaster = FindObjectOfType<CameraRaycaster>();
            _cameraRaycaster.onMouseOverEnemy += OnMouseOverEnemy;
        }

        private void OnMouseOverEnemy(Enemy enemy)
        {
            if (Input.GetMouseButton(0) && IsTargetInRange(enemy.gameObject))
            {
                AttackTarget(enemy.gameObject);
            }

            if (Input.GetMouseButtonDown(1))
            {
                AttemptSpecialAbility1();
            }
        }

        private void AttemptSpecialAbility1()
        {
            var energyComponent = GetComponent<Energy>();
            //TODO: Get energy amount from scriptable obj
            float energyAmount = 10f;
            if (energyComponent.IsEnergyAvailable(energyAmount))
            {
                energyComponent.ProcessEnergy(energyAmount);
                //TODO: Use ability
            }
        }

        private bool IsTargetInRange(GameObject target)
        {
            var distanceToTarget = (target.transform.position - transform.position).magnitude;
            return distanceToTarget <= _weaponInUse.GetMaxAttackRange();
        }

        private void AttackTarget(GameObject enemy)
        {
            _currentTarget = enemy;

            if (Time.time - _lastHitTime > _weaponInUse.GetAttackCooldown())
            {
                _animator.SetTrigger("AttackTrigger");
                _currentTarget.GetComponent<Enemy>().TakeDamage(_damage);
                _lastHitTime = Time.time;
            }
        }
    }
}
