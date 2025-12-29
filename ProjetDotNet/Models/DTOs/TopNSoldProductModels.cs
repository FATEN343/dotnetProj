namespace ProjetDotNet.Models.DTOs;

public record TopNSoldProductModel(string ProductName, string BrandName, int TotalUnitSold);
public record TopNSoldProductsVm(DateTime StartDate, DateTime EndDate, IEnumerable<TopNSoldProductModel> TopNSoldProducts);
