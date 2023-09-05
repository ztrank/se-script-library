namespace IngameScript
{
    using Sandbox.Game.EntityComponents;
    using Sandbox.ModAPI.Ingame;
    using Sandbox.ModAPI.Interfaces;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.GUI.TextPanel;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Ingame.Utilities;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRageMath;

    // This template is intended for extension classes. For most purposes you're going to want a normal
    // utility class.
    // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods
    static class MyIniExtensions
    {

        private static readonly ArrayConverter arrayConverter = new ArrayConverter();

        public static void ToStringArray(this MyIni myIni, List<string> output, string section, string name, string defaultValue = "[]")
        {
            output.AddRange(arrayConverter.Deserialize(myIni.Get(section, name).ToString(defaultValue)));
        }

        /// <summary>
        /// Array Converter Class.
        /// </summary>
        private class ArrayConverter
        {
            /// <summary>
            /// String builder instance.
            /// </summary>
            private readonly StringBuilder stringBuilder = new StringBuilder();

            /// <summary>
            /// Result list.
            /// </summary>
            private readonly List<string> result = new List<string>();

            /// <summary>
            /// Deserializes the string into an array of strings. Not recursive.
            /// </summary>
            /// <param name="input">Input string.</param>
            /// <returns>List of strings.</returns>
            public List<string> Deserialize(string input)
            {
                this.stringBuilder.Clear();
                this.result.Clear();
                input = this.RemoveUnquotedWhiteSpace(input);
                this.stringBuilder.Length = 0;
                input = input.Substring(1, input.Length - 2);
                for (int i = 0; i < input.Length; i++)
                {
                    char c = input[i];
                    if (c == '"')
                    {
                        i = this.AppendUntilStringEnd(true, i, input);
                        continue;
                    }

                    if (c == '[')
                    {
                        i = this.AppendUntilArrayEnd(true, i, input);
                        continue;
                    }

                    if (c == ',' || c == ']')
                    {
                        this.result.Add(this.stringBuilder.ToString());
                        this.stringBuilder.Length = 0;
                        continue;
                    }

                    this.stringBuilder.Append(c);
                }

                if (this.stringBuilder.Length > 0)
                {
                    this.result.Add(this.stringBuilder.ToString());
                }

                return this.result;
            }

            /// <summary>
            /// Removes unquoted whitespace.
            /// </summary>
            /// <param name="input">Input string.</param>
            /// <returns>Input string without whitespace outside of quotes.</returns>
            private string RemoveUnquotedWhiteSpace(string input)
            {
                this.stringBuilder.Length = 0;
                for (int i = 0; i < input.Length; i++)
                {
                    char c = input[i];
                    if (c == '"')
                    {
                        i = this.AppendUntilStringEnd(true, i, input);
                        continue;
                    }

                    if (char.IsWhiteSpace(c))
                    {
                        continue;
                    }

                    this.stringBuilder.Append(c);
                }

                return this.stringBuilder.ToString();
            }

            /// <summary>
            /// Adds the characters until the end of the array for capturing internal arrays without splitting on the comma.
            /// </summary>
            /// <param name="appendEscapeCharacter">Append escape character.</param>
            /// <param name="startIdx">Start index.</param>
            /// <param name="json">Incoming json formatted array.</param>
            /// <returns>Index to continue.</returns>
            private int AppendUntilArrayEnd(bool appendEscapeCharacter, int startIdx, string json)
            {
                this.stringBuilder.Append(json[startIdx]);
                for (int i = startIdx + 1; i < json.Length; i++)
                {
                    if (json[i] == '\\')
                    {
                        if (appendEscapeCharacter)
                        {
                            this.stringBuilder.Append(json[i]);
                        }

                        this.stringBuilder.Append(json[i + 1]);
                        i++;
                    }
                    else if (json[i] == '"')
                    {
                        i = this.AppendUntilStringEnd(appendEscapeCharacter, i, json);
                        continue;
                    }
                    else if (json[i] == ']')
                    {
                        this.stringBuilder.Append(json[i]);
                        return i;
                    }
                    else
                    {
                        this.stringBuilder.Append(json[i]);
                    }
                }

                return json.Length - 1;
            }

            /// <summary>
            /// Appends until the end of the string so preserve values inside of quotes.
            /// </summary>
            /// <param name="appendEscapeCharacter">Append escape character.</param>
            /// <param name="startIdx">Start index.</param>
            /// <param name="json">Json formatted string.</param>
            /// <returns>Index to continue.</returns>
            private int AppendUntilStringEnd(bool appendEscapeCharacter, int startIdx, string json)
            {
                this.stringBuilder.Append(json[startIdx]);
                for (int i = startIdx + 1; i < json.Length; i++)
                {
                    if (json[i] == '\\')
                    {
                        if (appendEscapeCharacter)
                        {
                            this.stringBuilder.Append(json[i]);
                        }

                        this.stringBuilder.Append(json[i + 1]);
                        i++;
                    }
                    else if (json[i] == '"')
                    {
                        this.stringBuilder.Append(json[i]);
                        return i;
                    }
                    else
                    {
                        this.stringBuilder.Append(json[i]);
                    }
                }

                return json.Length - 1;
            }
        }
    }
}
