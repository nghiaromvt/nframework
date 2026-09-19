using System;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework.Core.Tests
{
    public enum SampleItemRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    [Serializable]
    public class SampleItemData
    {
        [TSVColumn("Item ID")]
        public int Id;

        [TSVColumn("Item Name")]
        public string Name { get; set; }

        [SerializeField]
        private float _price;

        public float Price => _price;

        public bool IsAvailable;

        public SampleItemRarity Rarity;

        public List<string> Attributes;
    }

    [CreateAssetMenu(fileName = "SampleItemTableSO", menuName = "NFramework/Samples/SampleItemTableSO")]
    public class SampleItemTableSO : TsvConfigSO<SampleItemData>
    {
        public bool AfterFetchedCalled { get; private set; }

        protected override void OnAfterFetched()
        {
            base.OnAfterFetched();
            AfterFetchedCalled = true;
        }
    }
}
