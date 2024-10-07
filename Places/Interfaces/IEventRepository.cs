using Places.Models;

namespace Places.Interfaces
{
    public interface IEventRepository
    {
        ICollection<Event> GetEvents();
        Event GetEvent(int eventId);
        bool EventExists(int eventId);
        bool CreateEvent(Event createdEvent);
        bool UpdateEvent(Event updatedEvent);
        bool DeleteEvent(Event deletedEvent);
        bool AddImagesToEvent(int eventId, List<string> imageUrls);
        bool Save();
        Task<int> GetCurrentParticipantCount(int eventId);

    }
}
