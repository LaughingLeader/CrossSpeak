﻿using System;
using System.Runtime.InteropServices;

namespace CrossSpeak
{
    internal static class LibSpeechdWrapperAPI
    {
        internal readonly struct GoString
        {
            public string Msg { get; }
            public long Len { get; }

            public GoString(string msg, long len)
            {
                Msg = msg;
                Len = len;
            }
        }

        [DllImport("lib/screen-reader-libs/linux/libspeechdwrapper.so")]
        internal static extern int Initialize();

        [DllImport("lib/screen-reader-libs/linux/libspeechdwrapper.so")]
        internal static extern int Speak(GoString text, bool interrupt);

        [DllImport("lib/screen-reader-libs/linux/libspeechdwrapper.so")]
        internal static extern int Close();
    }

    internal class ScreenReaderLinux : IScreenReader
    {
        private bool _initialized;

        public void Initialize()
        {
            Console.WriteLine("Initializing speech dispatcher...");
            
            int res = LibSpeechdWrapperAPI.Initialize();
            if (res == 1)
            {
                _initialized = true;
                Console.WriteLine("Successfully initialized.");
            }
            else
            {
                Console.WriteLine("Unable to initialize.");
            }
        }

        public bool Speak(string text, bool interrupt = true)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            if (!_initialized) return false;

            LibSpeechdWrapperAPI.GoString str = new LibSpeechdWrapperAPI.GoString(text, text.Length);
            int re = LibSpeechdWrapperAPI.Speak(str, interrupt);

            if (re == 1)
            {
#if DEBUG
                Console.WriteLine($"Speaking(interrupt: {interrupt}) = {text}");
#endif
                return true;
            }
            else
            {
                Console.WriteLine($"Failed to output text: {text}");
                return false;
            }
        }

        public bool IsLoaded() => _initialized;

        public float GetVolume() => 0.0f;

        public void SetVolume(float volume) { }

        public float GetRate() => 0.0f;

        public void SetRate(float rate) { }

        public string? DetectScreenReader() => null;

        public bool HasSpeech() => false;

        public bool Output(string text, bool interrupt) => Speak(text, interrupt) || Braille(text);

        public bool IsSpeaking() => false;

        public void TrySAPI(bool trySAPI) { }

        public void PreferSAPI(bool preferSAPI) { }

        public bool HasBraille() => false;

        public bool Braille(string text) => false;

        public bool Silence() => false;

        public void Close()
        {
            if (!_initialized) return;
            LibSpeechdWrapperAPI.Close();
            _initialized = false;
        }
    }
}
