using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AddressablesSystem
{
	public class SelfReleaseOnDestroy : MonoBehaviour
	{
		public event Action<GameObject, AssetReference> Destroyed;
		public AssetReference AssetReference { get; set; }

		public void SelfRelease()
		{
			Destroy(gameObject);
		}

		void OnDestroy()
		{
			Destroyed?.Invoke(gameObject, AssetReference);
		}
	}
}