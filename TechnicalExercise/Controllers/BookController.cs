using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechnicalExercise.Models;

namespace TechnicalExercise.Controllers
{
    public class BookController : Controller
    {
        private readonly BookRepository bookRepo;

        public BookController() // constructor
        {
            bookRepo = new BookRepository("server=localhost;user=root;database=groupfour_technical;port=3306;"); 
        }
    
        public ActionResult Index() // GET: BookController
        {
            var books = bookRepo.GetBooks();
            foreach(var book in books)
            {
                Console.WriteLine(book.BookName);
            }
            return View(books); // Pass the list of books to the view
        }

        public ActionResult Search(string searchString)
        {
            var books = bookRepo.GetBooks();
            if (!String.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.BookName.Contains(searchString)).ToList();
            }
            return View(books);
        }

        public ActionResult Reserve(string bookId, string customerName)
        {
            // Reserve the book in the database
       
            // Pass the booking number to the view
            return View("BookingConfirmation");
        }

        // POST: BookController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: BookController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: BookController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: BookController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: BookController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
