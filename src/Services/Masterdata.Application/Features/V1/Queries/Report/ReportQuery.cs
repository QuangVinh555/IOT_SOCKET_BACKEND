﻿using Core.Exceptions;
using Infrastructure.Data;
using Masterdata.Application.Features.V1.DTOs.Report;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

        /// <summary>
        /// Bảng xếp hạng top 3 remote
        /// </summary>
        /// <param name="BeginGameId"></param>
        /// <returns></returns>
        Task<List<ReportTopRankingRemoteResponse>> GetReportTopRankingBy(Guid BeginGameId);

        /// <summary>
        /// Bảng xếp hạng Remote
        /// </summary>
        /// <param name="BeginGameId"></param>
        /// <returns></returns>
        Task<List<ReportRankingDetailResponse>> GetReportRankingDetailBy(Guid BeginGameId);
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
                    AnswerKey = anwser.AnswerKey,
                    TotalUserSelected = item.TotalUserSelected,
                };
                ListRes.Add(res);
            }

            return ListRes;
        }

        public async Task<List<ReportRankingDetailResponse>> GetReportRankingDetailBy(Guid BeginGameId)
        {
            var savegames = await _context.SaveAnswerModels.Where(x => x.BeginGameId == BeginGameId).ToListAsync();
            if (!savegames.Any()) throw new BadRequestException("BeginGameId này không tồn tại!");

            // List đáp án đúng
            var listAnswerD = new List<ReportRankingDetailTempResponse>();
            // List đáp án không đúng
            var listAnwserKD = new List<ReportRankingDetailTempResponse>();

            foreach(var item in savegames)
            {
                var listAnswer = await _context.AnswerModels.FirstOrDefaultAsync(x => x.AnswerId == item.AnswerId && x.IsCorrect == true);
                if(listAnswer == null)
                {
                    var tempRes = new ReportRankingDetailTempResponse
                    {
                        RemoteId = item.RemoteId,
                        AnwserId = item.AnswerId,
                        TTGC = 0,
                        IsCorrect = false,
                    };
                    listAnwserKD.Add(tempRes);
                }
                else
                {
                    var tempRes = new ReportRankingDetailTempResponse
                    {
                        RemoteId = item.RemoteId,
                        AnwserId = item.AnswerId,
                        TTGC = item.SelectedTime,
                        IsCorrect = true,
                    };
                    listAnswerD.Add(tempRes);
                }
            }

            string ConvertIntToTimeSpanString(int? TTGC)
            {
                if(!TTGC.HasValue) return TimeSpan.Zero.ToString();
                return TimeSpan.FromSeconds(TTGC.Value).ToString();
            }

            var listTH = listAnswerD.Concat(listAnwserKD);
            int noChosseQuestion = 0;
            var BeginModels = await _context.BeginGameModels.Where(x => x.BeginGameId == BeginGameId).FirstOrDefaultAsync();
            if(BeginModels !=null)
            {
                var lstquestionByTopicId = await _context.QuestionModels.Where(x => x.TopicId == BeginModels.TopicId).ToListAsync();
                noChosseQuestion = lstquestionByTopicId.Count - listTH.Count();
            }    

            var query = listTH.GroupBy(x => x.RemoteId).Select(x => new ReportRankingDetailResponse
            {
                RemoteId = x.Key,
                UserName = _context.UserGameModels.FirstOrDefault(y => y.RemoteId == x.Key && y.BeginGameId == BeginGameId)?.UserName,
                SCD = x.Where(x => x.IsCorrect == true).Count(),
                SCKD = x.Where(x => x.IsCorrect == false).Count(),
                SCKC = noChosseQuestion,
                TTGC = ConvertIntToTimeSpanString(x.Sum(x => x.TTGC))
            }).OrderByDescending(x =>x.SCD).ThenByDescending(x => x.TTGC).ToList();

            return query;
        }

        public async Task<List<ReportTopRankingRemoteResponse>> GetReportTopRankingBy(Guid BeginGameId)
        {
            var res = new List<ReportTopRankingRemoteResponse>();
            var savegames = await _context.SaveAnswerModels.Where(x => x.BeginGameId == BeginGameId)
                .ToListAsync();
            if (!savegames.Any()) throw new BadRequestException("BeginGameId này không tồn tại!");

            // List đáp án đúng
            var ListAnswerCorrectRes = new List<ReportTopRankingRemoteTempResponse>();

            foreach (var item in savegames)
            {
                var listAnswer = await _context.AnswerModels.FirstOrDefaultAsync(x => x.AnswerId == item.AnswerId && x.IsCorrect == true);
                if (listAnswer == null) continue;

                var tempRes = new ReportTopRankingRemoteTempResponse
                {
                    RemoteId = item.RemoteId,
                    AnwserId = item.AnswerId,
                    timeSelection = item.SelectedTime
                };
                ListAnswerCorrectRes.Add(tempRes);
            }

            var bxhs = ListAnswerCorrectRes.GroupBy(x => x.RemoteId).Select(y => new
            {
                RemoteId = y.Key,
                Total = y.Count(),
                timeSelection = y.Sum(x =>x.timeSelection)
            }).OrderByDescending(x => x.Total).OrderBy(x=>x.timeSelection);

            var query = bxhs.Take(3).Select((x,i) => new ReportTopRankingRemoteResponse
            {
                Position = i + 1,
                UserName = _context.UserGameModels.FirstOrDefault(y => y.RemoteId == x.RemoteId && y.BeginGameId == BeginGameId)?.UserName,
                RemoteName = _context.RemoteModels.FirstOrDefault(y => y.RemoteId == x.RemoteId)?.RemoteName
            }).ToList();
            return query;
        }  
    }
}
