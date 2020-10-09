using System;
using System.Drawing;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using ImagesApi.Model.ImageHandling.IO;
using Xunit;

namespace ImagesApi.Tests
{
    public class ImageLoaderTests
    {
        [Fact]
        public void Should_Implement_IImageLoader()
        {
            object sut = new ImageLoader(A.Dummy<IGdiImageLoader>());
            sut.Should().BeAssignableTo<IImageLoader>();
        }

        [Fact]
        public void WhenCreated_Should_Not_Throw()
        {
            Action factory = () => new ImageLoader(A.Dummy<IGdiImageLoader>());
            factory.Should().NotThrow();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void WhenCalling_LoadFromLibraryAsync_WithNullOrWhitespaceString_Should_ThrowArgumentException(string testImageName)
        {
            var sut = new ImageLoader(A.Dummy<IGdiImageLoader>());
            sut.Awaiting(il => il.LoadFromLibraryAsync(testImageName))
                .Should().Throw<ArgumentException>();
        }

        [Fact]
        public async Task WhenCalling_LoadFromLibraryAsync_Should_CallGdiImageLoaderToLoadGDIImage()
        {
            var gdiImageLoader = A.Fake<IGdiImageLoader>();
            var sut = new ImageLoader(gdiImageLoader);
            await sut.LoadFromLibraryAsync("01_04_2019_001103");

            A.CallTo(() => gdiImageLoader.LoadGdiImageAsync(A<string>.That.Matches(imageName => imageName == "01_04_2019_001103")))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCalling_LoadFromLibraryAsync_Should_ReturnImageContainingGDIImageIfFound()
        {
            var gdiImageLoader = A.Fake<IGdiImageLoader>();
            var testImage = TestHelpers.GetTestImage();
            A.CallTo(() => gdiImageLoader.LoadGdiImageAsync(A<string>._))
                .Returns(testImage);
            var sut = new ImageLoader(gdiImageLoader);
            var result = await sut.LoadFromLibraryAsync("01_04_2019_001103");

            result.Name.Should().BeEquivalentTo("01_04_2019_001103");
            TestHelpers.CompareImages(testImage, result.ToGdiImage()).Should().BeEquivalentTo(CompareResult.Same);
        }

        [Fact]
        public async Task WhenCalling_LoadFromLibraryAsync_Should_ReturnNullIfGDIImageNotFound()
        {
            var gdiImageLoader = A.Fake<IGdiImageLoader>();
            A.CallTo(() => gdiImageLoader.LoadGdiImageAsync(A<string>._))
                .Returns(Task.FromResult((Image)null));
            var sut = new ImageLoader(gdiImageLoader);
            var result = await sut.LoadFromLibraryAsync("01_04_2019_001103");

            result.Should().BeNull();
        }
    }
}
