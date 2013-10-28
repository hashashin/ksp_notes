using UnityEngine;
using KSP.IO;

[KSPAddon(KSPAddon.Startup.EveryScene, false)]
public class notes : MonoBehaviour
{
    public Vector2 scrollViewVector = Vector2.zero;
    public static string _file
    {
        get { return KSPUtil.ApplicationRootPath + "GameData/notes/Plugins/PluginData/notes.txt"; }
    }
    public string _text = KSP.IO.File.ReadAllText<notes>(_file);
    public bool _visible = false;
    
    void OnGUI()
    {
        if (_visible)
        {
            scrollViewVector = GUI.BeginScrollView(new Rect(50f, 25f, 390f, 390f), scrollViewVector, new Rect(0f, 0f, 400f, 600f));
            _text = GUI.TextArea(new Rect(0f, 0f, 400f, 600f), _text);
            
            GUI.EndScrollView();
            
            if (GUI.Button(new Rect(50f, 413f, 42f, 30f), "Save"))
            {
                KSP.IO.File.WriteAllText<notes>(_text, _file);
            }
            
            if (GUI.Button(new Rect(420f, 15f, 10f, 10f), ""))
            {
                _visible = false;
            }
        }
        
        if (_visible == false)
        {
            if (GUI.Button(new Rect(420f, 15f, 10f, 10f), ""))
            {
                _visible = true;
            }
        }
    }
}
