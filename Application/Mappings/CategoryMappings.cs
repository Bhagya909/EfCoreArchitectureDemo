using Application.DTOs.Categories;
using Domain.Entities.Catalog;

namespace Application.Mappings
{
    public static class CategoryMappings
    {
        public static CategoryResponseDto ToResponseDto(
            this Category category)
        {
            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                CreatedAt = category.CreatedAt
            };
        }
    }
}