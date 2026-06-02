using Live_Cryptocurrency_Watchlist.Services;
using Live_Cryptocurrency_Watchlist.Models;
using Live_Cryptocurrency_Watchlist.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Live_Cryptocurrency_Watchlist.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WatchlistController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CryptoPriceService _cryptoPriceService;

        public WatchlistController(AppDbContext context, CryptoPriceService cryptoPriceService)
        {
            _context = context;
            _cryptoPriceService = cryptoPriceService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User newUser)
        {
            if (newUser == null || string.IsNullOrEmpty(newUser.Username) || string.IsNullOrEmpty(newUser.Password))
                return BadRequest("Username and Password are required.");

            if (newUser.Password.Length < 8 || newUser.Password.Length > 16)
                return BadRequest("Password must be between 8 and 16 characters long.");

            bool exists = _context.Users.Any(u => u.Username.ToLower() == newUser.Username.ToLower().Trim());
            if (exists) return BadRequest("Username is already taken. Please choose another.");

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { UserId = newUser.UserId, Username = newUser.Username });
        }

        [HttpPost("login")]
        public IActionResult LoginUser([FromBody] User loginData)
        {
            if (loginData == null || string.IsNullOrEmpty(loginData.Username) || string.IsNullOrEmpty(loginData.Password))
                return BadRequest("Username and Password are required.");

            // Find the user where BOTH username and password match
            var existingUser = _context.Users.FirstOrDefault(u =>
                u.Username.ToLower() == loginData.Username.ToLower().Trim() &&
                u.Password == loginData.Password);

            if (existingUser == null) return Unauthorized("Invalid username or password.");

            return Ok(new { UserId = existingUser.UserId, Username = existingUser.Username });
        }

        //[HttpGet("users")]

        //[HttpPost("users")]

        [HttpPost("coins")]
        public async Task<IActionResult> AddCoins([FromBody] SavedCoin newCoin)
        {
            if (newCoin == null || string.IsNullOrEmpty(newCoin.CoinId))
            {
                return BadRequest("Invalid coin data.");
            }

            string safeCoinId = newCoin.CoinId.ToLower().Trim();

            bool exist = _context.Users.Any(u => u.UserId == newCoin.UserId);
            if (!exist)
            {
                return NotFound("User not found.");
            }

            decimal priceCheck = await _cryptoPriceService.GetPriceAsync(safeCoinId);

            if(priceCheck == 0m)
                return BadRequest($"We couldn't find a coin named '{newCoin.CoinId}'. Please check your spelling.");

            if (!exist)
            {
                return NotFound("User not found.");
            }

            var existingCoin = _context.SavedCoins.FirstOrDefault(c => c.UserId == newCoin.UserId && c.CoinId == safeCoinId);

            if (existingCoin != null)
            {
                existingCoin.Amount += newCoin.Amount;
            }
            else
            {

                newCoin.CoinId = safeCoinId;
                _context.SavedCoins.Add(newCoin);
            }

            await _context.SaveChangesAsync(); 

            return Ok(newCoin);
        }

        [HttpGet("{userId}/portfolio")]
        public async Task<IActionResult> GetPortfolio(int userId)
        {
            var user = _context.Users.Include(u => u.SavedCoins).FirstOrDefault(u => u.UserId == userId);

            if(user == null)
            {
                return BadRequest("Invalid user ID.");
            }

            var coinNames = user.SavedCoins.Select(c => c.CoinId).ToList();

            var bulkData = await _cryptoPriceService.GetBulkPricesAsync(coinNames);

            var portfolioData = new List<object>();

            foreach (var savedCoin in user.SavedCoins)
            {
                string safeName = savedCoin.CoinId.ToLower().Trim();

                bool found = bulkData.TryGetValue(safeName, out var coinInfo);

                portfolioData.Add(new
                {
                    CoinId = savedCoin.CoinId,
                    CurrentPrice = found ? coinInfo.Price : 0m,
                    ImageUrl = found ? coinInfo.ImageUrl : "https://cdn-icons-png.flaticon.com/512/888/888879.png",
                    Amount = savedCoin.Amount
                });
            }

            return Ok(new 
            {
                Username = user.Username,
                Portfolio = portfolioData
            });
        }

        [HttpDelete("{userId}/coins/{coinId}")]
        public async Task<IActionResult> DeleteCoin(int userId, string coinId)
        {
            var coinRemove = _context.SavedCoins.FirstOrDefault(c => c.UserId == userId && c.CoinId == coinId.ToLower().Trim());
            if (coinRemove == null)
            {
                return NotFound("Coin not found in user's watchlist.");
            }

            _context.SavedCoins.Remove(coinRemove);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{coinId.ToUpper()} successfully removed."});
        }

        [HttpGet("coins/{coinId}/history")]
        public async Task<IActionResult> GetCoinHistory(string coinId)
        {
            try
            {
                var history = await _cryptoPriceService.GetHistoricalDataAsync(coinId);
                return Ok(history);
            }
            catch
            {
                return BadRequest("Failed to load historical data.");
            }
        }

        [HttpPut("{userId}/coins/{coinId}")]
        public async Task<IActionResult> UpdateCoinAmount(int userId, string coinId, [FromBody] decimal newAmount)
        {
            var coinToUpdate = _context.SavedCoins.FirstOrDefault(c => c.UserId == userId && c.CoinId == coinId.ToLower().Trim());

            if (coinToUpdate == null) return NotFound("Coin not found.");

            coinToUpdate.Amount = newAmount;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Amount updated successfully!" });
        }

    }
}
