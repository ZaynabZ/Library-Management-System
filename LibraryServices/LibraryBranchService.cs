using LibraryData;
using LibraryData.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibraryServices
{
    public class LibraryBranchService : ILibraryBranch
    {
        private LibraryContext _context;
        public LibraryBranchService(LibraryContext context)
        {
            _context = context;
        }
        public void Add(LibraryBranch newBranch)
        {
            _context.Add(newBranch);
            _context.SaveChanges();
        }

        public IEnumerable<Patron> GetPatrons(int branchId)
        {
            return Get(branchId).Patrons;
        }

        public LibraryBranch Get(int branchId)
        {
            return GetAll()
                .FirstOrDefault(lb => lb.Id == branchId);
        }

        public IEnumerable<LibraryBranch> GetAll()
        {
            return _context.LibraryBranches
                .Include(lb => lb.Patrons)
                .Include(lb => lb.LibraryAssets);
        }

        public IEnumerable<LibraryAsset> GetAssets(int branchId)
        {
            return Get(branchId).LibraryAssets;
        }

        public IEnumerable<string> GetBranchHours(int branchId)
        {
            var hours = _context.BranchHours
                .Where(bh => bh.LibraryBranch.Id == branchId);
            return DataHelpers.HumanizeBizHours(hours);
        }

        public bool IsBranchOpen(int branchId)
        {
            var currentTimeHour = DateTime.Now.Hour;
            var currentDayOfWeek = (int)DateTime.Now.DayOfWeek + 1;
            var hours = _context.BranchHours
                .Where(bh => bh.LibraryBranch.Id == branchId);
            var daysHours = hours.FirstOrDefault(h => h.DayOfWeek == currentDayOfWeek);
            var isOpen = currentTimeHour < daysHours.CloseTime && currentTimeHour > daysHours.OpenTime;

            return isOpen;
        }
    }
}
