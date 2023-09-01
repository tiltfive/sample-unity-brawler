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

public class BoardScaler : BoardControl
{
    //An inverse scale for UI and content whose position and scale depend on the board.
    public float contentScale;

    //The minimum and maximum zoom of a board.
    public float lowZoomClamp = 0.5f;
    public float highZoomClamp = 2f;

    public float lerpSpeed = 3.0f;

    public int charCount = 0;

    public Vector3 averagePos;
    
    //We use LateUpdate here to move the board after the visual display update is complete.
    void LateUpdate()
    {
        float maxDistance = 0.0f;
        averagePos = new Vector3(0.0f, 0.0f, 0.0f);
        charCount = 0;
        //Determine the maximum distance between any two characters.
        foreach(KeyValuePair< TiltFive.PlayerIndex, PlayerController> firstPlayer in mc.activePlayers)
        {
            foreach (KeyValuePair<TiltFive.PlayerIndex, PlayerController> secondPlayer in mc.activePlayers)
            {
                if (firstPlayer.Value.controlledCharacter && secondPlayer.Value.controlledCharacter)
                {
                    var dis = Mathf.Abs(Vector3.Magnitude(firstPlayer.Value.controlledCharacter.gameObject.transform.position - secondPlayer.Value.controlledCharacter.gameObject.transform.position));
                    if (dis > maxDistance)
                    {
                        maxDistance = dis;
                    }
                }
            }
        }
        //Find the average character position to keep the board centered between all characters.
        foreach (KeyValuePair<TiltFive.PlayerIndex, PlayerController> player in mc.activePlayers)
        {
            if (player.Value.controlledCharacter)
            {
                var charPos = player.Value.controlledCharacter.gameObject.transform.position;
                if (charPos.y < 0.0)
                {
                    averagePos += new Vector3(charPos.x, 0.0f, charPos.z);
                }
                else
                {
                    averagePos += charPos;
                }
                charCount++;
            }
        }
        //Scale the board based on maximum character distance. This allows us to zoom in or out based on how far the characters are from eachother.
        contentScale = (maxDistance/10.0f);
        contentScale = Mathf.Clamp(contentScale, lowZoomClamp, highZoomClamp);
        this.gameObject.transform.localScale = Vector3.Lerp(this.gameObject.transform.localScale, new Vector3(contentScale, contentScale, contentScale), lerpSpeed / 60.0f) ;

        //Set us to default position if there are no characters, otherwise follow the average position between all chars.
        if (charCount == 0)
            
        {
            this.gameObject.transform.position = Vector3.Lerp(this.gameObject.transform.position, averagePos, lerpSpeed/60.0f);
        }
        else
        {
            this.gameObject.transform.position = Vector3.Lerp(this.gameObject.transform.position, averagePos / charCount, lerpSpeed / 60.0f);
        }
    }
}
