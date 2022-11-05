using System.Threading;
using System.Threading.Tasks;
using AddressablesSystem.Cancellation;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressablesSystem
{
	public static class AddressablesManager
	{
		static AddressablesLoader AddressablesLoader;
		static CancellationTokenSource Cts;

		public static void Initialize()
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
		
		public static async Task<T> InstantiateAsync<T>(AssetReference assetReference) where T : MonoBehaviour
		{
			return await AddressablesLoader.InstantiateAsync<T>(assetReference);
		}

		public static async Task<AsyncOperationHandle> LoadAssetReference(AssetReference assetReference)
		{
			return await AddressablesLoader.LoadAssetReference(assetReference);
		}
	}
}