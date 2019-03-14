using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using UnityEngine;

namespace lto_leveltools
{
    class ToolBoxBehaviour : SafeUIBehaviour
    {
        private ModBehaviour modBehaviour;
        

        public int varMode = 0;//变量模式，0：局部变量，1：全局变量

        public bool generateForward = false;
        public bool generateBackward = true;
        public string clkForward = "  0";
        public string clkBackward = "  1";

        public int relativeMode = 2;

        public bool logAfterGenerate = true;
        public string duration = "  1";
        public string durationRandom = "  0";
        public string userInfo = "";

        public string wait = "  1";
        public string waitRandom = "  0";

        public string clksControlModule = "  1";

        public string executeClk = "  1";

        public string clkAbsoluteLog = " -1";
        public string durationAbsoluteLog = "  1";

        public string scaleMultiplier = "  1";

        public Action OnLogButtonClick;
        public Action OnGenerateButtonClick;

        public ClkDefination GetClkDefination(int clk)
        {
            return new ClkDefination("clk", clk, this.varMode == 0 ? false : true);
        }

        void Awake()
        {
            this.modBehaviour = this.gameObject.transform.parent.gameObject.GetComponent<ModBehaviour>();
            this.OnLogButtonClick += this.modBehaviour.LogSelection;
            this.OnGenerateButtonClick += this.GenerateTransformEvent;
        }
        protected override void Start()
        {
            base.Start();
            this.name = "地形工具";
            this.windowRect = new Rect(20, 300, 150, 200);
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
                this.userInfo = "顺行clk不正确";
                return;
            }
            int clk2;
            try
            {
                clk2 = Convert.ToInt32(this.clkBackward);
            }
            catch (Exception e)
            {
                this.userInfo = "逆行clk不正确";
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
                this.userInfo = "持续时间不正确";
                return;
            }
            try
            {
                timeRandom = Convert.ToSingle(this.durationRandom);
            }
            catch (Exception e)
            {
                this.userInfo = "持续时间随机不正确";
                return;
            }
            string type = this.relativeMode == 0 ? "Absolute" : (this.relativeMode == 1 ? "WorldDirection" : "LocalDirection");
            if (generateForward)
            {
                modBehaviour.GenerateEvent(true, GetClkDefination(clk1), time, timeRandom, type);
            }
            if (generateBackward)
            {
                modBehaviour.GenerateEvent(false, GetClkDefination(clk2), time, timeRandom, type);
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
                this.userInfo = "顺行clk不正确";
                return;
            }
            int clk2;
            try
            {
                clk2 = Convert.ToInt32(this.clkBackward);
            }
            catch (Exception e)
            {
                this.userInfo = "逆行clk不正确";
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
                this.userInfo = "等待时间不正确";
                return;
            }
            try
            {
                timeRandom = Convert.ToSingle(this.waitRandom);
            }
            catch (Exception e)
            {
                this.userInfo = "等待时间随机不正确";
                return;
            }
            if (generateForward)
            {
                modBehaviour.GenerateWaitEvent(true, GetClkDefination(clk1), time, timeRandom);
            }
            if (generateBackward)
            {
                modBehaviour.GenerateWaitEvent(false, GetClkDefination(clk2), time, timeRandom);
            }
        }

        public override bool ShouldShowGUI()
        {
            return StatMaster.isMP && !StatMaster.levelSimulating && !StatMaster.inMenu;
        }
        
        protected override void WindowContent(int id)
        {
            this.varMode = GUILayout.SelectionGrid(this.varMode, new string[] { "局部变量", "全局变量"}, 2);
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("选中地形:" + modBehaviour.GetLevellEditorSelectionCount().ToString());
                GUILayout.Label("记录地形:" + modBehaviour.entityLogs.Count);
                GUILayout.EndVertical();
            }
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
                if (GUILayout.Button("移除clk"))
                {
                    this.RemoveClkClicked();
                }

            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            {
                this.scaleMultiplier = GUILayout.TextField(this.scaleMultiplier);
                if (GUILayout.Button("变换缩放"))
                {
                    this.ScaleButtonClicked();
                }

            }
            GUILayout.EndHorizontal();

            GUILayout.Label(this.userInfo);

            GUILayout.EndVertical();
            GUI.DragWindow();
        }
        public void ScaleButtonClicked()
        {
            float scale;
            try
            {
                scale = Convert.ToSingle(this.scaleMultiplier);
            }
            catch (Exception e)
            {
                this.userInfo = "缩放比例格式不正确";
                return;
            }
            modBehaviour.ScaleSelection(scale);
            this.userInfo = "选区的移动事件缩放为了" + scale.ToString() + "倍";
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
                this.userInfo = "绝对记录clk不正确";
                return;
            }
            float duration;
            try
            {
                duration = Convert.ToSingle(this.durationAbsoluteLog);
            }
            catch (Exception e)
            {
                this.userInfo = "绝对记录clk不正确";
                return;
            }
            modBehaviour.QuickAbsoluteLog(GetClkDefination(clk), duration);
            this.userInfo = "当前坐标已经记录在clk=" + clk.ToString() + "中";
        }
        public void RemoveClkClicked()
        {
            int clk;
            try
            {
                clk = Convert.ToInt32(this.clkAbsoluteLog);
            }
            catch (Exception e)
            {
                this.userInfo = "绝对记录clk不正确";
                return;
            }
            modBehaviour.RemoveSelectionClk(GetClkDefination(clk));
            this.userInfo = "移除了选区clk=" + clk.ToString() + "的触发器";
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
                this.userInfo = "执行clk不正确";
                return;
            }
            modBehaviour.TransformByClk(GetClkDefination(clk1), forward);
            this.userInfo = "执行了clk=" + clk1.ToString() + "的触发器";
        }
        public void GenerateButtonClicked()
        {
            List<ClkDefination> clks = new List<ClkDefination>();
            string[] strs = this.clksControlModule.Split(',');
            try
            {
                foreach(var str in strs){
                    int clk = Convert.ToInt32(str);
                    clks.Add(GetClkDefination(clk));
                }
            }
            catch (Exception e)
            {
                this.userInfo = "clk格式不正确";
                return;
            }
            this.modBehaviour.GenerateControlModule(clks);
            this.userInfo = "生成了clk=" + clksControlModule + "的控制模块";
        }
    }
}
