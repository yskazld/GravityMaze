#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.IO;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class GenerateGameSceneEditor
{
    [MenuItem("Tools/Generate/Game Scene and Prefabs")]
    public static void Generate()
    {
        // ensure folders
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        // ensure tags
        AddTagIfMissing("Player");
        AddTagIfMissing("Wall");
        AddTagIfMissing("Box");
        AddTagIfMissing("Enemy");
        AddTagIfMissing("Goal");

        // create prefabs
        string prefPath = "Assets/Prefabs";

        // Player prefab
        var playerGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        playerGO.name = "PlayerPrefab";
        var playerComp = playerGO.AddComponent<PlayerController>();
        playerComp.cell = new UnityEngine.Vector2Int(0, 0);
        playerGO.tag = "Player";
        playerGO.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
        ApplyMaterial(playerGO, "PlayerBlue", new Color(0.1f, 0.35f, 0.9f));
        var playerPrefabPath = Path.Combine(prefPath, "PlayerPrefab.prefab");
        PrefabUtility.SaveAsPrefabAsset(playerGO, playerPrefabPath);
        Object.DestroyImmediate(playerGO);

        // Boundary wall prefab
        var wallAreaGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallAreaGO.name = "WallAreaPrefab";
        wallAreaGO.tag = "Wall";
        wallAreaGO.transform.localScale = new Vector3(1f, 0.5f, 1f);
        ApplyMaterial(wallAreaGO, "WallGray", new Color(0.5f, 0.5f, 0.5f));
        var wallAreaPrefabPath = Path.Combine(prefPath, "WallAreaPrefab.prefab");
        PrefabUtility.SaveAsPrefabAsset(wallAreaGO, wallAreaPrefabPath);
        Object.DestroyImmediate(wallAreaGO);

        // Inside wall prefab
        var wallInsideGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallInsideGO.name = "WallInsidePrefab";
        wallInsideGO.tag = "Wall";
        wallInsideGO.transform.localScale = new Vector3(1f, 0.5f, 1f);
        ApplyMaterial(wallInsideGO, "WallInsideGray", new Color(0.42f, 0.46f, 0.52f));
        var wallInsidePrefabPath = Path.Combine(prefPath, "WallInsidePrefab.prefab");
        PrefabUtility.SaveAsPrefabAsset(wallInsideGO, wallInsidePrefabPath);
        Object.DestroyImmediate(wallInsideGO);

        // Box prefab
        var boxGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boxGO.name = "BoxPrefab";
        var boxComp = boxGO.AddComponent<BoxController>();
        boxGO.tag = "Box";
        boxGO.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        ApplyMaterial(boxGO, "BoxOrange", new Color(1f, 0.5f, 0.1f));
        var boxPrefabPath = Path.Combine(prefPath, "BoxPrefab.prefab");
        PrefabUtility.SaveAsPrefabAsset(boxGO, boxPrefabPath);
        Object.DestroyImmediate(boxGO);

        // Enemy prefab
        var enemyGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        enemyGO.name = "EnemyPrefab";
        var enemyComp = enemyGO.AddComponent<EnemyController>();
        enemyGO.tag = "Enemy";
        ApplyMaterial(enemyGO, "EnemyRed", new Color(0.9f, 0.15f, 0.15f));
        var enemyPrefabPath = Path.Combine(prefPath, "EnemyPrefab.prefab");
        PrefabUtility.SaveAsPrefabAsset(enemyGO, enemyPrefabPath);
        Object.DestroyImmediate(enemyGO);

        // Goal prefab
        var goalGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        goalGO.name = "GoalPrefab";
        goalGO.tag = "Goal";
        goalGO.transform.localScale = new Vector3(0.8f, 0.15f, 0.8f);
        ApplyMaterial(goalGO, "GoalGreen", new Color(0.2f, 0.8f, 0.25f));
        var goalPrefabPath = Path.Combine(prefPath, "GoalPrefab.prefab");
        PrefabUtility.SaveAsPrefabAsset(goalGO, goalPrefabPath);
        Object.DestroyImmediate(goalGO);

        AssetDatabase.Refresh();

        // create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

        // Main Camera
        var cameraGO = new GameObject("Main Camera");
        var camera = cameraGO.AddComponent<Camera>();
        cameraGO.tag = "MainCamera";
        camera.transform.position = new Vector3(1f, 13f, -5f);
        camera.transform.rotation = Quaternion.Euler(68f, 0f, 0f);
        camera.clearFlags = CameraClearFlags.Skybox;

        // GridManager
        var gridGO = new GameObject("GridManager");
        var gridComp = gridGO.AddComponent<GridManager>();
        gridComp.width = 3;
        gridComp.height = 3;
        gridComp.cellSize = 1f;

        // StageGenerator
        var stageGenGO = new GameObject("StageGenerator");
        var stageGenComp = stageGenGO.AddComponent<StageGenerator>();
        stageGenComp.grid = gridComp;
        stageGenComp.playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);
        stageGenComp.goalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(goalPrefabPath);
        stageGenComp.wallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(wallInsidePrefabPath);
        stageGenComp.boundaryWallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(wallAreaPrefabPath);
        stageGenComp.insideWallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(wallInsidePrefabPath);
        stageGenComp.boxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(boxPrefabPath);
        stageGenComp.enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(enemyPrefabPath);

        // Instantiate default 3x3 via StageGenerator
        stageGenComp.GenerateDefault3x3();

        // GameManager
        var gmGO = new GameObject("GameManager");
        var gmComp = gmGO.AddComponent<GameManager>();
        gmComp.grid = gridComp;
        gmComp.stageGenerator = stageGenComp;

        // Collect and assign Player, Boxes, Enemies into GameManager lists
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            gmComp.player = playerObj.GetComponent<PlayerController>();
        }

        var boxes = GameObject.FindGameObjectsWithTag("Box");
        foreach (var b in boxes)
        {
            var comp = b.GetComponent<BoxController>();
            if (comp != null) gmComp.boxes.Add(comp);
        }

        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var e in enemies)
        {
            var comp = e.GetComponent<EnemyController>();
            if (comp != null) gmComp.enemies.Add(comp);
        }

        // UI: Canvas and 4 buttons
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var buttonsGO = new GameObject("Buttons");
        buttonsGO.transform.SetParent(canvasGO.transform, false);
        var buttonsRect = buttonsGO.AddComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(1f, 0f);
        buttonsRect.anchorMax = new Vector2(1f, 0f);
        buttonsRect.pivot = new Vector2(1f, 0f);
        buttonsRect.sizeDelta = new Vector2(220f, 220f);
        buttonsRect.anchoredPosition = new Vector2(-30f, 30f);

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
#if ENABLE_INPUT_SYSTEM
        var inputModule = eventSystem.AddComponent<InputSystemUIInputModule>();
        inputModule.AssignDefaultActions();
