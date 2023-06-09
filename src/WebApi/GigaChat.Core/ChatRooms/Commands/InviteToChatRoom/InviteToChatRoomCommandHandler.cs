﻿using ErrorOr;

using GigaChat.Core.ChatRooms.Events;
using GigaChat.Core.Common.Repositories.Common.Interfaces;
using GigaChat.Core.Common.Repositories.Interfaces;
using GigaChat.Core.Common.Errors;

using MediatR;

namespace GigaChat.Core.ChatRooms.Commands.InviteToChatRoom;

public class InviteToChatRoomCommandHandler : IRequestHandler<InviteToChatRoomCommand, ErrorOr<Updated>>
{
    private readonly ISender _sender;
    private readonly IUserRepository _userRepository;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InviteToChatRoomCommandHandler(
        ISender sender,
        IUserRepository userRepository,
        IChatRoomRepository chatRoomRepository,
        IUnitOfWork unitOfWork)
    {
        _sender = sender;
        _userRepository = userRepository;
        _chatRoomRepository = chatRoomRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Updated>> Handle(InviteToChatRoomCommand request,
        CancellationToken cancellationToken)
    {
        var chatRoom = await _chatRoomRepository.FindOneByIdAsync(request.ChatRoomId, cancellationToken);
        if (chatRoom is null) return Errors.ChatRooms.RoomWithIdNotFound(request.ChatRoomId);

        var user = await _userRepository.FindOneByIdAsync(request.UserId, cancellationToken);
        if (user is null) return Errors.Users.UserWithIdNotFound(request.UserId);

        if (request.OwnerId == chatRoom.OwnerId)
            chatRoom.Users.Add(user);
        else
            return Errors.ChatRooms.UserIsNotOwnerForInvite;

        await _chatRoomRepository.UpdateAsync(chatRoom, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var inviteToChatRoomEvent = new InviteToChatRoomEvent(chatRoom, user);
        await _sender.Send(inviteToChatRoomEvent, cancellationToken);

        return Result.Updated;
    }
}