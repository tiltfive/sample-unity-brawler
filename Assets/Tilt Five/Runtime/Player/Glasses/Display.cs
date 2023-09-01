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
using System;
using System.Runtime.InteropServices;
using UnityEngine;

using TiltFive.Logging;

namespace TiltFive
{
    [Serializable]
    public class AxesBoolean
    {
        public bool x = true;
        public bool y = true;
        public bool z = true;

        public AxesBoolean(bool setX, bool setY, bool setZ)
        {
            x = setX;
            y = setY;
            z = setZ;
        }
    }

    [Serializable]
    public class AllAxesBoolean
    {
        public bool xyz = true;

        public AllAxesBoolean(bool setXYZ)
        {
            xyz = setXYZ;
        }
    }

    public struct ARProjectionFrustum
    {
        public float m_Left;
        public float m_Right;
        public float m_Bottom;
        public float m_Top;
        public float m_Near;
        public float m_Far;


        public ARProjectionFrustum(float l, float r, float b, float t, float n, float f)
        {
            m_Left = l; m_Right = r; m_Bottom = b; m_Top = t; m_Near = n; m_Far = f;
        }
    }

    public class Display : TiltFive.SingletonComponent<Display>
    {
        // Display Settings.
        [NonSerialized] int[] _displaySettings = new int[2];

        // Frame sender render-thread callback.
        [NonSerialized]
        IntPtr _sendFrameCallback = IntPtr.Zero;

        protected override void Awake()
        {
            base.Awake();

            try
            {
                _sendFrameCallback = NativePlugin.GetSendFrameCallback();
            }
            catch (System.DllNotFoundException e)
            {
                Log.Info("Could not connect to Tilt Five plugin to get callback: {0}", e);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            LogVersion();

            QualitySettings.vSyncCount = 0;
            QualitySettings.maxQueuedFrames = 0;
        }

        void Start()
        {

        }

        void Update()
        {

        }

        private void LogVersion()
        {
            string version = "NOT VERSIONED";

            // load version file and get the string value
            TextAsset asset = (TextAsset)Resources.Load("pluginversion", typeof(TextAsset));
            if (asset != null)
            {
                version = asset.text;
            }

            // turn on logging if it was turned off
            bool logEnabled = Debug.unityLogger.logEnabled;
            if (!logEnabled)
            {
                Debug.unityLogger.logEnabled = true;
            }

            // get previous setting
            StackTraceLogType logType = Application.GetStackTraceLogType(LogType.Log);

            // turn off stacktrace logging for our messaging
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

            Log.Info("\n********************************" +
                  "\n* Tilt Five: Unity SDK Version - " +
                  version +
                  "\n********************************");

            // reset to initial log settings
            Application.SetStackTraceLogType(LogType.Log, logType);

            // reset logging enabled to previous
            Debug.unityLogger.logEnabled = logEnabled;
        }

        public static bool SetApplicationInfo()
        {
            return Instance.SetApplicationInfoImpl();
        }

        private bool SetApplicationInfoImpl()
        {
            string applicationName = Application.productName;
#if UNITY_EDITOR
            // TODO: Localize
            applicationName = $"Unity Editor: {applicationName}";
#endif
            string applicationId = Application.identifier;
            string productVersion = Application.version;
            string engineVersion = Application.unityVersion;
            TextAsset pluginVersionAsset = (TextAsset)Resources.Load("pluginversion");
            string applicationVersionInfo = $"App: {productVersion}, Engine: {engineVersion}, T5 SDK: {pluginVersionAsset.text}";

            int result = 1;

            try
            {
                using (T5_StringUTF8 appName = applicationName)
                using (T5_StringUTF8 appId = applicationId)
                using (T5_StringUTF8 appVersion = applicationVersionInfo)
                {
                    result = NativePlugin.SetApplicationInfo(appName, appId, appVersion);
                }
            }
            catch (System.DllNotFoundException e)
            {
                Log.Info("Could not connect to Tilt Five plugin to register project info: {0}", e);
            }
            catch (Exception)
            {
                Log.Error("Failed to register project info with the Tilt Five service.");
            }

            return result == 0;
        }

        internal static bool SetPlatformContext()
        {
            return Instance.SetPlatformContextImpl();
        }

        private bool SetPlatformContextImpl()
        {
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                // Ensure the current thread is attached to the JVM
                AndroidJNI.AttachCurrentThread();

                IntPtr unityPlayerClazz = AndroidJNI.FindClass("com/unity3d/player/UnityPlayer");
                if (unityPlayerClazz == IntPtr.Zero)
                {
                    Log.Error("Failed to obtain UnityPlayer class via JNI");
                    return false;
                }

                IntPtr currentActivityFieldId = AndroidJNI.GetStaticFieldID(unityPlayerClazz, "currentActivity", "Landroid/app/Activity;");
                if (currentActivityFieldId == IntPtr.Zero)
                {
                    Log.Error("Failed to obtain UnityPlayer/currentActivity field via JNI");
                    return false;
                }

                IntPtr currentActivity = AndroidJNI.GetStaticObjectField(unityPlayerClazz, currentActivityFieldId);
                if (currentActivity == IntPtr.Zero)
                {
                    Log.Error("Failed to obtain UnityPlayer/currentActivity instance via JNI");
                    return false;
                }

                IntPtr t5ActivityClazz = AndroidJNI.FindClass("com/tiltfive/client/TiltFiveActivity");
                if (t5ActivityClazz == IntPtr.Zero)
                {
                    Log.Error("Failed to obtain TiltFive activity class via JNI");
                    return false;
                }

                IntPtr getPlatformContextMethodId = AndroidJNI.GetMethodID(t5ActivityClazz, "getT5PlatformContext", "()J");
                if (getPlatformContextMethodId == IntPtr.Zero)
                {
                    Log.Error("Failed to obtain TiltFive getT5PlatformContext() method via JNI");
                    return false;
                }

                var context = AndroidJNI.CallLongMethod(currentActivity, getPlatformContextMethodId, new jvalue[] { });
                if (context == 0)
                {
                    Log.Error("Failed to obtain TiltFive platform context via JNI");
                    return false;
                }

                // If we obtained a context from Java, send it to native
                try
                {
                    int result = NativePlugin.SetPlatformContext(new IntPtr(context));
                    if (result != 0)
                    {
                        Log.Error("Tilt Five platform context set returned error: {0}", result);
                    }
                }
                catch (System.DllNotFoundException e)
                {
                    Log.Info("Tilt Five plugin unavailable for set platform context: {0}", e);
                    return false;
                }
                catch (Exception e)
                {
                    Log.Error("Failed to set Tilt Five platform context: {0}", e);
                    return false;
                }
            }
#endif  // UNITY_ANDROID

            return true;
        }