#else
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif

        var uiControllerGO = new GameObject("UIController");
        var uiControllerComp = uiControllerGO.AddComponent<UIController>();
        uiControllerComp.gameManager = gmComp;

        CreateButton(buttonsGO.transform, "Up", new UnityEngine.Vector2(-70f, 140f), uiControllerComp, "OnUp");
        CreateButton(buttonsGO.transform, "Down", new UnityEngine.Vector2(-70f, 0f), uiControllerComp, "OnDown");
        CreateButton(buttonsGO.transform, "Left", new UnityEngine.Vector2(-140f, 70f), uiControllerComp, "OnLeft");
        CreateButton(buttonsGO.transform, "Right", new UnityEngine.Vector2(0f, 70f), uiControllerComp, "OnRight");

        // Save scene
        string scenePath = "Assets/Scenes/Game.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();

        Debug.Log("Game scene and prefabs generated at Assets/Scenes/Game.unity and Assets/Prefabs/");
    }

    static void CreateButton(Transform parent, string name, UnityEngine.Vector2 anchoredPos, UIController uiController, string methodName)
    {
        var btnGO = new GameObject(name + "Button");
        btnGO.transform.SetParent(parent, false);
        var rect = btnGO.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.sizeDelta = new UnityEngine.Vector2(80, 40);
        rect.anchoredPosition = anchoredPos;
        var img = btnGO.AddComponent<UnityEngine.UI.Image>();
        img.color = Color.white;
        var btn = btnGO.AddComponent<UnityEngine.UI.Button>();

        // add Text child
        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        var txtRect = txtGO.AddComponent<RectTransform>();
        txtRect.sizeDelta = rect.sizeDelta;
        txtRect.anchoredPosition = Vector2.zero;
        var txt = txtGO.AddComponent<UnityEngine.UI.Text>();
        txt.text = name;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.black;
        // Use LegacyRuntime font for newer Unity versions where Arial.ttf may be unavailable
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var action = btnGO.AddComponent<ButtonAction>();
        action.targetButton = btn;
        action.targetObject = uiController;
        action.methodName = methodName;
    }

    static void AddTagIfMissing(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tag)) return;
        }

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
        newTag.stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    static void ApplyMaterial(GameObject target, string materialName, Color color)
    {
        var renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        string materialPath = $"Assets/Materials/{materialName}.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, materialPath);
        }

        material.color = color;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        renderer.sharedMaterial = material;
    }
}
#endif
