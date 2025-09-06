using UnityEngine;

public class LoadingShow : MonoBehaviour
{

    [SerializeField]
    GameObject content;

    [SerializeField]
    GameObject loading;
    void Update()
    {
        if(content.transform.childCount > 0)
        {
            loading.SetActive(false);
        }
        else
        {
            loading.SetActive(true);
        }
    }
}
