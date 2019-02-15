using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using UnityEngine;

namespace lto_leveltools
{
    class ToolBoxBehaviour : MonoBehaviour
    {
        private ModBehaviour modBehaviour;

        private readonly int windowID = ModUtility.GetWindowId();
        Rect windowRect = new Rect(20, 300, 150, 200);

        public bool generateForward = false;
        public bool generateBackward = true;
        public string clkForward = "0";
        public string clkBackward = "1";

        public int relativeMode = 2;

        public bool logAfterGenerate = true;
        public string duration = "1";
        public string durationRandom = "0";
        public string errorInfo = "";

        public string wait = "1";
        public string waitRandom = "0";

        public string clksControlModule = "1";

        public string executeClk = "1";

        public string clkAbsoluteLog = "-1";
        public string durationAbsoluteLog = "1";

        public Action OnLogButtonClick;
        public Action OnGenerateButtonClick;
        //-------------------------------------------------------------------------
        public ModKey DisplaySceneSettingKey = ModKeys.GetKey("Scene SettingUI-key");


        public Action OnFogButtonClick;

        public Action OnFloorButtonClick;

        public Action OnWorldBoundsButtonClick;

        public Action OnReloadScenesButtonClick;

        public Action OnOpenScenePacksDirectoryButtonClick;

        private Vector2 scrollVector = Vector2.zero;

        private readonly int buttonHeight = 20;

        private Rect sceneButtonsRect;

        private GUIStyle windowStyle;

        void Awake()
        {
            this.modBehaviour = this.gameObject.transform.parent.gameObject.GetComponent<ModBehaviour>();
            this.OnLogButtonClick += this.modBehaviour.LogSelection;
            this.OnGenerateButtonClick += this.GenerateTransformEvent;
        }

        void GenerateTransformEvent()
        {
            int clk1;
            try
            {
                clk1 = Convert.ToInt32(this.clkForward);
            }
            catch(Exception e)
            {
                this.errorInfo = "顺行clk不正确";
                return;
            }
            int clk2;
            try
            {
                clk2 = Convert.ToInt32(this.clkBackward);
            }
            catch (Exception e)
            {
                this.errorInfo = "逆行clk不正确";
                return;
            }
            float time;
            float timeRandom;

            try
            {
                time = Convert.ToSingle(this.duration);
            }
            catch (Exception e)
            {
                this.errorInfo = "持续时间不正确";
                return;
            }
            try
            {
                timeRandom = Convert.ToSingle(this.durationRandom);
            }
            catch (Exception e)
            {
                this.errorInfo = "持续时间随机不正确";
                return;
            }
            string type = this.relativeMode == 0 ? "Absolute" : (this.relativeMode == 1 ? "WorldDirection" : "LocalDirection");
            if (generateForward)
            {
                modBehaviour.GenerateEvent(true, clk1, time, timeRandom, type);
            }
            if (generateBackward)
            {
                modBehaviour.GenerateEvent(false, clk2, time, timeRandom, type);
            }
            if (this.logAfterGenerate)
            {
                modBehaviour.LogSelection();
            }
        }

        void GenerateWaitEvent()
        {
            int clk1;
            try
            {
                clk1 = Convert.ToInt32(this.clkForward);
            }
            catch (Exception e)
            {
                this.errorInfo = "顺行clk不正确";
                return;
            }
            int clk2;
            try
            {
                clk2 = Convert.ToInt32(this.clkBackward);
            }
            catch (Exception e)
            {
                this.errorInfo = "逆行clk不正确";
                return;
            }
            float time;
            float timeRandom;

            try
            {
                time = Convert.ToSingle(this.wait);
            }
            catch (Exception e)
            {
                this.errorInfo = "等待时间不正确";
                return;
            }
            try
            {
                timeRandom = Convert.ToSingle(this.waitRandom);
            }
            catch (Exception e)
            {
                this.errorInfo = "等待时间随机不正确";
                return;
            }
            if (generateForward)
            {
                modBehaviour.GenerateWaitEvent(true, clk1, time, timeRandom);
            }
            if (generateBackward)
            {
                modBehaviour.GenerateWaitEvent(false, clk2, time, timeRandom);
            }
        }

