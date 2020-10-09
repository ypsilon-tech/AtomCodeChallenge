using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using ImagesApi.Model.ImageHandling.IO;
using ImagesApi.Model.ImageHandling.IO.LocalFileSystem;
using Xunit;

namespace ImagesApi.Tests
{
    public class LocalFileSystemGdiImageLoaderTests
    {
        [Fact]
        public void Should_Implement_IGdiImageLoader()
        {
            object sut = new LocalFileSystemGdiImageLoader(A.Dummy<ILocalFileSystemConfiguration>());
            sut.Should().BeAssignableTo<IGdiImageLoader>();
        }

        [Fact]
        public void WhenCreated_Should_Not_Throw()
        {
            Action factory = () => new LocalFileSystemGdiImageLoader(A.Dummy<ILocalFileSystemConfiguration>());
            factory.Should().NotThrow();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void WhenCalling_LoadGdiImageAsync_WithNullOrWhitespaceString_Should_ThrowArgumentException(string imageNameVal)
        {
            var sut = new LocalFileSystemGdiImageLoader(A.Dummy<ILocalFileSystemConfiguration>());
            sut.Awaiting(lilfs => lilfs.LoadGdiImageAsync(imageNameVal))
                .Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public async Task WhenCalling_LoadGdiImageAsync_Should_CallLocalFileSystemConfigurationToObtainPathToLibraryFolder()
        {
            var localFileSystemConfig = A.Fake<ILocalFileSystemConfiguration>();
            A.CallTo(() => localFileSystemConfig.GetLocalLibraryFolderPath())
                .Returns(string.Empty);
            var sut = new LocalFileSystemGdiImageLoader(localFileSystemConfig);
            await sut.LoadGdiImageAsync("01_04_2019_001103");

            A.CallTo(() => localFileSystemConfig.GetLocalLibraryFolderPath())
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void WhenCalling_LoadGdiImageAsync_Should_ThrowIfConfiguredLocalLibraryFolderDoesNotExist()
        {
            var localFileSystemConfig = A.Fake<ILocalFileSystemConfiguration>();
            A.CallTo(() => localFileSystemConfig.GetLocalLibraryFolderPath())
                .Returns("H:\\SomeRandomFolderThatDoesntExist");
            var sut = new LocalFileSystemGdiImageLoader(localFileSystemConfig);

            sut.Awaiting(illfs => illfs.LoadGdiImageAsync("01_04_2019_001103"))
                .Should().ThrowExactly<GdiImageLoaderFileSystemException>();
        }
    }
}
