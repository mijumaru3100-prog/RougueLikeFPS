using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab; // 貸し出すプレハブ
    [SerializeField] private int initialSize = 20; // 最初に用意する数

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        // 最初にまとめて作って、全員眠らせる
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }

    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(prefab);
        obj.transform.SetParent(this.transform); // ヒエラルキーが散らからないように
        obj.SetActive(false);
        pool.Enqueue(obj);
        return obj;
    }

    // 「貸して！」と言われたら呼ぶ
    public GameObject Get()
    {
        if (pool.Count == 0)
        {
            CreateNewObject(); // 足りなくなったら補充
        }

        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    // 「使い終わったから返すよ」と言われたら呼ぶ
    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}