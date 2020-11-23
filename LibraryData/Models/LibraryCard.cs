using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryData.Models
{
    public class LibraryCard
    {
        public int Id { get; set; }
        public decimal Fees { get; set; } // Overdue fees
        public DateTime Created { get; set; }
        public virtual IEnumerable<Checkout> Checkouts { get; set; }
    }
}
