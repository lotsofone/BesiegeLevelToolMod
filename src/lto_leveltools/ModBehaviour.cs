using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using UnityEngine;

namespace lto_leveltools
{
    public class ModBehaviour : MonoBehaviour
    {
        public List<EntityLog> entityLogs = new List<EntityLog>();
        public List<LevelEntity> GetLevelEditorSelection()
        {
            return LevelEditor.Instance.Selection;
        }
        public Vector3 GetLevelEditorSelectionCenter()
        {
            return LevelEditor.Instance.selectionController.GetSelectionCenter();
        }
        public int GetLevellEditorSelectionCount()
        {
            return LevelEditor.Instance.SelectionCount;
        }
        public void LogSelection()
        {
            entityLogs = new List<EntityLog>();
            foreach(LevelEntity e in GetLevelEditorSelection())
            {
                entityLogs.Add(new EntityLog(e));
            }
        }
        public void GenerateControlModule(List<ClkDefination> clks)
        {
            LevelEditor.Instance.AddEntity(9003, this.GetLevelEditorSelectionCenter(), Quaternion.identity, new Vector3(1, 1, 1), true);
            var targets = LevelEditor.Instance.Selection;
            LevelEntity newEntity = LevelEditor.Instance.Entities[LevelEditor.Instance.Entities.Count - 1];
            //给生成物增加逻辑
            GenericEntity ge = newEntity.EntityBehaviour;
            ge.logicData.Clear();//先清除
            foreach (ClkDefination clk in clks) {
                EntityLogic trigger = new EntityLogic(TriggerType.Variable, ge);
                trigger.varCompare = EntityLogic.VarCompareType.Equals;
                trigger.varGlobal = clk.global;
                trigger.varKey = clk.key;
                trigger.varThreshold = clk.value;
                ge.logicData.Add(trigger);

                EntityEvent evt = new EntityEvent(EventContainer.EventType.Variable);
                var data = (EventContainer.VariableEvent)evt.eventData;
                data.key = clk.key;
                data.val = clk.value;
                data.modifyType = EventContainer.VarModifyType.Set;
                //var pick = (EventContainer.PickContainer)evt.eventData;
                //pick.pickTargets = targets;
                if (!clk.global)
                {
                    foreach (var e in targets)
                    {
                        evt.entityList.Add(e.identifier);
                    }
                }
                //evt.entityList

                trigger.events.Add(evt);
            }
            //设置选区，选中生成物
            LevelEditor.Instance.selectionController.Select(newEntity, false, false);
        }
        public EntityLogic FindClk(GenericEntity ge, ClkDefination clk)
        {
            foreach(EntityLogic el in ge.logicData)
            {
                if (el.triggerType == TriggerType.Variable && el.varCompare == EntityLogic.VarCompareType.Equals &&
                    Mathf.Approximately(el.varThreshold, clk.value) && el.varKey.Equals(clk.key) && el.varGlobal == clk.global)
                {
                    return el;
                }
            }
            return null;
        }
        //移除一个物体的一个clk触发器及其事件链
        public void RemoveClk(GenericEntity ge, ClkDefination clk)
        {
            for(int i=0; i<ge.logicData.Count;)
            {
                var el = ge.logicData[i];
                if (el.triggerType == TriggerType.Variable && el.varCompare == EntityLogic.VarCompareType.Equals &&
                    Mathf.Approximately(el.varThreshold, clk.value) && el.varKey.Equals(clk.key) && el.varGlobal == clk.global)
                {
                    ge.logicData.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
        //
        public void RemoveSelectionClk(ClkDefination clk)
        {
            foreach(LevelEntity e in GetLevelEditorSelection())
            {
                RemoveClk(e.behaviour, clk);
            }
        }
        //生成一个clk事件
        public EntityLogic GenerateClk(GenericEntity ge, ClkDefination clk)
        {
            var trigger = new EntityLogic(TriggerType.Variable, ge);
            trigger.varCompare = EntityLogic.VarCompareType.Equals;
            trigger.varGlobal = clk.global;
            trigger.varKey = clk.key;
            trigger.varThreshold = clk.value;
            ge.logicData.Add(trigger);
            return trigger;
        }
        //根据记录集合，生成移动事件
        public void GenerateEvent(bool forward, ClkDefination clk, float time, float timeRandom, string type = "LocalDirection")
        {
            foreach(var el in this.entityLogs)
            {
                LevelEntity e = el.entity;
                if(e is null)
                {
                    continue;
                }
                GenericEntity ge = e.EntityBehaviour;
                //获得这个物体中条件为clk=clk的trigger。如果没有就创建一个
                var trigger = FindClk(ge, clk);
                if(trigger is null)
                {
                    trigger = GenerateClk(ge, clk);
                }

                EntityEvent evt = new EntityEvent(EventContainer.EventType.Transform);
                var data = (EventContainer.TransformEvent)evt.eventData;
                data.lerpTime = UnityEngine.Random.Range(time - timeRandom, time + timeRandom);
                data.transformType = EventContainer.TransformEvent.TransformType.Lerp;

                Quaternion fromRotation, toRotation;
                Vector3 fromPosition, toPosition;
                if (forward)
                {
                    fromRotation = el.rotation;
                    fromPosition = el.position;
                    toRotation = e.transform.localRotation;
                    toPosition = e.transform.localPosition;
                }
                else
                {
                    fromRotation = e.transform.localRotation;
                    fromPosition = e.transform.localPosition;
                    toRotation = el.rotation;
                    toPosition = el.position;
                }
                if (type == "WorldDirection")
                {
                    data.positionType = EventContainer.TransformEvent.TransformPositionType.WorldDirection;
                    data.rotationType = EventContainer.TransformEvent.TransformRotationType.AroundWorldAxis;
                    data.rotation = toRotation  * Quaternion.Inverse(fromRotation);
                    data.eulerAngles = data.rotation.eulerAngles;
                    for (int i = 0; i < 3; i++)
                    {
                        if (data.eulerAngles[i] > 180)
                        {
                            data.eulerAngles[i] -= 360;
                        }
                    }
                    data.position = toPosition-fromPosition;
                }
                else if(type == "Absolute")
                {
                    data.positionType = EventContainer.TransformEvent.TransformPositionType.WorldPosition;
                    data.rotationType = EventContainer.TransformEvent.TransformRotationType.SetRotation;
                    data.eulerAngles = toRotation.eulerAngles;
                    data.position = toPosition;
                }
                else //LocalDirection
                {
                    //使用LocalDirection的话，lerptime必须是一个fu的整数倍
                    data.lerpTime = Mathf.Round(data.lerpTime * 100) / 100.0f;

                    data.positionType = EventContainer.TransformEvent.TransformPositionType.LocalDirection;
                    data.rotationType = EventContainer.TransformEvent.TransformRotationType.AroundLocalAxis;

                    data.rotation = Quaternion.Inverse(fromRotation) * toRotation;
                    data.eulerAngles = data.rotation.eulerAngles;
                    for (int i = 0; i < 3; i++)
                    {
                        if (data.eulerAngles[i] > 180)
                        {
                            data.eulerAngles[i] -= 360;
                        }
                    }
                    //计算3个坐标基变换后的位置
                    Vector3 xSum = Vector3.zero, ySum = Vector3.zero, zSum = Vector3.zero;
                    Vector3 x = new Vector3(1, 0, 0) * 0.01f / data.lerpTime;
                    Vector3 y = new Vector3(0, 1, 0) * 0.01f / data.lerpTime;
                    Vector3 z = new Vector3(0, 0, 1) * 0.01f / data.lerpTime;
                    //模拟fu
                    float pastTime = 0;
                    while (pastTime + 0.01f <= data.lerpTime+0.005f)
                    {
                        float process = pastTime / data.lerpTime;
                        Vector3 currentEular = data.eulerAngles * process;
                        Quaternion currentRotation = Quaternion.Euler(currentEular);
                        xSum += currentRotation * x;
                        ySum += currentRotation * y;
                        zSum += currentRotation * z;

                        pastTime += 0.01f;
                    }
                    //获得3个坐标基，现在根据他们求出需要移动多少位置才能到达目标点
                    var targetOffset = Quaternion.Inverse(fromRotation) * (toPosition - fromPosition);
                    Debug.Log(Vector3.Dot(xSum, ySum));
                    Debug.Log(Vector3.Dot(zSum, ySum));
                    Debug.Log(Vector3.Dot(xSum, zSum));
                    //这里目前没有写除以0的异常操作
                    Matrix4x4 matrix = Matrix4x4.identity;
                    matrix[0, 0] = xSum.x; matrix[0, 1] = ySum.x; matrix[0, 2] = zSum.x;
                    matrix[1, 0] = xSum.y; matrix[1, 1] = ySum.y; matrix[1, 2] = zSum.y;
                    matrix[2, 0] = xSum.z; matrix[2, 1] = ySum.z; matrix[2, 2] = zSum.z;
                    matrix = matrix.inverse;
                    data.position = matrix.MultiplyVector(targetOffset);
                    //var u = Vector3.Dot(targetOffset, xSum) / xSum.sqrMagnitude;
                    //var v = Vector3.Dot(targetOffset, ySum) / ySum.sqrMagnitude;
                    //var w = Vector3.Dot(targetOffset, zSum) / zSum.sqrMagnitude;
                    //data.position = new Vector3(u, v, w);
                }
                //插入新生成的event到events中
                if (forward)
                {
                    trigger.events.Add(evt);
                }
                else
                {
                    trigger.events.Insert(0, evt);
                }
            }
        }
        //给选区中的entity生成等待事件
        public void GenerateWaitEvent(bool forward, ClkDefination clk, float time, float timeRandom)
        {
            foreach (var e in this.GetLevelEditorSelection())
            {
                if (e is null)
                {
                    continue;
                }
                GenericEntity ge = e.EntityBehaviour;
                //获得这个物体中条件为clk=clk的trigger。如果没有就创建一个
                var trigger = FindClk(ge, clk);
                if (trigger is null)
                {
                    trigger = new EntityLogic(TriggerType.Variable, ge);
                    trigger.varCompare = EntityLogic.VarCompareType.Equals;
                    trigger.varGlobal = clk.global;
                    trigger.varKey = clk.key;
                    trigger.varThreshold = clk.value;
                    ge.logicData.Add(trigger);
                }
                EntityEvent evt = new EntityEvent(EventContainer.EventType.Wait);
                var data = (EventContainer.WaitEvent)evt.eventData;
                data.waitTime = UnityEngine.Random.Range(time - timeRandom, time + timeRandom);
                if (forward)
                {
                    trigger.events.Add(evt);
                }
                else
                {
                    trigger.events.Insert(0, evt);
                }
            }
        }
        //执行某个clk，使其在编辑模式进行运动
        public void TransformByClk(ClkDefination clk, bool forward)
        {
            foreach(var entity in this.GetLevelEditorSelection())
            {
                var entityLogic = FindClk(entity.behaviour, clk);
                if (entityLogic is null) continue;
                if (forward)
                {
                    for(int i=0; i<entityLogic.events.Count; i++)
                    {
                        TransformEntityByEvent(entity, entityLogic.events[i], forward);
                    }
                }
                else
                {
                    for (int i = entityLogic.events.Count-1 ;i>=0; i--)
                    {
                        TransformEntityByEvent(entity, entityLogic.events[i], forward);
                    }
                }
            }
        }
        public void TransformEntityByEvent(LevelEntity entity, EntityEvent evt, bool forward){
            if (evt.eventType != EventContainer.EventType.Transform)
            {
                return;
            }
            var data = (EventContainer.TransformEvent)evt.eventData;
            //先做平移
            switch (data.positionType)
            {
                case EventContainer.TransformEvent.TransformPositionType.WorldPosition:
                    entity.SetPosition(data.position);
                    break;
                case EventContainer.TransformEvent.TransformPositionType.WorldDirection:
                    entity.SetPosition(entity.Position + (forward ? data.position : -data.position));
                    break;
                case EventContainer.TransformEvent.TransformPositionType.LocalDirection:
                    //使用LocalDirection的话，lerptime必须是一个fu的整数倍
                    data.lerpTime = Mathf.Round(data.lerpTime * 100) / 100.0f;

                    if (forward)
                    {
                        //模拟fu，求出世界空间下的位移
                        Vector3 offset = Vector3.zero;
                        Vector3 step = data.position * 0.01f / data.lerpTime;
                        float pastTime = 0;
                        while (pastTime + 0.01f <= data.lerpTime + 0.005f)
                        {
                            float process = pastTime / data.lerpTime;
                            Quaternion currentRotation;
                            if (data.rotationType == EventContainer.TransformEvent.TransformRotationType.SetRotation)
                            {
                                currentRotation = Quaternion.Lerp(entity.Rotation, Quaternion.Euler(data.eulerAngles), process);
                            }
                            else if(data.rotationType == EventContainer.TransformEvent.TransformRotationType.AroundWorldAxis)
                            {
                                Vector3 currentEular = data.eulerAngles * process;
                                currentRotation = Quaternion.Euler(currentEular) * entity.Rotation;
                            }
                            else
                            {
                                Vector3 currentEular = data.eulerAngles * process;
                                currentRotation = entity.Rotation * Quaternion.Euler(currentEular);
                            }
                            offset += currentRotation * step;

                            pastTime += 0.01f;
                        }
                        entity.SetPosition(entity.Position + offset);
                    }
                    else//反向
                    {
                        //先获得原来的旋转
                        Quaternion previousRotation;
                        switch (data.rotationType)
                        {
                            case EventContainer.TransformEvent.TransformRotationType.SetRotation:
                                previousRotation = Quaternion.Euler(data.eulerAngles);
                                break;
                            case EventContainer.TransformEvent.TransformRotationType.AroundWorldAxis:
                                previousRotation = (Quaternion.Inverse(Quaternion.Euler(data.eulerAngles)) * entity.Rotation);
                                break;
                            default:
                                previousRotation = entity.Rotation * Quaternion.Inverse(Quaternion.Euler(data.eulerAngles));
                                break;
                        }
                        //从原来的旋转模拟fu，求出世界空间下的位移
                        Vector3 offset = Vector3.zero;
                        Vector3 step = data.position * 0.01f / data.lerpTime;
                        float pastTime = 0;
                        while (pastTime + 0.01f <= data.lerpTime + 0.005f)
                        {
                            float process = pastTime / data.lerpTime;
                            Quaternion currentRotation;
                            if (data.rotationType == EventContainer.TransformEvent.TransformRotationType.SetRotation)
                            {
                                currentRotation = Quaternion.Euler(data.eulerAngles);
                            }
                            else if (data.rotationType == EventContainer.TransformEvent.TransformRotationType.AroundWorldAxis)
                            {
                                Vector3 currentEular = data.eulerAngles * process;
                                currentRotation = Quaternion.Euler(currentEular) * previousRotation;
                            }
                            else
                            {
                                Vector3 currentEular = data.eulerAngles * process;
                                currentRotation = previousRotation * Quaternion.Euler(currentEular);
                            }
                            offset += currentRotation * step;

                            pastTime += 0.01f;
                        }
                        entity.SetPosition(entity.Position - offset);
                    }
                    break;
            }
            //做旋转
            Quaternion rotation = Quaternion.Euler(data.eulerAngles);
            switch (data.rotationType)
            {
                case EventContainer.TransformEvent.TransformRotationType.SetRotation:
                    entity.SetRotation(rotation);
                    break;
                case EventContainer.TransformEvent.TransformRotationType.AroundWorldAxis:
                    if (forward)
                    {
                        entity.SetRotation(rotation * entity.Rotation);
                    }
                    else
                    {
                        entity.SetRotation(Quaternion.Inverse(rotation) * entity.Rotation);
                    }
                    break;
                case EventContainer.TransformEvent.TransformRotationType.AroundLocalAxis:
                    if (forward)
                    {
                        entity.SetRotation(entity.Rotation * rotation);
                    }
                    else
                    {
                        entity.SetRotation(entity.Rotation * Quaternion.Inverse(rotation));
                    }
                    break;
            }
        }
        //快速绝对记录
        public void QuickAbsoluteLog(ClkDefination clk, float duration)
        {
            foreach(var entity in this.GetLevelEditorSelection())
            {
                EntityLogic trigger = FindClk(entity.behaviour, clk);
                if(trigger is null)
                {
                    trigger = GenerateClk(entity.behaviour, clk);
                }
                trigger.events.Clear();
                EntityEvent evt = new EntityEvent(EventContainer.EventType.Transform);
                var data = (EventContainer.TransformEvent)evt.eventData;
                data.lerpTime = duration;
                data.transformType = EventContainer.TransformEvent.TransformType.Lerp;

                Quaternion toRotation;
                Vector3 toPosition;
                toRotation = entity.transform.localRotation;
                toPosition = entity.transform.localPosition;
                data.positionType = EventContainer.TransformEvent.TransformPositionType.WorldPosition;
                data.rotationType = EventContainer.TransformEvent.TransformRotationType.SetRotation;
                data.eulerAngles = toRotation.eulerAngles;
                data.position = toPosition;
                trigger.events.Add(evt);
            }
        }
        //将选区的事件进行缩放
        public void ScaleSelection(float scaleMultiplier)
        {
            foreach(LevelEntity entity in GetLevelEditorSelection())
            {
                var ge = entity.behaviour;
                foreach(EntityLogic trigger in ge.logicData)
                {
                    foreach(var evt in trigger.events)
                    {
                        if(evt.eventType == EventContainer.EventType.Transform)
                        {
                            var data = (EventContainer.TransformEvent)evt.eventData;
                            data.position *= scaleMultiplier;
                        }
                    }
                }
            }
        }
        public class EntityLog
        {
            public LevelEntity entity;
            public Quaternion rotation;
            public Vector3 position;
            public EntityLog(LevelEntity e)
            {
                this.entity = e;
                this.position = e.transform.localPosition;
                this.rotation = e.transform.localRotation;
            }
        }
    }
}
