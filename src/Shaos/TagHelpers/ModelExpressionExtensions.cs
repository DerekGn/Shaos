using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Shaos.TagHelpers
{
    public static class ModelExpressionExtensions
    {
        public static T? GetAttribute<T>(this ModelExpression modelExpression)
        {
            T? attribute = default;

            if (modelExpression.Metadata is DefaultModelMetadata metadata)
                attribute = (T?)metadata.Attributes.PropertyAttributes!.FirstOrDefault(p => p.GetType() == typeof(T?));

            return attribute;
        }
    }
}
