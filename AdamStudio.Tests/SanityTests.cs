using System;
using Xunit;

namespace AdamStudio.Tests
{
    /// <summary>
    /// Sanity tests — verify the test infrastructure works end-to-end.
    /// If these fail, the test runner setup is broken, not the production code.
    /// </summary>
    public class SanityTests
    {
        [Fact]
        public void TestRunner_ExecutesFactTests()
        {
            // Arrange
            const int expected = 42;

            // Act
            const int actual = 6 * 7;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(1, 2, 3)]
        [InlineData(10, 20, 30)]
        [InlineData(-5, 5, 0)]
        public void TestRunner_ExecutesTheoryTests(int a, int b, int expected)
        {
            Assert.Equal(expected, a + b);
        }

        [Fact]
        public void Net10Runtime_IsAvailable()
        {
            // Verifies the test project targets net10.0 as expected.
            Version runtimeVersion = Environment.Version;
            Assert.True(runtimeVersion.Major >= 10,
                $"Expected .NET 10+ runtime, got {runtimeVersion}");
        }
    }
}
