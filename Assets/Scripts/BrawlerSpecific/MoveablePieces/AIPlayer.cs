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

public class AIPlayer : PlayerController
{
    public int randomizerMax = 100;

    // Update is called once per frame
    override public void Update()
    {
        if (selectedCharacter)
        {
            int choice = Random.Range(0, randomizerMax);
            //Trigger light attack based on X button input
            if (choice == 0)
            {
                    Character.AttackInfo move = ((Character)controlledCharacter).GetMove("LightAttack");
                    if (move != null)
                    {
                        //Add a light aerial to the event queue if it exists
                        eventQueue.Add(new Attack(move, this, ((Character)controlledCharacter).m_Animator, controlledCharacter.rb));
                        //Only allow bufferring of 1 action at a time.
                        if (eventQueue.Count > 1)
                        {
                            eventQueue.RemoveAt(0);
                        }
                    }
            }
            //Trigger heavy attack based on B button input
            if (choice == 1)
            {
                    Character.AttackInfo move = ((Character)controlledCharacter).GetMove("HeavyAttack");
                    if (move != null)
                    {
                        //Add a heavy attack to the event queue if it exists
                        eventQueue.Add(new Attack(move, this, ((Character)controlledCharacter).m_Animator, controlledCharacter.rb));
                        //Only allow bufferring of 1 action at a time.
                        if (eventQueue.Count > 1)
                        {
                            eventQueue.RemoveAt(0);
                        }
                    }

            }
            //We only process new events if we're currently in the Neutral action state.
            if (currentActionState == ActionState.Neutral)
            {
                //Only process new events if we're not currently executing an event.
                if (currentEvent == null)
                {
                    //Only perform an event if there is an event to perform
                    if (eventQueue.Count > 0)
                    {
                        currentEvent = eventQueue[0];
                        eventQueue.RemoveAt(0);
                        StartCoroutine(currentEvent.Perform());
                    }
                }
            }
        }
    }
}
