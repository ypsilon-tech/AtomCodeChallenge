using System;
using FakeItEasy;
using FluentAssertions;
using ImagesApi.Model.ImageHandling.IO.LocalFileSystem;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ImagesApi.Tests
{
    public class LocalFileSystemConfigurationTests
    {
        [Fact]
        public void Should_Implement_ILocalFileSystemConfiguration()
        {
            object sut = new LocalFileSystemConfiguration(A.Dummy<IConfiguration>());
            sut.Should().BeAssignableTo<ILocalFileSystemConfiguration>();
        }

        [Fact]
        public void WhenCreated_Should_NotThrow()
        {
            Action factory = () => new LocalFileSystemConfiguration(A.Dummy<IConfiguration>());
            factory.Should().NotThrow();
        }

        [Fact]
        public void WhenCreated_Should_CallConfigurationToRetrieveLocalLibraryFolderPath()
        {
            var configuration = A.Fake<IConfiguration>();
            new LocalFileSystemConfiguration(configuration);

            A.CallTo(() => configuration["LocalFileSystemImageLibrary:LibraryFolderPath"])
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void WhenCalling_GetLocalLibraryFolderPath_Should_ReturnLocalLibraryFolderPathFromConfiguration()
        {
            var configuration = A.Fake<IConfiguration>();
            A.CallTo(() => configuration["LocalFileSystemImageLibrary:LibraryFolderPath"])
                .Returns("C:\\LocalLibraryFolder");
            var sut = new LocalFileSystemConfiguration(configuration);
            var result = sut.GetLocalLibraryFolderPath();

            result.Should().BeEquivalentTo("C:\\LocalLibraryFolder");
        }
    }
}
