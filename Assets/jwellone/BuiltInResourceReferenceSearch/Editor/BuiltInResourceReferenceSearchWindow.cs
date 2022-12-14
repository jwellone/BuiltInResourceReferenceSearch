using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

#nullable enable

namespace jwelloneEditor
{
    public class BuiltInResourceReferenceSearchWindow : EditorWindow
    {
		[SerializeField] Vector2 _scrollPosOfProject;
		[SerializeField] Vector2 _scrollPosOfHierarchy;
		[SerializeField] List<BuiltInResourceReferences> _projectReferences = new List<BuiltInResourceReferences>();
		[SerializeField] List<BuiltInResourceReferences> _hierarchyReferences = new List<BuiltInResourceReferences>();

		[MenuItem("jwellone/window/BuiltInResourceReferenceSearchWindow")]
        static void Open()
        {
            EditorWindow.GetWindow<BuiltInResourceReferenceSearchWindow>("BuiltIn Resource References");
        }

		void OnGUI()
		{
			OnGUIProject();
			GUILayout.Box("", GUILayout.Width(position.width), GUILayout.Height(8));
			OnGUIHierarchyOrPrefabMode();
		}

		void OnEnable()
		{
			EditorSceneManager.sceneOpened += OnSceneOpened;
			PrefabStage.prefabStageOpened += OnPrefabStageOpened;
			PrefabStage.prefabStageClosing += OnPrefabStageClosing;
		}

		void OnDisable()
		{
			EditorSceneManager.sceneOpened -= OnSceneOpened;
			PrefabStage.prefabStageOpened -= OnPrefabStageOpened;
			PrefabStage.prefabStageClosing -= OnPrefabStageClosing;
		}

		void OnPrefabStageOpened(PrefabStage stage)
		{
			OnFindHierarchy();
			Repaint();
		}

		void OnPrefabStageClosing(PrefabStage stage)
		{
			OnFindHierarchy();
			Repaint();
		}

		void OnSceneOpened(Scene scene, OpenSceneMode mode)
		{
			OnFindHierarchy();
			Repaint();
		}

		void OnGUIProject()
		{
			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField("Project");
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Find", GUILayout.Width(64)))
			{
				OnFindProject();
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Box("", GUILayout.Width(position.width), GUILayout.Height(1));

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Asset");
			EditorGUILayout.LabelField("Use Count");
			EditorGUILayout.EndHorizontal();

			var style = new GUIStyle(EditorStyles.textField)
			{
				alignment = TextAnchor.MiddleRight
			};

			using var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosOfProject, GUILayout.Height(position.height / 2f));
			_scrollPosOfProject = scrollView.scrollPosition;
			using (new EditorGUI.DisabledScope(true))
			{
				foreach (var reference in _projectReferences)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.ObjectField(string.Empty, reference.target, typeof(UnityEngine.Object), true);
					EditorGUILayout.TextField(reference.count.ToString(), style);
					EditorGUILayout.EndHorizontal();
				}
			}
		}

		void OnGUIHierarchyOrPrefabMode()
		{
			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField("Hierarchy");
			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Find", GUILayout.Width(64)))
			{
				OnFindHierarchy();
				var count = 0;
				foreach (var reference in _hierarchyReferences)
				{
					count += reference.count;
				}
				EditorUtility.DisplayDialog("Search", $"There are {count} references part.", "close");
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Box("", GUILayout.Width(position.width), GUILayout.Height(1));

			using var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosOfHierarchy);
			_scrollPosOfHierarchy = scrollView.scrollPosition;
			using (new EditorGUI.DisabledScope(true))
			{
				foreach (var reference in _hierarchyReferences)
				{
					foreach (var data in reference.data)
					{
						EditorGUILayout.ObjectField(string.Empty, data.target, typeof(UnityEngine.Object), true);
						EditorGUI.indentLevel += 1;
						for (var i = 0; i < data.componentNames.Count; ++i)
						{
							EditorGUILayout.TextField(data.componentNames[i], data.propertyPaths[i]);
							GUILayout.Box("", GUILayout.Width(position.width - 24), GUILayout.Height(1));
						}
						EditorGUI.indentLevel -= 1;
					}
				}
			}
		}

		void OnFindProject()
		{
			_scrollPosOfProject = Vector2.zero;
			_projectReferences = BuiltInResourceReferenceSearchUtil.FindAll();

			var count = 0;
			foreach (var reference in _projectReferences)
			{
				count += reference.count;
			}
			EditorUtility.DisplayDialog("Search", $"There are {count} references part.", "close");

		}

		void OnFindHierarchy()
		{
			_scrollPosOfHierarchy = Vector2.zero;
			_hierarchyReferences.Clear();
			_hierarchyReferences = BuiltInResourceReferenceSearchUtil.FindHierarchy();
		}
	}
}