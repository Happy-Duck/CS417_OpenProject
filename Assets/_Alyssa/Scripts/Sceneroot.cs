using UnityEngine;

namespace Alyssa
{
    public static class SceneRoot
    {
        static Transform cached;

        public static Transform Root
        {
            get
            {
                if (cached == null)
                {
                    GameObject go = GameObject.Find("AlyssaSceneRoot");
                    if (go != null) cached = go.transform;
                    else Debug.LogWarning("AlyssaSceneRoot not found. Spawned objects will not be parented.");
                }
                return cached;
            }
        }
    }
}