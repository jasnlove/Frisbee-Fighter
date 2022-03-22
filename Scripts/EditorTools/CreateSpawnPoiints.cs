using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

namespace FrisbeeThrow
{
    public class CreateSpawnPoints{
        [MenuItem("Tools/Create spawnpoints")]
        public static void GenerateSpawnPoints(){
            //Setup and clear
            Tilemap ground = GameObject.Find("Ground").GetComponent<Tilemap>();
            Transform parent = GameObject.Find("SpawnPointParent").transform;
            if(parent == null)
                parent = new GameObject("SpawnPointParent").transform;
            Director dr = GameObject.Find("Director").GetComponent<Director>();
            foreach(Transform child in parent.GetComponentsInChildren<Transform>()){
                if(child == parent) continue;
                MonoBehaviour.DestroyImmediate(child.gameObject);
            }
            dr.spawnpoints.Clear();

            BoundsInt b = ground.cellBounds;
            for(int i = b.xMin; i < b.xMax; i++){
                for(int j = b.yMin; j < b.yMax; j++){
                    if(!ground.HasTile(new Vector3Int(i,j,0))) continue;
                    GameObject tmp = new GameObject("SpawnPoint");
                    tmp.transform.position = ground.CellToWorld(new Vector3Int(i,j,0));
                    tmp.transform.SetParent(parent);
                }
            }

            foreach(Transform child in parent.GetComponentsInChildren<Transform>()){
                dr.spawnpoints.Add(child);
            }
        }
    }
}
