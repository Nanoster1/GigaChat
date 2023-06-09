using ErrorOr;

using GigaChat.Core.Common.Repositories.Common.Interfaces;
using GigaChat.Core.Common.Repositories.Interfaces;
using GigaChat.Core.Common.Entities.ChatMessages;
using GigaChat.Core.Common.Errors;

using MediatR;
using GigaChat.Core.ChatMessages.Events;

namespace GigaChat.Core.ChatMessages.Commands.SendTextMessage;

public class SendTextMessageCommandHandler : IRequestHandler<SendTextMessageCommand, ErrorOr<ChatMessage>>
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISender _sender;

    public SendTextMessageCommandHandler(
        IChatMessageRepository chatMessageRepository,
        IChatRoomRepository chatRoomRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ISender sender)
    {
        _chatMessageRepository = chatMessageRepository;
        _chatRoomRepository = chatRoomRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _sender = sender;
    }

    public async Task<ErrorOr<ChatMessage>> Handle(
        SendTextMessageCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindOneByIdAsync(request.UserId, cancellationToken);
        var chatRoom = await _chatRoomRepository.FindOneByIdAsync(request.ChatRoomId, cancellationToken);

        if (user is null) return Errors.Users.UserWithIdNotFound(request.UserId);
        if (chatRoom is null) return Errors.ChatRooms.RoomWithIdNotFound(request.ChatRoomId);
        if (!chatRoom.Users.Any(u => u.Id == user.Id)) return Errors.ChatRooms.ChatRoomNotContainUserForSendMessage;

        var chatMessage = new ChatMessage(request.Text, request.ChatRoomId, request.UserId);

        await _chatMessageRepository.InsertAsync(chatMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var sendTextMessageEvent = new SendTextMessageEvent(chatMessage, user);
        await _sender.Send(sendTextMessageEvent, cancellationToken);

        return chatMessage;
    }
}