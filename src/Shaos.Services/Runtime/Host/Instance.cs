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
using System.Text;
using System.Reflection;
using Shaos.Services.Runtime.Host;
using Microsoft.Extensions.Options;

namespace Shaos.Services.Runtime
{
    /// <summary>
    /// An executing <see cref="PlugIn"/> instance
    /// </summary>
    public class Instance()
    {
        /// <summary>
        /// The <see cref="Assembly"/> the <see cref="PlugIn"/> was loaded
        /// </summary>
        public Assembly? Assembly { get; internal set; }

        /// <summary>
        /// The assembly file path
        /// </summary>
        public string AssemblyFilePath { get; internal set; } = string.Empty;

        /// <summary>
        /// The captured <see cref="Exception"/> that occurs during the <see cref="IPlugIn"/> execution
        /// </summary>
        public Exception? Exception { get; internal set; }

        /// <summary>
        /// The <see cref="PlugInInstance"/> identifier
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// The <see cref="PlugInInstance"/> name
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// The <see cref="IPlugIn"/> <see cref="IOptions"/> configuration settings
        /// </summary>
        public IOptions<object>? Options { get; internal set; }

        /// <summary>
        /// The <see cref="IPlugIn"/> instance that executes its functions
        /// </summary>
        public IPlugIn? PlugIn { get; internal set; }

        /// <summary>
        /// The total running time of this instance
        /// </summary>
        public TimeSpan RunningTime => CalculateRunningTime();

        /// <summary>
        /// The last start time of this instance
        /// </summary>
        public DateTime? StartTime { get; internal set; }

        /// <summary>
        /// The <see cref="ExecutingState"/> of the <see cref="Instance"/>
        /// </summary>
        public InstanceState State { get; internal set; }

        /// <summary>
        /// The last stop time of this instance
        /// </summary>
        public DateTime? StopTime { get; internal set; }

        /// <summary>
        /// The <see cref="Task"/> that is executing the <see cref="Instance"/>
        /// </summary>
        public Task? Task { get; internal set; }

        /// <summary>
        /// The <see cref="CancellationTokenSource"/> used to cancel the executing <see cref="Instance"/>
        /// </summary>
        public CancellationTokenSource? TokenSource { get; internal set; }

        /// <summary>
        /// The <see cref="IRuntimeAssemblyLoadContext"/> for the loaded <see cref="PlugIn"/>
        /// </summary>
        public UnloadingWeakReference<IRuntimeAssemblyLoadContext>? Context { get; internal set; }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(Id)}: {Id}");
            stringBuilder.AppendLine($"{nameof(Name)}: {Name}");
            stringBuilder.AppendLine($"{nameof(State)}: {State}");
            stringBuilder.AppendLine($"{nameof(StartTime)}: {(StartTime == null ? "Empty" : StartTime)}");
            stringBuilder.AppendLine($"{nameof(StopTime)}: {(StopTime == null ? "Empty" : StopTime)}");
            stringBuilder.AppendLine($"{nameof(RunningTime)}: {RunningTime}");
            stringBuilder.AppendLine($"{nameof(PlugIn)}: {(PlugIn == null ? "Empty" : PlugIn.GetType().Name)}");
            stringBuilder.AppendLine($"{nameof(Assembly)}: {(Assembly == null ? "Empty" : Assembly.ToString())}");
            stringBuilder.AppendLine($"{nameof(AssemblyFilePath)}: {(AssemblyFilePath == null ? "Empty" : AssemblyFilePath.ToString())}");
            stringBuilder.AppendLine($"{nameof(Task)}: {(Task == null ? "Empty" : Task.Id)}");
            stringBuilder.AppendLine($"{nameof(TokenSource)}: {(TokenSource == null ? "Empty" : TokenSource.IsCancellationRequested)}");
            stringBuilder.AppendLine($"{nameof(Exception)}: {(Exception == null ? "Empty" : Exception.ToString())}");
            stringBuilder.AppendLine($"{nameof(Context)}: {(Context == null ? "Empty" : Context.Target.Name)}");

            return stringBuilder.ToString();
        }

        internal void CleanUp()
        {
            Assembly = null;
            PlugIn = null;
            Task = null;
            TokenSource = null;

            Context?.Dispose();
        }

        private TimeSpan CalculateRunningTime()
        {
            TimeSpan timeSpan = TimeSpan.Zero;

            if (StartTime != null)
            {
                if (StopTime != null)
                {
                    timeSpan = StopTime.Value.Subtract(StartTime.Value);
                }
                else
                {
                    timeSpan = DateTime.UtcNow.Subtract(StartTime.Value);
                }
            }

            return timeSpan;
        }
    }
}