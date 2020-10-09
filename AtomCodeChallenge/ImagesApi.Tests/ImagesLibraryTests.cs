using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using ImagesApi.Model;
using ImagesApi.Model.Caching;
using ImagesApi.Model.ImageHandling;
using ImagesApi.Model.ImageHandling.IO;
using Xunit;

namespace ImagesApi.Tests
{
    public class ImagesLibraryTests
    {
        [Fact]
        public void Should_Implement_IImagesLibrary()
        {
            object sut = new ImagesLibrary(null, null, null);
            sut.Should().BeAssignableTo<IImagesLibrary>();
        }

        [Fact]
        public void WhenCreated_Should_NotThrow()
        {
            Action factory = () => new ImagesLibrary(null, null, null);
            factory.Should().NotThrow();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_Should_ReturnImage()
        {
            var sut = new ImagesLibrary(
                A.Dummy<IImagesCache>(), 
                A.Dummy<IImageLoader>(),
                A.Dummy<IImageTransformer>());
            object result = await sut.GetImageAsync("01_04_2019_001103");
            result.Should().BeAssignableTo<Image>();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_Should_CheckCacheForImage()
        {
            var imageCache = A.Fake<IImagesCache>();
            var sut = new ImagesLibrary(
                imageCache, 
                A.Dummy<IImageLoader>(),
                A.Dummy<IImageTransformer>());
            await sut.GetImageAsync("01_04_2019_001103");

            A.CallTo(() => imageCache.GetImageAsync(A<ImageCacheKey>.That.Matches(cacheKey => cacheKey == new ImageCacheKey(
                    "01_04_2019_001103", 
                    null, 
                    null,
                    null,
                    null))))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_Should_ReturnCachedImageIfAvailable()
        {
            var imageCache = A.Fake<IImagesCache>();

            A.CallTo(() => imageCache.GetImageAsync(A<ImageCacheKey>._))
                .ReturnsLazily(callInfo =>
                {
                    var testImageName = callInfo.GetArgument<ImageCacheKey>("cacheKey")?.ImageName;
                    return Task.FromResult(new Image(testImageName));
                });
            var sut = new ImagesLibrary(
                imageCache, 
                A.Dummy<IImageLoader>(),
                A.Dummy<IImageTransformer>());
            var resultImage = await sut.GetImageAsync("01_04_2019_001103");

            resultImage.Name.Should().BeEquivalentTo("01_04_2019_001103");
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_And_CachedImageNotAvailable_Should_CallImageLoaderToLoadFromLibrary()
        {
            var imageCache = A.Fake<IImagesCache>();
            A.CallTo(() => imageCache.GetImageAsync(A<ImageCacheKey>._))
                .Returns(Task.FromResult((Image)null));
            var imageLoader = A.Fake<IImageLoader>();
            var sut = new ImagesLibrary(imageCache, imageLoader, A.Dummy<IImageTransformer>());
            await sut.GetImageAsync("01_04_2019_001103");

            A.CallTo(() => imageLoader.LoadFromLibraryAsync(
                    A<string>.That.Matches(imageName => imageName == "01_04_2019_001103")))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_And_CachedImageNotAvailable_Should_ReturnLoadedImage()
        {
            var imageCache = A.Fake<IImagesCache>();
            A.CallTo(() => imageCache.GetImageAsync(A<ImageCacheKey>._))
                .Returns(Task.FromResult((Image)null));
            var imageLoader = A.Fake<IImageLoader>();
            A.CallTo(() => imageLoader.LoadFromLibraryAsync(A<string>._))
                .ReturnsLazily(callInfo =>
                {
                    var testImage = new Image(callInfo.GetArgument<string>("imageName"));
                    return Task.FromResult(testImage);
                });
            var imageTransformer = A.Fake<IImageTransformer>();
            A.CallTo(() => imageTransformer.ApplyTransforms(
                    A<Image>._, 
                    A<string>._, 
                    A<string>._,
                    A<string>._,
                    A<string>._))
                .ReturnsLazily(callInfo => callInfo.GetArgument<Image>("image"));
            var sut = new ImagesLibrary(imageCache, imageLoader, imageTransformer);
            var resultImage = await sut.GetImageAsync("01_04_2019_001103");

            resultImage.Name.Should().BeEquivalentTo("01_04_2019_001103");
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_And_CachedImageNotAvailable_Should_CacheNewlyLoadedImage()
        {
            var imageCache = A.Fake<IImagesCache>();
            A.CallTo(() => imageCache.GetImageAsync(A<ImageCacheKey>._))
                .Returns(Task.FromResult((Image)null));
            var imageLoader = A.Fake<IImageLoader>();
            A.CallTo(() => imageLoader.LoadFromLibraryAsync(A<string>._))
                .ReturnsLazily(callInfo =>
                {
                    var testImage = new Image(callInfo.GetArgument<string>("imageName"));
                    return Task.FromResult(testImage);
                });
            var imageTransformer = A.Fake<IImageTransformer>();
            A.CallTo(() => imageTransformer.ApplyTransforms(
                    A<Image>._, 
                    A<string>._, 
                    A<string>._,
                    A<string>._,
                    A<string>._))
                .ReturnsLazily(callInfo => callInfo.GetArgument<Image>("image"));
            var sut = new ImagesLibrary(imageCache, imageLoader, imageTransformer);
            await sut.GetImageAsync("01_04_2019_001103");

            A.CallTo(() => imageCache.CacheImageAsync(
                    A<ImageCacheKey>.That.Matches(cacheKey => cacheKey == new ImageCacheKey(
                        "01_04_2019_001103", 
                        null, 
                        null,
                        null,
                        null)),
                    A<Image>.That.Matches(image => image.Name == "01_04_2019_001103")))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void WhenCalling_GetImageAsync_And_CachedImageNotAvailable_And_LoadedImagesNotAvailable_Should_ThrowImageNotAvailableException()
        {
            var imageCache = A.Fake<IImagesCache>();
            A.CallTo(() => imageCache.GetImageAsync(A<ImageCacheKey>._))
                .Returns(Task.FromResult((Image)null));
            var imageLoader = A.Fake<IImageLoader>();
            A.CallTo(() => imageLoader.LoadFromLibraryAsync(A<string>._))
                .Returns(Task.FromResult((Image)null));
            var sut = new ImagesLibrary(imageCache, imageLoader, A.Dummy<IImageTransformer>());

            sut.Awaiting(il => il.GetImageAsync("01_04_2019_001103"))
                .Should().ThrowExactly<ImageNotAvailableException>();
        }

        [Fact]
        public void WhenCalling_GetImageAsync_And_ImageCacheThrows_Should_ThrowImageLibraryExceptionContainingThrownCacheException()
        {
            var imageCache = A.Fake<IImagesCache>();
            A.CallTo(() => imageCache.GetImageAsync(A<ImageCacheKey>._))
                .Throws(new Exception("A caching error occurred"));
            var imageLoader = A.Dummy<IImageLoader>();
            var sut = new ImagesLibrary(imageCache, imageLoader, A.Dummy<IImageTransformer>());

            sut.Awaiting(il => il.GetImageAsync("01_04_2019_001103"))
                .Should().ThrowExactly<ImageLibraryException>()
                .WithInnerException<Exception>()
                .WithMessage("A caching error occurred");
        }

        [Fact]
        public void WhenCalling_GetImageAsync_And_ImageLoaderThrows_Should_ThrowImageLibraryExceptionContainingThrownProcessorException()
        {
            var imageCache = A.Fake<IImagesCache>();
            A.CallTo(() => imageCache.GetImageAsync(A<ImageCacheKey>._))
                .Returns(Task.FromResult((Image)null));
            var imageLoader = A.Fake<IImageLoader>();
            A.CallTo(() => imageLoader.LoadFromLibraryAsync(A<string>._))
                .Throws(new Exception("An image generation error occurred"));
            var sut = new ImagesLibrary(imageCache, imageLoader, A.Dummy<IImageTransformer>());

            sut.Awaiting(il => il.GetImageAsync("01_04_2019_001103"))
                .Should().ThrowExactly<ImageLibraryException>()
                .WithInnerException<Exception>()
                .WithMessage("An image generation error occurred");
        }

        [Fact]
        public void WhenCalling_GetImageAsync_And_ImageCacheAddThrows_Should_ThrowImageLibraryExceptionContainingThrownCacheException()
        {
            var imageCache = A.Fake<IImagesCache>();
            A.CallTo(() => imageCache.GetImageAsync(A<ImageCacheKey>._))
                .Returns(Task.FromResult((Image)null));
            A.CallTo(() => imageCache.CacheImageAsync(A<ImageCacheKey>._, A<Image>._))
                .Throws(new Exception("A cache store error occurred"));
            var imageLoader = A.Fake<IImageLoader>();
            A.CallTo(() => imageLoader.LoadFromLibraryAsync(A<string>._))
                .Returns(new Image());
            var sut = new ImagesLibrary(imageCache, imageLoader, A.Dummy<IImageTransformer>());

            sut.Awaiting(il => il.GetImageAsync("01_04_2019_001103"))
                .Should().ThrowExactly<ImageLibraryException>()
                .WithInnerException<Exception>()
                .WithMessage("A cache store error occurred");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void WhenCalling_GetImageAsync_WithNullOrWhitespaceString_Should_ThrowArgumentException(string imageNameVal)
        {
            var imageCache = A.Dummy<IImagesCache>();
            var imageProcessor = A.Dummy<IImageLoader>();
            var sut = new ImagesLibrary(imageCache, imageProcessor, A.Dummy<IImageTransformer>());

            sut.Awaiting(il => il.GetImageAsync(imageNameVal))
                .Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_And_ResolutionIsSpecified_Should_IncludeResolutionInCallToCheckImageCache()
        {
            var imageCache = A.Fake<IImagesCache>();
            var sut = new ImagesLibrary(imageCache, A.Dummy<IImageLoader>(), A.Dummy<IImageTransformer>());
            await sut.GetImageAsync("01_04_2019_001103", "100x120");

            A.CallTo(() => imageCache.GetImageAsync(
                    A<ImageCacheKey>.That.Matches(cacheKey => cacheKey == new ImageCacheKey(
                        "01_04_2019_001103", 
                        "100x120", 
                        null,
                        null,
                        null))))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_And_CachedImageNotAvailable_And_ResolutionIsSpecified_Should_CallImageTransformerToReScaleImage()
        {
            var imageCache = A.Fake<IImagesCache>();
            A.CallTo(() => imageCache.GetImageAsync(A<ImageCacheKey>._))
                .Returns(Task.FromResult((Image)null));
            var imageLoader = A.Fake<IImageLoader>();
            A.CallTo(() => imageLoader.LoadFromLibraryAsync(
                    A<string>.That.Matches(imageName => imageName == "01_04_2019_001103")))
                .ReturnsLazily(callInfo =>
                {
                    var testImage = new Image(callInfo.GetArgument<string>("imageName"));
                    return Task.FromResult(testImage);
                });
            var imageTransformer = A.Fake<IImageTransformer>();
            var sut = new ImagesLibrary(imageCache, imageLoader, imageTransformer);
            await sut.GetImageAsync("01_04_2019_001103", "100x100");

            A.CallTo(() => imageTransformer.ApplyTransforms(
                    A<Image>.That.Matches(image => image.Name == "01_04_2019_001103"),
                    A<string>.That.Matches(newResolution => newResolution == "100x100"),
                    A<string>.That.Matches(backgroundColour => backgroundColour == null),
                    A<string>.That.Matches(watermarkText => watermarkText == null),
                    A<string>.That.Matches(imageType => imageType == null)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_And_CachedImageNotAvailable_And_ResolutionSpecified_Should_IncludeResolutionInCallToCacheNewlyLoadedImage()
        {
            var imageCache = A.Fake<IImagesCache>();
            A.CallTo(() => imageCache.GetImageAsync(A<ImageCacheKey>._))
                .Returns(Task.FromResult((Image)null));
            var imageLoader = A.Fake<IImageLoader>();
            A.CallTo(() => imageLoader.LoadFromLibraryAsync(A<string>._))
                .ReturnsLazily(callInfo =>
                {
                    var testImage = new Image(callInfo.GetArgument<string>("imageName"));
                    return Task.FromResult(testImage);
                });
            var imageTransformer = A.Fake<IImageTransformer>();
            A.CallTo(() => imageTransformer.ApplyTransforms(
                    A<Image>._, 
                    A<string>._, 
                    A<string>._,
                    A<string>._,
                    A<string>._))
                .ReturnsLazily(callInfo => callInfo.GetArgument<Image>("image"));
            var sut = new ImagesLibrary(imageCache, imageLoader, imageTransformer);
            await sut.GetImageAsync("01_04_2019_001103", "100");

            A.CallTo(() => imageCache.CacheImageAsync(
                    A<ImageCacheKey>.That.Matches(cacheKey => cacheKey == new ImageCacheKey(
                        "01_04_2019_001103", 
                        "100", 
                        null,
                        null,
                        null)),
                    A<Image>.That.Matches(image => image.Name == "01_04_2019_001103")))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_And_BackgroundColourIsSpecified_Should_IncludeBackgroundColourInCallToCheckImageCache()
        {
            var imageCache = A.Fake<IImagesCache>();
            var sut = new ImagesLibrary(imageCache, A.Dummy<IImageLoader>(), A.Dummy<IImageTransformer>());
            await sut.GetImageAsync("01_04_2019_001103", backgroundColour: "4287f5");

            A.CallTo(() => imageCache.GetImageAsync(
                    A<ImageCacheKey>.That.Matches(cacheKey => cacheKey == new ImageCacheKey(
                        "01_04_2019_001103", 
                        null, 
                        "4287f5",
                        null,
                        null))))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_And_CachedImageNotAvailable_And_BackgroundColourIsSpecified_Should_CallImageTransformerToSetImageBackground()
        {
            var imageCache = A.Fake<IImagesCache>();
            A.CallTo(() => imageCache.GetImageAsync(A<ImageCacheKey>._))
                .Returns(Task.FromResult((Image)null));
            var imageLoader = A.Fake<IImageLoader>();
            A.CallTo(() => imageLoader.LoadFromLibraryAsync(
                    A<string>.That.Matches(imageName => imageName == "01_04_2019_001103")))
                .ReturnsLazily(callInfo =>
                {
                    var testImage = new Image(callInfo.GetArgument<string>("imageName"));
                    return Task.FromResult(testImage);
                });
            var imageTransformer = A.Fake<IImageTransformer>();
            var sut = new ImagesLibrary(imageCache, imageLoader, imageTransformer);
            await sut.GetImageAsync("01_04_2019_001103", backgroundColour: "4287f5");

            A.CallTo(() => imageTransformer.ApplyTransforms(
                    A<Image>.That.Matches(image => image.Name == "01_04_2019_001103"),
                    A<string>.That.Matches(newResolution => newResolution == null),
                    A<string>.That.Matches(backgroundColour => backgroundColour == "4287f5"),
                    A<string>.That.Matches(watermarkText => watermarkText == null),
                    A<string>.That.Matches(imageType => imageType == null)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_And_CachedImageNotAvailable_And_BackgroundColourIsSpecified_Should_IncludeBackgroundColourInCallToCacheNewlyLoadedImage()
        {
            var imageCache = A.Fake<IImagesCache>();
            A.CallTo(() => imageCache.GetImageAsync(A<ImageCacheKey>._))
                .Returns(Task.FromResult((Image)null));
            var imageLoader = A.Fake<IImageLoader>();
            A.CallTo(() => imageLoader.LoadFromLibraryAsync(A<string>._))
                .ReturnsLazily(callInfo =>
                {
                    var testImage = new Image(callInfo.GetArgument<string>("imageName"));
                    return Task.FromResult(testImage);
                });
            var imageTransformer = A.Fake<IImageTransformer>();
            A.CallTo(() => imageTransformer.ApplyTransforms(
                    A<Image>._, 
                    A<string>._, 
                    A<string>._,
                    A<string>._,
                    A<string>._))
                .ReturnsLazily(callInfo => callInfo.GetArgument<Image>("image"));
            var sut = new ImagesLibrary(imageCache, imageLoader, imageTransformer);
            await sut.GetImageAsync("01_04_2019_001103", backgroundColour: "4287f5");

            A.CallTo(() => imageCache.CacheImageAsync(
                    A<ImageCacheKey>.That.Matches(cacheKey => cacheKey == new ImageCacheKey(
                        "01_04_2019_001103", 
                        null, 
                        "4287f5",
                        null,
                        null)),
                    A<Image>.That.Matches(image => image.Name == "01_04_2019_001103")))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_And_WatermarkTextIsSpecified_Should_IncludeWatermarkTextInCallToCheckImageCache()
        {
            var imageCache = A.Fake<IImagesCache>();
            var sut = new ImagesLibrary(imageCache, A.Dummy<IImageLoader>(), A.Dummy<IImageTransformer>());
            await sut.GetImageAsync("01_04_2019_001103", watermarkText: "Some watermark text");

            A.CallTo(() => imageCache.GetImageAsync(
                    A<ImageCacheKey>.That.Matches(cacheKey => cacheKey == new ImageCacheKey(
                        "01_04_2019_001103", 
                        null, 
                        null, 
                        "Some watermark text",
                        null))))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_And_CachedImageNotAvailable_And_WatermarkTextIsSpecified_Should_CallImageTransformerToAddWatermark()
        {
            var imageCache = A.Fake<IImagesCache>();
            A.CallTo(() => imageCache.GetImageAsync(A<ImageCacheKey>._))
                .Returns(Task.FromResult((Image)null));
            var imageLoader = A.Fake<IImageLoader>();
            A.CallTo(() => imageLoader.LoadFromLibraryAsync(
                    A<string>.That.Matches(imageName => imageName == "01_04_2019_001103")))
                .ReturnsLazily(callInfo =>
                {
                    var testImage = new Image(callInfo.GetArgument<string>("imageName"));
                    return Task.FromResult(testImage);
                });
            var imageTransformer = A.Fake<IImageTransformer>();
            var sut = new ImagesLibrary(imageCache, imageLoader, imageTransformer);
            await sut.GetImageAsync("01_04_2019_001103", watermarkText: "Some watermark text");

            A.CallTo(() => imageTransformer.ApplyTransforms(
                    A<Image>.That.Matches(image => image.Name == "01_04_2019_001103"),
                    A<string>.That.Matches(newResolution => newResolution == null),
                    A<string>.That.Matches(backgroundColour => backgroundColour == null),
                    A<string>.That.Matches(watermarkText => watermarkText == "Some watermark text"),
                    A<string>.That.Matches(imageType => imageType == null)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_And_CachedImageNotAvailable_And_WatermarkTextIsSpecified_Should_IncludeWatermarkTextInCallToCacheNewlyLoadedImage()
        {
            var imageCache = A.Fake<IImagesCache>();
            A.CallTo(() => imageCache.GetImageAsync(A<ImageCacheKey>._))
                .Returns(Task.FromResult((Image)null));
            var imageLoader = A.Fake<IImageLoader>();
            A.CallTo(() => imageLoader.LoadFromLibraryAsync(A<string>._))
                .ReturnsLazily(callInfo =>
                {
                    var testImage = new Image(callInfo.GetArgument<string>("imageName"));
                    return Task.FromResult(testImage);
                });
            var imageTransformer = A.Fake<IImageTransformer>();
            A.CallTo(() => imageTransformer.ApplyTransforms(
                    A<Image>._,
                    A<string>._,
                    A<string>._,
                    A<string>._,
                    A<string>._))
                .ReturnsLazily(callInfo => callInfo.GetArgument<Image>("image"));
            var sut = new ImagesLibrary(imageCache, imageLoader, imageTransformer);
            await sut.GetImageAsync("01_04_2019_001103", watermarkText: "Some watermark text");

            A.CallTo(() => imageCache.CacheImageAsync(
                    A<ImageCacheKey>.That.Matches(cacheKey => cacheKey == new ImageCacheKey(
                        "01_04_2019_001103",
                        null,
                        null,
                        "Some watermark text",
                        null)),
                    A<Image>.That.Matches(image => image.Name == "01_04_2019_001103")))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_And_ImageTypeIsSpecified_Should_IncludeImageTypeInCallToCheckImageCache()
        {
            var imageCache = A.Fake<IImagesCache>();
            var sut = new ImagesLibrary(imageCache, A.Dummy<IImageLoader>(), A.Dummy<IImageTransformer>());
            await sut.GetImageAsync("01_04_2019_001103", imageType: "jpeg");

            A.CallTo(() => imageCache.GetImageAsync(
                    A<ImageCacheKey>.That.Matches(cacheKey => cacheKey == new ImageCacheKey(
                        "01_04_2019_001103",
                        null,
                        null,
                        null,
                        "jpeg"))))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_And_CachedImageNotAvailable_And_ImageTypeIsSpecified_Should_CallImageTransformerToRetrieveCorrectImageFormat()
        {
            var imageCache = A.Fake<IImagesCache>();
            A.CallTo(() => imageCache.GetImageAsync(A<ImageCacheKey>._))
                .Returns(Task.FromResult((Image)null));
            var imageLoader = A.Fake<IImageLoader>();
            A.CallTo(() => imageLoader.LoadFromLibraryAsync(
                    A<string>.That.Matches(imageName => imageName == "01_04_2019_001103")))
                .ReturnsLazily(callInfo =>
                {
                    var testImage = new Image(callInfo.GetArgument<string>("imageName"));
                    return Task.FromResult(testImage);
                });
            var imageTransformer = A.Fake<IImageTransformer>();
            var sut = new ImagesLibrary(imageCache, imageLoader, imageTransformer);
            await sut.GetImageAsync("01_04_2019_001103", imageType: "jpeg");

            A.CallTo(() => imageTransformer.ApplyTransforms(
                    A<Image>.That.Matches(image => image.Name == "01_04_2019_001103"),
                    A<string>.That.Matches(newResolution => newResolution == null),
                    A<string>.That.Matches(backgroundColour => backgroundColour == null),
                    A<string>.That.Matches(watermarkText => watermarkText == null),
                    A<string>.That.Matches(imageType => imageType == "jpeg")))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_And_CachedImageNotAvailable_And_ImageTypeIsSpecified_Should_IncludeImageTypeInCallToCacheNewlyLoadedImage()
        {
            var imageCache = A.Fake<IImagesCache>();
            A.CallTo(() => imageCache.GetImageAsync(A<ImageCacheKey>._))
                .Returns(Task.FromResult((Image)null));
            var imageLoader = A.Fake<IImageLoader>();
            A.CallTo(() => imageLoader.LoadFromLibraryAsync(A<string>._))
                .ReturnsLazily(callInfo =>
                {
                    var testImage = new Image(callInfo.GetArgument<string>("imageName"));
                    return Task.FromResult(testImage);
                });
            var imageTransformer = A.Fake<IImageTransformer>();
            A.CallTo(() => imageTransformer.ApplyTransforms(
                    A<Image>._,
                    A<string>._,
                    A<string>._,
                    A<string>._,
                    A<string>._))
                .ReturnsLazily(callInfo => callInfo.GetArgument<Image>("image"));
            var sut = new ImagesLibrary(imageCache, imageLoader, imageTransformer);
            await sut.GetImageAsync("01_04_2019_001103", imageType: "jpeg");

            A.CallTo(() => imageCache.CacheImageAsync(
                    A<ImageCacheKey>.That.Matches(cacheKey => cacheKey == new ImageCacheKey(
                        "01_04_2019_001103",
                        null,
                        null,
                        null,
                        "jpeg")),
                    A<Image>.That.Matches(image => image.Name == "01_04_2019_001103")))
                .MustHaveHappenedOnceExactly();
        }
    }
}
