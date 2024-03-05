using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NFramework.Editors
{
    public class ModifyDefineSymbols : ScriptableWizard
    {
        public const string DEVELOPMENT_SYMBOL = "DEVELOPMENT";
        public const string FIREBASE_SYMBOL = "USE_FIREBASE";
        public const string UNITY_PURCHASING_SYMBOL = "USE_UNITY_PURCHASING";
        public const string FIREBASE_REMOTECONFIG_SYMBOL = "USE_FIREBASE_REMOTECONFIG";
        public const string NO_ADS_SYMBOL = "NO_ADS";
        public const string IRONSOURCE_ADS_SYMBOL = "USE_IRONSOURCE_ADS";
        public const string IRONSOURCE_AD_QUALITY_SYMBOL = "USE_IRONSOURCE_AD_QUALITY";
        public const string APPLOVIN_ADS_SYMBOL = "USE_APPLOVIN_ADS";
        public const string ADMOB_ADS_SYMBOL = "USE_ADMOB_ADS";
        public const string NO_TRACKING_SYMBOL = "NO_TRACKING";
        public const string FIREBASE_ANALYTICS_SYMBOL = "USE_FIREBASE_ANALYTICS";
        public const string APPSFLYER_SYMBOL = "USE_APPSFLYER";
        public const string FIREBASE_CRASHLYTICS_SYMBOL = "USE_FIREBASE_CRASHLYTICS";
        public const string ADJUST_ANALYTICS_SYMBOL = "USE_ADJUST_ANALYTICS";

        [Separator("Development")]
        [SerializeField] private bool _isDevelopment;
        [Separator("Remote Config")]
        [SerializeField] private bool _useFirebaseRemoteConfig;
        [Separator("IAP")]
        [SerializeField] private bool _useUnityPurchasing;
        [Separator("Ads")]
        [SerializeField] private bool _isNoAds;
        [SerializeField] private bool _useIronSourceAds;
        [SerializeField, ConditionalField(nameof(_useIronSourceAds))] private bool _useIronSourceAdQuality;
        [SerializeField] private bool _useAppLovinAds;
        [SerializeField] private bool _useAdMobAds;
        [Separator("Tracking")]
        [SerializeField] private bool _isNoTracking;
        [SerializeField] private bool _useFirebaseAnalytics;
        [SerializeField] private bool _useAppsFlyer;
        [SerializeField] private bool _useFirebaseCrashlytics;
        [SerializeField] private bool _useAdjustAnalytics;


        [MenuItem("NFramework/Modify Define Symbols", priority = 100)]
        private static void ScriptableWizardMenu()
        {
            var wizard = DisplayWizard<ModifyDefineSymbols>("Modify Define Symbols", "Apply");

            wizard.helpString = "Use this tool to modify define symbols.";

            var scriptingDefinesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var scriptingDefinesStringList = scriptingDefinesString.Split(';').ToList();

            wizard._isDevelopment = scriptingDefinesStringList.Contains(DEVELOPMENT_SYMBOL);
            wizard._useFirebaseRemoteConfig = scriptingDefinesStringList.Contains(FIREBASE_REMOTECONFIG_SYMBOL) && scriptingDefinesStringList.Contains(FIREBASE_SYMBOL);
            wizard._useUnityPurchasing = scriptingDefinesStringList.Contains(UNITY_PURCHASING_SYMBOL);
            wizard._isNoAds = scriptingDefinesStringList.Contains(NO_ADS_SYMBOL);
            wizard._useIronSourceAds = scriptingDefinesStringList.Contains(IRONSOURCE_ADS_SYMBOL);
            wizard._useIronSourceAdQuality = scriptingDefinesStringList.Contains(IRONSOURCE_AD_QUALITY_SYMBOL);
            wizard._useAppLovinAds = scriptingDefinesStringList.Contains(APPLOVIN_ADS_SYMBOL);
            wizard._useAdMobAds = scriptingDefinesStringList.Contains(ADMOB_ADS_SYMBOL);
            wizard._isNoTracking = scriptingDefinesStringList.Contains(NO_TRACKING_SYMBOL);
            wizard._useFirebaseAnalytics = scriptingDefinesStringList.Contains(FIREBASE_ANALYTICS_SYMBOL) && scriptingDefinesStringList.Contains(FIREBASE_SYMBOL);
            wizard._useAppsFlyer = scriptingDefinesStringList.Contains(APPSFLYER_SYMBOL);
            wizard._useFirebaseCrashlytics = scriptingDefinesStringList.Contains(FIREBASE_CRASHLYTICS_SYMBOL) && scriptingDefinesStringList.Contains(FIREBASE_SYMBOL);
            wizard._useAdjustAnalytics = scriptingDefinesStringList.Contains(ADJUST_ANALYTICS_SYMBOL);

        }

        private void OnWizardCreate()
        {
            var scriptingDefinesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var scriptingDefinesStringHashSet = scriptingDefinesString.Split(';').ToHashSet();

            if (_isDevelopment)
                scriptingDefinesStringHashSet.Add(DEVELOPMENT_SYMBOL);
            else
                scriptingDefinesStringHashSet.Remove(DEVELOPMENT_SYMBOL);

            if (_useFirebaseRemoteConfig || _useFirebaseAnalytics || _useFirebaseCrashlytics)
                scriptingDefinesStringHashSet.Add(FIREBASE_SYMBOL);
            else
                scriptingDefinesStringHashSet.Remove(FIREBASE_SYMBOL);

            if (_useFirebaseRemoteConfig)
                scriptingDefinesStringHashSet.Add(FIREBASE_REMOTECONFIG_SYMBOL);
            else
                scriptingDefinesStringHashSet.Remove(FIREBASE_REMOTECONFIG_SYMBOL);

            if (_useUnityPurchasing)
                scriptingDefinesStringHashSet.Add(UNITY_PURCHASING_SYMBOL);
            else
                scriptingDefinesStringHashSet.Remove(UNITY_PURCHASING_SYMBOL);

            if (_isNoAds)
                scriptingDefinesStringHashSet.Add(NO_ADS_SYMBOL);
            else
                scriptingDefinesStringHashSet.Remove(NO_ADS_SYMBOL);

            if (_useIronSourceAds)
            {
                scriptingDefinesStringHashSet.Add(IRONSOURCE_ADS_SYMBOL);
                if (_useIronSourceAdQuality)
                    scriptingDefinesStringHashSet.Add(IRONSOURCE_AD_QUALITY_SYMBOL);
                else
                    scriptingDefinesStringHashSet.Remove(IRONSOURCE_AD_QUALITY_SYMBOL);
            }
            else
            {
                scriptingDefinesStringHashSet.Remove(IRONSOURCE_ADS_SYMBOL);
                scriptingDefinesStringHashSet.Remove(IRONSOURCE_AD_QUALITY_SYMBOL);
            }

            if (_useAppLovinAds)
                scriptingDefinesStringHashSet.Add(APPLOVIN_ADS_SYMBOL);
            else
                scriptingDefinesStringHashSet.Remove(APPLOVIN_ADS_SYMBOL);

            if (_useAdMobAds)
                scriptingDefinesStringHashSet.Add(ADMOB_ADS_SYMBOL);
            else
                scriptingDefinesStringHashSet.Remove(ADMOB_ADS_SYMBOL);

            if (_isNoTracking)
                scriptingDefinesStringHashSet.Add(NO_TRACKING_SYMBOL);
            else
                scriptingDefinesStringHashSet.Remove(NO_TRACKING_SYMBOL);

            if (_useFirebaseAnalytics)
                scriptingDefinesStringHashSet.Add(FIREBASE_ANALYTICS_SYMBOL);
            else
                scriptingDefinesStringHashSet.Remove(FIREBASE_ANALYTICS_SYMBOL);

            if (_useAppsFlyer)
                scriptingDefinesStringHashSet.Add(APPSFLYER_SYMBOL);
            else
                scriptingDefinesStringHashSet.Remove(APPSFLYER_SYMBOL);

            if (_useFirebaseCrashlytics)
                scriptingDefinesStringHashSet.Add(FIREBASE_CRASHLYTICS_SYMBOL);
            else
                scriptingDefinesStringHashSet.Remove(FIREBASE_CRASHLYTICS_SYMBOL);

            if (_useAdjustAnalytics)
                scriptingDefinesStringHashSet.Add(ADJUST_ANALYTICS_SYMBOL);
            else
                scriptingDefinesStringHashSet.Remove(ADJUST_ANALYTICS_SYMBOL);
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", scriptingDefinesStringHashSet.ToArray()));
        }
    }
}
