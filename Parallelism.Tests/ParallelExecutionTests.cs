using AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Parallelism.Tests
{
    public class ParallelExecutionTests
    {
        private readonly Fixture _fixture;

        public ParallelExecutionTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void ParallelExecution_BuildWithNonGenericProcessorImplementation_Success()
        {
            //Arrange
            var repositoryMock = _fixture.CreateMany<string>(50).ToList();

            Func<int, Task<IEnumerable<string>>> mockProvider =
                skip => Task.FromResult(repositoryMock.Skip(skip).Take(2));

            Func<string, Task> mockProcessor = item => Task.CompletedTask;

            //Act
            var sut = ParallelExecution.Build(mockProvider, mockProcessor);

            //Assert
            Assert.NotNull(sut);
            Assert.True(sut.GetType().GetGenericTypeDefinition() == typeof(ParallelExecutionImplementation<>));
        }

        [Fact]
        public void ParallelExecution_BuildWithNonGenericProcessorImplementation_ItemsAsSource_Success()
        {
            //Arrange
            var items = _fixture.CreateMany<string>(50).ToList();

            Func<string, Task> mockProcessor = item => Task.CompletedTask;

            //Act
            var sut = ParallelExecution.Build(items, mockProcessor);

            //Assert
            Assert.NotNull(sut);
            Assert.True(sut.GetType().GetGenericTypeDefinition() == typeof(ParallelExecutionImplementation<>));
        }

        [Fact]
        public void ParallelExecution_BuildWithNonGenericProcessorImplementation_ProviderArgumentIsNullException()
        {
            //Arrange
            var repositoryMock = _fixture.CreateMany<string>(50).ToList();

            Func<int, Task<IEnumerable<string>>> mockProvider = null;

            Func<string, Task> mockProcessor = item => Task.CompletedTask;

            //Assert
            Assert.Throws<ArgumentNullException>(() => ParallelExecution.Build(mockProvider, mockProcessor));
        }

        [Fact]
        public void ParallelExecution_BuildWithNonGenericProcessorImplementation_ProcessorArgumentIsNullException()
        {
            //Arrange
            var repositoryMock = _fixture.CreateMany<string>(50).ToList();

            Func<int, Task<IEnumerable<string>>> mockProvider =
                skip => Task.FromResult(repositoryMock.Skip(skip).Take(2));

            Func<string, Task> mockProcessor = null;

            //Assert
            Assert.Throws<ArgumentNullException>(() => ParallelExecution.Build(mockProvider, mockProcessor));
        }

        [Fact]
        public void ParallelExecution_BuildWithNonGenericProcessorImplementation_ItemsArgumentIsNullException()
        {
            //Arrange
            List<string> items = null;

            Func<string, Task> mockProcessor = item => Task.CompletedTask;

            //Assert
            Assert.Throws<ArgumentNullException>(() => ParallelExecution.Build(items, mockProcessor));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void ParallelExecution_BuildWithNonGenericProcessorImplementation_MaxConcurrentThreadsArgumentIsEqualOrLowerThanZeroException(
            int maxConcurrentThreadsValue)
        {
            //Arrange
            var repositoryMock = _fixture.CreateMany<string>(50).ToList();

            Func<int, Task<IEnumerable<string>>> mockProvider =
                skip => Task.FromResult(repositoryMock.Skip(skip).Take(2));

            Func<string, Task> mockProcessor = item => Task.CompletedTask;

            //Assert
            Assert.Throws<ArgumentException>(
                () => ParallelExecution.Build(
                    mockProvider,
                    mockProcessor,
                    maxConcurrentThreadsValue));
        }

        [Fact]
        public void ParallelExecution_BuildWithGenericProcessorImplementation_Success()
        {
            //Arrange
            var repositoryMock = _fixture.CreateMany<string>(50).ToList();

            Func<int, Task<IEnumerable<string>>> mockProvider =
                skip => Task.FromResult(repositoryMock.Skip(skip).Take(2));

            Func<string, Task<int>> mockProcessor = item => Task.FromResult(5);

            //Act
            var sut = ParallelExecution.Build(mockProvider, mockProcessor);

            //Assert
            Assert.NotNull(sut);
            Assert.True(sut.GetType().GetGenericTypeDefinition() == typeof(ParallelExecutionImplementation<,>));
        }

        [Fact]
        public void ParallelExecution_BuildWitGenericProcessorImplementation_ItemsAsSource_Success()
        {
            //Arrange
            var items = _fixture.CreateMany<string>(50).ToList();

            Func<string, Task<int>> mockProcessor = item => Task.FromResult(5);

            //Act
            var sut = ParallelExecution.Build(items, mockProcessor);

            //Assert
            Assert.NotNull(sut);
            Assert.True(sut.GetType().GetGenericTypeDefinition() == typeof(ParallelExecutionImplementation<,>));
        }
    }
}