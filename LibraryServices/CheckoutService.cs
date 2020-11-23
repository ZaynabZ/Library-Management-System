using LibraryData;
using LibraryData.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibraryServices
{
    public class CheckoutService : ICheckout
    {
        private LibraryContext _context;
        public CheckoutService(LibraryContext context)
        {
            _context = context;
        }
        public void Add(Checkout newCheckout)
        {
            _context.Add(newCheckout);
            _context.SaveChanges();
        }

        public void CheckInItem(int assetId)
        {
            var now = DateTime.Now;
            var theAsset = _context.LibraryAssets
                .FirstOrDefault(a => a.Id == assetId);

            // Remove any existing checkouts on the asset
            RemoveExistingCheckouts(assetId);

            // Close any checkout history
            CloseExistingCheckoutHistory(assetId, now);

            // Look for existing holds on the asset
            var currentHolds = _context.Holds
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .Where(h => h.LibraryAsset.Id == assetId);

            // If there are any holds, checkout to the library card with the oldest hold placed
            if (currentHolds.Any())
            {
                // Any returns a boolean for whether currentHolds has any elements or no
                CheckoutToEarliestHolds(assetId, currentHolds);
                return;
            }
            // Otherwise, just update the asset's status to available
            UpdateAssetStatus(assetId, "Available");

            _context.SaveChanges();
        }

        public void CheckOutItem(int assetId, int libraryCardId)
        {
            var now = DateTime.Now;
            // Check whether or not the item is already checked out
            if (IsCheckedOut(assetId))
            {
                return;
                // Add logic here to handle feedback to the user
                // Inform them that the item is altready checked out
            }

            var theAsset = _context.LibraryAssets
                .FirstOrDefault(a => a.Id == assetId);

           UpdateAssetStatus(assetId, "Checked Out");

            // Get the library card you want to check this asset out to
            var card = _context.LibraryCards
                .Include(c => c.Checkouts)
                .FirstOrDefault(c => c.Id == libraryCardId);

            var checkout = new Checkout
            {
                LibraryAsset = theAsset,
                LibraryCard = card,
                Since = now,
                Until = GetDefaultCheckOutTime(now)
            };

            _context.Add(checkout);

            var checkoutHistory = new CheckoutHistory
            {
                LibraryAsset = theAsset,
                LibraryCard = card,
                ChekedOut = now
            };

            _context.Add(checkoutHistory);
            _context.SaveChanges();
        }

        private DateTime GetDefaultCheckOutTime(DateTime now)
        {
            return now.AddDays(30);
        }

        public bool IsCheckedOut(int assetId)
        {
            return _context.Checkouts
                .Where(ch => ch.LibraryAsset.Id == assetId)
                .Any();
                
        }

        public void CheckoutToEarliestHolds(int assetId, IQueryable<Hold> currentHolds)
        {
            var earliestHold = currentHolds
                .OrderBy(holds => holds.HoldPlaced)
                .FirstOrDefault();

            // Get the id of the library card that made the earliest hold
            var card = earliestHold.LibraryCard;

            _context.Remove(earliestHold);
            _context.SaveChanges();
            CheckOutItem(assetId, card.Id);
        }

        public IEnumerable<Checkout> GetAll()
        {
            return _context.Checkouts;
        }

        public Checkout GetById(int checkoutId)
        {
            return GetAll()
                .FirstOrDefault(checkout => checkout.Id == checkoutId);
        }

        public IEnumerable<CheckoutHistory> GetCheckoutHistory(int id)
        {
            return _context.CheckoutHistories
                .Include(a => a.LibraryAsset)
                .Include(c => c.LibraryCard)
                .Where(asset => asset.LibraryAsset.Id == id);
        }

        public string GetCurrentHoldPatronName(int holdId)
        {
            var hold = _context.Holds
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .FirstOrDefault(h => h.Id == holdId);

            // ? here is a null conditional operator 
            //      in case there was no hold with the id in the parameter
            // It will provide a null reference exception
            var cardId = hold?.LibraryCard.Id;
            var patron = _context.Patrons
                .Include(p => p.LibraryCard)
                .FirstOrDefault(p => p.LibraryCard.Id == cardId);

            return patron?.FirstName + " " + patron?.LastName;
        }

        public DateTime GetCurrentHoldPlaced(int holdId)
        {
            return _context.Holds
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryAsset)
                .FirstOrDefault(h => h.Id == holdId)
                .HoldPlaced;
        }

        public IEnumerable<Hold> GetCurrentHolds(int id)
        {
            return _context.Holds
                .Include(h => h.LibraryAsset)
                .Where(hold => hold.LibraryAsset.Id == id);
        }

        public Checkout GetLatestCheckout(int assetId)
        {
            return _context.Checkouts
                .Where(checkout => checkout.LibraryAsset.Id == assetId)
                .OrderByDescending(c => c.Since)
                .FirstOrDefault();
        }

        public void MarkFound(int assetId)
        {
            var now = DateTime.Now;
           
            UpdateAssetStatus(assetId, "Available");
            RemoveExistingCheckouts(assetId);
            CloseExistingCheckoutHistory(assetId, now);
       
            _context.SaveChanges();
        }

        private void UpdateAssetStatus(int assetId, string newStatus)
        {
            var theAsset = _context.LibraryAssets
                .FirstOrDefault(a => a.Id == assetId);
            _context.Update(theAsset);

            // Mark the asset as available for checking out the moment it is found
            // Or as checked out when it is ...
            theAsset.Status = _context.Statuses
                .FirstOrDefault(s => s.Name == newStatus);

        }

        private void CloseExistingCheckoutHistory(int assetId, DateTime now)
        {
            // Fetch and clear any checkout history
            var history = _context.CheckoutHistories
                .FirstOrDefault(ch => ch.LibraryAsset.Id == assetId
                    && ch.ChekedIn == null);

            if (history != null)
            {
                _context.Update(history);

                // Mark the asset that was found as checked in the moment it is found
                history.ChekedIn = now;
            }
        }

        private void RemoveExistingCheckouts(int assetId)
        {
            // Rmove any existing checkouts on the asset
            var checkout = _context.Checkouts
                .FirstOrDefault(c => c.LibraryAsset.Id == assetId);

            if (checkout != null)
            {
                // Removing item from DB
                _context.Remove(checkout);
            }
        }

        public void MarkLost(int assetId)
        {
            UpdateAssetStatus(assetId, "Lost");

            _context.SaveChanges();
        }

        public void PlaceHold(int assetId, int libraryCardId)
        {
            var now = DateTime.Now;
            var theAsset = _context.LibraryAssets
                .Include(asset => asset.Status)
                .FirstOrDefault(asset => asset.Id == assetId);

            var theCard = _context.LibraryCards
                .FirstOrDefault(card => card.Id == libraryCardId);

            if (theAsset.Status.Name == "Available")
            {
                UpdateAssetStatus(assetId, "On Hold");
            }

            var hold = new Hold
            {
                LibraryAsset = theAsset,
                LibraryCard = theCard,
                HoldPlaced = now
            };
            _context.Add(hold);
            _context.SaveChanges();
        }

        public string GetCurrentCheckoutPatron(int assetId)
        {
            var checkout = GetCheckoutByAssetId(assetId);
            if (checkout == null)
            {
                // Configure what we want to dispaly in the Controller
                return "";
            };

            var cardId = checkout.LibraryCard.Id;
            var patron = _context.Patrons
                .Include(p => p.LibraryCard)
                .FirstOrDefault(p => p.LibraryCard.Id == cardId);

            return patron.FirstName + " " + patron.LastName;
        }

        private Checkout GetCheckoutByAssetId(int assetId)
        {
            return _context.Checkouts
                .Include(co => co.LibraryAsset)
                .Include(co => co.LibraryCard)
                .FirstOrDefault(co => co.LibraryAsset.Id == assetId);
        }

    }
}
