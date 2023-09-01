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

public abstract class ActionEvent
{
    public float duration; //The duration the event should lock the player out from performing another action, in frames.
    public bool canBeInterrupted; //Whether an event can be interrupted during execution or not.
    public Rigidbody rb; //The rb the action will act on
    public Animator m_Animator; //The aniamtor the action will act on
    public PlayerController controller; //The Player executing the action
    public PlayerController.ActionState newActionState; //The actionState the player controller should move in to
    public string animationTrigger; //The animator trigger the action should fire.
    public string animationState; //The state that the event is expected to kick off
    public abstract IEnumerator Perform(); // The execution for a given event. Should modify the controller's action state during execution, delay for duration number of frames and reset the controllers currentEvent and action state after it finishes.
    public abstract void CancelEvent();
}
