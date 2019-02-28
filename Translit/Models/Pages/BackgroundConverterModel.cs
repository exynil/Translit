﻿using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows.Automation;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;
using Gma.System.MouseKeyHook;
using LiteDB;
using Translit.Models.Other;

namespace Translit.Models.Pages
{
    internal class BackgroundConverterModel
    {
        public readonly IKeyboardMouseEvents GlobalHook;
        public Symbol[] Symbols;
        public Word[] Words;

        public BackgroundConverterModel()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["LiteDbConnection"].ConnectionString;
            GlobalHook = Hook.GlobalEvents();
            InputText = new StringBuilder();
            Simulator = new InputSimulator();
        }

        public string ConnectionString { get; }

        public StringBuilder InputText { get; set; }
        public InputSimulator Simulator { get; set; }
        public bool IsTransliteratorEnabled { get; set; }

        private void GlobalHookKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back && InputText.Length != 0)
            {
                InputText.Remove(InputText.Length - 1, 1);
            }
            else if (e.KeyCode == Keys.Pause && !e.Shift && InputText.Length != 0)
            {
                GlobalHook.KeyDown -= GlobalHookKeyDown;

                TransliterateInputString(true);

                GlobalHook.KeyDown += GlobalHookKeyDown;
            }
            else if (e.Shift && e.KeyCode == Keys.Pause)
            {
                InputText.Clear();
            }
            else if (e.Shift && e.KeyCode == Keys.Home)
            {
                GlobalHook.KeyDown -= GlobalHookKeyDown;

                try
                {
                    var element = AutomationElement.FocusedElement;

                    if (element != null)
                        if (element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern))
                        {
                            var tp = (TextPattern) pattern;

                            InputText.Clear();

                            foreach (var r in tp.GetSelection()) InputText.Append(r.GetText(-1));

                            TransliterateInputString(false);
                        }
                }
                catch (Exception)
                {
                    // ignored
                }

                GlobalHook.KeyDown += GlobalHookKeyDown;
            }
        }

        private void TransliterateInputString(bool simulateBackspace)
        {
            foreach (var w in Words)
                for (var j = 0; j < 3; j++)
                {
                    var cyryllic = w.Cyryllic;
                    var latin = w.Latin;

                    switch (j)
                    {
                        case 0:
                            cyryllic = cyryllic.ToUpper();
                            latin = latin.ToUpper();
                            break;
                        case 1:
                            cyryllic = cyryllic.ToLower();
                            latin = latin.ToLower();
                            break;
                        case 2:
                            cyryllic = cyryllic.First().ToString().ToUpper() + cyryllic.Substring(1);
                            latin = latin.First().ToString().ToUpper() + latin.Substring(1);
                            break;
                    }

                    InputText.Replace(cyryllic, latin);
                }

            Symbols.Aggregate(InputText, (current, s) => current.Replace(s.Cyryllic, s.Latin));

            if (simulateBackspace)
                for (var i = 0; i < InputText.Length; i++)
                    Simulator.Keyboard.KeyDown(VirtualKeyCode.BACK).Sleep(10);

            Simulator.Keyboard.TextEntry(InputText.ToString());

            InputText.Clear();
        }

        private void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b') InputText.Append(e.KeyChar);
        }

        public void Subscribe()
        {
            LoadWordsAnSymbols();
            IsTransliteratorEnabled = true;
            GlobalHook.KeyPress += GlobalHookKeyPress;
            GlobalHook.KeyDown += GlobalHookKeyDown;
        }

        public void Unsubscribe()
        {
            IsTransliteratorEnabled = false;
            GlobalHook.KeyPress -= GlobalHookKeyPress;
            GlobalHook.KeyDown -= GlobalHookKeyDown;

            //It is recommened to dispose it
            //GlobalHook.Dispose();
        }

        public bool CollectionExists()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                return db.CollectionExists("Symbols") && db.CollectionExists("Words");
            }
        }

        public void LoadWordsAnSymbols()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                Words = db.GetCollection<Word>("Words").FindAll().ToArray();
                Symbols = db.GetCollection<Symbol>("Symbols").FindAll().ToArray();
            }
        }
    }
}