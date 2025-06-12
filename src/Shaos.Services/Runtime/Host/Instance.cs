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
    public class Instance
    {
        public Instance(
            int id,
            int plugInId,
            string name,
            InstanceConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(configuration);

            Id = id;
            PlugInId = plugInId;
            Name = name;
            Configuration = configuration;
        }

        internal Instance(int id,
                          int parentId,
                          string name,
                          InstanceState state,
                          InstanceConfiguration configuration)
            : this(id, parentId, name, configuration)
        {
            State = state;
        }

        /// <summary>
        /// The <see cref="InstanceConfiguration"/> for this instances
        /// </summary>
        public InstanceConfiguration Configuration { get; private set; }

        /// <summary>
        /// The <see cref="IPlugIn"/> instance execution context
        /// </summary>
        public InstanceExecutionContext? Context { get; private set; }

        /// <summary>
        /// The captured <see cref="Exception"/> that occurs during the <see cref="IPlugIn"/> execution
        /// </summary>
        public Exception? Exception { get; private set; }

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
        /// The <see cref="InstanceState"/> of the <see cref="Instance"/>
        /// </summary>
        public InstanceState State { get; private set; }

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
            return State != InstanceState.Running && Configuration.RequiresConfiguration;
        }

        /// <summary>
        /// Indicates if this instance can be started
        /// </summary>
        /// <returns>true if this instance can be started</returns>
        public bool CanStart()
        {
            if (Configuration.RequiresConfiguration && !Configuration.IsConfigured)
                return false;
            else
                return State != InstanceState.Running;
        }

        /// <summary>
        /// Indicates if this instance can be stopped
        /// </summary>
        /// <returns>true if this instance can be stopped</returns>
        public bool CanStop()
        {
            return State == InstanceState.Running;
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"{nameof(Id)}: {Id}");
            stringBuilder.AppendLine($"{nameof(Configuration)}: {Configuration}");
            stringBuilder.AppendLine($"{nameof(Context)}: {Context}");
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
            await Context!.PlugIn!.ExecuteAsync(cancellationToken);
        }

        internal void LoadContext(IPlugIn plugIn)
        {
            ArgumentNullException.ThrowIfNull(plugIn);

            Context = new InstanceExecutionContext(plugIn);
        }

        internal void SetComplete()
        {
            State = InstanceState.Complete;
            StopTime = DateTime.UtcNow;
        }

        internal void SetFaulted(AggregateException? exception)
        {
            State = InstanceState.Faulted;
            Exception = exception;
        }

        internal void SetFaulted(Exception? exception)
        {
            State = InstanceState.Faulted;
            Exception = exception;
        }

        internal void SetRunning()
        {
            State = InstanceState.Running;
            StartTime = DateTime.UtcNow;
        }

        internal void StartExecution(
            Func<CancellationToken, Task> executeTask,
            Action<Task> completionTask)
        {
            State = InstanceState.Starting;

            Context!.StartExecution(executeTask, completionTask);
        }

        internal async Task<bool> StopExecutionAsync(TimeSpan stopTimeout)
        {
            await Context!.TokenSource!.CancelAsync();

            return await Task.WhenAny(Context!.Task!, Task.Delay(stopTimeout)) == Context.Task;
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