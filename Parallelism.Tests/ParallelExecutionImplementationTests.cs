using AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Parallelism.Tests
{
    public class ParallelExecutionImplementationTests
    {
        private readonly Fixture _fixture;

        public ParallelExecutionImplementationTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public async Task ParallelExecutionImplementation_RunAsync_Success()
        {
            //Arrange
            var repositoryMock = _fixture.CreateMany<string>(50).ToList();

            Func<int, Task<IEnumerable<string>>> provider =
               skip => Task.FromResult(repositoryMock.Skip(skip).Take(2));

            Func<string, Task> processor = item => Task.CompletedTask;

            //Act
            await ParallelExecution.Build(provider, processor).RunAsync()
                .ContinueWith(run =>
                {
                    //Assert
                    Assert.True(run.IsCompletedSuccessfully);
                });
        }

        [Fact]
        public async Task ParallelExecutionImplementation_RunAsync_CancellationRequested()
        {
            //Arrange
            var repositoryMock = _fixture.CreateMany<string>(50).ToList();

            Func<int, Task<IEnumerable<string>>> provider =
               skip => Task.FromResult(repositoryMock.Skip(skip).Take(2));

            Func<string, Task> processor = item => Task.CompletedTask;

            var cts = new CancellationTokenSource();
            cts.CancelAfter(5);

            //Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => ParallelExecution.Build(provider, processor).RunAsync(cts.Token));
        }

        [Fact]
        public async Task ParallelExecutionImplementation_RunAsync_ItemsAsSource_Success()
        {
            //Arrange
            var items = _fixture.CreateMany<string>(50).ToList();

            Func<string, Task> processor = item => Task.CompletedTask;

            //Act
            await ParallelExecution.Build(items, processor).RunAsync().ContinueWith(run =>
            {
                //Assert
                Assert.True(run.IsCompletedSuccessfully);
            });
        }

        [Fact]
        public async Task ParallelExecutionImplementation_RunAsync_ProviderReturnedNullObject()
        {
            //Arrange
            var repositoryMock = _fixture.CreateMany<string>(50).ToList();

            Func<int, Task<IEnumerable<string>>> provider =
               skip => Task.FromResult<IEnumerable<string>>(null);

            Func<string, Task> processor = item => Task.CompletedTask;

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => ParallelExecution.Build(provider, processor).RunAsync());
        }

        [Fact]
        public async Task ParallelExecutionImplementation_RunAsync_GenericTaskReturnType_Success()
        {
            //Arrange
            var numOfItemsInRepository = 50;
            var repositoryMock = _fixture.CreateMany<string>(numOfItemsInRepository).ToList();

            Func<int, Task<IEnumerable<string>>> provider =
                skip => Task.FromResult(repositoryMock.Skip(skip).Take(2));

            var processorReturnedValue = 5;
            Func<string, Task<int>> processor = item => Task.FromResult(processorReturnedValue);

            //Act
            var results = await ParallelExecution.Build(provider, processor).RunAsync();

            //Assert
            Assert.NotNull(results);
            Assert.True(results.Count() == numOfItemsInRepository);
            Assert.True(results.All(x => x == processorReturnedValue));
        }

        [Fact]
        public async Task ParallelExecutionImplementation_RunAsync_GenericTaskReturnType_ProcessorThrowsExeception()
        {
            //Arrange
            var numOfItemsInRepository = 50;
            var repositoryMock = _fixture.CreateMany<string>(numOfItemsInRepository).ToList();

            Func<int, Task<IEnumerable<string>>> provider =
                skip => Task.FromResult(repositoryMock.Skip(skip).Take(2));

            Func<string, Task<int>> processor = item => throw new Exception();

            //Assert
            await Assert.ThrowsAsync<Exception>(() => ParallelExecution.Build(provider, processor).RunAsync());
        }

        [Fact]
        public async Task ParallelExecutionImplementation_RunAsync_GenericTaskReturnType_ProviderThrowsException()
        {
            //Arrange
            var numOfItemsInRepository = 50;
            var repository = _fixture.CreateMany<string>(numOfItemsInRepository).ToList();

            Func<int, Task<IEnumerable<string>>> provider =
                skip => throw new Exception();

            var processorReturnedValue = 5;
            Func<string, Task<int>> processor = item => Task.FromResult(processorReturnedValue);

            //Assert
            await Assert.ThrowsAsync<Exception>(() => ParallelExecution.Build(provider, processor).RunAsync());
        }
    }
}
