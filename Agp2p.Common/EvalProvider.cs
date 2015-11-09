using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp;

namespace Agp2p.Common
{
    public static class EvalProvider
    {
        // 临时生成的命名空间不会回收，请缓存它们
        public static Func<T, TResult> CreateEvalMethod<T, TResult>(string code, IEnumerable<string> usingStatements = null, IEnumerable<string> assemblies = null)
        {
            var returnType = typeof(TResult);
            var inputType = typeof(T);

            if (usingStatements == null)
                usingStatements = new string[] { };
            if (assemblies == null)
                assemblies = new string[] { };

            using (var compiler = new CSharpCodeProvider())
            {
                var name = "F" + Guid.NewGuid().ToString().Replace("-", string.Empty);

                var parameters = new CompilerParameters(new[] { "system.dll" }.Concat(assemblies).Distinct().ToArray())
                {
                    GenerateInMemory = true
                };

                var usings = string.Join("",
                    new[] {"System", returnType.Namespace, inputType.Namespace}.Concat(usingStatements)
                        .Distinct()
                        .Select(s => $"using {s};"));

                var source = string.Format(@"
                    {0}
                    namespace {1}
                    {{
                        public static class EvalClass
                        {{
                            public static {2} Eval({3} arg)
                            {{
                                {4}
                            }}
                        }}
                    }}",
                    usings,
                    name,
                    returnType.Name,
                    inputType.Name,
                    code);

                var compilerResult = compiler.CompileAssemblyFromSource(parameters, source);
                var compiledAssembly = compilerResult.CompiledAssembly;
                var type = compiledAssembly.GetType($"{name}.EvalClass");
                var method = type.GetMethod("Eval");
                return (Func<T, TResult>)Delegate.CreateDelegate(typeof(Func<T, TResult>), method);
            }
        }
    }

}
