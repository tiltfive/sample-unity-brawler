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

public class Attack : ActionEvent
{
    public Character.AttackInfo moveData;
    public bool canHit = false;
    public bool hasHit = false;
    public bool endAttack = false;
    public SphereCollider spawnedCollider = null;
    public override IEnumerator Perform()
    {
        if (!controller.ChangeActionState(newActionState))
        {
            CancelEvent();
            yield break;
        }
        controller.enableQueue = false;
        //Play on-hit sound FX
 
        //Change movement amount based on the executing move
        controller.movementScale = moveData.movementReduction;
        controller.rotationScale = moveData.rotationMultiplier;

        Collider[] targets = Physics.OverlapSphere(controller.selectedCharacter.gameObject.transform.position, 3.0f);
        foreach(var hitCollider in targets)
        {
            if (hitCollider.gameObject.GetComponent<Character>())
            {
                if(hitCollider.gameObject.GetComponent<Character>().controller != controller)
                {
                    Vector3 heading = (hitCollider.gameObject.transform.position - controller.selectedCharacter.gameObject.transform.position).normalized;
                    float angle = Vector3.Dot(controller.selectedCharacter.gameObject.transform.forward, heading);
                    if(angle > 0.75)
                    {
                        Vector3 newDirection = Vector3.RotateTowards(controller.selectedCharacter.gameObject.transform.forward, heading, 2.0f, 10.0f);
                        controller.selectedCharacter.gameObject.transform.rotation = Quaternion.LookRotation(newDirection);
                    }
                }
            }
        }

        m_Animator.SetTrigger(moveData.animationTrigger);
        int timeoutCount = 0;
        while (!m_Animator.GetCurrentAnimatorStateInfo(0).IsName(moveData.animationState))
        {
            timeoutCount++;
            if(timeoutCount > 30)
            {
                CancelEvent();
                yield break;
            }
            yield return null;
        }

        if (moveData.attackSFX.Count > 0)
        {
            AudioClip hitSFX = moveData.attackSFX[Random.Range(0, moveData.attackSFX.Count)];
            Character character = (Character)controller.controlledCharacter;
            character.audioSource.clip = hitSFX;
            character.audioSource.Play();
        }


        for (int i = 0; i < duration; i++) { 
            if (endAttack)
            {
                CancelEvent();
                yield break;
            }
            yield return null;
        }

        CancelEvent();
    }

    public override void CancelEvent()
    {
        DestroyCollider();
        controller.ChangeActionState(PlayerController.ActionState.Neutral);
        controller.currentEvent = null;
        canBeInterrupted = true;
        controller.movementScale = 1.0f;
        controller.rotationScale = 1.0f;
        controller.enableQueue = true;
    }

    public void SpawnCollider()
    {
        spawnedCollider = moveData.attackHitboxSpawn.AddComponent<SphereCollider>();
        spawnedCollider.isTrigger = true;
        spawnedCollider.radius = moveData.hitboxSize;
    }

    public void DestroyCollider()
    {
        if (spawnedCollider)
        {
            GameObject.Destroy(spawnedCollider);
        }
    }
    public Attack(Character.AttackInfo info, PlayerController controller, Animator animator, Rigidbody rb)
    {
        this.duration = info.lockoutDuration;
        this.rb = rb;
        this.canBeInterrupted = true;
        this.controller = controller;
        this.m_Animator = animator;
        this.newActionState = PlayerController.ActionState.Attacking;
        this.animationTrigger = "Attacking";
        this.moveData = info;
    }
}
