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

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Shaos.Repository.Models;

namespace Shaos.TagHelpers
{
    [HtmlTargetElement("dashboard-parameters", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class DashboardParametersTagHelper : TagHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IHtmlGenerator _generator;
        private readonly ILogger<DashboardParametersTagHelper> _logger;

        public DashboardParametersTagHelper(ILogger<DashboardParametersTagHelper> logger,
                                            IConfiguration configuration,
                                            IHtmlGenerator generator)
        {
            _generator = generator;
            _logger = logger;
            _configuration = configuration;
        }

        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        public override void Process(TagHelperContext context,
                                     TagHelperOutput output)
        {
            CreateDivContainer(output);

            if (For.Model is IList<DashboardParameter> parameters)
            {
                foreach (var parameter in parameters)
                {
                }
            }
            else
            {
                _logger.LogWarning($"Model is not of type. {nameof(IList<DashboardParameter>)}");
            }
        }

        private static void CreateDivContainer(TagHelperOutput output)
        {
            output.Attributes.Add("class", "container-fluid");
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}