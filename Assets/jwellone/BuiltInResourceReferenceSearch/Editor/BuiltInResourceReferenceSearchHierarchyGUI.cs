using UnityEngine;
using UnityEditor;

#nullable enable

namespace jwelloneEditor
{
	public static class BuiltInResourceReferenceSearchHierarchyGUI
	{
		static Texture _icon = null!;

		[InitializeOnLoadMethod]
		static void OnInitializeOnLoadMethod()
		{
			var icon = EditorGUIUtility.IconContent("SceneAsset On Icon").image;
			_icon = icon == null ? Texture2D.whiteTexture : icon;

			EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
			EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
		}

		static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
		{
			if (EditorApplication.isCompiling || EditorApplication.isPlaying)
			{
				return;
			}

			var target = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
			if (target == null)
			{
				return;
			}

			foreach (var component in target.GetComponents<Component>())
			{
				if (BuiltInResourceReferenceSearchUtil.Exists(component))
				{
					var iconPos = selectionRect;
					iconPos.width = 16;
					iconPos.height = iconPos.width;
					iconPos.x = 32;
					GUI.DrawTexture(iconPos, _icon);
					return;
				}
			}
		}
	}
}