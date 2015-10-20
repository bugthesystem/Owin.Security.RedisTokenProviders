using System;
using System.Reflection;

namespace Owin.Security.RedisTokenProviders.Sample.Areas.HelpPage.ModelDescriptions
{
    public interface IModelDocumentationProvider
    {
        string GetDocumentation(MemberInfo member);

        string GetDocumentation(Type type);
    }
}