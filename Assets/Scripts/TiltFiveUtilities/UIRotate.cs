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

public class UIRotate : MonoBehaviour
{
    public MultiplayerController mc;
    public TiltFive.PlayerIndex currentPlayer;
    void Start()
    {
        mc = GameObject.Find("MultiplayerController").GetComponent<MultiplayerController>();
    }
    void LateUpdate()
    {
        if (mc.activePlayers.ContainsKey(currentPlayer))
        {
            Pose pose;
            if (TiltFive.Glasses.TryGetPose(currentPlayer, out pose))
            {
                Vector3 a = -transform.forward;
                Vector3 b = new Vector3(pose.position.x, 0.0f, pose.position.z);
                var angle = Vector3.Angle(a, b);
                var cross = Vector3.Cross(a, b);
                if (cross.y < 0)
                {
                    angle = -angle;
                }
                if (angle > 75)
                {
                    gameObject.transform.RotateAround(Vector3.zero, new Vector3(0.0f, 1.0f, 0.0f), 90);
                }
                else if (angle < -75)
                {
                    gameObject.transform.RotateAround(Vector3.zero, new Vector3(0.0f, 1.0f, 0.0f), -90);
                }
            }
        }
    }
}
