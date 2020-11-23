using LibraryData.Models;
using System;
using System.Collections.Generic;
using System.Text;
// The purpose of this interface is to define the series of methods 
// that'll be required for any service that implements this interface
namespace LibraryData
{
    public interface ILibraryAsset
    {
        IEnumerable<LibraryAsset> GetAll();
        LibraryAsset GetById(int id);
        void Add(LibraryAsset newAsset);
        string GetAuthorOrDirector(int id);
        string GetDeweyIndex(int id);
        string GetType(int id);
        string GetTitle(int id);
        string GetIsbn(int id);
        LibraryBranch GetCurrentLocation(int id);
    }
}
