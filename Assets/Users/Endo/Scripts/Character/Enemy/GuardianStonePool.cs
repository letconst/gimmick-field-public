using UnityEngine;

public class GuardianStonePool : UniRx.Toolkit.ObjectPool<GuardianStone>
{
    private readonly GameObject[] _stonePrefabs;

    public GuardianStonePool(GameObject[] prefabs)
    {
        _stonePrefabs = new GameObject[prefabs.Length];
        System.Array.Copy(prefabs, _stonePrefabs, prefabs.Length);
    }

    protected override GuardianStone CreateInstance()
    {
        // 生成する石をランダムに選択
        int        rnd    = Random.Range(0, _stonePrefabs.Length);
        GameObject prefab = _stonePrefabs[rnd];

        // 生成する位置・回転
        Vector3    pos = Guardian.Instance.transform.position;
        Quaternion rot = Random.rotation;

        return Object.Instantiate(prefab, pos, rot).GetComponent<GuardianStone>();
    }
}
