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

public class Jump : ActionEvent
{
    Character.JumpInfo jumpData;
    bool isCancelled = false;
    public override IEnumerator Perform()
    {
        //Prevent movement while the jump begins
        controller.movementScale = 0.0f;
        if (!controller.ChangeActionState(newActionState))
        {
            CancelEvent();
            yield break;
        }
        controller.jumpCount--;
        if(controller.jumpCount < 0)
        {
            CancelEvent();
            yield break;
        }
        m_Animator.SetTrigger(animationTrigger);

        while (!m_Animator.GetCurrentAnimatorStateInfo(0).IsName(jumpData.animationState))
        {
            yield return null;
        }

        for (int i = 0; i < jumpData.duration; i++)
        {
            if (isCancelled)
            {
                CancelEvent();
                yield break;
            }
            yield return null;
        }

        //Reset velocity to allow air maneuverability
        rb.velocity = new Vector3 (0.0f, 0.0f, 0.0f);
        //Apply jump velocity
        rb.AddForce(new Vector3(0, jumpData.jumpStrength, 0), ForceMode.VelocityChange);

        //Resolve the event
        CancelEvent();
    }

    public override void CancelEvent()
    {
        isCancelled = true;
        controller.ChangeActionState(PlayerController.ActionState.Neutral);
        controller.currentEvent = null;
        canBeInterrupted = true;
        controller.movementScale = 1.0f;
        controller.rotationScale = 1.0f;
        controller.enableQueue = true;
    }

    public Jump(Character.JumpInfo info, Rigidbody rb, Animator m_Animator, PlayerController controller)
    {
        this.jumpData = info;
        this.rb = rb;
        this.canBeInterrupted = true;
        this.controller = controller;
        this.m_Animator = m_Animator;
        this.newActionState = PlayerController.ActionState.Jumping;
        this.animationTrigger = "Jumping";
    }
}