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
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Shaos.Repository.Models;
using Shaos.Repository.Models.Devices.Parameters;

namespace Shaos.TagHelpers
{
    [HtmlTargetElement("dashboard-items", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class DashboardItemsTagHelper : TagHelper
    {
        private const string ClassAttribute = "class";
        private const string ColumnClass = "col";
        private const string DivTag = "div";
        private readonly ILogger<DashboardItemsTagHelper> _logger;

        public DashboardItemsTagHelper(ILogger<DashboardItemsTagHelper> logger)
        {
            _logger = logger;
        }

        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        public override void Process(TagHelperContext context,
                                     TagHelperOutput output)
        {
            CreateDivContainer(output, "container-fluid p-auto");

            if (For.Model is IList<DashboardItem> items)
            {
                CreateItems(items,
                            output);
            }
            else
            {
                TagBuilder warningBuilder = new TagBuilder("h1");
                warningBuilder.Attributes.Add(ClassAttribute, "text-danger");
                warningBuilder.InnerHtml.Append($"Model is not the correct type.");
                output.Content.AppendHtml(warningBuilder);

                _logger.LogWarning($"Model is not the correct type.");
            }
        }

        private static void CreateDivContainer(TagHelperOutput output,
                                               string attributes)
        {
            output.Attributes.Add(ClassAttribute, attributes);
            output.TagName = DivTag;
            output.TagMode = TagMode.StartTagAndEndTag;
        }

        private static TagBuilder CreateItem(DashboardItem parameter)
        {
            TagBuilder builder = new TagBuilder(DivTag);
            builder.Attributes.Add(ClassAttribute, "container-fluid p-auto");
            builder.InnerHtml.AppendHtml(CreateItemDivRow(parameter));
            return builder;
        }

        private static TagBuilder CreateItemAttributeLabel(string label)
        {
            TagBuilder builder = new TagBuilder(DivTag);
            builder.Attributes.Add(ClassAttribute, ColumnClass);
            builder.InnerHtml.Append(label);
            return builder;
        }

        private static TagBuilder CreateItemAttributes(DashboardItem parameter)
        {
            TagBuilder builder = new TagBuilder(DivTag);
            builder.Attributes.Add(ClassAttribute, ColumnClass);
            builder.InnerHtml.AppendHtml(CreateItemAttributeLabel(parameter.Label));
            builder.InnerHtml.AppendHtml(CreateItemAttributeLabel("Value"));
            builder.InnerHtml.AppendHtml(CreateItemAttributeLabel("Date"));
            return builder;
        }

        private static TagBuilder CreateItemControl(DashboardItem parameter)
        {
            TagBuilder builder = new TagBuilder(DivTag);
            builder.Attributes.Add(ClassAttribute, "col position-relative");
            builder.InnerHtml.AppendHtml(CreateItemParameterControl(parameter.Parameter!));
            return builder;
        }

        private static TagBuilder CreateItemDivRow(DashboardItem parameter)
        {
            TagBuilder builder = new TagBuilder(DivTag);
            builder.Attributes.Add(ClassAttribute, "row p-auto");
            builder.InnerHtml.AppendHtml(CreateItemAttributes(parameter));
            builder.InnerHtml.AppendHtml(CreateItemControl(parameter));
            return builder;
        }

        private static TagBuilder CreateItemParameterControl(BaseParameter parameter)
        {
            TagBuilder? builder = null;

            var type = parameter.GetType();

            switch (type)
            {
                case Type _ when type == typeof(BoolParameter):
                    builder = CreateItemParameterBoolControl((BoolParameter) parameter);
                    break;
                case Type _ when type == typeof(FloatParameter):
                    break;
                case Type _ when type == typeof(IntParameter):
                    break;
                case Type _ when type == typeof(StringParameter):
                    break;
                case Type _ when type == typeof(UIntParameter):
                    break;
            }

            return builder!;
        }

        private static TagBuilder CreateItemParameterBoolControl(BoolParameter parameter)
        {
            TagBuilder builder = new TagBuilder(DivTag);
            builder.Attributes.Add(ClassAttribute, "col border position-absolute top-50 start-50 translate-middle");
            builder.InnerHtml.AppendHtml(CreateItemParameterButton());
            return builder;
        }

        private static TagBuilder CreateItemParameterButton()
        {
            TagBuilder builder = new TagBuilder("button");
            builder.Attributes.Add(ClassAttribute, "btn btn-primary active");
            builder.Attributes.Add("type", "button");
            builder.InnerHtml.Append("Primary");
            return builder;
        }

        private static void CreateItems(IList<DashboardItem> items,
                                        TagHelperOutput output)
        {
            foreach (var item in items)
            {
                output.Content.AppendHtml(CreateItem(item));
            }
        }
    }
}