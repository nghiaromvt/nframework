using System;
using UnityEngine;

namespace NFramework.IAP
{
    [CreateAssetMenu(menuName = "ScriptableObject/IAPProductSO")]
    public class IAPProductSO : ScriptableObject
    {
        public event Action<string> OnPriceStringChanged;

        public string id;
        public string androidProductId;
        public string iosProductId;
#if USE_UNITY_PURCHASING
        public UnityEngine.Purchasing.ProductType productType;
#endif
        public float defaultPrice;

        private string _priceString;
        public string PriceString
        {
            get
            {
                if (string.IsNullOrEmpty(_priceString))
                    return $"{defaultPrice}$";
                else
                    return _priceString;
            }
            set
            {
                _priceString = value;
                OnPriceStringChanged?.Invoke(value);
            }
        }
    }
}

