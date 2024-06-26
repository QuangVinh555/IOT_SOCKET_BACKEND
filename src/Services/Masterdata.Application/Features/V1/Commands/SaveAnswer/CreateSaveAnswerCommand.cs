﻿using Core.Common;
using Infrastructure.Data;
using Masterdata.Application.Features.V1.Commands.Question;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Masterdata.Application.Features.V1.Commands.SaveAnswer
{
    public class CreateSaveAnswerCommand : IRequest<bool>
    {

        public Guid? BeginGameId { get; set; }
        public Guid? AnswerId { get; set; }
        public Guid? QuestionId { get; set; }
        public Guid? RemoteId { get; set; }
        public int? QuestionTime { get; set; }
        public int? CountTime { get; set; }
    }

    public class CreateSaveAnswerCommandHandler : IRequestHandler<CreateSaveAnswerCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOT_SOCKETContext _context;

        public CreateSaveAnswerCommandHandler(IOT_SOCKETContext context, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<bool> Handle(CreateSaveAnswerCommand request, CancellationToken cancellationToken)
        {
            if(request.CountTime>0)
            {
                // Add câu trả lời
                var saveAnswerEntity = new SaveAnswerModel
                {
                    SaveAnswerId = Guid.NewGuid(),
                    BeginGameId = request.BeginGameId,
                    QuestionId = request.QuestionId,
                    AnswerId = request.AnswerId,
                    RemoteId = request.RemoteId,
                    SelectedTime = request.QuestionTime - request.CountTime,
                    CreateTime = DateTime.Now
                };

                _context.SaveAnswerModels.Add(saveAnswerEntity);

                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            return false;
        }
    }
}
