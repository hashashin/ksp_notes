// -------------------------------------------------------------------------------------------------
// notes.cs 0.9
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

using KSP.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using File = System.IO.File;

namespace notes
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class Notes : MonoBehaviour
    {
        // Define the controls to block
        private const ControlTypes _blockAllControls = ControlTypes.ALL_SHIP_CONTROLS | ControlTypes.ACTIONS_ALL | ControlTypes.EVA_INPUT | ControlTypes.TIMEWARP | ControlTypes.MISC | ControlTypes.GROUPS_ALL | ControlTypes.CUSTOM_ACTION_GROUPS;

        // The actual note file.
        private string _file;

        // notes files extension
        private const string _notesExt = ".txt";

        // vessel logs prefix
        private const string _logPrefix = "log_";

        // The "show it" text of delete toggle button
        private const string _showButtonDelText = "Show del button";

        // The "hide it" text of delete toggle button
        private const string _hideButtonDelText = "Hide del button";

        // The directory where notes text files live.
        private string _notesDir;

        // The reload icon texture file location.
        private readonly string _reloadIconUrl = "file://" + KSPUtil.ApplicationRootPath.Replace("\\", "/") + "/GameData/notes/Textures/reload.png";

        // The toolbar texture off.
        private const string _btextureOff = "notes/Textures/icon_off";

        // The toolbar texture on.
        private const string _btextureOn = "notes/Textures/icon_on";

        // The button.
        private IButton _button;

        // The current delete toggle button text.
        private string _currentDelText;

        // The list of all notes.
        private List<string> _fileNames;

        // The font size.
        private int _fontSize;

        // The keybind.
        private string _keybind;

        // true to show the notes list window, false to hide.
        private bool _showList;

        // The reload icon texture.
        private WWW _reloadIconTex;

        // The scroll view vector.
        private Vector2 _scrollViewVector = Vector2.zero;

        // The second scroll view vector.
        private Vector2 _scrollViewVector2 = Vector2.zero;

        // The selection grid int for the notes list.
        private int _selectionGridInt;

        // The text.
        private string _text;

        // true to show delete button, false to hide.
        private bool _toggleDel;

        // true lock input, false to unlock.
        private bool _toggleInput;

        // The tooltip text for the toolbar icon if the plugin is off.
        private const string _tooltipOff = "Show Notepad";

        // The tooltip text for the toolbar icon if the plugin is on.
        private const string _tooltipOn = "Hide Notepad";

        // The version of the plugin.
        private string _version;

        // The version in the last run.
        private string _versionLastRun;

        // The vessel info.
        private string _vesselInfo;

        // The vessel name.
        private string _vesselName;

        // true to show the plugin window, false to hide.
        private bool _visible;

        // The rectangle for main window.
        private Rect _windowRect;

        // The rectangle for list windows.
        private Rect _windowRect2;

        //toggle skin.
        private bool _useKspSkin;

        // Awakes the plugin.
        private void Awake()
        {
            LoadVersion();
            VersionCheck();
            LoadSettings();
            _notesDir = KSPUtil.ApplicationRootPath.Replace("\\", "/") + "GameData/notes/Plugins/PluginData/notes/";
            CheckConfSanity();
            _text = File.ReadAllText(_notesDir + _file + _notesExt);
            _reloadIconTex = new WWW(_reloadIconUrl);
        }

        // Check config.xml sanity.
        private void CheckConfSanity()
        {
            if (_windowRect == new Rect(0, 0, 0, 0))
            {
                _windowRect = new Rect(50f, 25f, 425f, 487f);
            }
            if (_windowRect2 == new Rect(0, 0, 0, 0))
            {
                _windowRect2 = new Rect(Screen.width / 2 - 150f, Screen.height / 2 - 75f, 260f, 390f);
            }
            if (_keybind == null)
            {
                _keybind = "n";
            }
            if (_fontSize == 0)
            {
                _fontSize = 13;
            }
            if (_file == null)
            {
                _file = "notes";
            }
        }

        // Delete the note file.
        private void Delete()
        {
            File.Delete(_notesDir + _fileNames[_selectionGridInt] + _notesExt);
            if (!((HighLogic.LoadedScene == GameScenes.LOADING) || (HighLogic.LoadedScene == GameScenes.LOADINGBUFFER)))
            {
                ScreenMessages.PostScreenMessage(_fileNames[_selectionGridInt] + ".txt DELETED!", 3f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        // Get vessel log information.
        private void GetLogInfo()
        {
            if (!HighLogic.LoadedSceneIsFlight || !HighLogic.LoadedSceneHasPlanetarium) return;
            double _seconds = Planetarium.GetUniversalTime();
            _seconds = Math.Abs(_seconds);

            const int _minuteL = 60;
            const int _hourL = 60 * _minuteL;
            int _dayL = 24 * _hourL;
            int _yearL = 365 * _dayL;
            if (GameSettings.KERBIN_TIME)
            {
                _dayL = 6 * _hourL;
                _yearL = 426 * _dayL;
            }
            int _years = (int)Math.Floor(_seconds / _yearL);
            int _ryears = _years + 1;
            int _tseconds = (int)Math.Floor(_seconds);
            _seconds = _tseconds - _years * _yearL;
            int _days = (int)Math.Floor(_seconds / _dayL);
            int _rdays = _days + 1;
            _seconds -= _days * _dayL;
            int _hours = (int)Math.Floor(_seconds / _hourL);
            _seconds -= _hours * _hourL;
            int _minutes = (int)Math.Floor(_seconds / _minuteL);
            _seconds -= _minutes * _minuteL;

            const string _separator = "------------------------------------------------------------------------------------------------";
            string _metY = FlightLogger.met_years.ToString();
            string _metD = FlightLogger.met_days.ToString();
            string _metH = FlightLogger.met_hours.ToString();
            string _metM = FlightLogger.met_mins.ToString("00");
            string _metS = FlightLogger.met_secs.ToString("00");
            string _situation = Vessel.GetSituationString(FlightGlobals.ActiveVessel);
            _vesselInfo =
                "\n" +
                _separator + "\n" +
                _vesselName + " --- Year: " + _ryears + " Day: " + _rdays + " Time: "
                + _hours + ":" + _minutes.ToString("00") + ":" + _seconds.ToString("00") + "\n" +

                "MET: " + _metY + "y, " + _metD + "d, " + _metH + ":" + _metM + ":" + _metS +
                " --- Status: " + _situation + "\n" +
                _separator + "\n";
        }

        // Get list of the notes.
        private void GetNotes()
        {
            _fileNames = new List<string>(Directory.GetFiles(_notesDir, "*.txt"));

            for (int i = 0; i < _fileNames.Count; i++)
            {
                _fileNames[i] = Path.GetFileNameWithoutExtension(_fileNames[i]);
            }
        }

        // List window.
        //
        // <param name="windowId">Identifier for the window.</param>

        private void ListWindow(int windowId)
        {
            _scrollViewVector2 = GUI.BeginScrollView(new Rect(3f, 25f, 295f, 300f), _scrollViewVector2, new Rect(0f, 0f, 0f, 4360f));
            _selectionGridInt = GUILayout.SelectionGrid(_selectionGridInt, _fileNames.ToArray(), 1);
            GUI.EndScrollView();
            if (GUI.Button(new Rect(5f, 330f, 100f, 30f), "Load selected"))
            {
                _file = _fileNames[_selectionGridInt];
                Load();
                _fileNames = null;
                _showList = false;
            }

            if (GUI.Button(new Rect(115f, 330f, 30f, 30f), string.Empty))
            {
                _fileNames = null;
                _showList = false;
                GetNotes();
                _showList = true;
            }
            GUI.DrawTexture(new Rect(115f, 330f, 30f, 30f), _reloadIconTex.texture, ScaleMode.ScaleToFit, true, 0f);
            if (_toggleDel = GUI.Toggle(new Rect(75f, 360.5f, 115f, 20f), _toggleDel, _currentDelText))
            {
                GUI.contentColor = Color.red;
                if (GUI.Button(new Rect(155f, 330f, 100f, 30f), "Delete"))
                {
                    Delete();
                    _fileNames = null;
                    _showList = false;
                    GetNotes();
                    _showList = true;
                }
                GUI.contentColor = Color.white;
            }
            if (GUI.Button(new Rect(2f, 2f, 13f, 13f), "X"))
            {
                if (_showList)
                {
                    _fileNames = null;
                    _showList = false;
                }
            }
            if (Input.GetMouseButtonUp(2))
            {
                if (_fileNames != null && !_fileNames[_selectionGridInt].Contains(_file))
                {
                    Save();
                    _file = _fileNames[_selectionGridInt];
                    Load();
                }
            }
            GUI.DragWindow(); ;
        }

        // Load the selected note.
        private void Load()
        {
            if (File.Exists(_notesDir + _file + _notesExt))
            {
                _text = File.ReadAllText(_notesDir + _file + _notesExt);
            }
            else if ((HighLogic.LoadedScene != GameScenes.LOADING) && (HighLogic.LoadedScene != GameScenes.LOADINGBUFFER))
            {
                ScreenMessages.PostScreenMessage("File don't exist: " + _file + _notesExt, 3f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        // Load the settings.
        private void LoadSettings()
        {
            print("[notes.dll] Loading Config...");
            PluginConfiguration _configFile = PluginConfiguration.CreateForType<Notes>();
            _configFile.load();

            _windowRect = _configFile.GetValue<Rect>("main window position");
            _windowRect2 = _configFile.GetValue<Rect>("list window position");
            _keybind = _configFile.GetValue<string>("keybind");
            _versionLastRun = _configFile.GetValue<string>("version");
            _fontSize = _configFile.GetValue<int>("font size");
            _file = _configFile.GetValue<string>("last note opened");
            _useKspSkin = _configFile.GetValue<bool>("use ksp skin");
            _visible = _configFile.GetValue<bool>("main window state");

            print("[notes.dll] Config Loaded Successfully");
        }

        // Load the version.
        private void LoadVersion()
        {
            PluginConfiguration _configFile = PluginConfiguration.CreateForType<Notes>();
            _configFile.load();
            _versionLastRun = _configFile.GetValue<string>("version");
        }

        // Notes main window.
        //
        // <param name="windowId">Identifier for the window.</param>

        private void NotesWindow(int windowId)
        {
            GUI.SetNextControlName("notes");
            _scrollViewVector = GUI.BeginScrollView(new Rect(0f, 25f, 420f, 380f), _scrollViewVector, new Rect(0f, 0f, 400f, 5300f));
            GUIStyle _myStyle = new GUIStyle(GUI.skin.textArea) { fontSize = _fontSize };
            _text = GUI.TextArea(new Rect(3f, 0f, 400f, 5300f), _text, _myStyle);
            GUI.EndScrollView();

            _file = GUI.TextField(new Rect(5f, 410f, 150f, 20f), _file);

            if (GUI.Button(new Rect(155f, 410f, 80f, 30f), "Load"))
            {
                Load();
            }
            if (GUI.Button(new Rect(235f, 410f, 80f, 30f), "Save"))
            {
                Save();
            }
            if (GUI.Button(new Rect(315f, 410f, 80f, 30f), "List Notes"))
            {
                if (_fileNames == null)
                {
                    GetNotes();
                    _showList = true;
                }
                else
                {
                    _fileNames = null;
                    _showList = false;
                }
            }
            if (GUI.Button(new Rect(2f, 2f, 13f, 13f), "X"))
            {
                Toggle();
            }
            if (GUI.Button(new Rect(20f, 2f, 13f, 13f), "TS"))
            {
                _useKspSkin = !_useKspSkin;
            }

            if (HighLogic.LoadedSceneIsFlight && HighLogic.LoadedSceneHasPlanetarium)
            {
                _vesselName = FlightGlobals.ActiveVessel.GetName();
                if (GUI.Button(new Rect(5f, 432f, 100f, 20f), "Open ship log"))
                {
                    OpenLog();
                }
                if (_logPrefix + _vesselName == _file)
                {
                    if (GUI.Button(new Rect(5f, 452f, 100f, 20f), "New log entry"))
                    {
                        GetLogInfo();
                        _text = _text + _vesselInfo;
                    }
                }
            }
            if (Application.platform == RuntimePlatform.LinuxPlayer)
            {
                if (GUI.Toggle(new Rect(200f, 452f, 150f, 20f), _toggleInput, "Toggle input lock") != _toggleInput)
                {
                    _toggleInput = !_toggleInput;
                    if (_toggleInput)
                    {
                        InputLockManager.SetControlLock(_blockAllControls, "notes");
                    }
                    else
                    {
                        InputLockManager.RemoveControlLock("notes");
                    }
                }
            }
            GUI.DragWindow();
        }

        // Executes the destroy action.
        private void OnDestroy()
        {
            Save();
            SaveSettings();
            if (_button != null)
            {
                _button.Destroy();
            }
        }

        // Executes the graphical user interface action.
        private void OnGUI()
        {
            GUISkin _defGuiSkin = GUI.skin;
            GUI.skin = _useKspSkin ? HighLogic.Skin : _defGuiSkin;
            if (_visible)
            {
                _windowRect = GUI.Window(GUIUtility.GetControlID(FocusType.Passive), _windowRect, NotesWindow, "Notepad");
            }
            if (_showList)
            {
                _windowRect2 = GUI.Window(GUIUtility.GetControlID(FocusType.Passive), _windowRect2, ListWindow, "Notes list");
                UpdateDelButtonText();
            }
            GUI.skin = _defGuiSkin;
        }

        // Opens the vessel log.
        private void OpenLog()
        {
            GetLogInfo();
            if (File.Exists(_notesDir + _logPrefix + _vesselName + _notesExt))
            {
                _text = File.ReadAllText(_notesDir + _logPrefix + _vesselName + _notesExt);
                _file = _logPrefix + _vesselName;
            }
            else
            {
                ScreenMessages.PostScreenMessage("Log for " + _vesselName + " don't exist, creating new: " + _logPrefix + _vesselName + _notesExt, 3f, ScreenMessageStyle.UPPER_CENTER);
                _file = _logPrefix + _vesselName;
                _text = _vesselInfo;
                Save();
            }
        }

        // Saves the current note.
        private void Save()
        {
            File.WriteAllText(_notesDir + _file + _notesExt, _text);
            if (HighLogic.LoadedScene != GameScenes.LOADINGBUFFER && HighLogic.LoadedScene != GameScenes.LOADING)
            {
                ScreenMessages.PostScreenMessage("File saved: " + _file + _notesExt, 3f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        // Saves the settings.
        private void SaveSettings()
        {
            print("[notes.dll] Saving Config...");
            PluginConfiguration _configFile = PluginConfiguration.CreateForType<Notes>();

            _configFile.SetValue("main window position", _windowRect);
            _configFile.SetValue("list window position", _windowRect2);
            _configFile.SetValue("keybind", _keybind);
            _configFile.SetValue("version", _version);
            _configFile.SetValue("font size", _fontSize);
            _configFile.SetValue("last note opened", _file);
            _configFile.SetValue("use ksp skin", _useKspSkin);
            _configFile.SetValue("main window state", _visible);

            _configFile.save();
            print("[notes.dll] Config Saved ");
        }

        // Start toolbar if present.
        private void Start()
        {
            if (!ToolbarManager.ToolbarAvailable) return;
            _button = ToolbarManager.Instance.add("notes", "toggle");
            _button.TexturePath = _btextureOff;
            _button.ToolTip = _tooltipOff;
            _button.OnClick += e => Toggle();
        }

        // Toggles plugin visibility.
        private void Toggle()
        {
            if (_visible)
            {
                _visible = false;
                _fileNames = null;
                _showList = false;
                _button.TexturePath = _btextureOff;
                _button.ToolTip = _tooltipOff;
            }
            else
            {
                _visible = true;
                _button.TexturePath = _btextureOn;
                _button.ToolTip = _tooltipOn;
            }
        }

        // Detect the binded key press.
        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(_keybind))
            {
                Toggle();
            }
        }

        // Updates the delete button text.
        private void UpdateDelButtonText()
        {
            _currentDelText = _toggleDel ? _hideButtonDelText : _showButtonDelText;
        }

        // Version check.
        private void VersionCheck()
        {
            _version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            print("notes.dll version: " + _version);
            if ((_version != _versionLastRun) && (File.Exists(_notesDir + "config.xml")))
            {
                File.Delete(_notesDir + "config.xml");
            }
        }
    }
}