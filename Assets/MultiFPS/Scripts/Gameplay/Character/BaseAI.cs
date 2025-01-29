using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using System.Linq;
using Mirror;
namespace MultiFPS.Gameplay
{
    /// <summary>
    /// AI that controls bots
    /// </summary>
    public class BaseAI : NetworkBehaviour
    {
        protected Health targetedEnemy;
        protected CharacterInstance _characterInstance;

        public float RotationToTargetSpeed = 350f;
        private Coroutine triggerBurst;

        bool pushTrigger = false;

        public float fireTime = 1.55f;
        public float waitForFireTime = 0.75f; //gap between firing

        public bool SwitchItemsRandomly = false;
        private void Awake()
        {
            _characterInstance = GetComponent<CharacterInstance>();

            enabled = false;
            _characterInstance.Server_HealthDepleted += OnDeath;
            _characterInstance.Server_SetAsBOT += SetAsBot;

            _path = new NavMeshPath();
        }

        void SetAsBot(bool _set) 
        {
            StopAllCoroutines();

            if (_set)
            {
                if(SwitchItemsRandomly)
                    StartCoroutine(TakeRandomItemCoroutine());
            }
            else 
            {
            }

            IEnumerator TakeRandomItemCoroutine() 
            {
                while (true) 
                {

                    yield return new WaitForSeconds(UnityEngine.Random.Range(6f, 18f));
                    TakeRandomItem();
                }
            }

            enabled = _set;
        }

        void TakeRandomItem() 
        {
            bool success = false;
            int randomSlot = UnityEngine.Random.Range(0, _characterInstance.CharacterItemManager.Slots.Count);
            int startingSlot = randomSlot;
            while (!success)
            {

                if (_characterInstance.CharacterItemManager.Slots[randomSlot].Item &&
                    _characterInstance.CharacterItemManager.Slots[randomSlot].Item.CurrentAmmoSupply > 0)
                {

                    _characterInstance.CharacterItemManager.ServerCommandTakeItem(randomSlot);
                    success = true;
                }
                else
                {
                    randomSlot++;
                    if (randomSlot >= _characterInstance.CharacterItemManager.Slots.Count) randomSlot = 0;
                }

                if (randomSlot == startingSlot) break;
            }
        }
        private void Update()
        {
            MovementTick();
        }
        void OnDeath(byte _hittedPartID, AttackType attackType, uint _attackerID, int attackForce) 
        {
            enabled = false;
        }

        bool stepAside = true;
        Coroutine stepAsideCoolDownCounter;
        [SerializeField] float DistanceFromTarget = 34f;

        void FixedUpdate()
        {
            if (!isServer) return;

            //if item is out of ammo change it to something else
            if (_characterInstance.CharacterItemManager.CurrentlyUsedItem 
                && _characterInstance.CharacterItemManager.CurrentlyUsedItem.CurrentAmmoSupply <= 0)
                TakeRandomItem();

            if (GameManager.Gamemode.PeacefulBots) 
            {
                _characterInstance.movementInput = Vector2.zero;
                return;
            }

            bool enemyInSight = EnemyInSight();

            //AI LOGIC
            if(!enemyInSight || targetedEnemy && targetedEnemy.CurrentHealth <= 0)
                targetedEnemy = GetClosestEnemy();


            //If there is someone to attack
            if (targetedEnemy)
            {
                float distanceFromEnemy = Vector3.Distance(transform.position, targetedEnemy.transform.position);

                if (pushTrigger)
                {
                    if (distanceFromEnemy < 2f)
                        if(_characterInstance.CharacterItemManager.CurrentlyUsedItem)
                            _characterInstance.CharacterItemManager.CurrentlyUsedItem.PushMeele();

                    _characterInstance.CharacterItemManager.Fire1();
                }


                if (enemyInSight)
                {
                    _characterInstance.SetActionKeyCode(ActionCodes.Sprint, false);
                    SetBurst(true);

                    Vector3 newFreeVector;

                    if (distanceFromEnemy < DistanceFromTarget)
                    {
                        if (stepAside) //decision if ai wants to increase distance of target and if its able to                                                                //    if (distance < minDistanceFromTarget && !increasingDistanceFromTarget) //decision if ai wants to increase distance of target and if its able to
                        {
                            //setting cooldown to prevent enemy from moving all the time
                            stepAside = false;
                            if (stepAsideCoolDownCounter != null) //should never occur
                            {
                                StopCoroutine(stepAsideCoolDownCounter);
                            }
                            stepAsideCoolDownCounter = StartCoroutine(stepAsideCoolDown());

                            Vector3[] increasingDistancePossibleDirections = { -transform.right, transform.right, -transform.forward};
                            newFreeVector = freeSpace(increasingDistancePossibleDirections, 8f);
                            if (Vector3.Distance(newFreeVector, transform.position) > 1.5f)
                                SetTravelDestinationByNavMesh(newFreeVector, false);
                        }
                    }
                    else
                    {
                        stepAside = true;
                        SetTravelDestinationByNavMesh(targetedEnemy.transform.position, false);
                    }

                    LookAt(targetedEnemy.GetPositionToAttack());

                }
                else
                {
                    _characterInstance.SetActionKeyCode(ActionCodes.Crouch, false);
                    _characterInstance.SetActionKeyCode(ActionCodes.Sprint, distanceFromEnemy > 2f);
                    //if enemy is not in sight then do not shot and go to nearest enemy position
                    SetBurst(false);

                    SetTravelDestinationByNavMesh(targetedEnemy.transform.position, true);
                }
            }
            else
            {
                //if there is no enemy to attack than simply do nothing
                SetBurst(false);
                SetTravelDestinationByNavMesh(transform.position, false);
            }
        }

