using System.Collections.Generic;
using UnityEngine;

namespace CombatEditor
{
    [CreateAssetMenu(menuName = "Combat/Combat Data Storage")]
    public class CombatDataStorage : ScriptableObject
    {
        [SerializeField]
        public List<CombatGroup> CombatDatas = new List<CombatGroup>();

        /// <summary>
        /// 添加战斗组
        /// </summary>
        public void AddCombatGroup(CombatGroup group)
        {
            CombatDatas.Add(group);
        }

        /// <summary>
        /// 移除战斗组
        /// </summary>
        public void RemoveCombatGroup(CombatGroup group)
        {
            CombatDatas.Remove(group);
        }

        /// <summary>
        /// 清空所有战斗数据
        /// </summary>
        public void ClearCombatDatas()
        {
            CombatDatas.Clear();
        }

        /// <summary>
        /// 获取战斗组数量
        /// </summary>
        public int GetCombatGroupCount()
        {
            return CombatDatas.Count;
        }

        /// <summary>
        /// 获取指定索引的战斗组
        /// </summary>
        public CombatGroup GetCombatGroup(int index)
        {
            if (index >= 0 && index < CombatDatas.Count)
            {
                return CombatDatas[index];
            }
            return null;
        }
    }
}