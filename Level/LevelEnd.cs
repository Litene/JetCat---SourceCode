using UnityEngine;

namespace Level
{
    [RequireComponent(typeof(BoxCollider))]
    public class LevelEnd : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var player = other.gameObject.GetComponent<ThirdPersonController>();
            if(player == null) return;
        
            GameManager.Instance.EndLevel();
        }
    }
}
