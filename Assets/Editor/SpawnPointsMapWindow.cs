using UnityEditor;
public class SpawnPointsMapWindow : EditorWindow
{
    // Start is called before the first frame update
    private SpawnPointsMap _map;
    private Editor MainWindow;
    [MenuItem("Window/SpawnPointsMap")]
    public static void Show()// которая вызовет наше окно
    {
        EditorWindow.GetWindow<SpawnPointsMapWindow>();

    }
    public void OnGUI()
    {
        _map = FindObjectOfType<SpawnPointsMap>();
        if (_map != null)
        {
            MainWindow??= Editor.CreateEditor(_map);
            MainWindow.OnInspectorGUI();
        }
    }
}
