// -------------------------------------------------------------------------------------------------
// notes.cs 0.4
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
using Toolbar;

[KSPAddon(KSPAddon.Startup.EveryScene, false)]
public class notes : MonoBehaviour
{
    private Vector2 _scrollViewVector = Vector2.zero;
    private static string _configfile = "notes.cfg";
    private static string _notesdir = "notes/Plugins/PluginData/";
    private static string _file = File.ReadAllText<notes>(_notesdir + _configfile);
    private string _text = File.ReadAllText<notes>(_notesdir + _file + ".txt");
    private bool _visible = false;
    private Rect _windowRect = new Rect(50f, 25f, 425f, 440f);
    private IButton _button;

    private void OnGUI()
    {
        _button = ToolbarManager.Instance.add("notes", "toggle");
        _button.TexturePath = "notes/icon";
        _button.ToolTip = "Notes plugin";
        _button.OnClick += (e) =>
        {
            if (_visible == true)
            {
                _visible = false;
            }
            else
            {
                _visible = true;
            }
        };
        if (_visible)
        {
            _windowRect = GUI.Window(0, _windowRect, DoMyWindow, "Notes");
        }
    }

    private void DoMyWindow(int windowID)
    {
        _scrollViewVector = GUI.BeginScrollView(new Rect(0f, 15f, 420f, 380f), _scrollViewVector, new Rect(0f, 0f, 400f, 4360f));
        _text = GUI.TextArea(new Rect(5f, 0f, 400f, 4360f), _text);
        GUI.EndScrollView();

        _file = GUI.TextField(new Rect(5f, 400f, 100f, 20f), _file);

        if (GUI.Button(new Rect(105f, 400f, 80f, 30f), "Open"))
        {
            Load();
        }

        if (GUI.Button(new Rect(185f, 400f, 80f, 30f), "Save"))
        {
            Save();
        }
        GUI.DragWindow();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown("n"))
        {
            if (_visible == true)
            {
                _visible = false;
            }
            else
            {
                _visible = true;
            }

        }
    }

    void OnDestroy()
    {
        Save();
    }

    private void Save()
    {
        File.WriteAllText<notes>(_text, _notesdir + _file + ".txt");
        File.WriteAllText<notes>(_file, _notesdir + _configfile);
    }

    private void Load()
    {
        if (File.Exists<notes>(_file + ".txt") == true)
        {
            _text = File.ReadAllText<notes>(_notesdir + _file + ".txt");
        }
        else
        {
            print("[notes.dll] this file dont exist: " + _file + ".txt");
        }
    }
}
