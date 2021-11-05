﻿using System;
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
		
		public CancellationToken CtsToken { get; set; }
		public Action OnCtsTokenSet { get; set; }

		public AddressablesLoader()
		{
			OnCtsTokenSet += CheckForReleaseAssetReferences;
		}

		public async Task<GameObject> InstantiateAsync(AssetReference assetReference)
		{
			AsyncOperationHandle<GameObject> op;

			if (assetReferenceDataStore.ContainsKey(assetReference))
			{
				op = assetReferenceDataStore[assetReference].OperationHandle;
				
				while (!op.IsDone && !CtsToken.IsCancellationRequested)
				{
					await Task.Yield();
				}
			}
			else
			{
				op = Addressables.LoadAssetAsync<GameObject>(assetReference);
				
				assetReferenceDataStore[assetReference] = new AssetReferenceData{OperationHandle = op};
				
				await op.Task;
			}

			if (op.Status == AsyncOperationStatus.Succeeded)
			{
				var go = await Addressables.InstantiateAsync(assetReference);
				
				assetReferenceDataStore[assetReference].InstantiatedGameObjects.Add(go);
				assetReferenceDataStore[assetReference].IsReady = true;
				
				return go;
			}

			return null;
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
						Addressables.Release(currentAssetReferenceData.OperationHandle);
						
						referencesToDelete.Add(keyValuePair.Key);
					}
				}
				
				foreach (var assetReference in referencesToDelete)
				{
					assetReferenceDataStore.Remove(assetReference);
				}

				await Task.Delay(2000, CtsToken);
			}
		}
	}
}