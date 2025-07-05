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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shaos.Repository.Models;

namespace Shaos.Repository.EntityTypeConfigurations
{
    /// <summary>
    /// The <see cref="PlugIn"/> configuration
    /// </summary>
    public class PlugInEntityTypeConfiguration : IEntityTypeConfiguration<PlugIn>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<PlugIn> builder)
        {
            builder
                .HasKey(_ => _.Id);

            builder
                .HasIndex(_ => _.Name)
                .IsUnique(true);

            builder
                .Property(_ => _.Name)
                .HasMaxLength(ModelConstants.MaxNameLength)
                .IsRequired();

            builder
                .Property(_ => _.Description)
                .HasMaxLength(ModelConstants.MaxDescriptionLength)
                .IsRequired();

            builder
                .OwnsOne(
                    _ => _.Package,
                    _ =>
                    {
                        _.Property(_ => _.AssemblyFile).HasMaxLength(ModelConstants.MaxFilePathLength).IsRequired();
                        _.Property(_ => _.FileName).HasMaxLength(ModelConstants.MaxNameLength).IsRequired();
                        _.Property(_ => _.HasConfiguration).IsRequired();
                        _.Property(_ => _.HasLogger).IsRequired();
                        _.Property(_ => _.AssemblyVersion).HasMaxLength(ModelConstants.MaxVersionLength).IsRequired();
                    });

            builder
               .HasMany(_ => _.Instances)
               .WithOne(_ => _.PlugIn)
               .HasForeignKey(_ => _.PlugInId)
               .IsRequired(false);
        }
    }
}