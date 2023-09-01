/*
 * Copyright (C) 2020-2023 Tilt Five, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : ControllableObject
{
    
    public List<AttackInfo> moveSet; //A list of Attacks the character can perform
    public JumpInfo jumpData; //Information on the characters jump ability
    public ProjectileInfo projectileData; //Information on the characters projectile ability

    //A set of VFX prefabs
    public GameObject FootStepPrefab;
    public GameObject LandingPrefab;
    public GameObject PoundVFX;
    public GameObject LightHitPrefab;
    public GameObject deathParticles;

    public GameObject shieldObject;

    //References to the characters model and animator
    public SkinnedMeshRenderer model;
    public Animator m_Animator;
    public bool SwapAttack;

    public MeshRenderer colorIndicator; //An indicator beneath the chaarcter's feet to show individual player color
    public GameObject boneRoot; //The gameobject root of the character model
    
    //Information on knockback and hitshake
    public int shakeCount = 5;
    public float hitShakeAmount = 0.1f;
    public float knockbackRatio = 1.0f;

    //Audio effects and audio source
    public AudioSource audioSource;
    public List<AudioClip> isHitSFX;
    public List<AudioClip> hitsSFX;
    
    //Status variables 
    public float myVelocity;
    public bool isGrounded;
    public float playerAltitude;
    public float altitudeThreshold = 0.2f;

    public float shieldStrength = 100;

    public Attack myAttack;
    public int progressAttack;

    Vector3 hitLocation; //not using yet, but want to spawn particles at point where colliders hit, not just the hitbox bone

    [System.Serializable]
    public class MoveInfo
    {
        public string attackName; //This should match the animation trigger
        public float attackDamage; //The damage the attack deals
        public float lockoutDuration; //How long the attack prevents new actions from executing for.
        public float baseKnockback; //The minimum knockback amount of the attack
        public float knockbackScaling; //The rat ethe knockback sacles at
        public List<AudioClip> attackSFX; //Sound effect for hit collision
        public string animationState;
    }

    [System.Serializable]
    public class AttackInfo : MoveInfo {
         
        public GameObject attackHitboxSpawn; //The bone the hitbox spawns on
        public float hitboxSize; //The radiuis of the hitbox sphere
        public Vector3 attackHitVector; //Direction modifier for the attack.
        public float movementReduction; //How the attack modifies movement during execution
        public float rotationMultiplier; //Scales rotation speed during attack, 0 can't turn at all, 1 is normal speed
        public string animationTrigger;
    }

    [System.Serializable]
    public class ProjectileInfo : MoveInfo
    {
        public GameObject spawnBone; //the gameobject location to spawn projectiles
        public float energyCost;
        public GameObject projectilePrefab;
    }

    [System.Serializable]
    public class JumpInfo
    {
        public float duration; //How long the jump-squat should lock out player inputs(in frames)
        public float jumpStrength; //The impulse strength to be applied on jump
        public int jumpCount; //The max number of jumps the character can make
        public string animationState;
    }

    //Get a specific Attack form the moveSet based on the attack's name
    public AttackInfo GetMove(string moveName)
    {
        List<AttackInfo> attacks = new List<AttackInfo>();
        foreach(AttackInfo info in moveSet)
        {
            if (info.attackName.Equals(moveName))
            {
                attacks.Add(info);
            }
        }
        if(attacks.Count == 0)
        {
            return null;
        }
        else
        {
            AttackInfo ret;
            if(progressAttack >= attacks.Count)
            {
                progressAttack = 0;
                ret =  attacks[progressAttack];
            }
            else
            {
                ret =  attacks[progressAttack];
            }
            progressAttack++;
            return ret;
        }
    }

    //A funciton to pause the character on successful hit for a specific frame duration
    public IEnumerator HitStop(float duration)
    {
        AudioClip hitSFX = hitsSFX[Random.Range(0, hitsSFX.Count)];
        audioSource.clip = hitSFX;
        audioSource.Play();
        m_Animator.speed = 0.01f;
        controller.currentEvent.duration += duration;
        yield return new WaitForSeconds(duration / 60);
        m_Animator.speed = 1.0f;
    }

    //A function to randomly vibrate the character for a specific frame duraiton when hit before launching them in a specific direction
    public IEnumerator HitShake(float duration, Vector3 launchDirection, MoveInfo info)
    {
        AudioClip hitSFX = isHitSFX[Random.Range(0, isHitSFX.Count)];
        audioSource.clip = hitSFX;
        audioSource.Play();
        m_Animator.speed = 0.01f;
        rb.isKinematic = true;
        Vector3 startingPos = gameObject.transform.position;
        for (int i = 0; i < shakeCount; i++)
        {
            gameObject.transform.position += Random.insideUnitSphere * hitShakeAmount;
            yield return new WaitForSeconds((duration / 60) / shakeCount);
        }
        gameObject.transform.position = startingPos;
        m_Animator.speed = 1.0f;
        rb.isKinematic = false;
        Launch(launchDirection, info);
    }

    //Apply attack info to the controlling player class as well as prevent new input for the hit character for a given duration
    public void applyHit(MoveInfo attackInfo)
    {
        controller.damagePercentage += attackInfo.attackDamage;
        m_Animator.SetTrigger("HitStunned");
        var stunCoroutine = controller.Stunned(((controller.damagePercentage / 10.0f) + (controller.damagePercentage * attackInfo.attackDamage / 50.0f)));
        controller.StartCoroutine(stunCoroutine);
    }

    //Apply a velocity change to launch characters in a given direction.
    public void Launch(Vector3 launchDirection, MoveInfo info)
    {
        var forceCalc = ((((((controller.damagePercentage / 10.0f) + (controller.damagePercentage * info.attackDamage / 20.0f)) * (200.0f / (rb.mass + 180.0f)) * 1.4f) + 18.0f) * info.knockbackScaling) + info.baseKnockback) * knockbackRatio * 0.03f;
        var forceVector = launchDirection * forceCalc;
        rb.AddForce(forceVector, ForceMode.VelocityChange);
    }

    new private void Start()
    {
        base.Start();
        this.inputMovement = new Vector2(0.0f, 0.0f);
        this.rb = gameObject.GetComponent<Rigidbody>();
        this.m_Animator = gameObject.GetComponent<Animator>();
        this.audioSource = gameObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        //Set animator values to manage rising, falling, and running animations.
        m_Animator.SetFloat("Altitude", playerAltitude);
        m_Animator.SetFloat("VerticalVelocity", rb.velocity.y);
        myVelocity = Mathf.Sqrt(Mathf.Pow(inputMovement.x, 2) + Mathf.Pow(inputMovement.y, 2));
        m_Animator.SetFloat("HorizontalVelocity", myVelocity);
        currentRotationSpeed = rotationSpeed * controller.rotationScale;

        RaycastHit hit;
        Ray downRay = new Ray(transform.position, -Vector3.up);

        if (Physics.Raycast(downRay, out hit))
        {
            playerAltitude = hit.distance;
            hitLocation = hit.point;

        }

        if (playerAltitude >= altitudeThreshold)
        {
            isGrounded = false;
        }
        else 
        { 
            isGrounded = true; 
        }

        m_Animator.SetBool("isGrounded", isGrounded);
        m_Animator.SetFloat("Altitude", playerAltitude);


        if (isGrounded)
        {
            controller.jumpCount = jumpData.jumpCount;
        }

        //Set the color Indicator for the character based on the controlling Player
        if (colorIndicator != null)
        {
            colorIndicator.transform.position = hitLocation + new Vector3(0,.05f,0);

            colorIndicator.material = mc.playerIndicatorColors[(int)controller.playerNumber - 1];
        }

        if(shieldStrength < 100.0f && !controller.isBlocking)
        {
            shieldStrength += 1.0f / 6.0f;
        }

        var shieldPercentage = shieldStrength / 100.0f;
        shieldObject.transform.localScale = new Vector3(shieldPercentage, shieldPercentage, shieldPercentage);
    }
 

    public void TriggerHitboxSpawn()
    {
        if (controller.currentEvent is Attack)
        {
            ((Attack)controller.currentEvent).SpawnCollider();
 
        }
    }

    public void TriggerHitboxDestroy()
    {
        if (controller.currentEvent is Attack)
        {
            ((Attack)controller.currentEvent).DestroyCollider();
           
        }
    }


    //Below are Functions called by Animation Events, which are under the Events tab of individual clips. Mostly for spawning particles, SFX, or alternating attacks
    public void RightFootDown() 
    {
        //If we're running, spawn footstep effects. Triggered by Aniamtor.
        if (myVelocity > 0.2f)
        {
            var myLocation = model.transform.position;
            var footstep = Instantiate(FootStepPrefab, myLocation, model.transform.rotation);
            Destroy(footstep, 1);
        }
    }

    public void LeftFootDown()
    {
        //If we're running, spawn footstep effects. Triggered by Aniamtor.
        if (myVelocity > 0.2f)
        {
            var myLocation = model.transform.position;
            var footstep = Instantiate(FootStepPrefab, myLocation, model.transform.rotation);
            Destroy(footstep, 1);
        }
    }

    //Notify the Aniamtor an erial attack ahs finished.
    public void AerialFinished() 
    {
        m_Animator.SetTrigger("AerialFinished");
    }

    //Spawn VFX. Triggered by Animator.
    public void LandVFX()
    { 
            var myLocation = model.transform.position;
            var footstep = Instantiate(LandingPrefab, myLocation, model.transform.rotation);
            Destroy(footstep, 1);
    }

    public void Pound()
    {
        var move = GetMove("HeavyAttack");
        var pound = Instantiate(PoundVFX, move.attackHitboxSpawn.transform.position, model.transform.rotation);
        Destroy(pound, 2);
    }


    public void AlternateAttack() 
    {
        SwapAttack = !SwapAttack;
        m_Animator.SetBool("SwapAttack",  SwapAttack);
    }


    //Handle collision logic below.
    public void OnTriggerEnter(Collider other)
    {
        //Destroy the character if they collide with the death planes
        if(other.gameObject.tag == "DeathPlane")
        {

            GameObject Center = GameObject.Find("Tilt Five Prefab");
           
            Vector3 lookAtRotation = Vector3.RotateTowards(transform.forward, transform.position, 100, 1000f);

            Instantiate(deathParticles, transform.position, Quaternion.LookRotation(-transform.position, Vector3.up));

            controller.KillCharacter();
        }
        //Don't allow chain stunning
        else if(controller.currentActionState != PlayerController.ActionState.HitStunned)
        {
            Character attacker;
            var projectileInfo = other.gameObject.GetComponent<ProjectileMovement>();
            if (projectileInfo != null)
            {
                attacker = other.gameObject.GetComponent<ProjectileMovement>().charInfo;
                if (attacker.controller.playerNumber != this.controller.playerNumber)
                {
                    var info = other.gameObject.GetComponent<ProjectileMovement>().projInfo;
                    if (!controller.isBlocking)
                    {
                        //Update the controller
                        applyHit(info);
                    }
                    //Play a hit FX
                    var lightHitVFX = Instantiate(LightHitPrefab, other.gameObject.transform.position, other.gameObject.transform.rotation);
                    //Calculate the launch directoin based on the attack and the target.
                    var dir = (gameObject.transform.position - attacker.gameObject.transform.position).normalized;
                    //Apply hitstop to the attacker
                    var hitstopCoroutine = other.gameObject.GetComponentInParent<Character>().HitStop(10);
                    attacker.StartCoroutine(hitstopCoroutine);
                    //Apply hitshake to the target
                    if (!controller.isBlocking)
                    {
                        var shakeCoroutine = HitShake(10, dir, info);//This currently defines the duration in frames of the hitshake and the direction they will launch after the shake
                        StartCoroutine(shakeCoroutine);
                    }
                    projectileInfo.Destroy();
                }
            }
            //Get the information of the attack  from the attacker
            attacker = other.gameObject.GetComponentInParent<Character>();
            if (attacker && attacker.controller.currentActionState == PlayerController.ActionState.Attacking)
            {
                var attackEvent = (Attack)attacker.controller.currentEvent;
                //Don't allow attacks to collide multiple times per animation.
                if (!attackEvent.hasHit)
                {
                    attackEvent.hasHit = true;
                    var attackInfo = attackEvent.moveData;
                    if (!controller.isBlocking)
                    {
                        //Update the controller
                        applyHit(attackInfo);
                    }
                    
                    //Play a hit FX
                    var lightHitVFX = Instantiate(LightHitPrefab, attackInfo.attackHitboxSpawn.transform.position, attackInfo.attackHitboxSpawn.transform.rotation);
                    //Calculate the launch directoin based on the attack and the target.
                    var dir = (gameObject.transform.position - attacker.gameObject.transform.position).normalized;
                    //Apply attack hit modifier vector.
                    dir += attackInfo.attackHitVector;
                    //Apply hitstop to the attacker
                    var hitstopCoroutine = other.gameObject.GetComponentInParent<Character>().HitStop(10);
                    attacker.StartCoroutine(hitstopCoroutine);
                    //Apply hitshake to the target
                    if (!controller.isBlocking)
                    {
                        var shakeCoroutine = HitShake(10, dir, attackInfo);//This currently defines the duration in frames of the hitshake and the direction they will launch after the shake
                        StartCoroutine(shakeCoroutine);
                    }
                }
            }
        }
    }

}
