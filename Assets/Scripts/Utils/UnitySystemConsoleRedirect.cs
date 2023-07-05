using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Utils {
    /// <summary>
    /// Redirects writes to System.Console to Unity3D's Debug.Log.
    /// </summary>
    /// <author>
    /// Jackson Dunstan, http://jacksondunstan.com/articles/2986
    /// </author>
    public static class UnitySystemConsoleRedirect {
        private class UnityTextWriter : TextWriter {
            private readonly StringBuilder _buffer = new();

            public override void Flush() {
                Debug.Log(_buffer.ToString());
                _buffer.Length = 0;
            }

            public override void Write(string value) {
                _buffer.Append(value);
                if (value == null) return;
                var len = value.Length;
                if (len <= 0) return;
                var lastChar = value[len - 1];
                if (lastChar == '\n') {
                    Flush();
                }
            }

            public override void Write(char value) {
                _buffer.Append(value);
                if (value == '\n') {
                    Flush();
                }
            }

            public override void Write(char[] value, int index, int count) {
                Write(new string(value, index, count));
            }

            public override Encoding Encoding => Encoding.Default;
        }

        public static void Redirect() {
            Console.SetOut(new UnityTextWriter());
        }
    }
}