using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AddressablesSystem
{
	public class SelfReleaseOnDestroy : MonoBehaviour
	{
		public event Action<GameObject, AssetReference> Destroyed;
		public AssetReference AssetReference { get; set; }

		void OnDestroy()
		{
			Destroyed?.Invoke(gameObject, AssetReference);
		}
	}
}