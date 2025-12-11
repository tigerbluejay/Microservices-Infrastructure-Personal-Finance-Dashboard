namespace Portfolio.Web.Models.Analytics
{
    public class RefreshAnalyticsRequestDto
    {
        public string UserName { get; set; } = string.Empty;
        public List<RefreshAnalyticsAssetDto> Assets { get; set; } = new();
    }

    public class RefreshAnalyticsAssetDto
    {
        public string Symbol { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal CurrentPrice { get; set; }
    }
}