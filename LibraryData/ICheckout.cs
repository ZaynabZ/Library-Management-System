using LibraryData.Models;
using System;
using System.Collections.Generic;

namespace LibraryData
{
    public interface ICheckout
    {
      
        
        void Add(Checkout newCheckout);

        IEnumerable<Checkout> GetAll();
        IEnumerable<CheckoutHistory> GetCheckoutHistory(int id);
        IEnumerable<Hold> GetCurrentHolds(int id);

        Checkout GetById(int id);
        Checkout GetLatestCheckout(int assetId);

        void CheckOutItem(int assetId, int libraryCardId);
        void CheckInItem(int assetId);
        void PlaceHold(int assetId, int libraryCardId);
        void MarkLost(int assetId);
        void MarkFound(int assetId);

        string GetCurrentHoldPatronName(int id);
        string GetCurrentCheckoutPatron(int assetId);

        DateTime GetCurrentHoldPlaced(int id);
        bool IsCheckedOut(int assetId); 
      
       
    }



}