        /// <summary>Get whether any glasses are available</summary>
        ///
        /// <remarks>If you want to check whether glasses for a specific player index are available,
        /// use <see cref="T:TiltFive.Player.IsConnected"/> instead.</remarks>
        /// <returns><c>true</c> if glasses are available, <c>false</c> otherwise.</returns>
        public static bool GetGlassesAvailability()
        {
            return Player.IsConnected(PlayerIndex.One) || Player.IsConnected(PlayerIndex.Two) ||
                Player.IsConnected(PlayerIndex.Three) || Player.IsConnected(PlayerIndex.Four);
        }

        static public bool PresentStereoImages(
                PlayerIndex playerIndex,
                IntPtr leftTexHandle,
                IntPtr rightTexHandle,
                int texWidth_PIX,
                int texHeight_PIX,
                bool isSrgb,
                float fovYDegrees,
                float widthToHeightRatio,
                Quaternion rotToUGBD_ULVC,
                Vector3 posOfULVC_UGBD,
                Quaternion rotToUGBD_URVC,
                Vector3 posOfURVC_UGBD) {
            return Player.TryGetGlassesHandle(playerIndex, out var glassesHandle)
                && PresentStereoImages(glassesHandle,
                                        leftTexHandle,
                                        rightTexHandle,
                                        texWidth_PIX,
                                        texHeight_PIX,
                                        isSrgb,
                                        fovYDegrees,
                                        widthToHeightRatio,
                                        rotToUGBD_ULVC,
                                        posOfULVC_UGBD,
                                        rotToUGBD_URVC,
                                        posOfURVC_UGBD);
        }

