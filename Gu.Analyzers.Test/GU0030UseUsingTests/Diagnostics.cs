﻿namespace Gu.Analyzers.Test.GU0030UseUsingTests
{
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;

    internal class Diagnostics : DiagnosticVerifier<GU0030UseUsing>
    {
        [Test]
        public async Task NotUsingFileStream()
        {
            var testCode = @"
    using System.IO;

    public class Foo
    {
        public long Bar()
        {
            ↓var stream = File.OpenRead("""");
            return stream.Length;
        }
    }";

            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("Use using.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task UsingNewDisposable()
        {
            var disposableCode = @"
    using System;

    public class Disposable : IDisposable
    {
        public void Dispose()
        {
        }
    }";

            var testCode = @"
    public static class Foo
    {
        public static long Bar()
        {
            ↓var meh = new Disposable();
            return 1;
        }
    }";
            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("Use using.");
            await this.VerifyCSharpDiagnosticAsync(new[] { testCode, disposableCode }, expected, CancellationToken.None).ConfigureAwait(false);
        }
    }
}