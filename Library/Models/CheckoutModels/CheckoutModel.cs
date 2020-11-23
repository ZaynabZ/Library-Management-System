using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models.CheckoutModels
{
    public class CheckoutModel
    {
        public string LibraryCardId { get; set; }
        public string Title { get; set; }
        public int AssetId { get; set; }
        public string ImageUrl { get; set; }

        // Count of how many patrons placed a hold on the asset
        public int HoldCount { get; set; }
        public bool IsCheckedOut { get; set; }
    }
}
