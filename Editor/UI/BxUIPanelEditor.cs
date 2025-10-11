using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using BasyaFramework.Logger;
using BasyaFramework.UI;
using UnityEngine.EventSystems;

namespace BasyaFramework.Editor.UI
{
    [CustomEditor(typeof(BxUIPanel), true)]
    public class BxUIPanelEditor : UnityEditor.Editor
    {
        private BxUIPanel _panel;

        private void OnEnable()
        {
            _panel = target as BxUIPanel;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("BxUIPanel 工具", EditorStyles.boldLabel);

            // 初始化面板UI组件按钮
            if (GUILayout.Button("初始化面板UI组件", GUILayout.Height(30)))
            {
                InitializeUIBehaviours();
            }

            // 初始化面板GameObject组件按钮
            if (GUILayout.Button("初始化面板GameObject组件", GUILayout.Height(30)))
            {
                InitializeUIGameObjects();
            }

            // 生成UI控件代码按钮
            if (GUILayout.Button("生成UI控件代码", GUILayout.Height(30)))
            {
                // 然后生成代码
                GenerateUIControlCode();
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 初始化面板UI组件
        /// 遍历面板以及子节点中所有tag为BxUI的UIBehaviour，然后赋值到uiBehaviours数组
        /// </summary>
        private void InitializeUIBehaviours()
        {
            if (_panel == null)
            {
                BxDebug.LogError("面板为空，无法初始化UI组件");
                return;
            }

            // 查找所有tag为BxUI的UIBehaviour
            List<UIBehaviour> uiBehaviours = new List<UIBehaviour>();
            UIBehaviour[] allBehaviours = _panel.GetComponentsInChildren<UIBehaviour>(true);

            foreach (UIBehaviour behaviour in allBehaviours)
            {
                if (behaviour.CompareTag("BxUI"))
                {
                    uiBehaviours.Add(behaviour);
                }
            }

            // 更新序列化字段
            SerializedProperty uiBehavioursProp = serializedObject.FindProperty("uiBehaviours");
            if (uiBehavioursProp != null)
            {
                uiBehavioursProp.arraySize = uiBehaviours.Count;
                for (int i = 0; i < uiBehaviours.Count; i++)
                {
                    uiBehavioursProp.GetArrayElementAtIndex(i).objectReferenceValue = uiBehaviours[i];
                }

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_panel);
            }

            BxDebug.Log($"成功初始化面板UI组件，共找到 {uiBehaviours.Count} 个BxUI组件");
        }

        /// <summary>
        /// 初始化面板GameObject组件
        /// 遍历面板以及子节点中所有tag为BxUI的GameObject，然后赋值到uiGameObjects数组
        /// </summary>
        private void InitializeUIGameObjects()
        {
            if (_panel == null)
            {
                BxDebug.LogError("面板为空，无法初始化GameObject组件");
                return;
            }

            // 查找所有tag为BxUI的GameObject
            List<GameObject> uiGameObjects = new List<GameObject>();
            Transform[] allChildren = _panel.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in allChildren)
            {
                if (child.gameObject.CompareTag("BxUI"))
                {
                    uiGameObjects.Add(child.gameObject);
                }
            }

            // 更新序列化字段
            SerializedProperty uiGameObjectsProp = serializedObject.FindProperty("uiGameObjects");
            if (uiGameObjectsProp != null)
            {
                uiGameObjectsProp.arraySize = uiGameObjects.Count;
                for (int i = 0; i < uiGameObjects.Count; i++)
                {
                    uiGameObjectsProp.GetArrayElementAtIndex(i).objectReferenceValue = uiGameObjects[i];
                }

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_panel);
            }

            BxDebug.Log($"成功初始化面板GameObject组件，共找到 {uiGameObjects.Count} 个BxUI GameObject");
        }

