using System;
using System.Net;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using ImagesApi.Controllers;
using ImagesApi.Model;
using ImagesApi.Model.ImageHandling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ImagesApi.Tests
{
    public class ImagesControllerTests
    {
        [Fact]
        public void Should_Implement_ControllerBase()
        {
            object sut = new ImagesController(null, null);
            sut.Should().BeAssignableTo<ControllerBase>();
        }

        [Fact]
        public void WhenCreated_Should_NotThrow()
        {
            Action factory = () => new ImagesController(null, null);
            factory.Should().NotThrow();
        }

        [Fact]
        public async Task WhenCallingGetImage_Should_CallImagesLibraryToGetRequestedImage()
        {
            var imagesLibrary = A.Fake<IImagesLibrary>();
            var sut = new ImagesController(imagesLibrary, A.Dummy<ILogger<ImagesController>>());
            await sut.GetImage("01_04_2019_001103");

            A.CallTo(() => imagesLibrary.GetImageAsync(
                    A<string>.That.Matches(imageName => imageName == "01_04_2019_001103"),
                    A<string>.That.Matches(imageResolution => imageResolution == null),
                    A<string>.That.Matches(backgroundColour => backgroundColour == null),
                    A<string>.That.Matches(watermarkText => watermarkText == null),
                    A<string>.That.Matches(imageType => imageType == null)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCallingGetImage_Should_ReturnFileStreamResultIfImageAvailable()
        {
            var imagesLibrary = A.Fake<IImagesLibrary>();
            A.CallTo(() => imagesLibrary.GetImageAsync(
                    A<string>._,
                    A<string>._,
                    A<string>._,
                    A<string>._,
                    A<string>._))
                .ReturnsLazily(callInfo =>
                {
                    var testImage = new Image(
                        callInfo.GetArgument<string>("imageName"),
                        TestHelpers.GetTestImage());
                    return Task.FromResult(testImage);
                });
            var sut = new ImagesController(imagesLibrary, A.Dummy<ILogger<ImagesController>>());
            var result = await sut.GetImage("01_04_2019_001103");

            result.Should().BeOfType<FileStreamResult>();
            ((FileStreamResult) result).ContentType.Should().BeEquivalentTo("image/png");
            var actualGdiImage = TestHelpers.GetGdiImageFromStream(((FileStreamResult) result).FileStream);
            TestHelpers.CompareImages(actualGdiImage, TestHelpers.GetTestImage()).Should().Be(CompareResult.Same);
        }

        [Fact]
        public async Task WhenCallingGetImage_Should_ReturnNotFoundIfImageNotAvailable()
        {
            var imagesLibrary = A.Fake<IImagesLibrary>();
            A.CallTo(() => imagesLibrary.GetImageAsync(
                    A<string>._,
                    A<string>._,
                    A<string>._,
                    A<string>._,
                    A<string>._))
                .Throws<ImageNotAvailableException>();
            var sut = new ImagesController(imagesLibrary, A.Dummy<ILogger<ImagesController>>());
            var result = await sut.GetImage("01_04_2019_001103");

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task WhenCallingGetImage_Should_ReturnInternalServerErrorStatusIfExceptionThrown()
        {
            var imagesLibrary = A.Fake<IImagesLibrary>();
            A.CallTo(() => imagesLibrary.GetImageAsync(
                    A<string>._,
                    A<string>._,
                    A<string>._,
                    A<string>._,
                    A<string>._))
                .Throws<Exception>();
            var sut = new ImagesController(imagesLibrary, A.Dummy<ILogger<ImagesController>>());
            var result = await sut.GetImage("01_04_2019_001103");

            result.Should().BeOfType<StatusCodeResult>();
            ((StatusCodeResult) result).StatusCode.Should().Be((int) HttpStatusCode.InternalServerError);
        }

        [Theory]
        [InlineData("100")]
        [InlineData("120x100")]
        [InlineData("100x120")]
        public async Task
            WhenCallingGetImage_AndResolutionIsSpecified_Should_CallImagesLibraryAndSpecifyRequiredResolution(
                string resolutionString)
        {
            var imagesLibrary = A.Fake<IImagesLibrary>();
            var sut = new ImagesController(imagesLibrary, A.Dummy<ILogger<ImagesController>>());
            await sut.GetImage("01_04_2019_001103", resolution: resolutionString);

            A.CallTo(() => imagesLibrary.GetImageAsync(
                    A<string>.That.Matches(imageName => imageName == "01_04_2019_001103"),
                    A<string>.That.Matches(imageResolution => imageResolution == resolutionString),
                    A<string>.That.Matches(backgroundColour => backgroundColour == null),
                    A<string>.That.Matches(watermarkText => watermarkText == null),
                    A<string>.That.Matches(imageType => imageType == null)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCallingGetImage_Should_ReturnBadRequestIfResolutionFormatExceptionThrown()
        {
            var imagesLibrary = A.Fake<IImagesLibrary>();
            A.CallTo(() => imagesLibrary.GetImageAsync(
                    A<string>._,
                    A<string>._,
                    A<string>._,
                    A<string>._,
                    A<string>._))
                .Throws(new ImageLibraryException("", new ResolutionFormatException()));
            var sut = new ImagesController(imagesLibrary, A.Dummy<ILogger<ImagesController>>());
            var result = await sut.GetImage("01_04_2019_001103", resolution: "a bad resolution string");

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData("#4287f5")]
        [InlineData("4287f5")]
        [InlineData("Blue")]
        public async Task
            WhenCallingGetImage_AndBackgroundColourIsSpecified_Should_CallImagesLibraryAndSpecifyRequiredBackgroundColour(
                string colourString)
        {
            var imagesLibrary = A.Fake<IImagesLibrary>();
            var sut = new ImagesController(imagesLibrary, A.Dummy<ILogger<ImagesController>>());
            await sut.GetImage("01_04_2019_001103", backgroundColour: colourString);

            A.CallTo(() => imagesLibrary.GetImageAsync(
                    A<string>.That.Matches(imageName => imageName == "01_04_2019_001103"),
                    A<string>.That.Matches(imageResolution => imageResolution == null),
                    A<string>.That.Matches(backgroundColour => backgroundColour == colourString),
                    A<string>.That.Matches(watermarkText => watermarkText == null),
                    A<string>.That.Matches(imageType => imageType == null)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCallingGetImage_Should_ReturnBadRequestIfColourFormatExceptionThrown()
        {
            var imagesLibrary = A.Fake<IImagesLibrary>();
            A.CallTo(() => imagesLibrary.GetImageAsync(
                    A<string>._,
                    A<string>._,
                    A<string>._,
                    A<string>._,
                    A<string>._))
                .Throws(new ImageLibraryException("", new ColourFormatException()));
            var sut = new ImagesController(imagesLibrary, A.Dummy<ILogger<ImagesController>>());
            var result = await sut.GetImage("01_04_2019_001103", backgroundColour: "a bad colour string");

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task
            WhenCallingGetImage_AndWatermarkIsSpecified_Should_CallImagesLibraryAndSpecifyRequiredWatermark()
        {
            var imagesLibrary = A.Fake<IImagesLibrary>();
            var sut = new ImagesController(imagesLibrary, A.Dummy<ILogger<ImagesController>>());
            await sut.GetImage("01_04_2019_001103", watermark: "Some watermark text");

            A.CallTo(() => imagesLibrary.GetImageAsync(
                    A<string>.That.Matches(imageName => imageName == "01_04_2019_001103"),
                    A<string>.That.Matches(imageResolution => imageResolution == null),
                    A<string>.That.Matches(backgroundColour => backgroundColour == null),
                    A<string>.That.Matches(watermarkText => watermarkText == "Some watermark text"),
                    A<string>.That.Matches(imageType => imageType == null)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task
            WhenCallingGetImage_AndImageTypeIsSpecified_Should_CallImagesLibraryAndSpecifyRequiredImageType()
        {
            var imagesLibrary = A.Fake<IImagesLibrary>();
            var sut = new ImagesController(imagesLibrary, A.Dummy<ILogger<ImagesController>>());
            await sut.GetImage("01_04_2019_001103", imageType: "jpeg");

            A.CallTo(() => imagesLibrary.GetImageAsync(
                    A<string>.That.Matches(imageName => imageName == "01_04_2019_001103"),
                    A<string>.That.Matches(imageResolution => imageResolution == null),
                    A<string>.That.Matches(backgroundColour => backgroundColour == null),
                    A<string>.That.Matches(watermarkText => watermarkText == null),
                    A<string>.That.Matches(imageType => imageType == "jpeg")))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCallingGetImage_Should_ReturnBadRequestIfImageTypeFormatExceptionThrown()
        {
            var imagesLibrary = A.Fake<IImagesLibrary>();
            A.CallTo(() => imagesLibrary.GetImageAsync(
                    A<string>._,
                    A<string>._,
                    A<string>._,
                    A<string>._,
                    A<string>._))
                .Throws(new ImageLibraryException("", new ImageTypeFormatException()));
            var sut = new ImagesController(imagesLibrary, A.Dummy<ILogger<ImagesController>>());
            var result = await sut.GetImage("01_04_2019_001103", imageType: "a bad image type string");

            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
