using UnityEngine;
//Replace the "PrintLog" with the event you want to create
//If you want to create a object with handle in preview, please inherit the "AbilityEventObj" with "AbilityEventObj_CreateObjWithHandle"

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / PrintLog")]
    public class AbilityEventObj_PrintLog : AbilityEventObj
    {
        //Write the data you need here.
        public string text;
        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventTime;
        }
        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_PrintLog(this);
        }
#if UNITY_EDITOR
        public override AbilityEventPreview InitializePreview()
        {
            return new AbilityEventPreview_PrintLog(this);
        }
#endif
    }
    //Write you logic here
    public partial class AbilityEventEffect_PrintLog : AbilityEventEffect
    {
        public override void StartEffect()
        {
            base.StartEffect();
            Debug.Log(EventObj.text);
        }
        public override void EffectRunning()
        {
            base.EffectRunning();
        }
        public override void EndEffect()
        {
            base.EndEffect();
        }
    }

    public partial class AbilityEventEffect_PrintLog : AbilityEventEffect
    {
        AbilityEventObj_PrintLog EventObj => (AbilityEventObj_PrintLog)_EventObj;
        public AbilityEventEffect_PrintLog(AbilityEventObj InitObj) : base(InitObj)
        {
            _EventObj = InitObj;
        }
    }
}