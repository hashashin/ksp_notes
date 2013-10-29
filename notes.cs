// -------------------------------------------------------------------------------------------------
// notes.cs 0.2.1
//
// Simple KSP plugin to take notes ingame.
// Copyright (C) 2013 Iván Atienza
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
// 
// Email: mecagoenbush at gmail dot com
// Freenode: hashashin
//
// -------------------------------------------------------------------------------------------------


using UnityEngine;
using KSP.IO;

[KSPAddon(KSPAddon.Startup.EveryScene, false)]
public class notes : MonoBehaviour
{
    private Vector2 scrollViewVector = Vector2.zero;
    private static string _notesdir = "GameData/notes/Plugins/PluginData/";
    private static string _note
    {
        get { return KSPUtil.ApplicationRootPath + _notesdir + "notes.txt"; }
    }
    private string _text = KSP.IO.File.ReadAllText<notes>(_note);
    private bool _visible = false;
    private string _file = "notes.txt";

    private void OnGUI()
    {
        DontDestroyOnLoad(this);
        if (_visible)
        {
            scrollViewVector = GUI.BeginScrollView(new Rect(50f, 25f, 390f, 390f), scrollViewVector, new Rect(0f, 0f, 400f, 600f));
            _text = GUI.TextArea(new Rect(0f, 0f, 400f, 600f), _text);

            GUI.EndScrollView();

            _file = GUI.TextField(new Rect(100f, 413f, 60f, 30f), _file);

            if (GUI.Button(new Rect(160f, 413f, 80f, 30f), "Save"))
            {
                KSP.IO.File.WriteAllText<notes>(_text, _notesdir + _file);
            }

            if (GUI.Button(new Rect(240f, 413f, 80f, 30f), "Load"))
            {

                if (KSP.IO.File.Exists<notes>(_file) == true)
                {
                    _text = KSP.IO.File.ReadAllText<notes>(_notesdir + _file);
                }
                else
                {
                    print("[notes.dll] this file dont exist: " + _file);
                }
            }

            if (GUI.Button(new Rect(420f, 15f, 10f, 10f), ""))
            {
                KSP.IO.File.WriteAllText<notes>(_text, _notesdir + _file);
                _visible = false;
            }
        }

        else
        {
            if (GUI.Button(new Rect(420f, 15f, 10f, 10f), ""))
            {
                _visible = true;
            }
        }
    }
    
    private void OnDestroy()
    {
        KSP.IO.File.WriteAllText<notes>(_text, _notesdir + _file);
        Destroy(this);
    }
}
