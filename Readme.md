# About

[unity-adressables-manager](https://github.com/Kris-Daniel/unity-adressables-manager) is a repository that implements API for asset references smart loading and instantiating. Just use public methods from `AddressablesManager`!

`AddressablesManager` - save all instantiated asset references in special dictionary, and call `Release()` when those are destroyed.
Use EventViewer to see that all works as expectly. In unity _Window/Asset Management/Addressables/EventViewer_.

# Installation
1. Install Addressables package from unity package manager
2. Install [UniTask](https://github.com/Cysharp/UniTask)
3. Install [unity-adressables-manager](https://github.com/Kris-Daniel/unity-adressables-manager/raw/main/addressables-manager.unitypackage) package

# Getting Started
### ComponentReference

Use `ComponentReference` to serialize all kind of `MonoBehaviour` prefabs you want. In the unity inspector you will see only prefabs with that MonoBehaviour!

```
[SerializeField] ComponentReference<TestPrefab> prefab;
```

Note: _If you will setup a value in that serialized field, and after that, you will change the class inside `ComponentReference` generics, Unity `WILL NOT` delete the old value, evenly if the value does not match with new type_

### Loading Data

There are 2 options to load data. They works identically, and both support `await` operator with `UniTask` package.

```
[SerializeField] ComponentReference<TestPrefab> prefab;

async void Awake()
{
	// Load example 1  
	var op1 = await AddressablesManager.LoadAssetReference(prefab);  
	  
	// Load example 2  
	var op2 = await prefab.LoadAssetAsync();
}
```


### Instantiating Data

```
[SerializeField] ComponentReference<TestPrefab> prefab;

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
```
