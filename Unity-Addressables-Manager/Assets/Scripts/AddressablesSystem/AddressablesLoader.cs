using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils.Cancellation;

namespace AddressablesSystem
{
	public class AddressablesLoader : ICancellationTokenUser
	{
		readonly Dictionary<AssetReference, AssetReferenceData> assetReferenceDataStore = new Dictionary<AssetReference, AssetReferenceData>();
		readonly Dictionary<GameObject, AsyncOperationHandle> createdCompletedOperations = new Dictionary<GameObject, AsyncOperationHandle>();
		
		public CancellationToken CtsToken { get; set; }
		public Action OnCtsTokenSet { get; set; }

		public AddressablesLoader()
		{
			//OnCtsTokenSet += CheckForReleaseAssetReferences;
		}
		
		public async Task<AsyncOperationHandle<GameObject>> LoadAssetReference(AssetReference assetReference)
		{
			AsyncOperationHandle<GameObject> op;
			
			if (assetReference == null || assetReference.AssetGUID.Length == 0)
			{
				throw new Exception("Asset Reference is null.");
			}

			if (assetReferenceDataStore.ContainsKey(assetReference))
			{
				op = assetReferenceDataStore[assetReference].LoadOperationHandle;
				
				while (!op.IsDone && !CtsToken.IsCancellationRequested)
				{
					await Task.Yield();
				}
			}
			else
			{
				op = Addressables.LoadAssetAsync<GameObject>(assetReference);

				assetReferenceDataStore[assetReference] = new AssetReferenceData{LoadOperationHandle = op};
				
				await op.Task;
			}

			if (op.Status == AsyncOperationStatus.Succeeded)
			{
				return op;
			}
			
			throw new Exception($"Cannot Load Asset Reference: {assetReference}.");
		}
		
		public async Task<GameObject> InstantiateAsync(AssetReference assetReference)
		{
			await LoadAssetReference(assetReference);

			var go = await Addressables.InstantiateAsync(assetReference);

			InitInstantiatedGameObject(go, assetReference);
				
			return go;
		}

		public async Task<AsyncOperationHandle<T>> InstantiateAsync<T>(AssetReference assetReference) where T : MonoBehaviour
		{
			var go = await InstantiateAsync(assetReference);

			var comp = go.GetComponent<T>();

			if (!comp)
			{
				throw new Exception("Current serialized Object has wrong component.");
			}

			var completedOp = Addressables.ResourceManager.CreateCompletedOperation<T>(comp, string.Empty);
			
			createdCompletedOperations[go] = completedOp;

			return completedOp;
		}

		void InitInstantiatedGameObject(GameObject gameObject, AssetReference assetReference)
		{
			var selfReleaseOnDestroy = gameObject.AddComponent<SelfReleaseOnDestroy>();
			selfReleaseOnDestroy.AssetReference = assetReference;
			selfReleaseOnDestroy.Destroyed += RemoveInstantiatedGameObject;
			
			assetReferenceDataStore[assetReference].InstantiatedGameObjects.Add(gameObject);
			assetReferenceDataStore[assetReference].IsReady = true;	
		}

		void RemoveInstantiatedGameObject(GameObject gameObject, AssetReference assetReference)
		{
			if (assetReferenceDataStore.ContainsKey(assetReference))
			{
				if (assetReferenceDataStore[assetReference].InstantiatedGameObjects.Contains(gameObject))
				{
					assetReferenceDataStore[assetReference].InstantiatedGameObjects.Remove(gameObject);
					
					Addressables.ReleaseInstance(gameObject);

					if (createdCompletedOperations.ContainsKey(gameObject))
					{
						Addressables.ReleaseInstance(createdCompletedOperations[gameObject]);

						createdCompletedOperations.Remove(gameObject);
					}

					if (assetReferenceDataStore[assetReference].InstantiatedGameObjects.Count == 0)
					{
						Addressables.Release(assetReferenceDataStore[assetReference].LoadOperationHandle);
						
						assetReferenceDataStore.Remove(assetReference);
					}
				}
			}
		}

		async void CheckForReleaseAssetReferences()
		{
			List<AssetReference> referencesToDelete = new List<AssetReference>();
			
			while (!CtsToken.IsCancellationRequested)
			{
				referencesToDelete.Clear();

				foreach (var keyValuePair in assetReferenceDataStore)
				{
					AssetReferenceData currentAssetReferenceData = assetReferenceDataStore[keyValuePair.Key];
					
					if (currentAssetReferenceData.InstantiatedGameObjects.Count > 0)
					{
						for (int i = currentAssetReferenceData.InstantiatedGameObjects.Count - 1; i >= 0; i--)
						{
							if (currentAssetReferenceData.InstantiatedGameObjects[i] == null)
							{
								currentAssetReferenceData.InstantiatedGameObjects.RemoveAt(i);
							}
						}
					}
					else if(currentAssetReferenceData.IsReady)
					{
						Addressables.Release(currentAssetReferenceData.LoadOperationHandle);
						
						referencesToDelete.Add(keyValuePair.Key);
					}
				}

				foreach (var assetReference in referencesToDelete)
				{
					assetReferenceDataStore.Remove(assetReference);
				}

				await Task.Delay(2000);
			}
		}
	}
}