using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Shaos.TagHelpers
{
    [HtmlTargetElement("config-edit", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ConfigEditTagHelper : TagHelper
    {
        private readonly IHtmlGenerator _generator;

        public ConfigEditTagHelper(IHtmlGenerator generator)
        {
            _generator = generator;
        }

        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.Add("class", "form-group");
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            foreach (var property in For.ModelExplorer.Properties)
            {
                var modelExpression = new ModelExpression($"{property.Container.Metadata.Name}.{property.Metadata.Name}", property);


                TagBuilder tagBuilder = new TagBuilder("div");
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
            TagHelperOutput tagHelperOutput = null;

            if(modelExpression.ModelExplorer.ModelType == typeof(bool))
            {
                tagHelperOutput = GenerateInputTagHelperWithClass(modelExpression, "form-check-input");
            }
            else
            {
                tagHelperOutput = GenerateInputTagHelperWithClass(modelExpression, "form-control");
            }

            return tagHelperOutput;
        }

        private TagHelperOutput GenerateInputTagHelperWithClass(ModelExpression modelExpression, string? cssClass = default)
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
                { "value", modelExpression.Model?.ToString().ToLower() }
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