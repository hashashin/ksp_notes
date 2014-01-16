// -------------------------------------------------------------------------------------------------
// notes.cs 0.4.1
//
// Simple KSP plugin to take notes ingame.
// Copyright (C) 2014 Iván Atienza
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
using System.IO;
using System.Collections.Generic;

namespace notes
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class notes : MonoBehaviour
    {
        private Vector2 _scrollViewVector = Vector2.zero;
        private Vector2 _scrollViewVector2 = Vector2.zero;
        private static string _configfile = "notes.cfg";
        private static string _notesdir = "notes/Plugins/PluginData/";
        private static string _file = KSP.IO.File.ReadAllText<notes>(_notesdir + _configfile);
        private string _text = KSP.IO.File.ReadAllText<notes>(_notesdir + _file + ".txt");
        private bool _visible = false;
        private Rect _windowRect;
        private Rect _windowRect2;
        private ToolbarButtonWrapper _button;
        private string _keybind;
        private bool _popup = false;
        private static string _mypath = KSPUtil.ApplicationRootPath + "Gamedata/" + _notesdir + "notes/";
        private List<string> fileNames;
        private string _dirOutputString = "";

        public void Awake()
        {
            LoadSettings();
            if (_windowRect == new Rect(0, 0, 0, 0))
            {
                _windowRect = new Rect(50f, 25f, 425f, 440f);
            }
            if (_windowRect2 == new Rect(0, 0, 0, 0))
            {
                _windowRect2 = new Rect((Screen.width / 2) - 150f, (Screen.height / 2) - 75f, 310f, 405f);
            }
            if (_keybind == null)
            {
                _keybind = "n";
            }
        }

        public void OnGUI()
        {
            if (ToolbarButtonWrapper.ToolbarManagerPresent)
            {
                _button = ToolbarButtonWrapper.TryWrapToolbarButton("notes", "toggle");
                _button.TexturePath = "notes/icon";
                _button.ToolTip = "Notes plugin";
                _button.AddButtonClickHandler((e) =>
                {
                    if (_visible == true)
                    {
                        _visible = false;
                        _popup = false;
                    }
                    else
                    {
                        _visible = true;
                    }
                });
            }
            if (_visible)
            {
                _windowRect = GUI.Window(0, _windowRect, DoMyWindow, "Notes");
            }
            if (_popup)
            {
                _windowRect2 = GUI.Window(1, _windowRect2, Listnotes, "Notes list");
            }
        }

        public void Listnotes(int windowID)
        {
            _scrollViewVector2 = GUI.BeginScrollView(new Rect(3f, 15f, 300f, 350f), _scrollViewVector2, new Rect(0f, 0f, 200f, 4360f));
            GUI.Label(new Rect(0f, 0f, 285f, 4360f), _dirOutputString, "textfield");
            GUI.EndScrollView();
            if (GUI.Button(new Rect(5f, 370f, 80f, 30f), "OK"))
            {
                _dirOutputString = "";
                _popup = false;
            }
            GUI.DragWindow();
        }

        public void DoMyWindow(int windowID)
        {
            _scrollViewVector = GUI.BeginScrollView(new Rect(0f, 15f, 420f, 380f), _scrollViewVector, new Rect(0f, 0f, 400f, 4360f));
            _text = GUI.TextArea(new Rect(3f, 0f, 400f, 4360f), _text);
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
            if (GUI.Button(new Rect(265f, 400f, 80f, 30f), "List Notes"))
            {
                if (_dirOutputString == "")
                {
                    this.fileNames = new System.Collections.Generic.List<string>(Directory.GetFiles(_mypath, "*.txt"));

                    for (int i = 0; i < this.fileNames.Count; i++)
                    {
                        this.fileNames[i] = Path.GetFileName(this.fileNames[i]);
                        this._dirOutputString += i.ToString("D5") + "\t-\t" + this.fileNames[i] + "\n";
                    }
                }
                _popup = true;
            }
            GUI.DragWindow();
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(_keybind))
            {
                if (_visible == true)
                {
                    _visible = false;
                    _popup = false;
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
            SaveSettings();
        }

        private void Save()
        {
            KSP.IO.File.WriteAllText<notes>(_text, _notesdir + _file + ".txt");
            KSP.IO.File.WriteAllText<notes>(_file, _notesdir + _configfile);
        }

        private void Load()
        {
            if (KSP.IO.File.Exists<notes>(_file + ".txt") == true)
            {
                _text = KSP.IO.File.ReadAllText<notes>(_notesdir + _file + ".txt");
            }
            else
            {
                print("[notes.dll] this file dont exist: " + _file + ".txt");
            }
        }

        private void LoadSettings()
        {
            print("[notes.dll] Loading Config...");
            KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<notes>();
            configfile.load();

            _windowRect = configfile.GetValue<Rect>("windowpos");
            _windowRect2 = configfile.GetValue<Rect>("listwindowpos");
            _keybind = configfile.GetValue<string>("keybind");
            print("[notes.dll] Config Loaded Successfully");
        }

        private void SaveSettings()
        {
            print("[notes.dll] Saving Config...");
            KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<notes>();

            configfile.SetValue("windowpos", _windowRect);
            configfile.SetValue("listwindowpos", _windowRect2);
            configfile.SetValue("keybind", _keybind);

            configfile.save();
            print("[notes.dll] Config Saved ");
        }
    }
}
