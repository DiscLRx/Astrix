using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PocketExplorer.Web.Attr;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var formValueProviderFactory = context.ValueProviderFactories
            .OfType<FormValueProviderFactory>()
            .FirstOrDefault();
        if (formValueProviderFactory != null)
        {
            context.ValueProviderFactories.Remove(formValueProviderFactory);
        }

        var jqueryFormValueProviderFactory = context.ValueProviderFactories
            .OfType<JQueryFormValueProviderFactory>()
            .FirstOrDefault();
        if (jqueryFormValueProviderFactory != null)
        {
            context.ValueProviderFactories.Remove(jqueryFormValueProviderFactory);
        }

        var formFileProviderFactory = context.ValueProviderFactories
            .OfType<FormFileValueProviderFactory>()
            .FirstOrDefault();
        if (formFileProviderFactory != null)
        {
            context.ValueProviderFactories.Remove(formFileProviderFactory);
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}
