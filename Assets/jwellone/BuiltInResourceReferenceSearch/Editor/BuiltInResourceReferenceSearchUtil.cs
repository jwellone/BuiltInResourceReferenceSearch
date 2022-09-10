using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

#nullable enable

namespace jwelloneEditor
{
    [Serializable]
    public class BuiltInResourceReferencesData
    {
        [SerializeField] UnityEngine.Object _target = null!;
        [SerializeField] List<string> _componentNames = null!;
        [SerializeField] List<string> _propertyPaths = null!;

        public UnityEngine.Object target => _target;
        public List<string> componentNames => _componentNames;
        public List<string> propertyPaths => _propertyPaths;

        public BuiltInResourceReferencesData(in UnityEngine.Object target)
        {
            _target = target;
            _componentNames = new List<string>();
            _propertyPaths = new List<string>();
        }
    }

    [Serializable]
    public class BuiltInResourceReferences
    {
        [SerializeField] int _count;
        [SerializeField] UnityEngine.Object? _target;
        [SerializeField] List<BuiltInResourceReferencesData> _data = null!;

        public bool useBuiltInResource => count > 0;
        public int count => _count;
        public UnityEngine.Object? target => _target;
        public IReadOnlyList<BuiltInResourceReferencesData> data => _data;

        public BuiltInResourceReferences(in GameObject target)
        {
            _data = new List<BuiltInResourceReferencesData>();
            _target = target;
            OnCalc(target);
        }

        public BuiltInResourceReferences(string guid)
        {
            _data = new List<BuiltInResourceReferencesData>();

            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = PrefabUtility.LoadPrefabContents(path);
            OnCalc(prefab);

            if (useBuiltInResource)
            {
                _target = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            PrefabUtility.UnloadPrefabContents(prefab);
        }

        public BuiltInResourceReferences(in Scene scene)
        {
            _data = new List<BuiltInResourceReferencesData>();
            _target = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
            foreach (var rootObject in scene.GetRootGameObjects())
            {
                OnCalc(rootObject);
            }
        }

        void OnCalc(in GameObject gameOject)
        {
            _count += OnMakeData(gameOject);
            for (var i = 0; i < gameOject.transform.childCount; ++i)
            {
                var child = gameOject.transform.GetChild(i);
                OnCalc(child.gameObject);
            }
        }

        int OnMakeData(in GameObject target)
        {
            var refCount = 0;
            var refData = new BuiltInResourceReferencesData(target);
            foreach (var component in target.GetComponents<Component>())
            {
                if (component == null)
                {
                    continue;
                }

                var property = new SerializedObject(component).GetIterator();
                while (property.NextVisible(true))
                {
                    if (property.propertyType != SerializedPropertyType.ObjectReference ||
                        property.objectReferenceValue == null)
                    {
                        continue;
                    }

                    var objectRef = property.objectReferenceValue;
                    if (AssetDatabase.GetAssetPath(objectRef) != "Resources/unity_builtin_extra")
                    {
                        continue;
                    }

                    refData.componentNames.Add(component.GetType().Name);
                    refData.propertyPaths.Add(property.propertyPath);
                    ++refCount;
                }
            }

            if (refCount > 0)
            {
                _data.Add(refData);
            }
            return refCount;
        }
    }

    public static class BuiltInResourceReferenceSearchUtil
    {
        public static bool Exists(in Component component)
        {
            if (component == null)
            {
                return false;
            }

            var property = new SerializedObject(component).GetIterator();
            while (property.NextVisible(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference ||
                    property.objectReferenceValue == null)
                {
                    continue;
                }

                var objectRef = property.objectReferenceValue;
                if (AssetDatabase.GetAssetPath(objectRef) != "Resources/unity_builtin_extra")
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public static List<BuiltInResourceReferences> FindHierarchy()
        {
            var list = new List<BuiltInResourceReferences>();

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                var data = new BuiltInResourceReferences(prefabStage.prefabContentsRoot);
                if (data.useBuiltInResource)
                {
                    list.Add(data);
                }
            }
            else
            {
                var scene = SceneManager.GetActiveScene();
                var data = new BuiltInResourceReferences(scene);
                if (data.useBuiltInResource)
                {
                    list.Add(data);
                }
            }

            return list;

        }

        public static List<BuiltInResourceReferences> FindAll()
        {
            var list = new List<BuiltInResourceReferences>();

            var prefabs = AssetDatabase.FindAssets("t:Prefab");
            for (var i = 0; i < prefabs.Length; ++i)
            {
                var guid = prefabs[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);

                if (!path.StartsWith("Assets"))
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar("Search prefab", path, (i + 1) / (float)prefabs.Length);
                var data = new BuiltInResourceReferences(guid);
                if (data.useBuiltInResource)
                {
                    list.Add(data);
                }
            }

            EditorUtility.ClearProgressBar();

            var activeScene = SceneManager.GetActiveScene();
            var scenes = AssetDatabase.FindAssets("t:Scene");
            for (var i = 0; i < scenes.Length; ++i)
            {
                var guid = scenes[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                EditorUtility.DisplayProgressBar("Search scene", path, (i + 1) / (float)scenes.Length);

                if (!path.StartsWith("Assets"))
                {
                    continue;
                }

                if (path == activeScene.path)
                {
                    var data = new BuiltInResourceReferences(activeScene);
                    if (data.useBuiltInResource)
                    {
                        list.Add(data);
                    }
                }
                else
                {
                    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    var data = new BuiltInResourceReferences(scene);
                    if (data.useBuiltInResource)
                    {
                        list.Add(data);
                    }

                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            EditorUtility.ClearProgressBar();

            return list;
        }
    }
}