        /// <summary>
        /// 生成UI控件代码
        /// 提取UIBehaviour数组和GameObject数组中的所有控件，生成对应的字段和初始化方法代码
        /// </summary>
        private void GenerateUIControlCode()
        {
            if (_panel == null)
            {
                BxDebug.LogError("面板为空，无法生成UI控件代码");
                return;
            }

            SerializedProperty uiBehavioursProp = serializedObject.FindProperty("uiBehaviours");
            SerializedProperty uiGameObjectsProp = serializedObject.FindProperty("uiGameObjects");

            if (uiBehavioursProp == null && uiGameObjectsProp == null)
            {
                BxDebug.LogError("数组为空，请先点击\"初始化面板UI组件\"按钮 或 \"初始化面板GameObject组件\" ");
                return;
            }

            bool flag1 = false;
            bool flag2 = false;
            
            if (uiBehavioursProp != null && uiBehavioursProp.arraySize > 0)
            {
                flag1 = true;
            }

            if (uiGameObjectsProp != null && uiGameObjectsProp.arraySize > 0)
            {
                flag2 = true;
            }

            if (!flag1 && !flag2)
            {
                BxDebug.LogError("数组为空，请先点击\"初始化面板UI组件\"按钮 或 \"初始化面板GameObject组件\" ");
                return;
            }

            StringBuilder fieldsBuilder = new StringBuilder();
            StringBuilder initBuilder = new StringBuilder();
            StringBuilder initGameObjectsBuilder = new StringBuilder();

            // 生成UIBehaviour字段和初始化代码
            for (int i = 0; i < uiBehavioursProp.arraySize; i++)
            {
                SerializedProperty behaviourProp = uiBehavioursProp.GetArrayElementAtIndex(i);
                UIBehaviour behaviour = behaviourProp.objectReferenceValue as UIBehaviour;

                if (behaviour != null)
                {
                    string typeName = behaviour.GetType().Name;
                    string gameObjectName = behaviour.gameObject.name;
                    string fieldName = GetFieldName(gameObjectName, typeName);

                    // 生成字段代码
                    // fieldsBuilder.AppendLine($"\t[Tooltip(\"{gameObjectName}\")]");
                    fieldsBuilder.AppendLine($"\tprivate {typeName} _{fieldName};");

                    // 生成初始化代码
                    initBuilder.AppendLine($"\t\t_{fieldName} = uiBehaviours[{i}] as {typeName};");
                }
            }

            // 生成GameObject字段和初始化代码
            if (uiGameObjectsProp != null && uiGameObjectsProp.arraySize > 0)
            {
                fieldsBuilder.AppendLine();
                fieldsBuilder.AppendLine("\t\t// ==== GameObject字段 ====");
                initGameObjectsBuilder.AppendLine();

                for (int i = 0; i < uiGameObjectsProp.arraySize; i++)
                {
                    SerializedProperty gameObjectProp = uiGameObjectsProp.GetArrayElementAtIndex(i);
                    GameObject gameObject = gameObjectProp.objectReferenceValue as GameObject;

                    if (gameObject != null)
                    {
                        string gameObjectName = gameObject.name;
                        string fieldName = GetFieldName(gameObjectName, "GameObject");

                        // 生成字段代码
                        fieldsBuilder.AppendLine($"\tprivate GameObject _{fieldName};");

                        // 生成初始化代码
                        initGameObjectsBuilder.AppendLine($"\t\t_{fieldName} = uiGameObjects[{i}];");
                    }
                }
            }

            // 生成完整代码
            StringBuilder fullCodeBuilder = new StringBuilder();
            fullCodeBuilder.AppendLine("\t\t// ==== 字段定义 ====");
            fullCodeBuilder.AppendLine(fieldsBuilder.ToString());
            fullCodeBuilder.AppendLine();
            fullCodeBuilder.AppendLine("\t\t// ==== 初始化方法 ====");
            fullCodeBuilder.AppendLine(
                "private void InitializeUIComponents(UIBehaviour[] uiBehaviours, GameObject[] uiGameObjects = null)");
            fullCodeBuilder.AppendLine("{");
            fullCodeBuilder.AppendLine(initBuilder.ToString());

            if (initGameObjectsBuilder.Length > 0)
            {
                fullCodeBuilder.AppendLine("\t\t\t\n// 初始化GameObject");
                fullCodeBuilder.AppendLine("\tif (uiGameObjects != null)");
                fullCodeBuilder.AppendLine("\t{");
                fullCodeBuilder.AppendLine(initGameObjectsBuilder.ToString());
                fullCodeBuilder.AppendLine("\t}");
            }

            fullCodeBuilder.AppendLine("}");

            // 复制到剪贴板
            EditorGUIUtility.systemCopyBuffer = fullCodeBuilder.ToString();
            BxDebug.Log($"UI控件代码已生成并复制到剪贴板");
        }

        /// <summary>
        /// 根据游戏对象名称和类型生成字段名
        /// </summary>
        private string GetFieldName(string gameObjectName, string typeName)
        {
            // 移除类型后缀（如Button、Text等）
            if (gameObjectName.EndsWith(typeName))
            {
                gameObjectName = gameObjectName.Substring(0, gameObjectName.Length - typeName.Length);
            }

            // 驼峰命名转换
            string[] parts =
                gameObjectName.Split(new char[] {'_', ' ', '-'}, System.StringSplitOptions.RemoveEmptyEntries);
            StringBuilder fieldNameBuilder = new StringBuilder();

            for (int i = 0; i < parts.Length; i++)
            {
                if (i == 0)
                {
                    fieldNameBuilder.Append(parts[i].ToLower());
                }
                else
                {
                    fieldNameBuilder.Append(char.ToUpper(parts[i][0]));
                    fieldNameBuilder.Append(parts[i].Substring(1));
                }
            }

            // 添加类型名（首字母大写）
            fieldNameBuilder.Append(char.ToUpper(typeName[0]));
            fieldNameBuilder.Append(typeName.Substring(1));

            return fieldNameBuilder.ToString();
        }

        [MenuItem("GameObject/BasyaFramework/UI/BxUIPanel", false, 10)]
        private static void CreateBxUIPanel()
        {
            GameObject panelObj = new GameObject("BxUIPanel");
            panelObj.AddComponent<BxUIPanel>();
            panelObj.AddComponent<CanvasRenderer>();
            panelObj.AddComponent<RectTransform>();

            // 如果有选中对象，将新建的面板作为子对象
            if (Selection.activeGameObject != null)
            {
                panelObj.transform.SetParent(Selection.activeGameObject.transform);
                panelObj.transform.localPosition = Vector3.zero;
                panelObj.transform.localRotation = Quaternion.identity;
                panelObj.transform.localScale = Vector3.one;
            }

            // 选中新创建的对象
            Selection.activeGameObject = panelObj;
        }
    }
}