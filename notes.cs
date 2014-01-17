// -------------------------------------------------------------------------------------------------
// notes.cs 0.6
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
        private static string _configfile = "notes.dat";
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
        private List<string> _filenames;
        public int _selectiongridint = 0;

        public void Awake()
        {
            LoadSettings();
            CheckDefaults();
        }

        public void OnGUI()
        {
            if (ToolbarButtonWrapper.ToolbarManagerPresent)
            {
                _button = ToolbarButtonWrapper.TryWrapToolbarButton("notes", "toggle");
                _button.TexturePath = "notes/Textures/icon";
                _button.ToolTip = "Notes plugin";
                _button.AddButtonClickHandler((e) =>
                {
                    Toggle();
                });
            }
            if (_visible)
            {
                _windowRect = GUI.Window(9999999, _windowRect, DoMyWindow, "Notes");
            }
            if (_popup)
            {
                _windowRect2 = GUI.Window(9999998, _windowRect2, Listnotes, "Notes list");
            }
        }

        public void Listnotes(int windowID)
        {
            _scrollViewVector2 = GUI.BeginScrollView(new Rect(3f, 15f, 295f, 300f), _scrollViewVector2, new Rect(0f, 0f, 0f, 4360f));
            _selectiongridint = GUILayout.SelectionGrid(_selectiongridint, _filenames.ToArray(), 1);
            GUI.EndScrollView();
            if (GUI.Button(new Rect(5f, 320f, 100f, 30f), "Select & Load"))
            {
                _file = _filenames[_selectiongridint];
                Load();
                _filenames = null;
                _popup = false;
            }
            if (GUI.Button(new Rect(155f, 320f, 100f, 30f), "Delete"))
            {
                Delete();
                _filenames = null;
                _popup = false;
                GetNotes();
                _popup = true;
            }
            if (GUI.Button(new Rect(2f, 2f, 13f, 13f), "X"))
            {
                if (_popup)
                {
                    _filenames = null;
                    _popup = false;
                }
            }
            GUI.DragWindow();
        }

        public void DoMyWindow(int windowID)
        {
            _scrollViewVector = GUI.BeginScrollView(new Rect(0f, 15f, 420f, 380f), _scrollViewVector, new Rect(0f, 0f, 400f, 4360f));
            _text = GUI.TextArea(new Rect(3f, 0f, 400f, 4360f), _text);
            GUI.EndScrollView();

            _file = GUI.TextField(new Rect(5f, 400f, 150f, 20f), _file);

            if (GUI.Button(new Rect(155f, 400f, 80f, 30f), "Open"))
            {
                Load();
            }

            if (GUI.Button(new Rect(235f, 400f, 80f, 30f), "Save"))
            {
                Save();
            }
            if (GUI.Button(new Rect(315f, 400f, 80f, 30f), "List Notes"))
            {
                if (_filenames == null)
                {
                    GetNotes();
                    _popup = true;
                }
                else
                {
                    _filenames = null;
                    _popup = false;
                }
            }
            if (GUI.Button(new Rect(2f, 2f, 13f, 13f), "X"))
            {
                Toggle();
            }
            GUI.DragWindow();
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(_keybind))
            {
                Toggle();
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
            if (KSP.IO.File.Exists<notes>(_file + ".txt"))
            {
                _text = KSP.IO.File.ReadAllText<notes>(_notesdir + _file + ".txt");
            }
            else
            {
                ScreenMessages.PostScreenMessage("File dont exist: " + _file + ".txt", 4f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        private void Delete()
        {
            KSP.IO.File.Delete<notes>(_filenames[_selectiongridint] + ".txt");
        }

        private void LoadSettings()
        {
            KSPLog.print("[notes.dll] Loading Config...");
            KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<notes>();
            configfile.load();

            _windowRect = configfile.GetValue<Rect>("windowpos");
            _windowRect2 = configfile.GetValue<Rect>("listwindowpos");
            _keybind = configfile.GetValue<string>("keybind");
            KSPLog.print("[notes.dll] Config Loaded Successfully");
        }

        private void SaveSettings()
        {
            KSPLog.print("[notes.dll] Saving Config...");
            KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<notes>();

            configfile.SetValue("windowpos", _windowRect);
            configfile.SetValue("listwindowpos", _windowRect2);
            configfile.SetValue("keybind", _keybind);

            configfile.save();
            KSPLog.print("[notes.dll] Config Saved ");
        }

        private void Toggle()
        {
            if (_visible == true)
            {
                _visible = false;
                _filenames = null;
                _popup = false;
            }
            else
            {
                _visible = true;
            }
        }

        private void GetNotes()
        {
            this._filenames = new List<string>(Directory.GetFiles(_mypath, "*.txt"));

            for (int i = 0; i < this._filenames.Count; i++)
            {
                this._filenames[i] = Path.GetFileNameWithoutExtension(this._filenames[i]);
            }
        }

        private void CheckDefaults()
        {
            if (_windowRect == new Rect(0, 0, 0, 0))
            {
                _windowRect = new Rect(50f, 25f, 425f, 440f);
            }
            if (_windowRect2 == new Rect(0, 0, 0, 0))
            {
                _windowRect2 = new Rect((Screen.width / 2) - 150f, (Screen.height / 2) - 75f, 260f, 355f);
            }
            if (_keybind == null)
            {
                _keybind = "n";
            }
        }
    }
}
