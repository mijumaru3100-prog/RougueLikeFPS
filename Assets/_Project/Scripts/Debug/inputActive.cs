using UnityEngine;

public class inputActive : MonoBehaviour
{
    [SerializeField]private KeyCode actionkey = KeyCode.M;
    [SerializeField]private GameObject target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(actionkey))
        {
            target.SetActive(true);
        }
        else if (Input.GetKeyUp(actionkey))
        {
            target.SetActive(false);
        }
    }
}
