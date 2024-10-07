using Microsoft.EntityFrameworkCore;
using Places.Dto;
using Places.Interfaces;

public class FriendService
{
    private readonly IUserProfileRepository _userProfileRepository;

    public FriendService(IUserProfileRepository userProfileRepository)
    {
        _userProfileRepository = userProfileRepository;
    }

    public async Task SendFriendRequest(int senderId, int receiverId, float latitude, float longitude)
    {
        await _userProfileRepository.SendFriendRequest(senderId, receiverId, latitude, longitude);
    }

    public async Task<List<FriendRequestDto>> GetPendingFriendRequests(int userId)
    {
        return await _userProfileRepository.GetPendingFriendRequests(userId);
    }
    public async Task<List<object>> GetAcceptedFriendRequests(int userId)
    {
        return await _userProfileRepository.GetAcceptedFriendRequests(userId);
    }

    public async Task AcceptFriendRequest(int requestId)
    {
        await _userProfileRepository.AcceptFriendRequest(requestId);
    }

    // Add method to decline a friend request
    public async Task DeclineFriendRequest(int requestId)
    {
        await _userProfileRepository.DeclineFriendRequest(requestId);
    }
    public async Task<int> GetFriendCount(int userId)
    {
        return await _userProfileRepository.GetFriendCount(userId);
    }

}
