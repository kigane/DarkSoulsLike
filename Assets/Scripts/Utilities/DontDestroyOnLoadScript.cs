using UnityEngine;

namespace DarkSoulsLike
{
    public class DontDestroyOnLoadScript : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
