using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Helpers
{
    // The API stores uploaded images as paths on the API site ("/uploads/products/x.jpg").
    // The browser would look for those on the MVC site, so prefix them with the API address.
    public static class ImageUrlHelper
    {
        private static string _apiBaseUrl = string.Empty;

        public static void Configure(string apiBaseUrl) => _apiBaseUrl = apiBaseUrl.TrimEnd('/');

        public static string? Resolve(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return url;
            if (Uri.TryCreate(url, UriKind.Absolute, out _)) return url; // already a full URL
            return $"{_apiBaseUrl}/{url.TrimStart('/')}";
        }

        // Full URLs, with the main photo first
        public static ProductVM? Normalize(ProductVM? product)
        {
            if (product == null) return null;
            product.Images = Normalize(product.Images);
            return product;
        }

        public static List<ProductVM> Normalize(List<ProductVM> products)
        {
            foreach (var p in products) Normalize(p);
            return products;
        }

        public static List<ProductImageVM> Normalize(List<ProductImageVM> images)
        {
            foreach (var img in images) Normalize(img);
            return images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.Id).ToList();
        }

        public static ProductImageVM? Normalize(ProductImageVM? image)
        {
            if (image != null) image.ImageUrl = Resolve(image.ImageUrl) ?? string.Empty;
            return image;
        }
    }
}
