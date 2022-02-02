using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AddressablesSystem
{
    [Serializable]
    public class ComponentReference<TComponent> : AssetReference where TComponent : MonoBehaviour
    {
        public ComponentReference(string guid) : base(guid)
        {
        }
    
        public new async Task<AsyncOperationHandle<TComponent>> InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var op = await AddressablesManager.InstantiateAsync<TComponent>(this);

            var tf = op.Result.transform;
            
            tf.position = position;
            tf.rotation = rotation;
            tf.SetParent(parent);

            return op;
        }
   
        public new async Task<AsyncOperationHandle<TComponent>> InstantiateAsync(Transform parent = null, bool instantiateInWorldSpace = false)
        {
            var op = await AddressablesManager.InstantiateAsync<TComponent>(this);

            var tf = op.Result.transform;
            
            if (!instantiateInWorldSpace)
            {
                tf.SetParent(parent);
            }
            else
            {
                tf.transform.position = parent.position;
                tf.transform.rotation = parent.rotation;
            }

            return op;
        }
        
        public async Task<AsyncOperationHandle> LoadAssetAsync()
        {
            return await AddressablesManager.LoadAssetReference(this);
        }

        public override bool ValidateAsset(Object obj)
        {
            var go = obj as GameObject;
            return go != null && go.GetComponent<TComponent>() != null;
        }
    
        public override bool ValidateAsset(string path)
        {
#if UNITY_EDITOR
            //this load can be expensive...
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return go != null && go.GetComponent<TComponent>() != null;
#else
            return false;
#endif
        }

        public void ReleaseInstance(AsyncOperationHandle<TComponent> op)
        {
            var component = op.Result as Component;
            
            if (component != null)
            {
                if (component.TryGetComponent(out SelfReleaseOnDestroy selfReleaseOnDestroy))
                {
                    selfReleaseOnDestroy.SelfRelease();
                    
                    return;
                }
                
                Debug.LogWarningFormat($"GameObject {component.gameObject.name} has no SelfReleaseOnDestroy component.");
            }
            
            Debug.LogWarningFormat($"{typeof(TComponent)} is null.");
        }
    }
}