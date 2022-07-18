using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AddressablesSystem.Cancellation;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressablesSystem
{
	public class AddressablesLoader : ICancellationTokenUser
	{
		readonly Dictionary<AssetReference, AssetReferenceData> _assetReferenceDataStore = new Dictionary<AssetReference, AssetReferenceData>();

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

			if (_assetReferenceDataStore.ContainsKey(assetReference))
			{
				op = _assetReferenceDataStore[assetReference].LoadOperationHandle;

				while (!op.IsDone && !CtsToken.IsCancellationRequested)
				{
					await Task.Yield();
				}
			}
			else
			{
				op = Addressables.LoadAssetAsync<GameObject>(assetReference);

				_assetReferenceDataStore[assetReference] = new AssetReferenceData {LoadOperationHandle = op};

				await op.Task;

				if (op.Status == AsyncOperationStatus.Succeeded)
				{
					return op;
				}
			}

			throw new Exception($"Cannot Load Asset Reference: {assetReference}.");
		}

		public async Task<GameObject> InstantiateAsync(AssetReference assetReference)
		{
			await LoadAssetReference(assetReference);

			var go = Addressables.InstantiateAsync(assetReference);

			await go.Task;

			InitInstantiatedGameObject(go.Result, assetReference);

			return go.Result;
		}

		public async Task<T> InstantiateAsync<T>(AssetReference assetReference) where T : MonoBehaviour
		{
			var go = await InstantiateAsync(assetReference);

			var comp = go.GetComponent<T>();

			if (!comp)
			{
				throw new Exception("Current serialized Object has wrong component.");
			}

			return comp;
		}

		void InitInstantiatedGameObject(GameObject gameObject, AssetReference assetReference)
		{
			var selfReleaseOnDestroy = gameObject.AddComponent<SelfReleaseOnDestroy>();
			selfReleaseOnDestroy.AssetReference = assetReference;
			selfReleaseOnDestroy.Destroyed += RemoveInstantiatedGameObject;

			_assetReferenceDataStore[assetReference].InstantiatedGameObjects.Add(gameObject);
			_assetReferenceDataStore[assetReference].IsReady = true;
		}

		void RemoveInstantiatedGameObject(GameObject gameObject, AssetReference assetReference)
		{
			if (_assetReferenceDataStore.ContainsKey(assetReference))
			{
				if (_assetReferenceDataStore[assetReference].InstantiatedGameObjects.Contains(gameObject))
				{
					_assetReferenceDataStore[assetReference].InstantiatedGameObjects.Remove(gameObject);

					Addressables.ReleaseInstance(gameObject);

					if (_assetReferenceDataStore[assetReference].InstantiatedGameObjects.Count == 0)
					{
						Addressables.Release(_assetReferenceDataStore[assetReference].LoadOperationHandle);

						_assetReferenceDataStore.Remove(assetReference);
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

				foreach (var keyValuePair in _assetReferenceDataStore)
				{
					AssetReferenceData currentAssetReferenceData = _assetReferenceDataStore[keyValuePair.Key];

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
					else if (currentAssetReferenceData.IsReady)
					{
						Addressables.Release(currentAssetReferenceData.LoadOperationHandle);

						referencesToDelete.Add(keyValuePair.Key);
					}
				}

				foreach (var assetReference in referencesToDelete)
				{
					_assetReferenceDataStore.Remove(assetReference);
				}

				await Task.Delay(2000);
			}
		}
	}
}