        static internal bool PresentStereoImages(
                GlassesHandle glassesHandle,
                IntPtr leftTexHandle,
                IntPtr rightTexHandle,
                int texWidth_PIX,
                int texHeight_PIX,
                bool isSrgb,
                float fovYDegrees,
                float widthToHeightRatio,
                Quaternion rotToUGBD_ULVC,
                Vector3 posOfULVC_UGBD,
                Quaternion rotToUGBD_URVC,
                Vector3 posOfURVC_UGBD)
        {
            return Instance.PresentStereoImagesImpl(glassesHandle,
                                                    leftTexHandle,
                                                    rightTexHandle,
                                                    texWidth_PIX,
                                                    texHeight_PIX,
                                                    isSrgb,
                                                    fovYDegrees,
                                                    widthToHeightRatio,
                                                    rotToUGBD_ULVC,
                                                    posOfULVC_UGBD,
                                                    rotToUGBD_URVC,
                                                    posOfURVC_UGBD);
        }

        bool PresentStereoImagesImpl(
                UInt64 glassesHandle,
                IntPtr leftTexHandle,
                IntPtr rightTexHandle,
                int texWidth_PIX,
                int texHeight_PIX,
                bool isSrgb,
                float fovYDegrees,
                float widthToHeightRatio,
                Quaternion rotToUGBD_ULVC,
                Vector3 posOfULVC_UGBD,
                Quaternion rotToUGBD_URVC,
                Vector3 posOfURVC_UGBD)
        {
            // Unity reference frames:
            //
            // ULVC / URVC - Unity Left/Right Virtual Camera space.
            //               +x right, +y up, +z forward
            // UGBD        - Unity Gameboard space.
            //               +x right, +y up, +z forward
            //
            // Tilt Five reference frames:
            //
            // DC          - Our right-handed version of Unity's default camera space
            //               (the LVC/RVC if there is no transform set on ULVC/URVC).
            //               +x right, +y up, +z backward
            // LVC / RVC   - Left/Right Virtual Camera space.
            //               +x right, +y up, +z backward
            // GBD         - Gameboard space.
            //               +x right, +y forward, +z up

            Quaternion rotToDC_GBD = Quaternion.AngleAxis(-90f, Vector3.right);

            // Calculate the VCI (the image rectangle in the normalized (z=1) image space of the virtual cameras)
            float startY_VCI = -Mathf.Tan(fovYDegrees * (0.5f * Mathf.PI / 180.0f));
            float startX_VCI = startY_VCI * widthToHeightRatio;
            float width_VCI = -2f * startX_VCI;
            float height_VCI = -2f * startY_VCI;
            Rect vci = new Rect(startX_VCI, startY_VCI, width_VCI, height_VCI);

            // Swizzle the left-handed Unity-based coordinates into our right-handed reference frames
            Quaternion rotToLVC_DC = new Quaternion(rotToUGBD_ULVC.x, rotToUGBD_ULVC.y, -rotToUGBD_ULVC.z, rotToUGBD_ULVC.w);
            Quaternion rotToRVC_DC = new Quaternion(rotToUGBD_URVC.x, rotToUGBD_URVC.y, -rotToUGBD_URVC.z, rotToUGBD_URVC.w);

            Quaternion rotToLVC_GBD = rotToLVC_DC * rotToDC_GBD;
            Quaternion rotToRVC_GBD = rotToRVC_DC * rotToDC_GBD;

            // Swap the Y and Z axes to switch from left-handed to right-handed coords
            Vector3 posOfLVC_GBD = new Vector3(posOfULVC_UGBD.x, posOfULVC_UGBD.z, posOfULVC_UGBD.y);
            Vector3 posOfRVC_GBD = new Vector3(posOfURVC_UGBD.x, posOfURVC_UGBD.z, posOfURVC_UGBD.y);

            // Build our frame info struct now that we're finished converting from Unity's coord space
            T5_FrameInfo frameInfo = new T5_FrameInfo();

            frameInfo.LeftTexHandle = leftTexHandle;
            frameInfo.RightTexHandle = rightTexHandle;

            frameInfo.TexWidth_PIX = (UInt16) texWidth_PIX;
            frameInfo.TexHeight_PIX = (UInt16) texHeight_PIX;

            frameInfo.IsSrgb = isSrgb;
            frameInfo.IsUpsideDown = false;     // False for Unity, but possibly true for other engines

            frameInfo.VCI = vci;

            frameInfo.RotToLVC_GBD = rotToLVC_GBD;
            frameInfo.PosOfLVC_GBD = posOfLVC_GBD;

            frameInfo.RotToRVC_GBD = rotToRVC_GBD;
            frameInfo.PosOfRVC_GBD = posOfRVC_GBD;

            int result = 1;
            try
            {
                result = NativePlugin.QueueStereoImages(glassesHandle, frameInfo);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            if (result != 0) {
                return false;
            }

            if(_sendFrameCallback == IntPtr.Zero)
            {
                // We failed to set _sendFrameCallback during Awake() - let's try again
                try
                {
                    _sendFrameCallback = NativePlugin.GetSendFrameCallback();
                }
                catch (Exception)
                {
                    Log.Error("Unable to send frame - the native plugin DLL may be failing to load");
                    return false;
                }

                if (_sendFrameCallback == IntPtr.Zero)
                {
                    // If we reach this point, the native plugin loaded, but erroneously gave us a null callback
                    Log.Error("Unable to send frame - the native plugin returned a null SendFrame callback");
                    return false;
                }
            }

            try
            {
                GL.IssuePluginEvent(_sendFrameCallback, 0);
                GL.InvalidateState();
            }
            catch (Exception e)
            {
                Log.Error($"Failed to execute sendFrame callback: {e.Message}");
                return false;
            }

            return true;
        }

        public static bool GetDisplayDimensions(ref Vector2Int displayDimensions)
        {
            return Instance.GetDisplayDimensionsImpl(ref displayDimensions);
        }

        private bool GetDisplayDimensionsImpl(ref Vector2Int displayDimensions)
        {
            int result = 1;
            try
            {
                result = NativePlugin.GetMaxDisplayDimensions(_displaySettings);

                if(result == 0)
                {
                    displayDimensions = new Vector2Int(_displaySettings[0], _displaySettings[1]);
                }
                else Log.Warn("Display.cs: Failed to retrieve display settings from plugin.");
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            return (0 == result);
        }

        public static bool GetGlassesIPD(UInt64 glassesHandle, ref float glassesIPD)
        {
            return Instance.GetGlassesIPDImpl(glassesHandle, ref glassesIPD);
        }

        private bool GetGlassesIPDImpl(UInt64 glassesHandle, ref float glassesIPD)
        {
            int result = 1;
            try
            {
                result = NativePlugin.GetGlassesIPD(glassesHandle, ref glassesIPD);

                if(result != 0 && GetGlassesAvailability())
                {
                    Log.Warn("Display.cs: Failed to retrieve glasses IPD");
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            return (0 == result);
        }
    }

    public class DisplayHelper
    {
        private static Matrix4x4 Frustum(ARProjectionFrustum f)
        {
            return Frustum(f.m_Left, f.m_Right, f.m_Bottom, f.m_Top, f.m_Near, f.m_Far);
        }

        /***********************************************************************
         * This is our interpretation of glFrustum. CalculateObliqueMatrix
         * has some params that I haven't figured out yet, so I'll use this
         * instead.
        ***********************************************************************/
        public static Matrix4x4 Frustum(float L, float R, float B, float T, float n, float f)
        {
            Matrix4x4 m = new Matrix4x4();

            m[0, 0] = (2 * n) / (R - L);
            m[1, 1] = (2 * n) / (T - B);
            m[0, 2] = (R + L) / (R - L);
            m[1, 2] = (T + B) / (T - B);
            m[2, 2] = -(f + n) / (f - n);
            m[2, 3] = -(2 * f * n) / (f - n);
            m[3, 2] = -1.0f;
            m[3, 3] = 0.0f;

            return m;
        }
    }
}
