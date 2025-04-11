using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Managers
{
    public class DataManager : Singleton<DataManager>
    {
        //Load ScriptableObject
        public async UniTask<T> LoadDataAsync<T>(string address) where T : ScriptableObject
        {
            return await DataLoader.LoadAsync<T>(address);
        }

        //Load Prefab
        public async UniTask<GameObject> LoadPrefabAsync(string address)
        {
            return await PrefabLoader.LoadAsync(address);
        }

        public RectTransform LoadUIPrefab(string address)
        {
            return PrefabLoader.LoadUI(address);
        }
        
        private static class DataLoader
        {
            //cache
            private static readonly Dictionary<string, ScriptableObject> _caches = new();

            public static async UniTask<T> LoadAsync<T>(string address) where T : ScriptableObject
            {
                if (_caches.TryGetValue(address, out var cached))
                {
                    return cached as T;
                }

                var asset = await Addressables.LoadAssetAsync<T>(address).ToUniTask();
                _caches[address] = asset;
                return asset;
            }

            public static void ClearCache()
            {
                foreach (var asset in _caches.Values)
                {
                    Addressables.Release(asset);
                }
                _caches.Clear();
            }

            //Debug
            public static int CachedCount => _caches.Count;
        }
        
        private static class PrefabLoader
        {
            //cache
            private static readonly Dictionary<string, GameObject> _caches = new();

            public static async UniTask<GameObject> LoadAsync(string address)
            {
                if (_caches.TryGetValue(address, out var cached))
                {
                    return cached;
                }

                var asset = await Addressables.LoadAssetAsync<GameObject>(address).ToUniTask();
                _caches[address] = asset;
                return asset;
            }
            
            public static RectTransform LoadUI(string address)
            {
                var request = Addressables.LoadAssetAsync<GameObject>(address);
                request.WaitForCompletion();
    
                if (request.Status == AsyncOperationStatus.Succeeded)
                    return request.Result.GetComponent<RectTransform>();
        
                Debug.LogError($"Failed to load UI prefab: {address}");
                return null;
            }

            public static void ClearCache()
            {
                foreach (var asset in _caches.Values)
                {
                    Addressables.Release(asset);
                }
                _caches.Clear();
            }

            //Debug
            public static int CachedCount => _caches.Count;
        }
    }
}