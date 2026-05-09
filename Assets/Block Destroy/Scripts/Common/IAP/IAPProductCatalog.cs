using UnityEngine;

/// <summary>
/// Product configuration for IAP system
/// </summary>
[System.Serializable]
public class IAPProduct
{
    public string productId;
    public string productName;
    public ProductType productType;
    public int rewardAmount;
    public float priceUSD;
}

public enum ProductType
{
    Consumable,      // Coins, Gems - can be purchased multiple times
    NonConsumable,   // Remove Ads, Unlock Features - one-time purchase
    Subscription     // Premium membership - recurring
}

/// <summary>
/// IAP Product catalog for Block Destroy
/// </summary>
public static class IAPProductCatalog
{
    // Product IDs - ДОЛЖНЫ СОВПАДАТЬ с Google Play Console
    public const string GEMS_SMALL = "gem__30";
    public const string GEMS_MEDIUM = "gem__80";
    public const string GEMS_LARGE = "gem_170";
    public const string GEMS_HUGE = "gem_360";
    public const string GEMS_MEGA = "gem_950";
    public const string GEMS_ULTIMATE = "gem_2000";

    // Product definitions
    public static readonly IAPProduct[] Products = new IAPProduct[]
    {
        // Gems
        new IAPProduct
        {
            productId = GEMS_SMALL,
            productName = "30 Gems",
            productType = ProductType.Consumable,
            rewardAmount = 30,
            priceUSD = 0.99f
        },
        new IAPProduct
        {
            productId = GEMS_MEDIUM,
            productName = "80 Gems",
            productType = ProductType.Consumable,
            rewardAmount = 80,
            priceUSD = 1.99f
        },
        new IAPProduct
        {
            productId = GEMS_LARGE,
            productName = "170 Gems",
            productType = ProductType.Consumable,
            rewardAmount = 170,
            priceUSD = 3.99f
        },
        new IAPProduct
        {
            productId = GEMS_HUGE,
            productName = "360 Gems",
            productType = ProductType.Consumable,
            rewardAmount = 360,
            priceUSD = 7.99f
        },
        new IAPProduct
        {
            productId = GEMS_MEGA,
            productName = "950 Gems",
            productType = ProductType.Consumable,
            rewardAmount = 950,
            priceUSD = 19.99f
        },
        new IAPProduct
        {
            productId = GEMS_ULTIMATE,
            productName = "2000 Gems",
            productType = ProductType.Consumable,
            rewardAmount = 2000,
            priceUSD = 39.99f
        }
    };

    /// <summary>
    /// Get product by ID
    /// </summary>
    public static IAPProduct GetProduct(string productId)
    {
        foreach (var product in Products)
        {
            if (product.productId == productId)
                return product;
        }
        return null;
    }
}