using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
	}
}