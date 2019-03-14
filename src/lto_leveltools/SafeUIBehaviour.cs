﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;

namespace lto_leveltools
{
    abstract class SafeUIBehaviour : MonoBehaviour
    {
        public Rect windowRect = new Rect(0, 0, 192, 128);
        public int windowID { get; protected set; } = ModUtility.GetWindowId();
        public string windowName = "";
        GameObject background;
        protected virtual void Start()
        {
            this.background = new GameObject("UIBackGround");
            background.transform.parent = gameObject.transform;
            background.layer = 13;
            background.AddComponent<BoxCollider>();
        }
        void OnGUI()
        {
            if (GameObject.Find("HUD Cam") == null) return;

            if (ShouldShowGUI())
            {
                if (!background.activeSelf)
                    background.SetActive(true);
                Camera hudCamera = GameObject.Find("HUD Cam").GetComponent<Camera>();
                Vector3 leftTop = hudCamera.ScreenPointToRay(new Vector3(windowRect.xMin, hudCamera.pixelHeight - windowRect.yMin, 0)).origin;
                Vector3 rightButtom = hudCamera.ScreenPointToRay(new Vector3(windowRect.xMax, hudCamera.pixelHeight - windowRect.yMax, 0)).origin;

                Vector3 pos = (leftTop + rightButtom) / 2; pos.z += 0.3f;
                background.transform.position = pos;
                Vector3 sca = rightButtom - leftTop; sca.z = 0.1f;
                sca.x = Mathf.Abs(sca.x); sca.y = Mathf.Abs(sca.y);
                background.transform.localScale = sca;



                this.windowRect = GUILayout.Window(this.windowID, this.windowRect, new GUI.WindowFunction(this.WindowContent), this.windowName);
            }
            else
            {
                if (background.activeSelf)
                    background.SetActive(false);
            }
        }
        protected abstract void WindowContent(int windowID);
        public abstract bool ShouldShowGUI();
    }
}
