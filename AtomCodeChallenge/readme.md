# Images API

## Dependencies

1. This api has been implemented using Redis for distributed caching - endpoint and instance name should be specified in the "RedisCache" section of appsettings.json.

	Redis can be installed locally if required - see https://www.c-sharpcorner.com/article/installing-redis-cache-on-windows/

	Alternatively other distributed cache implementations can be substituted by adding the appropriate Nuget package and making the necessary changes to Startup.cs - see https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-3.1

2. Physical image library folder - this is not included in the code base and must be provided seperately. This implementation expects the library folder to be available in a local or shared operating system folder - the location should be be specified in the "LocalFileSystemImageLibrary" section of appsettings.json.

## Caching Overview

The distributed cache is shared among all API instances, so in a load-balanced scenario where multiple API instances are running once an instance has succesfully processed and cached a response all other instances will retrieve and serve the cached response rather than performing full re-processing of the request for the same image parameters.

The api is stateless so can be scalled-out by adding new instances to meet demand as required. The redis cache can also be scaled horizontally by adding more nodes if needed.

## API Usage

The api can be run within the Visual Studio environment or hosted seperately, the API itself is configured to run in Kestrel on https://localhost:5000.

* Basic API usage - make a get request to https://localhost:5000/api/images/{imageName} where {imageName} is the name of the image being requested minus any file extensions, e.g. to obtain 01_04_2019_001103.png from the library request GET https://localhost:5000/api/images/01_04_2019_001103

* Further transforms can be applied to the request by adding query parameter as follows:

	1. Specify image resolution - Add "?resolution={v}" or "?resolution={w}x{h}" to the request url where {v}, {w} & {h} are dimensions measured in pixels, e.g. GET https://localhost:5000/api/images/01_04_2019_001103?resolution=300 or https://localhost:5000/api/images/01_04_2019_001103?resolution=300x600. (Note: If only one resolution value is specified both the width and height are set to this amount, so resolution=300 is equivalent to resolution=300x300)

	2. Specify background colour - Add "?backgroundColour={colour spec}" to the request url where {colour spec} is either the name (most standard css colour names are supported) or an RGB hex string, e.g. https://localhost:5000/api/images/01_04_2019_001103?backgroundColour=Blue and https://localhost:5000/api/images/01_04_2019_001103?backgroundColour=0000ff are equivalent

	3. Specify watermark text - Add "?watermark={watermark spec}" to the request url where {watermark spec} is the text to add. The text for this parameter should be url escaped to avoid any potential problems with reserved characters. e.g. https://localhost:5000/api/images/01_04_2019_001103?watermark=Copyright%20%C2%A9%202020 adds the text "Copyright &copy; 2020" to the upper left hand corner of the returned image

	4. Specify image content-type - Add "?imageType={file type}" to the request url where {file type} is the file format of the image required. The values "bmp", "gif", "jpeg", "png" & "tiff" are supported. e.g. https://localhost:5000/api/images/01_04_2019_001103?imageType=bmp to request the image as content-type "image/bmp"

		If image content-type is not specified the image is returned using the content-type of the file in the source image library folder.

With the exception of the {imageName} part of the URL all of the above query parameters are optional and can be specified or omitted as required to achieve different combinations of output:

### Examples 

https://localhost:5000/api/images/01_04_2019_001103?resolution=300x600&backgroundColour=Blue - returns image identified by "01_04_2019_001103" with size set to 300x600 pixels and a background colour of Blue

https://localhost:5000/api/images/01_04_2019_001103?backgroundColour=Red&imageType=gif - returns image identified by "01_04_2019_001103" with a background colour of Red and content-type of "image/gif"

Parameters can be supplied in any order: 
https://localhost:5000/api/images/01_04_2019_001103?watermark=Copyright%20%C2%A9%202020&resolution=500x800 and https://localhost:5000/api/images/01_04_2019_001103?resolution=500x800&watermark=Copyright%20%C2%A9%202020 are equivalent.
