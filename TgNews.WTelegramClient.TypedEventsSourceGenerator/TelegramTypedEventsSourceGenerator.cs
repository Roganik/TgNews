using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using TL;

namespace TgNews.WTelegramClient.TypedEventsSourceGenerator
{
    [Generator]
    public class TelegramTypedEventsSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var allTypesSource = GetUpdateSubTypes();

            // switch case should be ordered from children to the most base classes
            var baseClasses = allTypesSource.Select(t => t.BaseType).Distinct().ToList();
            var baseClasses2 = baseClasses.Select(t => t.BaseType).Distinct().ToList();

            var allTypes = allTypesSource.Where(t => !baseClasses.Contains(t) && !baseClasses2.Contains(t)).ToList();
            allTypes.AddRange(baseClasses.Where(t => !baseClasses2.Contains(t)));
            allTypes.AddRange(baseClasses2);

            // filter out Update from swtich cases, also exclude System.Object
            allTypes = allTypes.Where(t => typeof(Update).IsAssignableFrom(t) && typeof(Update) != t).ToList();

            var sb = new StringBuilder();

            // Build up the source code

            // beginning
            sb.Append(@" // Auto-generated code
using TL;
using WTelegram;

namespace WTelegram
{
    public class TypedUpdates
    {
        public void Subscription(IObject arg)
        {
            if (arg is not UpdatesBase updates)
            {
                return;
            }

            foreach (var update in updates.UpdateList)
            {
                switch (update)
                {
");
                    // switch case body
                    sb.Append(string.Join(Environment.NewLine, allTypes.Select(t => BuildCase(t.Name))));

                    // end switch case
                    sb.Append(@"
                    default:
                        break;
                }
            }
        }
");
            // typed properties
            sb.Append(string.Join(Environment.NewLine, allTypes.Select(t => BuildActionProp(t.Name))));

            // end of class and namespace
            sb.Append(@"
    }
}");

            // Add the source code to the compilation
            var source = sb.ToString();
            Console.WriteLine(source);
            Trace.Write(source);
            context.AddSource($"TypedUpdates.g.cs", source);

        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one

            // uncomment lines below to debug
// #if DEBUG
//             if (!Debugger.IsAttached)
//             {
//                 Debugger.Launch();
//             }
// #endif
        }

        public List<Type> GetUpdateSubTypes()
        {
            var wTelegramAssembly = typeof(Update).Assembly;
            var allTypes = wTelegramAssembly.GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract).ToList();

            var updateSubTypes = new List<Type>();
            foreach (var type in allTypes)
            {
                if (!typeof(Update).IsAssignableFrom(type))
                {
                    continue;
                }

                updateSubTypes.Add(type);
            }

            return updateSubTypes;
        }

        public string BuildCase(string typeStr)
        {
            var type = typeStr;
            var variable = char.ToLower(typeStr[0]) + typeStr.Substring(1);

            return
                $"                    case {type} {variable}: On{type}({variable}); break;";
        }

        public string BuildActionProp(string typeStr) =>
            $"        public Action<{typeStr}> On{typeStr} {{ get; set; }} = _ => {{ }};";
    }

}

