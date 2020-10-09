using System;
using GDI = System.Drawing;
using FluentAssertions;
using ImagesApi.Model;
using ImagesApi.Model.ImageHandling;
using Xunit;

namespace ImagesApi.Tests
{
    public class ImageTransformerTests
    {
        [Fact]
        public void Should_ImplementIImageTransformer()
        {
            object sut = new ImageTransformer();
            sut.Should().BeAssignableTo<IImageTransformer>();
        }

        [Fact]
        public void WhenCreated_Should_NotThrow()
        {
            Action factory = () => new ImageTransformer();
            factory.Should().NotThrow();
        }

        [Fact]
        public void WhenCalling_ApplyTransforms_WithNullImage_Should_ThrowArgumentNullException()
        {
            var sut = new ImageTransformer();
            sut.Invoking(it => it.ApplyTransforms(null, "100x100"))
                .Should().ThrowExactly<ArgumentNullException>();
        }
        
        [Fact]
        public void WhenCalling_ApplyTransforms_WithNoGdiImageInSuppliedImageArg_Should_ThrowArgumentException()
        {
            var sut = new ImageTransformer();
            sut.Invoking(it => it.ApplyTransforms(new Image("01_04_2019_001103"), "100x100"))
                .Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void WhenCalling_ApplyTransforms_WithNoTransforms_Should_ReturnOriginalImage()
        {
            var sut = new ImageTransformer();
            var testImage = new Image("01_04_2019_001103", TestHelpers.GetTestImage());
            var result = sut.ApplyTransforms(testImage);

            result.Name.Should().BeEquivalentTo("01_04_2019_001103");
            TestHelpers.CompareImages(result.ToGdiImage(), TestHelpers.GetTestImage()).Should().Be(CompareResult.Same);
        }

        [Theory]
        [InlineData("100", 100, 100)]
        [InlineData("100x100", 100, 100)]
        [InlineData("100x120", 100, 120)]
        public void WhenCalling_ApplyTransforms_WithResolution_Should_ReturnImageWithCorrectResolution(string resolutionParameter, int expectedWidth, int expectedHeight)
        {
            var sut = new ImageTransformer();
            var testImage = new Image("01_04_2019_001103", TestHelpers.GetTestImage());
            var result = sut.ApplyTransforms(testImage, resolutionParameter);

            result.ToGdiImage().Width.Should().Be(expectedWidth);
            result.ToGdiImage().Height.Should().Be(expectedHeight);
            result.ToGdiImage().RawFormat.Should().Be(TestHelpers.GetTestImage().RawFormat);
        }

        [Theory]
        [InlineData("f0f8ff", "AliceBlue")]
        [InlineData("AliceBlue", "AliceBlue")]
        public void WhenCalling_ApplyTransforms_WithBackgroundColour_Should_ReturnImageWithCorrectBackgroundColour(string backgroundColourParameter, string expectedBackgroundColour)
        {
            var sut = new ImageTransformer();
            var testImage = new Image("01_04_2019_001103", TestHelpers.GetTestImage(makeTransparent: true));
            var result = sut.ApplyTransforms(testImage, backgroundColour: backgroundColourParameter);

            var expectedGdiImage = TestHelpers.GetTestImage(GDI.Color.FromName(expectedBackgroundColour));
            TestHelpers.CompareImages(result.ToGdiImage(), expectedGdiImage).Should().Be(CompareResult.Same);
        }

        [Fact]
        public void WhenCalling_ApplyTransforms_WithWatermarkText_Should_ReturnImageWithWatermark()
        {
            var sut = new ImageTransformer();
            var testImage = new Image("01_04_2019_001103", TestHelpers.GetTestImage(width: 1000, height: 600));
            var result = sut.ApplyTransforms(testImage, watermarkText: "Some watermark text");

            var expectedGdiImage = TestHelpers.GetTestImage(width: 1000, height: 600, watermark: "Some watermark text");
            TestHelpers.CompareImages(result.ToGdiImage(), expectedGdiImage).Should().Be(CompareResult.Same);
        }

        [Theory]
        [InlineData("png", "Png")]
        [InlineData("jpeg", "Jpeg")]
        [InlineData("Jpeg", "Jpeg")]
        [InlineData("JPEG", "Jpeg")]
        [InlineData("bmp", "Bmp")]
        [InlineData("gif", "Gif")]
        [InlineData("tiff", "Tiff")]
        public void WhenCalling_ApplyTransforms_WithImageType_Should_ReturnImageWithCorrectImageFormat(string imageTypeParameter, string expectedImageFormatName)
        {
            var sut = new ImageTransformer();
            var testImage = new Image("01_04_2019_001103", TestHelpers.GetTestImage());
            var result = sut.ApplyTransforms(testImage, imageType: imageTypeParameter);

            result.ToGdiImage().RawFormat.ToString().Should().BeEquivalentTo(expectedImageFormatName);
        }
    }
}