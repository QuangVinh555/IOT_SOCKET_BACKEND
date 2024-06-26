﻿using Core.Common;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Masterdata.Application.Features.V1.Commands.Topic
{
    public class CreateTopicCommand : IRequest<Guid>
    {
        public string TopicName { get; set; }
    }

    public class CreateTopicCommandHandler : IRequestHandler<CreateTopicCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOT_SOCKETContext _context;

        public CreateTopicCommandHandler(IOT_SOCKETContext context, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<Guid> Handle(CreateTopicCommand request, CancellationToken cancellationToken)
        {
            var topicEntity = new TopicModel
            {
                TopicId = Guid.NewGuid(),
                TopicName = request.TopicName,
                CreateTime = DateTime.Now,
                Actived = true
            };

            _context.TopicModels.Add(topicEntity);

            await _unitOfWork.SaveChangesAsync();

            return topicEntity.TopicId;
        }
    }
}
