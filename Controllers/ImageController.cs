using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ImageResizer.Controllers
{
    public class ImageController : Controller
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        public ImageController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

        }
        public async Task<IActionResult> Index(string url, int width, int height)
        {
            string root = _httpContextAccessor.HttpContext.Request.Scheme + "://" 
                + _httpContextAccessor.HttpContext.Request.Host.Value;
            string path = root + "/" + url;
            string empty = "images/empty.jpg";
            using (Image sourceImage = await this.LoadImageFromUrl(path))
            {
                if (sourceImage != null)
                {
                    return this.ImageProcess(sourceImage, width, height);
                }
                else
                {
                    path = root + "/" + empty;
                    Image emptyImage = await this.LoadImageFromUrl(path);
                    return this.ImageProcess(emptyImage, width, height);
                }
            }
        }

        private IActionResult ImageProcess(Image sourceImage, int width, int height)
        {
            try
            {
                using (Image rezisedImage = this.CropImage(sourceImage,
            width, height))
                {
                    Stream outputStream = new MemoryStream();
                    rezisedImage.Save(outputStream, ImageFormat.Jpeg);
                    outputStream.Seek(0, SeekOrigin.Begin);
                    return this.File(outputStream, "image/png");
                }
            }
            catch (Exception)
            {
                return this.NotFound();
            }

        }
        private async Task<Image> LoadImageFromUrl(string url)
        {
            Image image = null;

            try
            {
                using (HttpClient httpClient = new HttpClient())
                using (HttpResponseMessage response = await httpClient.GetAsync(url))
                using (Stream inputStream = await response.Content.ReadAsStreamAsync())
                using (Bitmap temp = new Bitmap(inputStream))
                    image = new Bitmap(temp);
            }

            catch
            {
                // error
            }

            return image;
        }
        private Image CropImage(Image image, int width, int height)
        {
            Image resizedImage = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(resizedImage))
                g.DrawImage(
                  image,
                  new Rectangle(0, 0, width, height),
                  new Rectangle(0, 0, image.Width, image.Height),
                  GraphicsUnit.Pixel
                );

            return resizedImage;
        }
    }
}
