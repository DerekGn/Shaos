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

using Shaos.Sdk;
using Shaos.Repository.Models;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace Shaos.Services.Runtime
{
    /// <summary>
    /// An executing <see cref="PlugIn"/> instance
    /// </summary>
    public class ExecutingInstance()
    {
        /// <summary>
        /// The PlugIn assembly
        /// </summary>
        public Assembly? Assembly { get; private set; }

        /// <summary>
        /// The <see cref="RuntimeAssemblyLoadContext"/>
        /// </summary>
        public RuntimeAssemblyLoadContext? AssemblyLoadContext { get; private set; }

        /// <summary>
        /// The <see cref="PlugInInstance"/> identifier
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// The <see cref="PlugInInstance"/> name
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        ///
        /// </summary>
        public IPlugIn? PlugIn { get; private set; }

        /// <summary>
        /// The <see cref="ExecutingState"/> of the <see cref="ExecutingInstance"/>
        /// </summary>
        public ExecutionState State { get; internal set; }

        /// <summary>
        /// The <see cref="Task"/> that is executing the <see cref="ExecutingInstance"/>
        /// </summary>
        public Task? Task { get; private set; }

        /// <summary>
        /// The <see cref="CancellationTokenSource"/> used to cancel the executing <see cref="ExecutingInstance"/>
        /// </summary>
        public CancellationTokenSource? TokenSource { get; private set; }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(Id)}: {Id}");
            stringBuilder.AppendLine($"{nameof(Task)}: {(Task == null ? "Empty" : Task.Id)}");
            stringBuilder.AppendLine($"{nameof(TokenSource)}: {(TokenSource == null ? "Empty" : TokenSource.IsCancellationRequested)}");
            stringBuilder.AppendLine($"{nameof(State)}: {State}");
            stringBuilder.AppendLine($"{nameof(PlugIn)}: {(PlugIn == null ? "Empty" : PlugIn.GetType().Name)}");
            stringBuilder.AppendLine($"{nameof(Assembly)}: {(Assembly == null ? "Empty" : Assembly.FullName)} ");
            stringBuilder.AppendLine($"{nameof(AssemblyLoadContext)}: {(AssemblyLoadContext == null ? "Empty" : AssemblyLoadContext.Name)} ");

            return stringBuilder.ToString();
        }

        internal void Load(string name, AssemblyName assemblyName, string assemblyPath)
        {
            try
            {
                State = ExecutionState.PlugInLoading;
                AssemblyLoadContext = new RuntimeAssemblyLoadContext(name, assemblyPath);
                Assembly = AssemblyLoadContext.LoadFromAssemblyName(assemblyName);
                PlugIn = LoadPlugIn(Assembly);
                State = ExecutionState.PlugInLoaded;
            }
            catch (Exception)
            {
                State = ExecutionState.PlugInLoadFailure;
                throw;
            }
        }

        internal void Start()
        {
            State = ExecutionState.Starting;
            TokenSource = new CancellationTokenSource();
            Task = Task.Run(async () => await PlugIn!.ExecuteAsync(TokenSource.Token));
            State = ExecutionState.Active;
        }

        private static IPlugIn? LoadPlugIn(Assembly assembly)
        {
            IPlugIn? result = null;
            foreach (var type in from Type type in assembly.GetTypes()
                                 where typeof(IPlugIn).IsAssignableFrom(type)
                                 select type)
            {
                result = Activator.CreateInstance(type) as IPlugIn;
                if (result != null)
                {
                    break;
                }
            }

            return result;
        }
    }
}