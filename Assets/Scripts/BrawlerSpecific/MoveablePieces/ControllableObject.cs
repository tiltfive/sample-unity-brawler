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

public class ControllableObject : MonoBehaviour
{
    public PlayerController controller; //The Player script that represents this character's controller
    public Vector2 inputMovement; //A vector representing the amount a character should move. Is modified by incoming controller inputs, as well as various states
    public MultiplayerController mc;
    public float walkSpeed; //The speed the character moves
    public float rotationSpeed; //The speed the character model rotates at
    public float currentRotationSpeed;
    public Rigidbody rb; //The physics body of this character

    public void Start()
    {
        mc = GameObject.Find("MultiplayerController").GetComponent<MultiplayerController>();
    }
    void FixedUpdate()
    {
        //Only allow movement once the character is not locked out from input.
        if (controller.currentActionState != PlayerController.ActionState.HitStunned)
        {
            //We rotate the input controller to match the x/z plane because we use a sideways controller grip.
            Vector3 movementDirection = new Vector3(-inputMovement.y, 0, inputMovement.x);
            rb.MovePosition(transform.position + movementDirection * Time.deltaTime * walkSpeed);
            if (movementDirection != Vector3.zero)
            {
                //Rotate the character to match the movement direction.
                Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, currentRotationSpeed * Time.deltaTime);
            }
        }
    }

    private void Update()
    {
        if(mc == null)
        {
            Debug.Log("No Multiplayer Controller found at start, attempting to relocate.");
            mc = GameObject.Find("MultiplayerController").GetComponent<MultiplayerController>();
        }
    }
}
