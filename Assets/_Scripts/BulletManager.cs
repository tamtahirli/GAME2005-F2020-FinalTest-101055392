using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public int MaxBullets;
    public GameObject bullet;

    public Queue<GameObject> m_playerBulletPool;

    // Start is called before the first frame update
    void Start()
    {
        _BuildBulletPool();
    }

    private void _BuildBulletPool()
    {
        // create empty Queue structures
        m_playerBulletPool = new Queue<GameObject>();

        for (int count = 0; count < MaxBullets; count++)
        {
            var tempPlayerBullet = Instantiate(bullet);
            tempPlayerBullet.transform.SetParent(transform);
            tempPlayerBullet.SetActive(false);
            m_playerBulletPool.Enqueue(tempPlayerBullet);
        }
    }

    public GameObject GetBullet(Vector3 position, Vector3 direction)
    {
        GameObject newBullet = null;
        newBullet = m_playerBulletPool.Dequeue();
        newBullet.SetActive(true);
        newBullet.transform.position = position;
        newBullet.GetComponent<BulletBehaviour>().direction = direction;

        return newBullet;
    }

    public bool HasBullets()
    {
        return m_playerBulletPool.Count > 0;
    }

    public void ReturnBullet(GameObject returnedBullet)
    {
        returnedBullet.SetActive(false);
        m_playerBulletPool.Enqueue(returnedBullet);
    }
    
}
