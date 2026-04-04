using System;
using System.Collections.Generic;
using System.Text;

namespace PheeLeep.ArgSharp
{
    internal static class Miscellaneous
    {

        /// <summary>
        /// Generates the table format containing 2D array strings.
        /// </summary>
        /// <param name="array">A two-dimensional array of strings.</param>
        /// <param name="padLength">The padded length of each rows, except in the last columns.</param>
        /// <returns>Returns the string containing table formatted values.</returns>
        internal static string GenerateTable(string[][] array, int padLength = 4)
        {
            if (array == null || array.Length == 0) return string.Empty;

            int consoleWidth = Console.WindowWidth > 0 ? Console.WindowWidth : 80;
            StringBuilder sb = new StringBuilder();

            // Find the widest left column
            int leftColWidth = 0;
            foreach (string[] row in array)
                if (row[0].Length > leftColWidth)
                    leftColWidth = row[0].Length;

            int rightColStart = leftColWidth + padLength;

            // Cap rightColStart so the right column always has at least 30 chars
            if (rightColStart > consoleWidth - 30)
                rightColStart = consoleWidth - 30;

            int rightColWidth = consoleWidth - rightColStart;

            foreach (string[] row in array)
            {
                string left = row[0];
                string right = row.Length > 1 ? (row[1] ?? "") : "";

                // Pad left column
                string leftPadded = left.PadRight(rightColStart);

                if (string.IsNullOrWhiteSpace(right))
                {
                    sb.AppendLine(leftPadded.TrimEnd());
                    continue;
                }

                // Word-wrap the right column
                List<string> wrappedLines = WordWrap(right, rightColWidth);

                for (int i = 0; i < wrappedLines.Count; i++)
                {
                    if (i == 0)
                        sb.AppendLine($"{leftPadded}{wrappedLines[i]}");
                    else
                        sb.AppendLine($"{new string(' ', rightColStart)}{wrappedLines[i]}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Wraps text at word boundaries for a given max width.
        /// Respects explicit newlines (\n) in the source text.
        /// </summary>
        internal static List<string> WordWrap(string text, int maxWidth)
        {
            List<string> result = new List<string>();
            if (maxWidth <= 0)
            {
                result.Add(text);
                return result;
            }

            // Split on explicit newlines first
            string[] hardLines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (string hardLine in hardLines)
            {
                if (hardLine.Length <= maxWidth)
                {
                    result.Add(hardLine);
                    continue;
                }

                // Word-wrap the line
                string[] words = hardLine.Split(' ');
                StringBuilder line = new StringBuilder();

                foreach (string word in words)
                {
                    if (line.Length == 0)
                    {
                        line.Append(word);
                    }
                    else if (line.Length + 1 + word.Length <= maxWidth)
                    {
                        line.Append(' ');
                        line.Append(word);
                    }
                    else
                    {
                        result.Add(line.ToString());
                        line.Clear();
                        line.Append(word);
                    }
                }

                if (line.Length > 0)
                    result.Add(line.ToString());
            }

            return result;
        }

    }
}
