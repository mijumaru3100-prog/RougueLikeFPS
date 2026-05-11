using System.Collections.Generic;
using UnityEngine;
public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab; 
    [SerializeField] private int initialSize = 20; 
    private Queue<GameObject> pool = new Queue<GameObject>();
    void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }
    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(prefab);
        obj.transform.SetParent(this.transform); 
        obj.SetActive(false);
        pool.Enqueue(obj);
        return obj;
    }
    public GameObject Get()
    {
        if (pool.Count == 0)
        {
            CreateNewObject(); 
        }
        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }
    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}