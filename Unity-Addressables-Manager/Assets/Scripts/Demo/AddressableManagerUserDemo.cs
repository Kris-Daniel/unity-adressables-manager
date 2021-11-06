using System.Threading.Tasks;
using AddressablesSystem;
using UnityEngine;

namespace Demo
{
	public class AddressableManagerUserDemo : MonoBehaviour
	{
		[SerializeField] ComponentReference<TestPrefab2> prefab;
		
		async void Awake()
		{
			var op = await prefab.InstantiateAsync(Vector3.down, Quaternion.identity, transform);
			await Task.Delay(10000);
			DestroyImmediate(op.Result.gameObject);
		}
	}
}