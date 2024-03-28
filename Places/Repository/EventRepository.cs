using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Places.Data;
using Places.Interfaces;
using Places.Models;

namespace Places.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly PlacesContext _context;
      

        public EventRepository(PlacesContext context)
        {
            _context = context;
         
        }

        public async Task<int> GetCurrentParticipantCount(int eventId)
        {
           
            return await _context.UserProfileEvents.CountAsync(upe => upe.EventId == eventId);
        }

        public bool CreateEvent(Event createdEvent)
        {
            _context.Events.Add(createdEvent);
            return Save();
        }

        public bool DeleteEvent(Event deletedEvent)
        {
            _context.Events.Remove(deletedEvent);
            return Save();
        }

        public bool EventExists(int eventId)
        {
            return _context.Events.Any(e => e.Id == eventId);
        }

        public Event GetEvent(int eventId)
        {
            return _context.Events.Where(e => e.Id == eventId).FirstOrDefault();
        }

        public ICollection<Event> GetEvents()
        {
          
            return _context.Events.ToList();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool UpdateEvent(Event updatedEvent)
        {
            _context.Events.Update(updatedEvent);
            return Save();
        }
    }
}
