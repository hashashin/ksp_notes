// -------------------------------------------------------------------------------------------------
// notes.cs 0.14.1
//
// Simple KSP plugin to take notes ingame.
// Copyright (C) 2016 Iván Atienza
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
// Freenode & EsperNet: hashashin
//
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using KSP.IO;
using UnityEngine;
using File = System.IO.File;

using KSP.UI.Screens;
using ToolbarControl_NS;

namespace notes
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class Notes : MonoBehaviour
    {
        // Define the controls to block.
        private const ControlTypes _blockAllControls =
            ControlTypes.ALL_SHIP_CONTROLS | ControlTypes.ACTIONS_ALL | ControlTypes.EVA_INPUT | ControlTypes.TIMEWARP |
            ControlTypes.MISC | ControlTypes.GROUPS_ALL | ControlTypes.CUSTOM_ACTION_GROUPS;

        // The actual note file name without extension.
        private string _file;

        // Notes file extension.
        private const string _notesExt = ".txt";

        // Vessel log prefix.
        private const string _logPrefix = "log_";

        // The "show it" text of toggle delete button.
        private const string _showButtonDelText = "Show delete";

        // The "hide it" text of toggle delete button.
        private const string _hideButtonDelText = "Hide delete";

        // The mouse button for open notes in the list on click 0=left 1=right 2=middle(default).
        private int _mouseButton = -1;

        // The directory where notes text files live.
        private string _notesDir;

        // The reload icon texture file location.
        private readonly string _reloadIconUrl = "file://" + KSPUtil.ApplicationRootPath.Replace("\\", "/") +
                                                 "/GameData/notes/Textures/reload.png";

        // The toolbar texture off.
        private const string _btextureOff = "notes/Textures/icon_off";

        // The toolbar texture on.
        private const string _btextureOn = "notes/Textures/icon_on";

#if false
        // The button for the toolbar.
        private IButton _button;
#endif
        // The current delete toggle button text.
        private string _currentDelText;

        // The list of all notes without extension.
        private List<string> _fileNames;

        // The font size.
        private int _fontSize;

        // The keybind.
        private string _keybind;

        //linux workaround keybind
        private string _keybind2;

        // true to show the notes list window, false to hide.
        private bool _showList;

        // The reload icon texture.
        private WWW _reloadIconTex;

        // The scroll view vector.
        private Vector2 _scrollViewVector = Vector2.zero;

        // The second scroll view vector.
        private Vector2 _scrollViewVector2 = Vector2.zero;

        private Vector2 _scrollViewVector3 = Vector2.zero;

        // The selection grid int for the notes list.
        private int _selectFileGridInt;

        private int _selectDirGridInt;

        // The text of the note.
        public string _text;

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
        public string _vesselInfo;

        // The vessel name.
        private string _vesselName;

        // true to show the plugin window, false to hide.
        private bool _visible;

        // The rectangle for main window.
        private Rect _windowRect;

        // The rectangle for list windows.
        private Rect _windowRect2;

        // rectangles for the dialogs windows
        private Rect _windowRect3; //delete

        private Rect _windowRect4; //delete directory

        private Rect _windowRect5; //reload note

        private Rect _windowRect6; //new note

        //true use ksp skin, false use unity stock.
        private bool _useKspSkin;

        private List<string> _dirs;

        private string _newdir = "newdir";

        private bool _showdeldial;
        private bool _showdeldirdial;
        private bool _showreloaddial;
        private bool _shownewnotedial;


        // Awakes the plugin.
        private void Awake()
        {
            LoadVersion();

            VersionCheck();
            LoadSettings();
            _text = File.ReadAllText(_notesDir + _file + _notesExt);
            _reloadIconTex = new WWW(_reloadIconUrl);
            DontDestroyOnLoad(this);
        }

        // Delete note action.
        private void Delete()
        {
            File.Delete(_notesDir + _fileNames[_selectFileGridInt] + _notesExt);
            if (!((HighLogic.LoadedScene == GameScenes.LOADING) || (HighLogic.LoadedScene == GameScenes.LOADINGBUFFER)))
            {
                ScreenMessages.PostScreenMessage(_fileNames[_selectFileGridInt] + ".txt DELETED!", 3f);
            }
        }

        // Delete note dialog
        private void DelWindow(int windowId)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUI.contentColor = Color.red;
            GUILayout.Label("Are you sure want to delete: " + _fileNames[_selectFileGridInt] + "?");
            GUI.contentColor = Color.white;
            GUILayout.BeginVertical();
            if (GUILayout.Button("Yes"))
            {
                Delete();
                _fileNames = null;
                _showList = false;
                GetNotes();
                _showList = true;
                _showdeldial = false;
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            if (GUILayout.Button("No"))
            {
                _showdeldial = false;
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        //delete directory dialog
        private void DelDirWindow(int windowId)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUI.contentColor = Color.red;
            GUILayout.Label("Are you sure want to delete: " + _dirs[_selectDirGridInt] + "?");
            GUI.contentColor = Color.white;
            GUILayout.BeginVertical();
            if (GUILayout.Button("Yes"))
            {
                try
                {
                    Directory.Delete(_notesDir + _dirs[_selectDirGridInt]);
                }
                catch (Exception)
                {
                    ScreenMessages.PostScreenMessage("You need empty the folder before try to delete it.", 3f);
                }
                _fileNames = null;
                _showList = false;
                GetNotes();
                _showList = true;
                _showdeldirdial = false;
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            if (GUILayout.Button("No"))
            {
                _showdeldirdial = false;
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        // Get vessel log information.
        public void GetLogInfo()
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

            const string _separator =
                "------------------------------------------------------------------------------------------------";
            TimeSpan diff = TimeSpan.FromSeconds(FlightGlobals.ActiveVessel.missionTime);
            string _formatted = string.Format(
                  CultureInfo.CurrentCulture,
                  "{0}y, {1}d, {2}:{3}:{4}",
                  diff.Days / 365,
                  (diff.Days - (diff.Days / 365) * 365) - ((diff.Days - (diff.Days / 365) * 365) / 30) * 30,
                  diff.Hours.ToString("00"),
                  diff.Minutes.ToString("00"),
                  diff.Seconds.ToString("00"));
            string _situation = Vessel.GetSituationString(FlightGlobals.ActiveVessel);
            _vesselInfo =
                string.Format("\n{0}\n{1} --- Year: {2} Day: {3} Time: {4}:{5:00}:{6:00}\n" + "MET: {7} --- Status: {8}\n{0}\n",
                    _separator, _vesselName, _ryears, _rdays, _hours, _minutes, _seconds, _formatted, _situation);
        }

        // Get list of the notes.
        private void GetNotes()
        {
            _fileNames = new List<string>(Directory.GetFiles(_notesDir, "*" + _notesExt));

            for (int i = 0; i < _fileNames.Count; i++)
            {
                _fileNames[i] = Path.GetFileNameWithoutExtension(_fileNames[i]);
            }
            _dirs = new List<string>(Directory.GetDirectories(_notesDir));

            for (int i = 0; i < _dirs.Count; i++)
            {
                _dirs[i] = new DirectoryInfo(_dirs[i]).Name;
            }
        }

        // List window.
        //
        // <param name="windowId">Identifier for the window.</param>

        private void ListWindow(int windowId)
        {
            // Notes list gui.
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            // Loads selected note in the list.
            if (GUILayout.Button("Load Selected File"))
            {
                _file = _fileNames[_selectFileGridInt];
                Load();
                _fileNames = null;
                _showList = false;
            }
            if (GUILayout.Button("Change to Selected Directory"))
            {
                if (_dirs.Count == 0)
                {
                    _notesDir = KSPUtil.ApplicationRootPath.Replace("\\", "/") +
                                "GameData/notes/Plugins/PluginData/notes/";
                    _fileNames = null;
                    _showList = false;
                    GetNotes();
                    _showList = true;
                }
                else
                {
                    _notesDir = _notesDir + _dirs[_selectDirGridInt] + "/";
                    _fileNames = null;
                    _showList = false;
                    GetNotes();
                    _showList = true;
                }
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Directory"))
            {
                Directory.CreateDirectory(_notesDir + _newdir);
                _fileNames = null;
                _showList = false;
                GetNotes();
                _showList = true;
            }
            _newdir = GUILayout.TextField(_newdir);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUI.contentColor = Color.red;
            if (_dirs.Count > 0)
            {
                if (GUILayout.Button("Delete Directory"))
                {
                    _showdeldirdial = true;
                }
            }
            // Delete the selected note.
            if (_fileNames.Count > 0)
            {
                if (GUILayout.Button("Delete File"))
                {
                    _showdeldial = true;
                }
            }
            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            // Refresh the notes list.
            var _reloadopts = new[] { GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false) };
            if (GUILayout.Button(_reloadIconTex.texture, _reloadopts))
            {
                _fileNames = null;
                _showList = false;
                GetNotes();
                _showList = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            _scrollViewVector2 = GUILayout.BeginScrollView(_scrollViewVector2);
            GUILayout.Label("Directories -- Current: " + new DirectoryInfo(_notesDir).Name);
            var _options2 = new[] { GUILayout.Width(225f), GUILayout.ExpandWidth(false) };
            _selectDirGridInt = GUILayout.SelectionGrid(_selectDirGridInt, _dirs.ToArray(), 1, _options2);
            GUILayout.EndScrollView();
            _scrollViewVector3 = GUILayout.BeginScrollView(_scrollViewVector3);
            GUILayout.Label("Notes");
            if (_fileNames != null)
            {
                _selectFileGridInt = GUILayout.SelectionGrid(_selectFileGridInt, _fileNames.ToArray(), 1, _options2);
                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                // Close the list window.
                if (GUI.Button(new Rect(2f, 2f, 13f, 13f), "X"))
                {
                    if (_showList)
                    {
                        _fileNames = null;
                        _showList = false;
                    }
                }
                // detect middle clicks and load the clicked note if it's not already loaded.
                if (Input.GetMouseButtonUp(_mouseButton))
                {
                    if (_fileNames != null && !_fileNames[_selectFileGridInt].Equals(_file))
                    {
                        Save();
                        _file = _fileNames[_selectFileGridInt];
                        Load();
                    }
                }
            }
            // Makes the window dragable.
            GUI.DragWindow();
        }

        // Action to load the selected note.
        private void Load()
        {
            if (File.Exists(_notesDir + _file + _notesExt))
            {
                _text = File.ReadAllText(_notesDir + _file + _notesExt);
            }
            // screen messages don't appear on those scenes
            else if ((HighLogic.LoadedScene != GameScenes.LOADING) && (HighLogic.LoadedScene != GameScenes.LOADINGBUFFER))
            {
                ScreenMessages.PostScreenMessage("File don't exist: " + _file + _notesExt, 3f);
            }
        }

        // Load the settings of the plugin.
        private void LoadSettings()
        {
            print("[notes.dll] Loading Config...");
            PluginConfiguration _configFile = PluginConfiguration.CreateForType<Notes>();
            _configFile.load();

            _windowRect = _configFile.GetValue("main window position", new Rect(50f, 25f, 425f, 487f));
            _windowRect2 = _configFile.GetValue("list window position", new Rect(Screen.width / 2f - 150f,
                                                                        Screen.height / 2f - 75f, 520f, 390f));
            _windowRect3 = _configFile.GetValue("del dialog position", new Rect(Screen.width / 2f - 150f,
                                                                        Screen.height / 2f - 75f, 220f, 100f));
            _windowRect4 = _windowRect3;
            _windowRect5 = _windowRect3;
            _windowRect6 = _windowRect3;
            _keybind = _configFile.GetValue("keybind", "n");
            _keybind2 = _configFile.GetValue("keybind2", "l");
            _versionLastRun = _configFile.GetValue<string>("version");
            _fontSize = _configFile.GetValue("font size", 13);
            _file = _configFile.GetValue("last note opened", "notes");
            _useKspSkin = _configFile.GetValue<bool>("use ksp skin");
            _visible = _configFile.GetValue<bool>("main window state");
            _mouseButton = _configFile.GetValue("mouse button", 2);
            _notesDir = _configFile.GetValue("notesdir", KSPUtil.ApplicationRootPath.Replace("\\", "/") +
                                                        "GameData/notes/Plugins/PluginData/notes/");

            print("[notes.dll] Config Loaded Successfully");
        }

        // Load only the version.
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
            // Set the control name for later usage.
            GUI.SetNextControlName("notes");
            // Text area with scroll bar
            _scrollViewVector = GUI.BeginScrollView(new Rect(0f, 25f, 420f, 380f), _scrollViewVector,
                new Rect(0f, 0f, 400f, 5300f));
            // Configurable font size, independent from the skin.
            GUIStyle myStyle = new GUIStyle(GUI.skin.textArea)
            {
                fontSize = _fontSize,
                richText = true
            };
            _text = GUI.TextArea(new Rect(3f, 0f, 400f, 5300f), _text, myStyle);
            GUI.EndScrollView();
            // Show the actual note file name.
            _file = GUI.TextField(new Rect(5f, 410f, 150f, 20f), _file);
            // Load note file button.
            if (GUI.Button(new Rect(155f, 410f, 80f, 30f), "Reload"))
            {
                _showreloaddial = true;
            }
            // Save note file button.
            if (GUI.Button(new Rect(235f, 410f, 80f, 30f), "Save"))
            {
                Save();
            }
            // Opens the notes list windows.
            if (GUI.Button(new Rect(315f, 410f, 80f, 30f), "List Notes"))
            {
                if (_showList)
                {
                    _showList = false;
                }
                else if (!_showList)
                {
                    GetNotes();
                    _showList = true;
                }
            }
            //New file
            if (GUI.Button(new Rect(155f, 445f, 80f, 30f), "New Note"))
            {
                _shownewnotedial = true;
            }
            // Close the notes window.
            if (GUI.Button(new Rect(2f, 2f, 13f, 13f), "X"))
            {
                Toggle();
            }
            // Toggle current skin.
            if (GUI.Button(new Rect(20f, 2f, 22f, 16f), "S"))
            {
                _useKspSkin = !_useKspSkin;
            }
            // buttons for change the font size.
            if (GUI.Button(new Rect(80f, 2f, 15f, 15f), "-"))
            {
                // Who wants a 0 size font?
                if (_fontSize <= 1) return;
                _fontSize--;
            }
            GUI.Label(new Rect(95f, 0f, 60f, 20f), "Font size");
            if (GUI.Button(new Rect(150f, 2f, 15f, 15f), "+"))
            {
                // Big big big!!!
                _fontSize++;
            }
            if (GUI.Button(new Rect(260f, 2f, 15f, 15f), "<"))
            {
                SelectNote(false);
            }
            GUI.Label(new Rect(275f, 0f, 60f, 20f), "Note");
            if (GUI.Button(new Rect(305f, 2f, 15f, 15f), ">"))
            {
                SelectNote(true);
            }
            GUI.Label(new Rect(340f, 0f, 60, 20f), _version);
            // If we are on flight show the vessel logs buttons
            if (HighLogic.LoadedSceneIsFlight && HighLogic.LoadedSceneHasPlanetarium)
            {
                // Just in case
                if (FlightGlobals.ActiveVessel != null) _vesselName = FlightGlobals.ActiveVessel.GetName();
                // Button for open the vessel log file
                if (GUI.Button(new Rect(5f, 432f, 100f, 20f), "Open ship log"))
                {
                    OpenLog();
                }
                // If the vessel log opened is the one for the current vessel, show the button to add a new entry.
                if (_logPrefix + _vesselName == _file)
                {
                    if (GUI.Button(new Rect(5f, 452f, 100f, 20f), "New log entry"))
                    {
                        GetLogInfo();
                        _text = _text + _vesselInfo;
                    }
                }
            }
            // Workaround for http://bugs.kerbalspaceprogram.com/issues/1230
            if (Application.platform == RuntimePlatform.LinuxPlayer)
            {
                if (GUI.Toggle(new Rect(255f, 452f, 150f, 20f), _toggleInput, "Toggle input lock") != _toggleInput)
                {
                    toggleLock();
                }
            }
            // Make this window dragable
            GUI.DragWindow();
        }
        //reload dialog
        private void Reloaddial(int windowId)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUI.contentColor = Color.red;
            GUILayout.Label($"Are you sure want to load/reload: {_file}? Unsaved changes will be lost!");
            GUI.contentColor = Color.white;
            GUILayout.BeginVertical();
            if (GUILayout.Button("Yes"))
            {
                Load();
                _showreloaddial = false;
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            if (GUILayout.Button("No"))
            {
                _showreloaddial = false;
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        //new note dialog
        private void NewFiledial(int windowId)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUI.contentColor = Color.red;
            GUILayout.Label("Are you sure want to create new file? Unsaved changes will be lost!");
            GUI.contentColor = Color.white;
            GUILayout.BeginVertical();
            if (GUILayout.Button("Yes"))
            {
                _file = "newnote";
                _text = String.Empty;
                _shownewnotedial = false;
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            if (GUILayout.Button("No"))
            {
                _shownewnotedial = false;
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        // Executes the destroy action.
        private void OnDestroy()
        {
            Save();
            SaveSettings();
#if false
            if (_button != null)
            {
                _button.Destroy();
            }
#endif
            toolbarControl.OnDestroy();
            Destroy(toolbarControl);
        }

        // Executes the graphical user interface action.
        private void OnGUI()
        {
            // Saves the current Gui.skin for later restore
            GUISkin _defGuiSkin = GUI.skin;
            if (_visible)
            {
                GUI.skin = _useKspSkin ? HighLogic.Skin : _defGuiSkin;
                _windowRect = GUI.Window(GUIUtility.GetControlID(FocusType.Passive), _windowRect, NotesWindow, "Notepad");
            }
            if (_showList)
            {
                _windowRect2 = GUI.Window(GUIUtility.GetControlID(FocusType.Passive), _windowRect2, ListWindow,
                    "Notes list");
                UpdateDelButtonText();
            }
            if (_showdeldial)
            {
                _windowRect3 = GUI.Window(GUIUtility.GetControlID(FocusType.Passive), _windowRect3, DelWindow,
                    "File Deletion Dialog");
            }
            if (_showdeldirdial)
            {
                _windowRect4 = GUI.Window(GUIUtility.GetControlID(FocusType.Passive), _windowRect4, DelDirWindow,
                    "Directory Deletion Dialog");
            }
            if (_showreloaddial)
            {
                _windowRect5 = GUI.Window(GUIUtility.GetControlID(FocusType.Passive), _windowRect5, Reloaddial,
                   "File Reload Dialog");
            }
            if (_shownewnotedial)
            {
                _windowRect6 = GUI.Window(GUIUtility.GetControlID(FocusType.Passive), _windowRect6, NewFiledial,
                   "New File Dialog");
            }
            //Restore the skin
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
                ScreenMessages.PostScreenMessage(
                    "Log for " + _vesselName + " don't exist, creating new: " + _logPrefix + _vesselName + _notesExt, 3f);
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
                ScreenMessages.PostScreenMessage("File saved: " + _file + _notesExt, 3f);
            }
        }

        // Saves the settings.
        private void SaveSettings()
        {
            print("[notes.dll] Saving Config...");
            PluginConfiguration _configFile = PluginConfiguration.CreateForType<Notes>();

            _configFile.SetValue("main window position", _windowRect);
            _configFile.SetValue("list window position", _windowRect2);
            _configFile.SetValue("del dialog position", _windowRect3);
            _configFile.SetValue("keybind", _keybind);
            _configFile.SetValue("keybind2", _keybind2);
            _configFile.SetValue("version", _version);
            _configFile.SetValue("font size", _fontSize);
            _configFile.SetValue("last note opened", _file);
            _configFile.SetValue("use ksp skin", _useKspSkin);
            _configFile.SetValue("main window state", _visible);
            _configFile.SetValue("mouse button", _mouseButton);
            _configFile.SetValue("notesdir", _notesDir);

            _configFile.save();
            print("[notes.dll] Config Saved ");
        }

        // Start toolbar if present.
        private void Start()
        {
            CreateButtonIcon();
#if false
            if (!ToolbarManager.ToolbarAvailable) return;
            _button = ToolbarManager.Instance.add("notes", "toggle");
            _button.TexturePath = _btextureOff;
            _button.ToolTip = _tooltipOff;
            _button.OnClick += e => Toggle();
#endif
        }

        internal const string MODID = "Notes_NS";
        internal const string MODNAME = "Notes";
        ToolbarControl toolbarControl = null;

        private void CreateButtonIcon()
        {
            if (toolbarControl == null)
            {
                toolbarControl = gameObject.AddComponent<ToolbarControl>();
                toolbarControl.AddToAllToolbars(Toggle, Toggle,
                    ApplicationLauncher.AppScenes.ALWAYS,
                    MODID,
                    "notesButton",
                    _btextureOff + "_38",
                    _btextureOff + "_24",
                    MODNAME
                );
            }
        }


        // Toggles plugin visibility.
        private void Toggle()
        {
            if (_visible)
            {
                _visible = false;
                _showList = false;
                toolbarControl.SetTexture(_btextureOff + "_38", _btextureOff + "_24");
#if false
                if (!ToolbarManager.ToolbarAvailable) return;
                _button.TexturePath = _btextureOff;
                _button.ToolTip = _tooltipOff;
#endif
            }
            else
            {
                _visible = true;
                toolbarControl.SetTexture(_btextureOn + "_38", _btextureOn + "_24");
#if false
                if (!ToolbarManager.ToolbarAvailable) return;
                _button.TexturePath = _btextureOn;
                _button.ToolTip = _tooltipOn;
#endif
            }
        }

        private void toggleLock()
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

        // Detect the binded key press.
        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(_keybind))
            {
                Toggle();
            }
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(_keybind2) && _visible && Application.platform == RuntimePlatform.LinuxPlayer)
            {
                toggleLock();
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
            // Delete the config.xml if the version changes to avoid problems.
            _version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            print("notes.dll version: " + _version);
            // Check for remains of old versions
            if (File.Exists(_notesDir + "notes.dat"))
            {
                File.Delete(_notesDir + "notes.dat");
            }
            if (_version == _versionLastRun || !File.Exists(_notesDir + "config.xml")) return;
            File.Delete(_notesDir + "config.xml");
        }

        public void SelectNote(bool direction)
        {
            if (!direction)
            {
                GetNotes();
                if (_selectFileGridInt == 0) return;
                _selectFileGridInt--;
                Save();
                _file = _fileNames[_selectFileGridInt];
                Load();
            }
            GUI.Label(new Rect(275f, 0f, 60f, 20f), "Note");
            if (direction)
            {
                GetNotes();
                if (_selectFileGridInt == _fileNames.Count - 1) return;
                _selectFileGridInt++;
                Save();
                _file = _fileNames[_selectFileGridInt];
                Load();
            }
        }
    }
}