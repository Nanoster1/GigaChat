using ErrorOr;

using GigaChat.Core.Common.Repositories.Common.Interfaces;
using GigaChat.Core.Common.Repositories.Interfaces;
using GigaChat.Core.Common.Entities.ChatRooms;

using MediatR;

using GigaChat.Core.ChatRooms.Events;

namespace GigaChat.Core.ChatRooms.Commands.OpenChatRoom;

public class OpenChatRoomCommandHandler : IRequestHandler<OpenChatRoomCommand, ErrorOr<ChatRoom>>
{
    private readonly ISender _sender;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OpenChatRoomCommandHandler(
        IChatRoomRepository chatRoomRepository,
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        ISender sender)
    {
        _chatRoomRepository = chatRoomRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _sender = sender;
    }

    public async Task<ErrorOr<ChatRoom>> Handle(OpenChatRoomCommand request, CancellationToken cancellationToken)
    {
        var uniqueUserIds = request.UserIds.Distinct().ToList();

        var users = await _userRepository.FindManyByIds(uniqueUserIds)
            .ToListAsync(cancellationToken);

        var chatRoom = new ChatRoom(request.OwnerId, request.Title) { Users = users };

        await _chatRoomRepository.InsertAsync(chatRoom, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var inviteToChatRoomEvent = new OpenToChatRoomEvent(chatRoom);
        await _sender.Send(inviteToChatRoomEvent, cancellationToken);

        return chatRoom;
    }
}