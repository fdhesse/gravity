using UnityEditor;
using UnityEngine;

namespace FullInspector.BackupService {
    /// <summary>
    /// Manages the backup storage that lives in the scene.
    /// </summary>
    public class fiSceneManager {
        private const string SceneStorageName = "fiBackupSceneStorage";
        private static fiStorageComponent _storage;

        public static fiStorageComponent Storage {
            get {
                if (_storage == null) {
                    _storage = GameObject.FindObjectOfType<fiStorageComponent>();

                    if (_storage == null) {
                        // If we use new GameObject(), then for a split second Unity will show the
                        // game object in the hierarchy, which is bad UX.
                        var obj = EditorUtility.CreateGameObjectWithHideFlags(SceneStorageName,
                            HideFlags.HideInHierarchy);
                        _storage = obj.AddComponent<fiStorageComponent>();
                    }
                }

                return _storage;
            }
        }
    }
}