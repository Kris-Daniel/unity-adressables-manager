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
    
        public new async Task<TComponent> InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var comp = await AddressablesManager.InstantiateAsync<TComponent>(this);

            var tf = comp.transform;
            
            tf.position = position;
            tf.rotation = rotation;
            tf.SetParent(parent);

            return comp;
        }
   
        public new async Task<TComponent> InstantiateAsync(Transform parent = null, bool instantiateInWorldSpace = false)
        {
            var comp = await AddressablesManager.InstantiateAsync<TComponent>(this);

            if (!instantiateInWorldSpace)
            {
                comp.transform.SetParent(parent);
            }
            else
            {
                comp.transform.transform.position = parent.position;
                comp.transform.transform.rotation = parent.rotation;
            }

            return comp;
        }

        public AssetReference GetThis()
        {
            return this;
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