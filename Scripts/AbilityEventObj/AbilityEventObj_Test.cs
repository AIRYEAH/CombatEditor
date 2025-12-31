using UnityEngine;
//Replace the "Test" with the event you want to create
//If you want to create a object with handle in preview, please inherit the "AbilityEventObj" with "AbilityEventObj_CreateObjWithHandle"

namespace CombatEditor
{
    [AbilityEvent]
    [CreateAssetMenu(menuName = "AbilityEvents / Test")]
    public class AbilityEventObj_Test : AbilityEventObj
    {
        //Write the data you need here.
        public string testMsg = "Test";
        public override EventTimeType GetEventTimeType()
        {
            return EventTimeType.EventTime;
        }
        public override AbilityEventEffect Initialize()
        {
            return new AbilityEventEffect_Test(this);
        }

#if UNITY_EDITOR
        public override AbilityEventPreview InitializePreview()
        {
            return new AbilityEventPreview_Test(this);
        }
#endif
    }
    //Write you logic here
    public partial class AbilityEventEffect_Test : AbilityEventEffect
    {
        public PlayerManager player;
        public override void StartEffect()
        {
            base.StartEffect();
            player = _combatController.gameObject.GetComponent<PlayerManager>();
            player.testMsg = EventObj.testMsg;
            // Debug.Log(EventObj.testMsg);
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

    public partial class AbilityEventEffect_Test : AbilityEventEffect
    {
        AbilityEventObj_Test EventObj => (AbilityEventObj_Test)_EventObj;
        public AbilityEventEffect_Test(AbilityEventObj InitObj) : base(InitObj)
        {
            _EventObj = InitObj;
        }
    }
}