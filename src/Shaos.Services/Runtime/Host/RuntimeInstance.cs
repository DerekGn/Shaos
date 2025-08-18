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

using Shaos.Repository.Models;
using Shaos.Sdk;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Shaos.Services.Runtime.Host
{
    /// <summary>
    /// An executing <see cref="PlugIn"/> instance
    /// </summary>
    public class RuntimeInstance
    {
        private readonly bool _configurable;

        /// <summary>
        /// Create a <see cref="PlugIn"/> instance
        /// </summary>
        /// <param name="id">The <see cref="RuntimeInstance"/> identifier</param>
        /// <param name="plugInId">The parent <see cref="PlugIn"/> identifier</param>
        /// <param name="name"><see cref="RuntimeInstance"/> name </param>
        /// <param name="assemblyPath">The path to the <see cref="PlugIn"/> assembly</param>
        /// <param name="configurable"></param>
        public RuntimeInstance(int id,
                               int plugInId,
                               string name,
                               string assemblyPath,
                               bool configurable = false)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(assemblyPath);

            Id = id;
            PlugInId = plugInId;
            Name = name;
            AssemblyPath = assemblyPath;
            _configurable = configurable;
        }

        internal RuntimeInstance(int id,
                                 int parentId,
                                 string name,
                                 string assemblyPath,
                                 RuntimeInstanceState state,
                                 bool configurable = false)
            : this(id, parentId, name, assemblyPath, configurable)
        {
            State = state;
        }

        /// <summary>
        /// The assembly path for this <see cref="PlugIn"/> instance.
        /// </summary>
        public string AssemblyPath { get; }

        /// <summary>
        /// The captured <see cref="Exception"/> that occurs during the <see cref="IPlugIn"/> execution
        /// </summary>
        public Exception? Exception { get; private set; }

        /// <summary>
        /// The <see cref="IPlugIn"/> instance execution context
        /// </summary>
        public RuntimeExecutionContext? ExecutionContext { get; private set; }

        /// <summary>
        /// The <see cref="PlugInInstance"/> identifier
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The <see cref="PlugInInstance"/> name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The PlugIn identifier
        /// </summary>
        public int PlugInId { get; }

        /// <summary>
        /// The total running time of this instance
        /// </summary>
        public TimeSpan RunningTime => CalculateRunningTime();

        /// <summary>
        /// The last start time of this instance
        /// </summary>
        public DateTime? StartTime { get; private set; }

        /// <summary>
        /// The <see cref="RuntimeInstanceState"/> of the <see cref="RuntimeInstance"/>
        /// </summary>
        public RuntimeInstanceState State { get; private set; }

        /// <summary>
        /// The last stop time of this instance
        /// </summary>
        public DateTime? StopTime { get; private set; }

        /// <summary>
        /// Indicates if this instance can be configured
        /// </summary>
        /// <returns>true if this instance can be configured</returns>
        public bool CanConfigure()
        {
            return State != RuntimeInstanceState.Running && _configurable;
        }

        /// <summary>
        /// Indicates if this instance can be started
        /// </summary>
        /// <returns>true if this instance can be started</returns>
        public bool CanStart()
        {
            return State != RuntimeInstanceState.Running;
        }

        /// <summary>
        /// Indicates if this instance can be stopped
        /// </summary>
        /// <returns>true if this instance can be stopped</returns>
        public bool CanStop()
        {
            return State == RuntimeInstanceState.Running;
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(Id)}: {Id}");
            stringBuilder.AppendLine($"{nameof(ExecutionContext)}: {ExecutionContext}");
            stringBuilder.AppendLine($"{nameof(Exception)}: {(Exception == null ? "Empty" : Exception.ToString())}");
            stringBuilder.AppendLine($"{nameof(Name)}: {Name}");
            stringBuilder.AppendLine($"{nameof(PlugInId)}: {PlugInId}");
            stringBuilder.AppendLine($"{nameof(RunningTime)}: {RunningTime}");
            stringBuilder.AppendLine($"{nameof(StartTime)}: {(StartTime == null ? "Empty" : StartTime)}");
            stringBuilder.AppendLine($"{nameof(State)}: {State}");
            stringBuilder.AppendLine($"{nameof(StopTime)}: {(StopTime == null ? "Empty" : StopTime)}");

            return stringBuilder.ToString();
        }

        internal async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            await ExecutionContext!.PlugIn!.ExecuteAsync(cancellationToken);
        }

        internal void LoadContext(IPlugIn plugIn)
        {
            ArgumentNullException.ThrowIfNull(plugIn);

            ExecutionContext = new RuntimeExecutionContext(plugIn);
        }

        internal void SetComplete()
        {
            State = RuntimeInstanceState.Complete;
            StopTime = DateTime.UtcNow;
        }

        internal void SetFaulted(AggregateException? exception)
        {
            State = RuntimeInstanceState.Faulted;
            Exception = exception;
        }

        internal void SetFaulted(Exception? exception)
        {
            State = RuntimeInstanceState.Faulted;
            Exception = exception;
        }

        internal void SetRunning()
        {
            State = RuntimeInstanceState.Running;
            StartTime = DateTime.UtcNow;
        }

        internal void StartExecution(Func<CancellationToken, Task> executeTask,
                                     Action<Task> completionTask)
        {
            State = RuntimeInstanceState.Starting;

            ExecutionContext!.StartExecution(executeTask, completionTask);
        }

        internal async Task<bool> StopExecutionAsync(TimeSpan stopTimeout)
        {
            await ExecutionContext!.TokenSource!.CancelAsync();

            return await Task.WhenAny(ExecutionContext!.Task!, Task.Delay(stopTimeout)) == ExecutionContext.Task;
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