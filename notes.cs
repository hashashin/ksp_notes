// -------------------------------------------------------------------------------------------------
// notes.cs
//
// Simple KSP plugin to take notes ingame.
// Copyright (C) 2013 Iván Atienza
//
// This program is free software: you can redistribute it and/or modify
// it under the +terms of the GNU General Public License as published by
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
    private static string _file
    {
        get { return KSPUtil.ApplicationRootPath + "GameData/notes/Plugins/PluginData/notes.txt"; }
    }
    private string _text = KSP.IO.File.ReadAllText<notes>(_file);
    private bool _visible = false;
    
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
