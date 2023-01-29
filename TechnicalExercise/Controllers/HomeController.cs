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

        public IActionResult Privacy() // Github link
        {
            return View();
        }

        public IActionResult About() // About section
        {
            return View();
        }

        public IActionResult Unreserve()
        {
            return View();
        }

        public IActionResult ReservationFailure(string error, string reason) // action result to display reservation failure with reasoning - so the user understands what went wrong
        {
            ViewData["error"] = error;
            ViewData["reason"] = reason;
            return View();
        }

        public IActionResult ReservationSuccess(string reservationStatus, string bookingNumber, string customerName, string bookName, string bookId) // action result to display reservation success
        {
            ViewData["reservationStatus"] = reservationStatus;
            ViewData["bookingNumber"] = bookingNumber;
            ViewData["customerName"] = customerName;
            ViewData["bookName"] = bookName;
            ViewData["bookId"] = bookId;
            return View();
        }

        public IActionResult Reserve(Dictionary<string, string> formValues)
        {
            string bId = bookRepo.GetBookId(formValues); // book id

            // initial screening to check if the Dictionary contains the specific key to resolve throwing an error
            if (!formValues.ContainsKey("customerName_" + bId) || !formValues.ContainsKey("customerEmail_" + bId)) return RedirectToAction("ReservationFailure", new { reservationStatus = "Book Reservation Failure", error = "Sorry, we were unable to reserve the book", reason = "Your Information was not entered correctly" }); // returns failure if the user sends an empty string

            // assigning required values to later be used to unreserve books 
            string customerName = formValues["customerName_"+ bId];
            string customerEmail = formValues["customerEmail_"+ bId];
            string bookId = formValues["bookId_" + bId];

            // returns failure message if the user sends an empty string or the email doesn't contain an @ symbol
            if (customerName == null || customerEmail == null || (!customerEmail.Contains('@'))) return RedirectToAction("ReservationFailure", new { reservationStatus = "Book Reservation Failure", error = "Sorry, we were unable to reserve the book", reason = "Your Information was not entered correctly" }); // returns failure if the user sends an empty string

            List<BookModel> books = bookRepo.GetBooks();
            var book = bookRepo.GetBookById(bookId, books); // retrieve the book that the customer wants to reserve

            Console.WriteLine("reserving...");
            if (book.Reserved == false && book.CustomerName == "Empty" && book.BookingNumber == "0") // check if the book is already reserved
            {
                // generate a booking number
                var bookingNumber = Guid.NewGuid().ToString();

                // update the book's reservation status and booking number in the database
                book.Reserved = !book.Reserved;
                book.BookingNumber = bookingNumber;
                book.CustomerName = customerName;
                book.CustomerEmail = customerEmail;

                bookRepo.ReserveBook(book); // calls the reserve method to update the mysql database field values for the specific books

                return RedirectToAction("ReservationSuccess", new { reservationStatus = "Book Reservation Success", bookingNumber = book.BookingNumber, customerName = customerName, bookName = book.BookName, bookId = book.BookId });
            }
            else
            {
                return RedirectToAction("ReservationFailure", new { reservationStatus = "Book Reservation Failure", error = "Sorry, we were unable to reserve the book", reason = $"The book '{book.BookName}' is already reserved by someone else" });
            }
        }

        public IActionResult UnreserveBook(string name, string email, string bookId, string bookingNumber)
        {
            Console.WriteLine($"{name} {email} {bookId} {bookingNumber}");
            
            // returns failure message if the user sends an empty string or the email doesn't contain an @ symbol
            if (!email.Contains('@')) return RedirectToAction("ReservationFailure", new { reservationStatus = "Book Reservation Failure", error = "Sorry, we couldn't unreserve that book", reason = "Your Information was not entered correctly" }); // returns failure if the user sends an empty string
            var book = bookRepo.GetBookById(bookId);

            if (book != null && book.CustomerName == name && book.CustomerEmail == email && book.BookingNumber == bookingNumber)
            {
                Console.WriteLine("found book");
                Console.WriteLine($"{book.BookName} {book.BookId} {book.CustomerName} | {book.CustomerEmail} | {book.BookingNumber}");
                
                bool unreserved = bookRepo.UnReserveBook(book);

                if (unreserved)
                {
                    return RedirectToAction("ReservationSuccess", new { reservationStatus = "Book Unreservation Success", customerName = book.CustomerName, bookName = book.BookName });
                }
                else
                {
                    return RedirectToAction("ReservationFailure", new { reservationStatus = "Book Reservation Failure", error = "Sorry, we couldn't unreserve that book", reason = "You don't have the correct credentials to unreserve the book." }); // returns failure if the user sends an empty string
                }
                
            }
            else
            {
                Console.WriteLine("didnt find book");
                return RedirectToAction("ReservationFailure", new { reservationStatus = "Book Reservation Failure", error = "Sorry, we couldn't unreserve that book", reason = "You don't have the correct credentials to unreserve the book." }); // returns failure if the user sends an empty string
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}