        IEnumerator stepAsideCoolDown()
        {
            yield return new WaitForSeconds(2f);
            stepAside = true;
        }

        protected Vector3 freeSpace(Vector3[] direction, float range)
        {
            //   Debug.Log("checked for free space");
            Vector3 bestVector = transform.position;
            for (int i = 0; i < direction.Length; i++)
            {
                Ray rayCheck = new Ray(transform.position + transform.up * 1f, direction[i]);
                RaycastHit freePoint;
                if (Physics.Raycast(rayCheck, out freePoint, range, GameManager.environmentLayer)) //if we hit something
                {
                    if (Vector3.Distance(transform.position, freePoint.point) > Vector3.Distance(transform.position, bestVector) && freePoint.collider.gameObject.layer != 13) //avoiding this layer to make bots not stucking on each other
                    {
                        bestVector = freePoint.point;
                    }
                }
                else
                {
                    float difference = Vector3.Distance(transform.position, transform.position + direction[i] * range) - Vector3.Distance(transform.position, bestVector);
                    if (Mathf.Abs(difference) < 2f)
                    {
                        if (UnityEngine.Random.Range(0, 3) == 1) bestVector = transform.position + direction[i] * range;
                    }
                    else if (difference > 0)
                    {
                        bestVector = transform.position + direction[i] * range;
                    }
                }
            }
            return bestVector;
        }

        /// <summary>
        /// coroutine for pulling trigger
        /// it makes bots not shoot all the time when they have enemy in range
        /// </summary>
        /// 

