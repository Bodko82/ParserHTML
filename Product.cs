using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParserHTML
{
    public class Product
    {
        [JsonPropertyName("productVendor")]
        public string Vendor { get; set; }

        [JsonPropertyName("productName")]
        public string Title { get; set; }

        [JsonPropertyName("productPartNumber")]
        public string Number { get; set; }

        [JsonPropertyName("productDetails")]
        public string Details { get; set; }

        [JsonPropertyName("productDiscountPrice")]
        public string Price { get; set; }

        [JsonPropertyName("statusMessage")]
        public string Status { get; set; }

        [JsonPropertyName("longURL")]
        public string Url { get; set; }
    }

    public class ProductParser
    {
        private readonly HttpClient _httpClient;

        public ProductParser()
        {
            _httpClient = new HttpClient();
        }

        public async Task<ObservableCollection<Product>> ParseProducts()
        {
            var baseUrl = "https://www.swansonvitamins.com/ncat1/Vitamins+and+Supplements/ncat2/Multivitamins/ncat3/Multivitamins+with+Iron/q";
            var pageNumber = 1;
            var products = new ObservableCollection<Product>();

            while (true)
            {
                var url = $"{baseUrl}?page={pageNumber}";
                    var html = await _httpClient.GetStringAsync(url);

                    var pattern = @"adobeRecords"":(.+),""topProduct";
                    var matches = Regex.Matches(html, pattern);

                    if (matches.Count > 0)
                    {
                        var json = matches[0].Groups[1].Value;
                        var productList = JsonSerializer.Deserialize<List<Product>>(json);
                    if (productList.Count <=0) { break; }
                        productList.ForEach(p =>
                        {
                            products.Add(p);
                        });
                    }
                    pageNumber++;
            }

            return products;
        }

        private async Task<bool> UrlExists(string url)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Head, url);
                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}
