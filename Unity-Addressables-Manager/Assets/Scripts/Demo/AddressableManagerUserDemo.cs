using AddressablesSystem;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Demo
{
	public class AddressableManagerUserDemo : MonoBehaviour
	{
		[SerializeField] ComponentReference<TestPrefab> prefab;
		[SerializeField] GameObject simplePrefab;
		[SerializeField] bool useAddressables;

		async void Awake()
		{
			// Load example 1
			var op1 = await AddressablesManager.LoadAssetReference(prefab);
			
			// Load example 2
			var op2 = await prefab.LoadAssetAsync();
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.S))
			{
				for (int i = 0; i < 2; i++)
				{
					if (useAddressables)
					{
						SpawnAddressablePrefab();
					}
					else
					{
						for (int j = 0; j < 4; j++)
						{
							var go = Instantiate(simplePrefab);
						}
					}
				}
			}
		}

		async void SpawnAddressablePrefab()
		{
			// Spawn Example 1
			var go1 = await AddressablesManager.InstantiateAsync(prefab);
			
			// Spawn Example 2
			var go2 = await AddressablesManager.InstantiateAsync<TestPrefab>(prefab).Result;
			
			// Spawn Example 3
			var go3 = await prefab.InstantiateAsync().Result;
			
			// Spawn Example 4
			var go4 = await prefab.InstantiateAsync(transform, true).Result;
		}
	}
}