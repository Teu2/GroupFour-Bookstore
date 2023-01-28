using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TechnicalExercise.Models;

namespace TechnicalExercise.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BookRepository bookRepo;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            bookRepo = new BookRepository("server=localhost;user=root;database=groupfour_technical;port=3306;");
        }

        public IActionResult Index()
        {
            List<BookModel> books = bookRepo.GetBooks();
            foreach (var book in books){Console.WriteLine($"book name: {book.BookName} | bookId: {book.BookId}");}

            return View(books); // pass the list of books to the view
        }

        public IActionResult Search(string searchString) // used for search the book library
        {
            List<BookModel> books = bookRepo.GetBooks();
            Console.WriteLine("Searching...");

            if(searchString == null) return View(books);

            if (!string.IsNullOrEmpty(searchString)) // searches for books that contains the search string - no need to query the database and return the result
            {
                books = books.Where(b => b.BookName.Contains(searchString)).ToList();
            }

            return View(books);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult ReservationFailure(string reason) // action result to display reservation failure with reasoning - so the user understands what went wrong
        {
            ViewData["reason"] = reason;
            return View();
        }

        public IActionResult ReservationSuccess(string bookingNumber, string customerName, string bookName) // action result to display reservation success
        {
            ViewData["bookingNumber"] = bookingNumber;
            ViewData["customerName"] = customerName;
            ViewData["bookName"] = bookName;
            return View();
        }

        public IActionResult Reserve(Dictionary<string, string> formValues)
        {
            string bId = bookRepo.GetBookId(formValues); // book id

            // initial screening to check if the Dictionary contains the specific key to resolve throwing an error
            if (!formValues.ContainsKey("customerName_" + bId) || !formValues.ContainsKey("customerEmail_" + bId)) return RedirectToAction("ReservationFailure", new { reservationStatus = "failure", reason = "Your Information was not entered" }); // returns failure if the user sends an empty string

            // assigning required values to later be used to unreserve books 
            string customerName = formValues["customerName_"+ bId];
            string customerEmail = formValues["customerEmail_"+ bId];
            string bookId = formValues["bookId_" + bId];

            // returns failure message if the user sends an empty string
            if (customerName == null || customerEmail == null) return RedirectToAction("ReservationFailure", new { reservationStatus = "failure", reason = "Your Information was not entered" }); // returns failure if the user sends an empty string

            List<BookModel> books = bookRepo.GetBooks();
            var book = bookRepo.GetBookById(bookId, books); // retrieve the book that the customer wants to reserve

            Console.WriteLine("reserving...");
            if (book.Reserved == false) // check if the book is already reserved
            {
                // generate a booking number
                var bookingNumber = Guid.NewGuid().ToString();

                // update the book's reservation status and booking number in the database
                book.Reserved = !book.Reserved;
                book.BookingNumber = bookingNumber;
                book.CustomerName = customerName;
                book.CustomerEmail = customerEmail;

                bookRepo.ReserveBook(book); // calls the reserve method to update the mysql database field values for the specific books

                return RedirectToAction("ReservationSuccess", new { reservationStatus = "success", bookingNumber = book.BookingNumber, customerName = customerName, bookName = book.BookName });
            }
            else
            {
                return RedirectToAction("ReservationFailure", new { reservationStatus = "failure", reason = $"The book '{book.BookName}' is already reserved by someone else" });
            }
        }

        public IActionResult UnReserve(Dictionary<string, string> formValues)
        {
            string bId = bookRepo.GetBookId(formValues); // book id

            // initial screening to check if the Dictionary contains the specific key to resolve throwing an error
            if (!formValues.ContainsKey("customerName_" + bId) || !formValues.ContainsKey("customerEmail_" + bId)) return RedirectToAction("ReservationFailure", new { reservationStatus = "failure", reason = "Your Information was not entered" }); // returns failure if the user sends an empty string

            // assigning required values to later be used to unreserve books 
            string customerName = formValues["customerName_" + bId];
            string customerEmail = formValues["customerEmail_" + bId];
            string bookId = formValues["bookId_" + bId];

            // returns failure message if the user sends an empty string
            if (customerName == null || customerEmail == null) return RedirectToAction("ReservationFailure", new { reservationStatus = "failure", reason = "Your Information was not entered" }); // returns failure if the user sends an empty string

            List<BookModel> books = bookRepo.GetBooks();
            var book = bookRepo.GetBookById(bookId, books); // retrieve the book that the customer wants to reserve

            Console.WriteLine("reserving...");
            if (book.Reserved == false) // check if the book is already reserved
            {
                // generate a booking number
                var bookingNumber = Guid.NewGuid().ToString();

                // update the book's reservation status and booking number in the database
                book.Reserved = !book.Reserved;
                book.BookingNumber = bookingNumber;
                book.CustomerName = customerName;
                book.CustomerEmail = customerEmail;

                bookRepo.UnReserveBook(book); // calls the reserve method to update the mysql database field values for the specific books

                return RedirectToAction("ReservationSuccess", new { reservationStatus = "success", bookingNumber = book.BookingNumber, customerName = customerName, bookName = book.BookName });
            }
            else
            {
                return RedirectToAction("ReservationFailure", new { reservationStatus = "failure", reason = $"The book '{book.BookName}' is already reserved by someone else" });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}