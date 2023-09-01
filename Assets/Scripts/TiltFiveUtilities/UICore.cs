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

public class UICore : MonoBehaviour
{
    public PlayerController currentPlayer;

    public BoardScaler boardScaler;
    public MultiplayerController mc;
    string gameboardName = "Tilt Five Game Board";

    public float lerpSpeed = 3.0f;
    // Start is called before the first frame update
    void Start()
    {
        
        boardScaler = GameObject.Find(gameboardName).GetComponent<BoardScaler>();
        mc = GameObject.Find("MultiplayerController").GetComponent<MultiplayerController>();
    }

    public void LateUpdate()
    {
        if(boardScaler == null)
        {
            boardScaler = GameObject.Find(gameboardName).GetComponent<BoardScaler>();
        }
        if(mc == null)
        {
            mc = GameObject.Find("MultiplayerController").GetComponent<MultiplayerController>();
        }

        //Ensure the UI follows the Tilt Five Game Board
        gameObject.transform.position = boardScaler.gameObject.transform.position;
        gameObject.transform.localScale = Vector3.Lerp(this.gameObject.transform.localScale, new Vector3(((BoardScaler)boardScaler).contentScale, ((BoardScaler)boardScaler).contentScale, ((BoardScaler)boardScaler).contentScale), lerpSpeed / 60.0f);

        if (mc.activePlayers.ContainsKey(currentPlayer.playerNumber))
        {
            Pose pose;
            if (TiltFive.Glasses.TryGetPose(currentPlayer.playerNumber, out pose))
            {
                Vector3 a = -transform.forward;
                Vector3 b = new Vector3(pose.position.x, 0.0f, pose.position.z);
                var angle = Vector3.Angle(a, b);
                var cross = Vector3.Cross(a, b);
                if (cross.y < 0)
                {
                    angle = -angle;
                }
                if (angle > 45)
                {
                    gameObject.transform.RotateAround(Vector3.zero, new Vector3(0.0f, 1.0f, 0.0f), 90);
                }
                else if (angle < -45)
                {
                    gameObject.transform.RotateAround(Vector3.zero, new Vector3(0.0f, 1.0f, 0.0f), -90);
                }
            }
        }
        if (currentPlayer)
        {
            //Set the UI layer to match the player number to ensure it's only visible to one person
            if (gameObject.layer == 0)
            {
                gameObject.layer = LayerMask.NameToLayer(mc.playerLayers[currentPlayer.playerNumber]);
                foreach (Transform child in transform)
                {
                    child.gameObject.layer = LayerMask.NameToLayer(mc.playerLayers[currentPlayer.playerNumber]);
                }
            }
        }
    }
}
