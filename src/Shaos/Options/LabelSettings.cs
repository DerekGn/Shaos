/*
* MIT License
*
* Copyright (c) 2025 Derek Goslin https://github.com/DerekGn
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using System.ComponentModel.DataAnnotations;

namespace Shaos.Options
{
    /// <summary>
    /// The label settings
    /// </summary>
    public class LabelSettings
    {
        /// <summary>
        /// The label colour
        /// </summary>
        public string Colour { get; set; } = "rgba(128, 128, 128, 0.5)";

        /// <summary>
        /// Enables/disables labels showing the min/max values
        /// </summary>
        public bool Disabled { get; set; } = true;

        /// <summary>
        /// The font family for the label
        /// </summary>
        public string FontFamily { get; set; } = "sans-serif";

        /// <summary>
        /// The label font size
        /// </summary>
        [Range(1, 20)]
        public int FontSize { get; set; } = 15;

        /// <summary>
        ///
        /// </summary>
        public bool IntermediateLabelSameAxis { get; set; } = true;

        /// <summary>
        /// The label number of decimal places
        /// </summary>
        public int Precision { get; set; } = 2;

        /// <summary>
        /// Show intermediate labels
        /// </summary>
        public bool ShowIntermediateLabels { get; set; } = false;
    }
}