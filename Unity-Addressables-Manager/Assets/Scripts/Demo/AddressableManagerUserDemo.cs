using System.Threading.Tasks;
using AddressablesSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Demo
{
	public class AddressableManagerUserDemo : MonoBehaviour
	{
		[SerializeField] AssetReference prefab;
		
		async void Awake()
		{
			GameObject a = await AddressablesManager.InstantiateAsync(prefab);
			await Task.Delay(10000);
			Destroy(a);
		}
	}
}