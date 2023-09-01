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
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public TiltFive.PlayerIndex playerNumber = TiltFive.PlayerIndex.None; //Keep track of the Tilt Five player number a Player controller has.
    public ControllableObject controlledCharacter; //The controlled Character object
    public float damagePercentage = 0; //The current damage percentage for a player.
    public int currentLives = 3; //The current number of lives remaining a player has
    public int jumpCount = 2; //The number of remaining jumps a player's character has.
    public ActionEvent currentEvent; //The currently executing event.
    public List<ActionEvent> eventQueue; //An event queue to allow bufferring oof attacks.
    public float movementScale = 1.0f; //A modifier to movement to allow slowing and movement lockout.
    public float rotationScale = 1.0f; //A modifier to rotation to allow slowing and rotation lockout.

    public float stickDeadzone = 0.05f;

    public GameObject selectedCharacter; //The prefab the player has selected for their char.

    public BattleManager bm;

    public MultiplayerController mc;
    
    public GameObject uiCore; //The UI prefab for a given player.

    public GameObject uiReference;

    public bool enableQueue = true;
    public bool isBlocking = false;
    public bool isSpectator = false;

    //Possible Action states for a player to be in
    public enum ActionState
    {
        Jumping,
        Attacking,
        Blocking,
        Special,
        HitStunned,
        Neutral
    }

    public ActionState currentActionState; //The current state of the Player State Machine

    //The state transition diagram depicting valid state transitions.
    public Dictionary<ActionState, List<ActionState>> stateTransitions = new Dictionary<ActionState, List<ActionState>>()
    { {ActionState.Neutral, new List<ActionState>(){ActionState.Jumping, ActionState.Attacking, ActionState.Blocking, ActionState.Special, ActionState.Neutral, ActionState.HitStunned } },
    {ActionState.Jumping, new List<ActionState>(){ActionState.Neutral, ActionState.HitStunned } },
    {ActionState.Attacking, new List<ActionState>(){ActionState.Attacking, ActionState.Blocking, ActionState.Special, ActionState.Neutral, ActionState.HitStunned } },
    {ActionState.Blocking, new List<ActionState>(){ActionState.Jumping, ActionState.Attacking, ActionState.Blocking, ActionState.Special, ActionState.Neutral, ActionState.HitStunned } },
    {ActionState.HitStunned, new List<ActionState>(){ActionState.Jumping, ActionState.Attacking, ActionState.Blocking, ActionState.Special, ActionState.Neutral, ActionState.HitStunned } },
    {ActionState.Special, new List<ActionState>(){ActionState.Attacking, ActionState.Blocking, ActionState.Special, ActionState.Neutral, ActionState.HitStunned } },};


    //The relative side of the gamebaord the player is currently on.
    public enum BoardSide
    {
        South,
        West,
        North,
        East
    }

    public BoardSide currentBoardSide = BoardSide.South;

    //Prevent action processing for a given frame duration
    public IEnumerator Stunned(float duration)
    {
        if (currentEvent != null)
        {
            currentEvent.CancelEvent();
        }
        ChangeActionState(PlayerController.ActionState.HitStunned);
        enableQueue = true;
        for (int i = 0; i < duration ; i++)
        {
            yield return null;
        }
        ((Character)controlledCharacter).m_Animator.SetTrigger("EndHitStun");
        ChangeActionState(PlayerController.ActionState.Neutral);
    }

    //Spawn a new character at a specific spawn point for the current map.
    public IEnumerator Spawn()
    {
        if (selectedCharacter != null)
        {
            yield return new WaitForSeconds(3.0f);
            this.currentActionState = ActionState.Neutral;
            this.currentEvent = null;
            this.eventQueue.Clear();
        
            GameObject spawnedChar = Instantiate(selectedCharacter, bm.spawnPoints[(int)playerNumber - 1].position, Quaternion.identity);

            this.controlledCharacter = spawnedChar.GetComponent<ControllableObject>();
            this.controlledCharacter.controller = this;

            if (controlledCharacter is Character)
            {
                jumpCount = ((Character)controlledCharacter).jumpData.jumpCount;
            }
            if (controlledCharacter is Selector)
            {
                selectedCharacter = null;
            }
            movementScale = 1.0f;
        }
    }
    void Start()
    {
        currentActionState = ActionState.Neutral;
        eventQueue = new List<ActionEvent>();
        mc = GameObject.Find("MultiplayerController").GetComponent<MultiplayerController>();
    }

    //Instatiate the UI prefab
    public void SpawnUI()
    {
        if (uiCore != null)
        {
            uiReference = GameObject.Instantiate(uiCore);
            uiReference.GetComponent<UICore>().currentPlayer = this;
        }
    }

    public void BeginSpawn()
    {
        StartCoroutine("Spawn");
    }

    virtual public void Update()
    {
        if (GameObject.Find("BattleManager"))
        {
            bm = GameObject.Find("BattleManager").GetComponent<BattleManager>();
        }
        
        if (controlledCharacter)
        {

            if (TiltFive.Wand.TryGetWandDevice(playerNumber, TiltFive.ControllerIndex.Right, out var wandDevice))
            {
                if (wandDevice != null)
                {
                    //Set the character's movement based on wand joystick controls.
                    var controls = wandDevice.Stick.ReadValue();
                    var deadzone = new Vector2();
                    if((controls.x < stickDeadzone && controls.x > 0) || (controls.x > -stickDeadzone && controls.x < 0))
                    {
                        deadzone.x = 0;
                    }
                    else
                    {
                        deadzone.x = controls.x;
                    }
                    if ((controls.y < stickDeadzone && controls.y > 0) || (controls.y > -stickDeadzone && controls.y < 0))
                    {
                        deadzone.y = 0;
                    }
                    else
                    {
                        deadzone.y = controls.y;
                    }
                    controlledCharacter.inputMovement = RotateControls(deadzone) * movementScale;
                }
            }

            if (controlledCharacter is Character)
            {
                DetectBoardSide();
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
                         if(currentEvent is not Block)
                         {
                            enableQueue = false;
                         }
                         
                         StartCoroutine(currentEvent.Perform());
                    }
                }
            }
        }
    }

    private void OnBlock(InputValue value)
    {
        if (!bm.isPaused)
        {
            if (controlledCharacter is Character && selectedCharacter != null)
            {
                if (value.Get<float>() == 0)
                {
                    if (currentEvent is Block)
                    {
                        currentEvent.CancelEvent();
                    }
                }
                else if (((Character)controlledCharacter).shieldStrength > 30.0f)
                {
                    eventQueue.Add(new Block(controlledCharacter.rb, ((Character)controlledCharacter).m_Animator, this));
                    if (eventQueue.Count > 1)
                    {
                        eventQueue.RemoveAt(0);
                    }
                }
            }
            else if (controlledCharacter is Selector)
            {
                if (value.Get<float>() == 1)
                {
                    if (movementScale > 0.0f)
                    {
                        if(((Selector)controlledCharacter).isSpectator || ((Selector)controlledCharacter).characterPrefab != null)
                        {
                            movementScale = 0.0f;
                            if (((Selector)controlledCharacter).isSpectator)
                            {
                                isSpectator = true;
                                selectedCharacter = null;
                            }
                            else
                            {
                                isSpectator = false;
                                selectedCharacter = ((Selector)controlledCharacter).characterPrefab;
                            }
                        
                            var soundA = ((Selector)controlledCharacter).SFXselected;
                            ((Selector)controlledCharacter).audioSource.clip = soundA;
                            ((Selector)controlledCharacter).audioSource.Play();
                            controlledCharacter.gameObject.transform.position -= new Vector3(0.0f, 0.5f, 0.0f);
                        }
                    }
                    else
                    {
                        controlledCharacter.gameObject.transform.position += new Vector3(0.0f, 0.5f, 0.0f);
                        movementScale = 1.0f;
                        selectedCharacter = null;
                    }
                }
            }
        }
    }

    private void OnHeavyAttack()
    {
        if (!bm.isPaused)
        {
            if (controlledCharacter && controlledCharacter is Character && enableQueue && selectedCharacter != null)
            {
                if (isBlocking)
                {
                    if (((Character)controlledCharacter).shieldStrength > ((Character)controlledCharacter).projectileData.energyCost)
                    {
                        if (currentEvent is Block)
                        {
                            currentEvent.CancelEvent();
                        }
                        eventQueue.Add(new Projectile(((Character)controlledCharacter).projectileData, this, controlledCharacter.rb, ((Character)controlledCharacter).m_Animator));
                        //Only allow bufferring of 1 action at a time.
                        if (eventQueue.Count > 1)
                        {
                            eventQueue.RemoveAt(0);
                        }

                    }
                }
                //Determine aerial or non-aerial based on player vertical velocity.
                else if ((controlledCharacter.rb.velocity.y > 0.05 || controlledCharacter.rb.velocity.y < -0.05) && !((Character)controlledCharacter).isGrounded)
                {
                    Character.AttackInfo move = ((Character)controlledCharacter).GetMove("HeavyAerial");
                    if (move != null)
                    {
                        //Add a heavy aerial to the event queue if it exists
                        eventQueue.Add(new Attack(move, this, ((Character)controlledCharacter).m_Animator, controlledCharacter.rb));
                        //Only allow bufferring of 1 action at a time.
                        if (eventQueue.Count > 1)
                        {
                            eventQueue.RemoveAt(0);
                        }
                    }
                }
                else
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
            }
        }
        if (bm.isPaused && !MultiplayerController.scene2Loaded && selectedCharacter != null)
        {
            Application.Quit(0);
        }
    }

    private void OnLightAttack()
    {
        if (!bm.isPaused)
        {
            if (controlledCharacter && controlledCharacter is Character && enableQueue && selectedCharacter != null)
            {
                //Determine aerial or non-aerial based on player vertical velocity.
                if ((controlledCharacter.rb.velocity.y > 0.05 || controlledCharacter.rb.velocity.y < -0.05) && !((Character)controlledCharacter).isGrounded)
                {
                    Character.AttackInfo move = ((Character)controlledCharacter).GetMove("LightAerial");
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
                else
                {
                    Character.AttackInfo move = ((Character)controlledCharacter).GetMove("LightAttack");
                    if (move != null)
                    {
                        //Add a light attack to the event queue if it exists
                        eventQueue.Add(new Attack(move, this, ((Character)controlledCharacter).m_Animator, controlledCharacter.rb));
                        //Only allow bufferring of 1 action at a time.
                        if (eventQueue.Count > 1)
                        {
                            eventQueue.RemoveAt(0);
                        }
                    }

                }
            }
        }
    }

    private void OnJump()
    {
        if (!bm.isPaused)
        {
            if (controlledCharacter && controlledCharacter is Character && enableQueue && selectedCharacter != null)
            {
                eventQueue.Add(new Jump(((Character)controlledCharacter).jumpData, controlledCharacter.rb, ((Character)controlledCharacter).m_Animator, this));
                if (eventQueue.Count > 1)
                {
                    eventQueue.RemoveAt(0);
                }
            }
        }
    }

    private void OnSelect()
    {
        if (!bm.isPaused )
        {
            if (mc.gameEnded && selectedCharacter != null)
            {
                mc.ResetGame();
            }
        }
    }

    private void OnCancel()
    {
        if (!bm.isPaused)
        {
            if (controlledCharacter is Selector)
            {
                mc.StartGame();
            }
            else if (mc.gameEnded && selectedCharacter != null)
            {
                SceneManager.LoadScene(1);
            }
        }
    }

    private void OnPause() {
        if (!mc.gameEnded && selectedCharacter != null)
        {
            if (bm.isPaused)
            {
                bm.UnpauseGame();
            }
            else if (!bm.isPaused)
            {
                bm.PauseGame();
            }
        }
    }

    //Modify the player's action state is valid. Returns false otherwise.
    public bool ChangeActionState(PlayerController.ActionState newState)
    {
        bool ret = false;
        var lst = stateTransitions[currentActionState];
        foreach (PlayerController.ActionState actionState in lst)
        {
            if (actionState == newState)
            {
                ret = true;
                currentActionState = newState;
            }
        }
        return ret;
        
    }

    //Destroy the current character and trigger a respawn if applicable
    public void KillCharacter()
    {
        this.currentActionState = ActionState.Neutral;
        this.currentEvent = null;
        this.eventQueue.Clear();
        StopAllCoroutines();
        Destroy(controlledCharacter.gameObject);
        currentLives--;
        damagePercentage = 0;
        if(currentLives != 0)
        {
            StartCoroutine("Spawn");
        }
    }

    //Return the necessary control vector modification absed on current board side
    Vector2 RotateControls(Vector2 input)
    {
        switch (currentBoardSide)
        {
            case BoardSide.South:
                {
                    return input;
                }
            case BoardSide.West:
                {
                    return new Vector2(input.y, -input.x);
                }
            case BoardSide.North:
                {
                    return new Vector2(-input.x, -input.y);
                }
            case BoardSide.East:
                {
                    return new Vector2(-input.y, input.x);
                }
            default:
                {
                    return input;
                }
        }
    }

    void DetectBoardSide()
    {
        //Modify the player's current side based on glasses rotation.
        if (TiltFive.Glasses.TryGetPose(playerNumber, out var glassesPose))
        {
            Vector3 a = new Vector3(0, 0, -1.0f);
            Vector3 b = new Vector3(glassesPose.position.x, 0.0f, glassesPose.position.z);
            var angle = Vector3.Angle(a, b);
            var cross = Vector3.Cross(a, b);
            if (cross.y < 0)
            {
                angle = -angle;
            }

            if (angle >= -45 && angle <= 45)
            {
                currentBoardSide = BoardSide.South;
            }
            else if (angle <= -45 && angle >= -135)
            {
                currentBoardSide = BoardSide.East;
            }
            else if (angle >= 45 && angle <= 135)
            {
                currentBoardSide = BoardSide.West;
            }
            else
            {
                currentBoardSide = BoardSide.North;
            }
        }
    }
}