        bool shooting = false;
        private void SetBurst(bool _start)
        {
            if (_start == shooting) return;

            shooting = _start;



            if (triggerBurst != null)
            {
                StopCoroutine(triggerBurst);
                triggerBurst = null;
            }

            if (_start)
            {
                triggerBurst = StartCoroutine(c_pushTrigger());
            }
            pushTrigger = false;

            IEnumerator c_pushTrigger()
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 1f));
                while (true)
                {
                    if (!pushTrigger)
                    {
                        pushTrigger = true;
                        yield return new WaitForSeconds(fireTime);
                    }
                    else
                    {
                        pushTrigger = false;
                        int crouch = UnityEngine.Random.Range(0, 4);
                        _characterInstance.SetActionKeyCode(ActionCodes.Crouch, crouch == 0);
                        yield return new WaitForSeconds(waitForFireTime);
                    }
                }
            }
        }
        Quaternion mindRotation;
        //makes bot look at given spot
        protected void LookAt(Vector3 _lookTarget)
        {
            _characterInstance.characterMind.position = _characterInstance.GetPositionToAttack();

            mindRotation = Quaternion.Lerp(mindRotation, Quaternion.LookRotation(_lookTarget - _characterInstance.characterMind.position), RotationToTargetSpeed * Time.fixedDeltaTime);

            _characterInstance.lookInput.y = mindRotation.eulerAngles.y;

            //look up/down correction
            float lookx = -mindRotation.eulerAngles.x;
            float fixedLookX = lookx < -90 ? lookx += 360 : lookx;
            _characterInstance.lookInput.x = -fixedLookX;
        }

        /// <summary>
        /// Can we see nearest enemy or there is something beetwen us?
        /// </summary>
        protected bool EnemyInSight()
        {
            //TODO: just make raycast beetwen those two and check if there is something between them
            if (!targetedEnemy) return false;

            Vector3 direction = targetedEnemy.GetPositionToAttack() - _characterInstance.FPPLook.position;
            return !Physics.Raycast(_characterInstance.FPPLook.position, direction, direction.magnitude,GameManager.environmentLayer);
        }


        #region navigation tools

        bool followingPath;
        Vector3 desiredVelocity;
        Vector3 directTravelTargetPoint;
        NavMeshPath _path;
        Coroutine c_pathChecker;
        protected virtual void MovementTick()
        {
            if (followingPath)
            {
                desiredVelocity = transform.InverseTransformDirection(directTravelTargetPoint - transform.position).normalized;
            }
            else
            {
                desiredVelocity = Vector3.zero;
            }

            if (!_characterInstance.Block) //dont move bot if he is blocked
            {
                _characterInstance.movementInput.x = Mathf.Clamp(desiredVelocity.x, -1.0f, 1.0f);
                _characterInstance.movementInput.y = Mathf.Clamp(desiredVelocity.z, -1.0f, 1.0f);
            }
            else 
            {
                _characterInstance.movementInput = Vector2.zero;
            }
        }

        //executing this method will make bot travel to given destination. It can be stopped by giving him new destination or 
        //simply by reaching given target at some point in time

        float lastDistanceFromDestination;

        protected void SetTravelDestinationByNavMesh(Vector3 _destrinationPosition, bool _rotateToDestination)
        {
            NavMesh.CalculatePath(transform.position, _destrinationPosition, NavMesh.AllAreas, _path);

            if (_path.corners.Length > 1) //checking if path is not very close to player
            {
                if (_path.status == NavMeshPathStatus.PathPartial)
                {
                    return;
                }
                followingPath = true; //needed to tell animator when to stop movement animations
                directTravelTargetPoint = _path.corners[1];
                if (_rotateToDestination)
                    LookAt(_path.corners[1] + new Vector3(0, _characterInstance.CameraHeight-0.2f, 0)); ;
            }
            else
            {
                return;
            }

            int cornerId = 1;

            if (c_pathChecker != null)
            {
                StopCoroutine(c_pathChecker);
                c_pathChecker = null;
            }

            c_pathChecker = StartCoroutine(PathChecker());
            IEnumerator PathChecker()  //moving beetwen corners
            {
            Loop:
                while (cornerId < _path.corners.Length)
                {
                    float distanceFromCurrentCorner = Vector3.Distance(transform.position, _path.corners[cornerId]);

                    //jump if got stuck, maybe will unstuck
                    if (!_characterInstance.Block && Mathf.Abs(distanceFromCurrentCorner - lastDistanceFromDestination) < 0.000000005f)
                        _characterInstance.GetComponent<CharacterMotor>().Jump();

                    lastDistanceFromDestination = distanceFromCurrentCorner;

                    if (distanceFromCurrentCorner <= 0.15f)
                    {
                        // transform.position = _path.corners[cornerId]; //teleport character at the end of the route to certaint point to exacly match destination and avoid getting stuck
                        cornerId++;
                        if (cornerId < _path.corners.Length)
                        {
                            directTravelTargetPoint = _path.corners[cornerId];
                            goto Loop;
                        }
                        else break;
                    }
                    NavMesh.CalculatePath(transform.position, _destrinationPosition, NavMesh.AllAreas, _path);
                    yield return new WaitForSeconds(1f);
                }
                followingPath = false;
            }
        }
        #endregion

        /// <summary>
        /// find nearest enemy. Enemy is everyone that is not in our team, or litarally everyone on the map when checkbox "FFA" is true in current 
        /// Gamemode
        /// </summary>
        protected Health GetClosestEnemy()
        {
            Health closestEnemy = null;
            if (targetedEnemy && targetedEnemy.CurrentHealth <= 0) targetedEnemy = null; //if current target is dead we no longer see him

            float lastDistance = float.MaxValue;

            for (int characterUnit = 0; characterUnit < CustomSceneManager.spawnedCharacters.Count; characterUnit++)
            {
                Health potentialEnemy = CustomSceneManager.spawnedCharacters[characterUnit];

                if (potentialEnemy.Team == -1) continue; //ignore characters without team

                if ((potentialEnemy.Team != _characterInstance.Team || GameManager.Gamemode.FFA) && potentialEnemy != this._characterInstance && potentialEnemy.CurrentHealth > 0)
                {
                    float distance = Vector3.Distance(transform.position, potentialEnemy.transform.position);
                    if (distance < lastDistance)
                    {
                        closestEnemy = potentialEnemy;
                        lastDistance = distance;
                    }
                }
            }
            return closestEnemy;
        }
    }
}
