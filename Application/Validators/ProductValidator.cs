using Application.DTOs.Products;

namespace Application.Validators;

public static class ProductValidator
{
    public static void ValidateCreateProduct(CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Product name is required.");

        if (string.IsNullOrWhiteSpace(dto.SKU))
            throw new ArgumentException("SKU is required.");

        if (dto.BasePrice <= 0)
            throw new ArgumentException(
                "Base price must be greater than zero.");
    }

    public static void ValidateUpdateProduct(UpdateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Product name is required.");

        if (string.IsNullOrWhiteSpace(dto.SKU))
            throw new ArgumentException("SKU is required.");

        if (dto.BasePrice <= 0)
            throw new ArgumentException(
                "Base price must be greater than zero.");
    }
}