        void OnGUI()
        {
            if(StatMaster.isMP && !StatMaster.levelSimulating && !StatMaster.inMenu)
            {
                this.windowRect = GUILayout.Window(this.windowID, this.windowRect, new GUI.WindowFunction(this.ToolBoxWindow), "地形工具");
            }
        }
        void ToolBoxWindow(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("选中地形:" + modBehaviour.GetLevellEditorSelectionCount().ToString());
            GUILayout.Label("记录地形:" + modBehaviour.entityLogs.Count);
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    this.generateForward = GUILayout.Toggle(this.generateForward, "生成顺行");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("clk:");
                    clkForward = GUILayout.TextField(clkForward);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                {
                    this.generateBackward = GUILayout.Toggle(this.generateBackward, "生成逆行");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("clk:");
                    clkBackward = GUILayout.TextField(clkBackward);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                // End the Groups and Area
            }
            GUILayout.EndHorizontal();

            this.relativeMode = GUILayout.SelectionGrid(this.relativeMode, new string[] { "绝对", "世界", "相对" }, 3);

            GUILayout.BeginHorizontal();
            {
                if(GUILayout.Button("记录")){
                    OnLogButtonClick();
                };
                if (GUILayout.Button("生成")) {
                    OnGenerateButtonClick();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("时长");
                this.duration = GUILayout.TextField(this.duration);
                GUILayout.Label("+-");
                this.durationRandom = GUILayout.TextField(this.durationRandom);
            }
            GUILayout.EndHorizontal();
            this.logAfterGenerate= GUILayout.Toggle(this.logAfterGenerate, "生成后同时记录");

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("等待"))
                {
                    GenerateWaitEvent();
                }
                this.wait = GUILayout.TextField(this.wait);
                GUILayout.Label("+-");
                this.waitRandom = GUILayout.TextField(this.waitRandom);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                this.clksControlModule = GUILayout.TextField(this.clksControlModule);
                if (GUILayout.Button("生成控制模块"))
                {
                    this.GenerateButtonClicked();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("执行");
            GUILayout.BeginHorizontal();
            {
                this.executeClk = GUILayout.TextField(this.executeClk);
                if (GUILayout.Button("顺向执行"))
                {
                    this.ExecuteButtonClicked(true);
                }
                if (GUILayout.Button("逆向执行"))
                {
                    this.ExecuteButtonClicked(false);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("clk绝对记录");
            GUILayout.BeginHorizontal();
            {
                this.clkAbsoluteLog = GUILayout.TextField(this.clkAbsoluteLog);
                this.durationAbsoluteLog = GUILayout.TextField(this.durationAbsoluteLog);
                if (GUILayout.Button("快速记录"))
                {
                    this.AbsoluteLogButtonClicked();
                }

            }
            GUILayout.EndHorizontal();

            GUILayout.Label(this.errorInfo);

            GUILayout.EndVertical();
            GUI.DragWindow();
        }
        public void AbsoluteLogButtonClicked()
        {
            int clk;
            try
            {
                clk = Convert.ToInt32(this.clkAbsoluteLog);
            }
            catch (Exception e)
            {
                this.errorInfo = "绝对记录clk不正确";
                return;
            }
            float duration;
            try
            {
                duration = Convert.ToSingle(this.durationAbsoluteLog);
            }
            catch (Exception e)
            {
                this.errorInfo = "绝对记录clk不正确";
                return;
            }
            modBehaviour.QuickAbsoluteLog(clk, duration);
        }
        public void ExecuteButtonClicked(bool forward)
        {
            int clk1;
            try
            {
                clk1 = Convert.ToInt32(this.executeClk);
            }
            catch (Exception e)
            {
                this.errorInfo = "执行clk不正确";
                return;
            }
            modBehaviour.TransformByClk(clk1, forward);
        }
        public void GenerateButtonClicked()
        {
            List<int> clks = new List<int>();
            string[] strs = this.clksControlModule.Split(',');
            try
            {
                foreach(var str in strs){
                    int clk = Convert.ToInt32(str);
                    clks.Add(clk);
                }
            }
            catch (Exception e)
            {
                this.errorInfo = "clk格式不正确";
                return;
            }
            this.modBehaviour.GenerateControlModule(clks);
        }
    }
}
