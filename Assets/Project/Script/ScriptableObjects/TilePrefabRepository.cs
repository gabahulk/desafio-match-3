using UnityEngine;
using UnityEngine.Serialization;

namespace Gazeus.DesafioMatch3.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TilePrefabRepository", menuName = "Gameplay/TilePrefabRepository")]
    public class TilePrefabRepository : ScriptableObject
    {
        [FormerlySerializedAs("_tileTypePrefabList")]
        [SerializeField] private GameObject[] _colorPrefabList;

        public GameObject[] ColorPrefabList => _colorPrefabList;
    }
}
