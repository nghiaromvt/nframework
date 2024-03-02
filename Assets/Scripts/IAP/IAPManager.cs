using System;
using System.Collections.Generic;
using UnityEngine;
#if USE_UNITY_PURCHASING
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
#endif

namespace NFramework.IAP
{
    public class IAPManager : SingletonMono<IAPManager>
#if USE_UNITY_PURCHASING
    , IDetailedStoreListener
#endif
    {
#if USE_UNITY_PURCHASING
        public static event Action<Product, string> OnPurchased;
#endif

        [SerializeField] protected List<IAPProductSO> _iapProductSOs = new List<IAPProductSO>();

#if USE_UNITY_PURCHASING
        private Dictionary<string, IAPProductSO> _idToIAPProductSODic = new Dictionary<string, IAPProductSO>();
        private IStoreController _controller;
        private IExtensionProvider _extensions;
        private bool _isPurchaseInProgress;
        private Action<bool> _purchaseCallback;
        private string _purchaseLocation;

        #region Init
        public void Init()
        {
            if (IsInitialized())
                return;

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var iapProductSO in _iapProductSOs)
            {
                var id = iapProductSO.id;
                _idToIAPProductSODic.Add(id, iapProductSO);
                builder.AddProduct(id, iapProductSO.productType, new IDs()
            {
                {iapProductSO.androidProductId, GooglePlay.Name},
                {iapProductSO.iosProductId, AppleAppStore.Name }
            });
            }
            UnityPurchasing.Initialize(this, builder);
        }

        private bool IsInitialized() => _controller != null && _extensions != null;

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            NFramework.Logger.Log("IAP Initialized", this);
            _controller = controller;
            _extensions = extensions;

            foreach (var iapProductSO in _iapProductSOs)
            {
                var product = _controller.products.WithID(iapProductSO.id);
                if (product != null)
                    iapProductSO.PriceString = product.metadata.localizedPriceString;
            }
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            NFramework.Logger.Log("IAP init failed");
            switch (error)
            {
                case InitializationFailureReason.AppNotKnown:
                    NFramework.Logger.LogError("Is your App correctly uploaded on the relevant publisher console?", this);
                    break;
                case InitializationFailureReason.PurchasingUnavailable:
                    // Ask the user if billing is disabled in device settings.
                    NFramework.Logger.Log("Billing disabled!", this);
                    break;
                case InitializationFailureReason.NoProductsAvailable:
                    // Developer configuration error; check product metadata.
                    NFramework.Logger.Log("No products available for purchase!", this);
                    break;
            }
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message) => NFramework.Logger.Log("IAP init failed: " + message, this);
        #endregion

        #region Purchase
        public void Purchase(IAPProductSO iapProductSO, Action<bool> callback, string location = "")
            => Purchase(iapProductSO.id, callback, location);

        public void Purchase(string iapProductSO, Action<bool> callback, string location = "")
        {
            if (Application.isEditor || DeviceInfo.IsTestIAP)
            {
                callback?.Invoke(true);
                return;
            }

            if (_isPurchaseInProgress)
            {
                callback?.Invoke(false);
                NFramework.Logger.Log("Another Purchase in progress", this);
                return;
            }

            if (IsInitialized())
            {
                var product = _controller.products.WithID(iapProductSO);
                if (product != null && product.availableToPurchase)
                {
                    _isPurchaseInProgress = true;
                    _purchaseCallback = callback;
                    _purchaseLocation = location;
                    ShowLoadingPayment();
                    NFramework.Logger.Log($"Purchasing product asynchronously: '{product.definition.storeSpecificId}'", this);
                    _controller.InitiatePurchase(product);
                }
                else
                {
                    callback?.Invoke(false);
                    NFramework.Logger.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase", this);
                    _isPurchaseInProgress = false;
                }
            }
            else
            {
                callback?.Invoke(false);
                NFramework.Logger.LogError($"Not IsInitialized", this);
                return;
            }
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var product = purchaseEvent.purchasedProduct;
            if (_idToIAPProductSODic.TryGetValue(product.definition.id, out var iapProductSO))
            {
                if (_isPurchaseInProgress)
                {
                    NFramework.Logger.Log($"Purchase OK: {product.hasReceipt} {product.transactionID} {product.definition.id}", this);
                    _isPurchaseInProgress = false;
                    HideLoadingPayment();
                    _purchaseCallback?.Invoke(true);
                    _purchaseCallback = null;
                    OnPurchased?.Invoke(product, _purchaseLocation);
                }
                else
                {
                    // We suppose its a restore if the method is called while _isPurchaseInProgress == false
                    NFramework.Logger.Log($"Restoring: {product.hasReceipt} {product.transactionID} {product.definition.id}", this);
                    ProcessRestore(iapProductSO);
                }
            }
            else
            {
                NFramework.Logger.LogError($"Cannot find iapProductSO id:{product.definition.id}", this);
                HideLoadingPayment();
                _purchaseCallback?.Invoke(true);
                _purchaseCallback = null;
            }
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            NFramework.Logger.LogError("IAP Purchase Failed: " + failureReason, this);
            _isPurchaseInProgress = false;
            HideLoadingPayment();
            _purchaseCallback?.Invoke(false);
            _purchaseCallback = null;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            NFramework.Logger.LogError("IAP Purchase Failed: " + failureDescription.message, this);
            _isPurchaseInProgress = false;
            HideLoadingPayment();
            _purchaseCallback?.Invoke(false);
            _purchaseCallback = null;
        }

        protected virtual void ProcessRestore(IAPProductSO iapProductSO) { }
        #endregion

        public IAPProductSO GetProductSO(string iapId)
        {
            if (_idToIAPProductSODic.TryGetValue(iapId, out var product))
                return product;

            NFramework.Logger.LogError($"iapId:{iapId} not found", this);
            return null;
        }

        protected virtual void ShowLoadingPayment() => NFramework.Logger.Log("ShowLoadingPayment");

        protected virtual void HideLoadingPayment() => NFramework.Logger.Log("HideLoadingPayment");
#endif
    }

    public class IAPRevenueData
    {
        public string productID;
        public string currency;
        public decimal price;
        public string transactionID;
        public string purchaseToken;
        public string location;

        public IAPRevenueData(string productID, string currency, decimal price, string transactionID, string purchaseToken, string location)
        {
            this.productID = productID;
            this.currency = currency;
            this.price = price;
            this.transactionID = transactionID;
            this.purchaseToken = purchaseToken;
            this.location = location;
        }
    }
}

