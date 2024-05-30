﻿using Infrastructure.Data;
using Masterdata.Application.Features.V1.DTOs.Report;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masterdata.Application.Features.V1.Queries.Report
{
    public interface IReportQuery
    {
        /// <summary>
        /// Thống kê số lượng đáp án được chọn theo từng câu hỏi
        /// </summary>
        /// <param name="BeginGameId"></param>
        /// <param name="QuestionId"></param>
        /// <returns></returns>
        Task<List<ReportCountAnswerResponse>> GetReportCountAnswerBy(Guid BeginGameId, Guid QuestionId);
    }
    public class ReportQuery : IReportQuery
    {
        private readonly IOT_SOCKETContext _context;

        public ReportQuery(IOT_SOCKETContext context) {
            _context = context;
        }
        public async Task<List<ReportCountAnswerResponse>> GetReportCountAnswerBy(Guid BeginGameId, Guid QuestionId)
        {
            var query = await _context.SaveAnswerModels.Where(x => x.BeginGameId == BeginGameId && x.QuestionId == QuestionId)
                .GroupBy(x => x.AnswerId)
                .Select(x => new 
                {
                    AnswerId = x.Key,
                    TotalUserSelected = x.Count(x => x.RemoteId.HasValue)
                })
                .ToListAsync();
            var ListRes = new List<ReportCountAnswerResponse>();
            foreach (var item in query)
            {
                var anwser = await _context.AnswerModels.FirstOrDefaultAsync(x => x.AnswerId == item.AnswerId);
                var res = new ReportCountAnswerResponse
                {
                    AnswerId = anwser.AnswerId,
                    AnswerName = anwser.AnswerName,
                    TotalUserSelected = item.TotalUserSelected,
                };
                ListRes.Add(res);
            }

            return ListRes;
        }
    }
}
