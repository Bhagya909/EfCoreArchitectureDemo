using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Models
{
    public class ProductQueryParameters
    {
        private const int MaxPageSize = 50;

        private int _pageNumber = 1;

        public int PageNumber
        {
            get => _pageNumber;

            set => _pageNumber =
                value <= 0
                    ? 1
                    : value;
        }

        private int _pageSize = 10;

        public int PageSize
        {
            get => _pageSize;

            set
            {
                if (value <= 0)
                {
                    _pageSize = 10;
                }
                else
                {
                    _pageSize =
                        value > MaxPageSize
                            ? MaxPageSize
                            : value;
                }
            }
        }

        public string? SearchTerm { get; set; }

        public string? SortBy { get; set; }
    }
}
