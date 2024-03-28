using Places.Models;

namespace Places.Interfaces
{
    public interface IFeedbackRepository
    {
        Task AddFeedbackAsync(Feedback feedback);
    }
}
