using System;
using System.Collections.Generic;
using System.Linq;
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
        private IStoreController _controller;
        private IExtensionProvider _extensions;
        private bool _isPurchaseInProgress;
        private Action<bool> _purchaseCallback;
        private string _purchaseLocation;
        private string _curPurchaseProductId;

        #region Init
        public void Init()
        {
            if (IsInitialized())
                return;

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var iapProductSO in _iapProductSOs)
            {
                var id = iapProductSO.id;
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
            Logger.Log("IAP Initialized", this);
            _controller = controller;
            _extensions = extensions;

            foreach (var iapProductSO in _iapProductSOs)
            {
                var product = _controller.products.WithID(iapProductSO.id);
                if (product == null)
                    Logger.LogError($"Add product fail! id:{iapProductSO.id}");
                else
                    iapProductSO.PriceString = product.metadata.localizedPriceString;
            }
        }

        // Obsolete
        public void OnInitializeFailed(InitializationFailureReason error) { }

        public void OnInitializeFailed(InitializationFailureReason error, string message) => Logger.LogError("IAP init failed: " + message, this);
        #endregion

        #region Purchase
        public void Purchase(IAPProductSO iapProductSO, Action<bool> callback, string location = "")
            => Purchase(iapProductSO.id, callback, location);

        public void Purchase(string productId, Action<bool> callback, string location = "")
        {
            if (Application.isEditor || DeviceInfo.IsTestIAP)
            {
                callback?.Invoke(true);
                return;
            }

            if (_isPurchaseInProgress)
            {
                callback?.Invoke(false);
                Logger.LogError("Another Purchase in progress", this);
                return;
            }

            if (IsInitialized())
            {
                var product = _controller.products.WithID(productId);
                if (product != null && product.availableToPurchase)
                {
                    _isPurchaseInProgress = true;
                    _purchaseCallback = callback;
                    _purchaseLocation = location;
                    _curPurchaseProductId = productId;
                    ShowLoadingPayment();
                    Logger.Log($"Purchasing product asynchronously: id:{product.definition.id}", this);
                    _controller.InitiatePurchase(product);
                }
                else
                {
                    callback?.Invoke(false);
                    Logger.Log($"BuyProductID: FAIL. Not purchasing product, product not found:{product == null}", this);
                    _isPurchaseInProgress = false;
                }
            }
            else
            {
                callback?.Invoke(false);
                Logger.LogError($"Not IsInitialized", this);
                return;
            }
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var purchasedProduct = purchaseEvent.purchasedProduct;
            Logger.Log($"Start ProcessPurchase purchasedProductId:{purchasedProduct.definition.id}");

            var product = _controller.products.WithID(purchasedProduct.definition.id);

            if (_isPurchaseInProgress)
            {
                if (product == null)
                {
                    Logger.LogError($"ProcessPurchase failed! id:{purchasedProduct.definition.id} - reason:ERROR_UNKNOWN_PRODUCT");
                    FinishPurchaseFail();
                    return PurchaseProcessingResult.Pending;
                }
                else if (_curPurchaseProductId != product.definition.id)
                {
                    Logger.LogError($"ProcessPurchase failed! id:{purchaseEvent.purchasedProduct.definition.id} - reason:ERROR_WRONG_PRODUCT_ID");
                    FinishPurchaseFail();
                    return PurchaseProcessingResult.Pending;
                }
                else
                {
                    Logger.Log($"Purchase OK => hasReceipt:{purchasedProduct.hasReceipt} - transactionID:{purchasedProduct.transactionID} - id:{purchasedProduct.definition.id}", this);
                    FinishPurchaseSuccess();
                    OnPurchased?.Invoke(purchasedProduct, _purchaseLocation);
                    return PurchaseProcessingResult.Complete;
                }
            }
            else
            {
                // We suppose its a restore if the method is called while _isPurchaseInProgress == false
                if (product == null)
                {
                    Logger.Log($"Skip restore => reason:ERROR_UNKNOWN_PRODUCT - purchasedProduct:{purchasedProduct.definition.id}", this);
                }
                else
                {
                    Logger.Log($"Restoring => hasReceipt:{purchasedProduct.hasReceipt} - transactionID:{purchasedProduct.transactionID} - id:{purchasedProduct.definition.id}", this);
                    ProcessRestore(product.definition.id);
                }
                return PurchaseProcessingResult.Complete;
            }
        }

        // Obsolete
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) { }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Logger.LogError("IAP Purchase Failed: " + failureDescription.message, this);
            FinishPurchaseFail();
        }

        protected virtual void ProcessRestore(string productId) { }

        private void FinishPurchaseSuccess()
        {
            HideLoadingPayment();
            _isPurchaseInProgress = false;
            _purchaseCallback?.Invoke(true);
        }

        private void FinishPurchaseFail()
        {
            HideLoadingPayment();
            _isPurchaseInProgress = false;
            _purchaseCallback?.Invoke(false);
        }
        #endregion

        public IAPProductSO GetProductSO(string productId) => _iapProductSOs.FirstOrDefault(x => x.id == productId);

        protected virtual void ShowLoadingPayment() => UIManager.I.DisableInteract();

        protected virtual void HideLoadingPayment() => UIManager.I.EnableInteract();
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

