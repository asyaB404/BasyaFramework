using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BasyaFramework.Logger;
using BasyaFramework.UI;
using UnityEngine.EventSystems;

namespace BasyaFramework.Editor.UI
{
    public class BxUIEditorWindow : EditorWindow
    {
        private BxUIPanel _selectedPanel;
        private Vector2 _scrollPosition;
        private bool _showGeneratedCode = false;
        private string _generatedCode = string.Empty;

        [MenuItem("Tools/BasyaFramework/UI Editor", false, 100)]
        private static void ShowWindow()
        {
            BxUIEditorWindow window = GetWindow<BxUIEditorWindow>("UI Editor");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnSelectionChange()
        {
            // 当选择改变时，如果选中的是BxUIPanel，更新_selectedPanel
            if (Selection.activeGameObject != null)
            {
                _selectedPanel = Selection.activeGameObject.GetComponent<BxUIPanel>();
            }
            else
            {
                _selectedPanel = null;
            }
            Repaint();
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // 面板选择区域
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("面板选择", EditorStyles.boldLabel);

            _selectedPanel = (BxUIPanel)EditorGUILayout.ObjectField(
                "选择面板", 
                _selectedPanel, 
                typeof(BxUIPanel), 
                true
            );

            if (_selectedPanel != null)
            {
                EditorGUILayout.LabelField("当前面板: " + _selectedPanel.name, EditorStyles.label);
            }
            else
            {
                EditorGUILayout.HelpBox("请选择一个包含BxUIPanel组件的GameObject", MessageType.Info);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // 面板操作区域
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("面板操作", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(_selectedPanel == null);

            // 初始化面板UI组件按钮
            if (GUILayout.Button("初始化面板UI组件", GUILayout.Height(30)))
            {
                InitializeUIBevaviours();
            }

            // 初始化面板GameObject组件按钮
            if (GUILayout.Button("初始化面板GameObject组件", GUILayout.Height(30)))
            {
                InitializeUIGameObjects();
            }

            // 生成UI控件代码按钮
            if (GUILayout.Button("生成UI控件代码", GUILayout.Height(30)))
            {
                GenerateUIControlCode();
                _showGeneratedCode = true;
            }

            // 一键生成并复制所有UI代码按钮
            if (GUILayout.Button("一键生成并复制所有UI代码", GUILayout.Height(30)))
            {
                InitializeUIBevaviours();
                InitializeUIGameObjects();
                GenerateUIControlCode();
                _showGeneratedCode = true;
                GUIUtility.ExitGUI();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // 生成的代码显示区域
            if (_showGeneratedCode && !string.IsNullOrEmpty(_generatedCode))
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("生成的代码", EditorStyles.boldLabel);

                EditorGUILayout.TextArea(_generatedCode, GUILayout.ExpandHeight(true));

                if (GUILayout.Button("复制到剪贴板"))
                {
                    EditorGUIUtility.systemCopyBuffer = _generatedCode;
                    BxDebug.Log("代码已复制到剪贴板");
                }

                if (GUILayout.Button("保存到文件"))
                {
                    SaveCodeToFile();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            // UI工具集
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("UI工具集", EditorStyles.boldLabel);

            if (GUILayout.Button("创建BxUIPanel", GUILayout.Height(25)))
            {
                CreateBxUIPanel();
            }

            if (GUILayout.Button("批量设置BxUI标签", GUILayout.Height(25)))
            {
                BatchSetBxUITag();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 初始化面板UI组件
        /// 遍历面板以及子节点中所有tag为BxUI的UIBehaiour，然后赋值到uiBehaviours数组
        /// </summary>
        private void InitializeUIBevaviours()
        {
            if (_selectedPanel == null)
            {
                BxDebug.LogError("面板为空，无法初始化UI组件");
                return;
            }

            // 查找所有tag为BxUI的UIBehaviour
            List<UIBehaviour> uiBehaviours = new List<UIBehaviour>();
            UIBehaviour[] allBehaviours = _selectedPanel.GetComponentsInChildren<UIBehaviour>(true);

            foreach (UIBehaviour behaviour in allBehaviours)
            {
                if (behaviour.CompareTag("BxUI"))
                {
                    uiBehaviours.Add(behaviour);
                }
            }

            // 更新序列化字段
            SerializedObject serializedPanel = new SerializedObject(_selectedPanel);
            SerializedProperty uiBehavioursProp = serializedPanel.FindProperty("uiBehaviours");
            if (uiBehavioursProp != null)
            {
                uiBehavioursProp.arraySize = uiBehaviours.Count;
                for (int i = 0; i < uiBehaviours.Count; i++)
                {
                    uiBehavioursProp.GetArrayElementAtIndex(i).objectReferenceValue = uiBehaviours[i];
                }

                serializedPanel.ApplyModifiedProperties();
                EditorUtility.SetDirty(_selectedPanel);
            }

            BxDebug.Log($"成功初始化面板UI组件，共找到 {uiBehaviours.Count} 个BxUI组件");
        }

        /// <summary>
        /// 初始化面板GameObject组件
        /// 遍历面板以及子节点中所有tag为BxUI的GameObject，然后赋值到uiGameObjects数组
        /// </summary>
        private void InitializeUIGameObjects()
        {
            if (_selectedPanel == null)
            {
                BxDebug.LogError("面板为空，无法初始化GameObject组件");
                return;
            }

            // 查找所有tag为BxUI的GameObject
            List<GameObject> uiGameObjects = new List<GameObject>();
            Transform[] allChildren = _selectedPanel.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in allChildren)
            {
                if (child.gameObject.CompareTag("BxUI"))
                {
                    uiGameObjects.Add(child.gameObject);
                }
            }

            // 更新序列化字段
            SerializedObject serializedPanel = new SerializedObject(_selectedPanel);
            SerializedProperty uiGameObjectsProp = serializedPanel.FindProperty("uiGameObjects");
            if (uiGameObjectsProp != null)
            {
                uiGameObjectsProp.arraySize = uiGameObjects.Count;
                for (int i = 0; i < uiGameObjects.Count; i++)
                {
                    uiGameObjectsProp.GetArrayElementAtIndex(i).objectReferenceValue = uiGameObjects[i];
                }

                serializedPanel.ApplyModifiedProperties();
                EditorUtility.SetDirty(_selectedPanel);
            }

            BxDebug.Log($"成功初始化面板GameObject组件，共找到 {uiGameObjects.Count} 个BxUI GameObject");
        }

        /// <summary>
        /// 生成UI控件代码
        /// 提取UIBehaviour数组和GameObject数组中的所有控件，生成对应的字段和初始化方法代码
        /// </summary>
        private void GenerateUIControlCode()
        {
            if (_selectedPanel == null)
            {
                BxDebug.LogError("面板为空，无法生成UI控件代码");
                return;
            }

            SerializedObject serializedPanel = new SerializedObject(_selectedPanel);
            SerializedProperty uiBehavioursProp = serializedPanel.FindProperty("uiBehaviours");
            SerializedProperty uiGameObjectsProp = serializedPanel.FindProperty("uiGameObjects");

            if (uiBehavioursProp == null || uiBehavioursProp.arraySize == 0)
            {
                BxDebug.LogError("uiBehaviours数组为空，请先点击\"初始化面板UI组件\"按钮");
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
                initGameObjectsBuilder.AppendLine("\t\t// ==== 初始化GameObject ====");
                
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
            fullCodeBuilder.AppendLine("private void InitializeUIComponents(UIBehaviour[] uiBehaviours, GameObject[] uiGameObjects = null)");
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

            // 保存到成员变量以便显示
            _generatedCode = fullCodeBuilder.ToString();

            // 复制到剪贴板
            EditorGUIUtility.systemCopyBuffer = _generatedCode;

            BxDebug.Log($"UI控件代码已生成并复制到剪贴板。");
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
            string[] parts = gameObjectName.Split(new char[] { '_', ' ', '-' }, System.StringSplitOptions.RemoveEmptyEntries);
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

        /// <summary>
        /// 保存代码到文件
        /// </summary>
        private void SaveCodeToFile()
        {
            string defaultName = _selectedPanel != null ? _selectedPanel.name + "_UICode.cs" : "UIControlCode.cs";
            string filePath = EditorUtility.SaveFilePanel("保存UI控件代码", Application.dataPath, defaultName, "cs");

            if (!string.IsNullOrEmpty(filePath))
            {
                File.WriteAllText(filePath, _generatedCode);
                AssetDatabase.Refresh();
                BxDebug.Log("代码已保存到: " + filePath);
            }
        }

        /// <summary>
        /// 创建BxUIPanel
        /// </summary>
        private void CreateBxUIPanel()
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
            _selectedPanel = panelObj.GetComponent<BxUIPanel>();

            BxDebug.Log("已创建BxUIPanel对象");
        }

        /// <summary>
        /// 批量设置BxUI标签
        /// </summary>
        private void BatchSetBxUITag()
        {
            // 确保BxUI标签存在
            EnsureBxUITagExists();

            // 获取选中的所有GameObject
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                BxDebug.LogWarning("请先选择要设置标签的GameObject");
                return;
            }

            int count = 0;
            foreach (GameObject obj in selectedObjects)
            {
                UIBehaviour behaviour = obj.GetComponent<UIBehaviour>();
                if (behaviour != null)
                {
                    obj.tag = "BxUI";
                    count++;
                }
            }

            BxDebug.Log($"已为 {count} 个UI组件设置了BxUI标签");
        }

        /// <summary>
        /// 确保BxUI标签存在
        /// </summary>
        private void EnsureBxUITagExists()
        {
            // 检查BxUI标签是否存在
            bool tagExists = false;
            foreach (string tag in UnityEditorInternal.InternalEditorUtility.tags)
            {
                if (tag == "BxUI")
                {
                    tagExists = true;
                    break;
                }
            }

            // 如果不存在，创建BxUI标签
            if (!tagExists)
            {
                UnityEditorInternal.InternalEditorUtility.AddTag("BxUI");
                BxDebug.Log("已创建BxUI标签");
            }
        }
    }
}