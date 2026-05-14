using Application.DTOs.Products;

namespace Application.Validators;

public static class ProductValidator
{
    public static void ValidateCreateProduct(
        CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new Exception(
                "Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.SKU))
        {
            throw new Exception(
                "SKU is required.");
        }

        if (dto.BasePrice <= 0)
        {
            throw new Exception(
                "Base price must be greater than zero.");
        }
    }
}
