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
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            foreach (var property in For.ModelExplorer.Properties)
            {
                var modelExpression = new ModelExpression($"{property.Container.Metadata.Name}.{property.Metadata.Name}", property);

                TagBuilder builder = new TagBuilder("div");
                //group.Attributes.Add("class", styles.FormGroup);

                builder.InnerHtml.AppendHtml(_generator.GenerateLabel(ViewContext,
                    modelExpression.ModelExplorer,
                    modelExpression.Name, null, null));

                builder.InnerHtml.AppendHtml(GenerateInputTagHelper(modelExpression));

                output.Content.AppendHtml(builder);
            }
        }

        private TagHelperOutput GenerateInputTagHelper(ModelExpression modelExpression)
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
            //tagOutput.Attributes.Add(new TagHelperAttribute("class", css));
            return tagOutput;
        }
    }
}