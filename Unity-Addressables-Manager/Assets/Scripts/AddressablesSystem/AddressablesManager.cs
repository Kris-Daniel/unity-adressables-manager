using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils.Cancellation;

namespace AddressablesSystem
{
	public static class AddressablesManager
	{
		static AddressablesLoader AddressablesLoader;
		static CancellationTokenSource Cts;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void Initialize()
		{
			Cts = new CancellationTokenSource();

			Application.quitting += () =>
			{
				Cts.Cancel();
			};
			
			AddressablesLoader = new AddressablesLoader();
			AddressablesLoader.SetToken(Cts.Token);
		}

		public static async Task<GameObject> InstantiateAsync(AssetReference assetReference)
		{
			return await AddressablesLoader.InstantiateAsync(assetReference);
		}
		
		public static async Task<AsyncOperationHandle<T>> InstantiateAsync<T>(AssetReference assetReference, Action<Transform> callback) where T : MonoBehaviour
		{
			return await AddressablesLoader.InstantiateAsync<T>(assetReference, callback);
		}
	}
}