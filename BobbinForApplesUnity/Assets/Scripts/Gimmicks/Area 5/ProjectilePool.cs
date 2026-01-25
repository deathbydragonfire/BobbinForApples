using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialPoolSize = 100;
    [SerializeField] private bool expandPool = true;
    
    private Queue<GameObject> availableObjects = new Queue<GameObject>();
    private List<GameObject> allObjects = new List<GameObject>();
    
    private void Awake()
    {
        InitializePool();
    }
    
    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewObject();
        }
        
        Debug.Log($"ProjectilePool initialized with {initialPoolSize} objects");
    }
    
    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        availableObjects.Enqueue(obj);
        allObjects.Add(obj);
        return obj;
    }
    
    public GameObject GetObject()
    {
        if (availableObjects.Count == 0)
        {
            if (expandPool)
            {
                Debug.LogWarning("Pool expanded - consider increasing initial pool size");
                return CreateNewObject();
            }
            else
            {
                Debug.LogError("Pool exhausted and expansion disabled");
                return null;
            }
        }
        
        GameObject obj = availableObjects.Dequeue();
        obj.SetActive(true);
        return obj;
    }
    
    public void ReturnObject(GameObject obj)
    {
        if (obj == null) return;
        
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;
        
        if (!availableObjects.Contains(obj))
        {
            availableObjects.Enqueue(obj);
        }
    }
    
    public void ReturnAllObjects()
    {
        foreach (GameObject obj in allObjects)
        {
            if (obj != null && obj.activeInHierarchy)
            {
                ReturnObject(obj);
            }
        }
    }
}
