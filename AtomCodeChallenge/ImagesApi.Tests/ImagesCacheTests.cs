using System;
using System.Security.Cryptography;
using GDI = System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using ImagesApi.Model;
using ImagesApi.Model.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Xunit;

namespace ImagesApi.Tests
{
    public class ImagesCacheTests
    {
        [Fact]
        public void Should_Implement_IImagesCache()
        {
            object sut = new ImagesCache(A.Dummy<IDistributedCache>());
            sut.Should().BeAssignableTo<IImagesCache>();
        }

        [Fact]
        public void WhenCreated_Should_NotThrow()
        {
            Action factory = () => new ImagesCache(A.Dummy<IDistributedCache>());
            factory.Should().NotThrow();
        }

        [Fact]
        public void WhenCalling_GetImageAsync_WithNullParameter_Should_ThrowArgumentNullException()
        {
            var sut = new ImagesCache(A.Dummy<IDistributedCache>());
            sut.Awaiting(ic => ic.GetImageAsync(null))
                .Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_Should_CallDistributedCacheToRetrieveImage()
        {
            var distributedCache = A.Fake<IDistributedCache>();
            var sut = new ImagesCache(distributedCache);
            await sut.GetImageAsync(new ImageCacheKey("01_04_2019_001103"));

            A.CallTo(() => distributedCache.GetAsync(
                    A<string>.That.Matches(key => key == "01_04_2019_001103"),
                        A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_Should_ReturnCachedImageIfFound()
        {
            var distributedCache = A.Fake<IDistributedCache>();
            var testGdiImage = TestHelpers.GetTestImage();
            A.CallTo(() => distributedCache.GetAsync(A<string>._, A<CancellationToken>._))
                .ReturnsLazily(callinfo =>
                {
                    var testImage = new Image("01_04_2019_001103", testGdiImage);
                    var bytes = testImage.ToBytes();
                    return Task.FromResult(bytes);
                });
            var sut = new ImagesCache(distributedCache);
            var result = await sut.GetImageAsync(new ImageCacheKey("01_04_2019_001103"));

            result.Name.Should().BeEquivalentTo("01_04_2019_001103");
            TestHelpers.CompareImages(testGdiImage, result.ToGdiImage()).Should().BeEquivalentTo(CompareResult.Same);
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_Should_ReturnNullIfNotFound()
        {
            var distributedCache = A.Fake<IDistributedCache>();
            A.CallTo(() => distributedCache.GetAsync(A<string>._, A<CancellationToken>._))
                .Returns(Task.FromResult((byte[])null));
            var sut = new ImagesCache(distributedCache);
            var result = await sut.GetImageAsync(new ImageCacheKey("01_04_2019_001103"));

            result.Should().BeNull();
        }

        [Fact]
        public void WhenCalling_CacheImageAsync_WithNullKeyParameter_Should_ThrowArgumentNullException()
        {
            var distributedCache = A.Dummy<IDistributedCache>();
            var sut = new ImagesCache(distributedCache);

            sut.Awaiting(ic => ic.CacheImageAsync(null, new Image()))
                .Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void WhenCalling_CacheImageAsync_WithNullImageParameter_Should_ThrowArgumentNullException()
        {
            var distributedCache = A.Dummy<IDistributedCache>();
            var sut = new ImagesCache(distributedCache);

            sut.Awaiting(ic => ic.CacheImageAsync(new ImageCacheKey("01_04_2019_001103"), null))
                .Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public async Task WhenCalling_CacheImageAsync_Should_CallDistributedCacheToSetImage()
        {
            var distributedCache = A.Fake<IDistributedCache>();
            var sut = new ImagesCache(distributedCache);
            var testGdiImage = TestHelpers.GetTestImage();
            await sut.CacheImageAsync(new ImageCacheKey("01_04_2019_001103"), new Image("01_04_2019_001103", testGdiImage));

            A.CallTo(() => distributedCache.SetAsync(
                    A<string>.That.Matches(key => key == "01_04_2019_001103"),
                    A<byte[]>.That.Matches(value => IsMatchingImageBytes(value, "01_04_2019_001103", testGdiImage)),
                    A<DistributedCacheEntryOptions>._,
                    A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData("100", "01_04_2019_001103|res=100x100")]
        [InlineData("100x100", "01_04_2019_001103|res=100x100")]
        [InlineData("100x120", "01_04_2019_001103|res=100x120")]
        public async Task WhenCalling_GetImageAsync_Should_CallDistributedCacheWithCorrectResolutionInKey(string resolutionString, string expectedCacheKey)
        {
            var distributedCache = A.Fake<IDistributedCache>();
            var sut = new ImagesCache(distributedCache);
            await sut.GetImageAsync(new ImageCacheKey("01_04_2019_001103", resolutionString));

            A.CallTo(() => distributedCache.GetAsync(
                    A<string>.That.Matches(key => key == expectedCacheKey),
                    A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData("100", "01_04_2019_001103|res=100x100")]
        [InlineData("100x100", "01_04_2019_001103|res=100x100")]
        [InlineData("100x120", "01_04_2019_001103|res=100x120")]
        public async Task WhenCalling_CacheImageAsync_Should_CallDistributedCacheWithCorrectResolutionInKey(string resolutionString, string expectedCacheKey)
        {
            var distributedCache = A.Fake<IDistributedCache>();
            var sut = new ImagesCache(distributedCache);
            var testGdiImage = TestHelpers.GetTestImage();
            await sut.CacheImageAsync(
                new ImageCacheKey("01_04_2019_001103", resolutionString),
                new Image("01_04_2019_001103", testGdiImage));

            A.CallTo(() => distributedCache.SetAsync(
                    A<string>.That.Matches(key => key == expectedCacheKey),
                    A<byte[]>.That.Matches(value => IsMatchingImageBytes(value, "01_04_2019_001103", testGdiImage)),
                    A<DistributedCacheEntryOptions>._,
                    A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData("f0f8ff", "01_04_2019_001103|bgr=#fff0f8ff")]
        [InlineData("AliceBlue", "01_04_2019_001103|bgr=#fff0f8ff")]
        public async Task WhenCalling_GetImageAsync_Should_CallDistributedCacheWithBackgroundColourInKey(string backgroundColour, string expectedCacheKey)
        {
            var distributedCache = A.Fake<IDistributedCache>();
            var sut = new ImagesCache(distributedCache);
            await sut.GetImageAsync(new ImageCacheKey("01_04_2019_001103", null, backgroundColour));

            A.CallTo(() => distributedCache.GetAsync(
                    A<string>.That.Matches(key => key == expectedCacheKey),
                    A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData("4682b4", "01_04_2019_001103|bgr=#ff4682b4")]
        [InlineData("SteelBlue", "01_04_2019_001103|bgr=#ff4682b4")]
        public async Task WhenCalling_CacheImageAsync_Should_CallDistributedCacheWithCorrectBackgroundColourInKey(string backgroundColour, string expectedCacheKey)
        {
            var distributedCache = A.Fake<IDistributedCache>();
            var sut = new ImagesCache(distributedCache);
            var testGdiImage = TestHelpers.GetTestImage();
            await sut.CacheImageAsync(
                new ImageCacheKey("01_04_2019_001103", null, backgroundColour),
                new Image("01_04_2019_001103", testGdiImage));

            A.CallTo(() => distributedCache.SetAsync(
                    A<string>.That.Matches(key => key == expectedCacheKey),
                    A<byte[]>.That.Matches(value => IsMatchingImageBytes(value, "01_04_2019_001103", testGdiImage)),
                    A<DistributedCacheEntryOptions>._,
                    A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCalling_GetImageAsync_Should_CallDistributedCacheWithWatermarkHashInKey()
        {
            var distributedCache = A.Fake<IDistributedCache>();
            var sut = new ImagesCache(distributedCache);
            await sut.GetImageAsync(new ImageCacheKey("01_04_2019_001103", null, null, "Some watermark text"));

            A.CallTo(() => distributedCache.GetAsync(
                    A<string>.That.Matches(key => key == "01_04_2019_001103|wm=NZyQwos0iWRAmdBFA9T6q+MZn9B0fgauzH3MJpNplsc="),
                    A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenCalling_CacheImageAsync_Should_CallDistributedCacheWithWatermarkHashInKey()
        {
            var distributedCache = A.Fake<IDistributedCache>();
            var sut = new ImagesCache(distributedCache);
            var testGdiImage = TestHelpers.GetTestImage();
            await sut.CacheImageAsync(
                new ImageCacheKey("01_04_2019_001103", null, null, "Some watermark text"),
                new Image("01_04_2019_001103", testGdiImage));

            A.CallTo(() => distributedCache.SetAsync(
                    A<string>.That.Matches(key => key == "01_04_2019_001103|wm=NZyQwos0iWRAmdBFA9T6q+MZn9B0fgauzH3MJpNplsc="),
                    A<byte[]>.That.Matches(value => IsMatchingImageBytes(value, "01_04_2019_001103", testGdiImage)),
                    A<DistributedCacheEntryOptions>._,
                    A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData("png", "01_04_2019_001103|type=png")]
        [InlineData("jpeg", "01_04_2019_001103|type=jpeg")]
        [InlineData("JPEG", "01_04_2019_001103|type=jpeg")]
        [InlineData("Jpeg", "01_04_2019_001103|type=jpeg")]
        [InlineData("bmp", "01_04_2019_001103|type=bmp")]
        public async Task WhenCalling_GetImageAsync_Should_CallDistributedCacheWithCorrectImageTypeInKey(string imageType, string expectedCacheKey)
        {
            var distributedCache = A.Fake<IDistributedCache>();
            var sut = new ImagesCache(distributedCache);
            await sut.GetImageAsync(new ImageCacheKey("01_04_2019_001103", null, null, null, imageType));

            A.CallTo(() => distributedCache.GetAsync(
                    A<string>.That.Matches(key => key == expectedCacheKey),
                    A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData("png", "01_04_2019_001103|type=png")]
        [InlineData("jpeg", "01_04_2019_001103|type=jpeg")]
        [InlineData("JPEG", "01_04_2019_001103|type=jpeg")]
        [InlineData("Jpeg", "01_04_2019_001103|type=jpeg")]
        [InlineData("bmp", "01_04_2019_001103|type=bmp")]
        public async Task WhenCalling_CacheImageAsync_Should_CallDistributedCacheWithCorrectImageTypeInKey(string imageType, string expectedCacheKey)
        {
            var distributedCache = A.Fake<IDistributedCache>();
            var sut = new ImagesCache(distributedCache);
            var testGdiImage = TestHelpers.GetTestImage();
            await sut.CacheImageAsync(
                new ImageCacheKey("01_04_2019_001103", null, null, null, imageType),
                new Image("01_04_2019_001103", testGdiImage));

            A.CallTo(() => distributedCache.SetAsync(
                    A<string>.That.Matches(key => key == expectedCacheKey),
                    A<byte[]>.That.Matches(value => IsMatchingImageBytes(value, "01_04_2019_001103", testGdiImage)),
                    A<DistributedCacheEntryOptions>._,
                    A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        private bool IsMatchingImageBytes(byte[] value, string expectedImageName, GDI.Image expectedGdiImage)
        {
            var actualImage = Image.FromBytes(value);
            return actualImage.Name == expectedImageName &&
                   TestHelpers.CompareImages(actualImage.ToGdiImage(), expectedGdiImage) == CompareResult.Same;
        }

    }
}
