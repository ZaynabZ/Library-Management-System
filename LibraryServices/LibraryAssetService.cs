using LibraryData;
using LibraryData.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryServices
{
    public class LibraryAssetService : ILibraryAsset
    {
        private LibraryContext _context;
        public LibraryAssetService(LibraryContext context)
        {
            _context = context;
        }
        public void Add(LibraryAsset newAsset)
        {
            _context.Add(newAsset);

            // This method commits changes to db
            _context.SaveChanges();
        }

        public IEnumerable<LibraryAsset> GetAll()
        {
           return _context.LibraryAssets
                .Include(asset => asset.Status)
                .Include(asset => asset.Location);
        }

        public LibraryAsset GetById(int assetId)
        {
            return _context.LibraryAssets
                .Include(asset => asset.Status)
                .Include(asset => asset.Location)
                .FirstOrDefault(asset => asset.Id == assetId);
        }

        public LibraryBranch GetCurrentLocation(int id)
        {
            return _context.LibraryAssets
                .FirstOrDefault(asset => asset.Id == id)
                .Location;
            // Refactor with the following expression
            // return GetById(id).Location;
        }

        public string GetDeweyIndex(int id)
        {
            // It's the case of a Discriminator!!
            if (_context.Books.Any(book => book.Id == id))
            {
                return _context.Books
                    .FirstOrDefault(book => book.Id == id)
                    .DeweyIndex;
            }
            // Also to refactor what's above use the OfType<>:
            // var isBook = _context.LibraryAssets.OfType<Book>().Where(a =>a.Id == id).Any();
            else return "";
            
        }

        public string GetIsbn(int id)
        {
            if (_context.LibraryAssets.Any(book => book.Id == id))
            {
                return _context.Books
                    .FirstOrDefault(book => book.Id == id)
                    .ISBN;
            }
            else return "";
        }

        public string GetTitle(int id)
        {
            return _context.LibraryAssets
                .FirstOrDefault(book => book.Id == id)
                .Title;
        }

        public string GetAuthorOrDirector(int id)
        {
            var isBook = _context.LibraryAssets.OfType<Book>()
                .Where(asset => asset.Id == id)
                .Any();

            var isVideo = _context.LibraryAssets.OfType<Video>()
                .Where(asset => asset.Id == id)
                .Any();

            return isBook ?
                _context.Books.FirstOrDefault(book => book.Id == id).Author :
                _context.Videos.FirstOrDefault(video => video.Id == id).Director
                ?? "Unknown";
        }



        public string GetType(int id)
        {
            var book = _context.LibraryAssets.OfType<Book>()
                .Where(b => b.Id == id);

            return book.Any() ? "Book" : "Video";

        }
    }
}
