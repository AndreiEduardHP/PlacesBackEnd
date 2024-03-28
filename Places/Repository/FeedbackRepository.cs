using Places.Data;
using Places.Interfaces;
using Places.Models;

namespace Places.Repository
{
    public class FeedbackRepository : IFeedbackRepository
    {

        private readonly PlacesContext _context;

        public FeedbackRepository(PlacesContext context)
        {
            _context = context;
        }

        public async Task AddFeedbackAsync(Feedback feedback)
        {
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
        }
    }
}
