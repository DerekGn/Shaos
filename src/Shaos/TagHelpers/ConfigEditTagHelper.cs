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

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Shaos.TagHelpers
{
    [HtmlTargetElement("config-edit", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ConfigEditTagHelper(IHtmlGenerator generator) : TagHelper
    {
        private const string DivTag = "div";

        private readonly IHtmlGenerator _generator = generator;

        [HtmlAttributeName("asp-for")]
        public required ModelExpression For { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext? ViewContext { get; set; }

        public override void Process(TagHelperContext context,
                                     TagHelperOutput output)
        {
            output.Attributes.Add("class", "form-group");
            output.TagName = DivTag;
            output.TagMode = TagMode.StartTagAndEndTag;

            foreach (var property in For.ModelExplorer.Properties)
            {
                var modelExpression = new ModelExpression($"{property.Container.Metadata.Name}.{property.Metadata.Name}", property);

                TagBuilder tagBuilder = new TagBuilder(DivTag);
                tagBuilder.Attributes.Add("class", "form-group");
                tagBuilder.InnerHtml.AppendHtml(GenerateLabel(modelExpression));

                tagBuilder.InnerHtml.AppendHtml(GenerateInputTagHelper(modelExpression));
                tagBuilder.InnerHtml.AppendHtml(GenerateValidation(modelExpression));
                output.Content.AppendHtml(tagBuilder);
            }
        }

        private TagBuilder GenerateLabel(ModelExpression modelExpression)
        {
            var cssClass = modelExpression.ModelExplorer.ModelType == typeof(bool) ? "form-check-label" : "control-label";

            return _generator.GenerateLabel(ViewContext,
                                            modelExpression.ModelExplorer,
                                            modelExpression.Name,
                                            null,
                                            new { @class = cssClass });
        }

        private TagHelperOutput GenerateInputTagHelper(ModelExpression modelExpression)
        {
            TagHelperOutput tagHelperOutput;
            if (modelExpression.ModelExplorer.ModelType == typeof(bool))
            {
                tagHelperOutput = GenerateInputTagHelperWithClass(modelExpression, "form-check-input");
            }
            else
            {
                tagHelperOutput = GenerateInputTagHelperWithClass(modelExpression, "form-control");
            }

            return tagHelperOutput;
        }

        private TagHelperOutput GenerateInputTagHelperWithClass(ModelExpression modelExpression,
                                                                string? cssClass = default)
        {
            var tagHelper = new InputTagHelper(_generator)
            {
                For = modelExpression,
                Name = modelExpression.Name,
                ViewContext = ViewContext
            };

            var tagOutput = new TagHelperOutput("input", [], (b, e) => null)
            {
                TagMode = TagMode.SelfClosing
            };

            var attributes = new TagHelperAttributeList
            {
                { "name",  modelExpression.Name },
                { "type",  "text" },
                { "value", modelExpression.Model.ToString()?.ToLower()}
            };

            var tagContext = new TagHelperContext(
                attributes,
                new Dictionary<object, object>(),
                Guid.NewGuid().ToString());

            tagHelper.Init(tagContext);
            tagHelper.Process(tagContext, tagOutput);
            tagOutput.Attributes.Add(new TagHelperAttribute("class", cssClass));
            return tagOutput;
        }

        private IHtmlContent GenerateValidation(ModelExpression modelExpression)
        {
            return _generator.GenerateValidationMessage(
                ViewContext,
                modelExpression.ModelExplorer,
                modelExpression.Name,
                null,
                null,
                new { @class = "text-danger field-validation-valid" });
        }
    }
}