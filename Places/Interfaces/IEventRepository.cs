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
        bool Save();
        Task<int> GetCurrentParticipantCount(int eventId);

    }
}
