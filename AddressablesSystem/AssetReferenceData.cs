using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressablesSystem
{
	public class AssetReferenceData
	{
		public List<GameObject> InstantiatedGameObjects { get; set; } = new List<GameObject>();
		public AsyncOperationHandle<GameObject> LoadOperationHandle { get; set; }
		public bool IsReady { get; set; }
	}
}