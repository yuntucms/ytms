﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YTMS.Domain;
using YTMS.DAL;
using YTMS.Common.Core;
namespace YTMS.BLL.Order
{
    public class RoomOrderServer : IRoomOrderServer
    {
        private static object reserveLock = new object();
        private static object checkInLock = new object();
      

        public bool Reserve(RoomReserveDto reserve, List<RoomReserveItemDto> rooms)
        {
            if (reserve == null)
                throw new CustomerException("预定信息为null");
            if (rooms == null || rooms.Count == 0)
                throw new CustomerException("请选择需要预定的客房");

            if (string.IsNullOrWhiteSpace(reserve.UserName))
                throw new CustomerException("客人姓名不能为空");

            if (string.IsNullOrWhiteSpace(reserve.UserPhone))
                throw new CustomerException("客人联系电话不能为空");

            if (!reserve.ArrvingTime.HasValue)
                throw new CustomerException("预抵时间不能为空");
            if (!reserve.LeavingTime.HasValue)
                throw new CustomerException("预离时间不能为空");

            if(reserve.ArrvingTime.Value.Ticks>= reserve.LeavingTime.Value.Ticks)
                throw new CustomerException("预离时间必须晚于预抵时间");

            if (reserve.Days == 0)
                throw new CustomerException("预定天数必须大于零");

            //验证客服是否可预定

            lock (reserveLock)
            {
                using (var db = DBManager.GetInstance())
                {
                    var objs = rooms.Select(row => new T_Room_Records
                    {
                        ArrvingTime = reserve.ArrvingTime,
                        CardNo = reserve.CardNo,
                        CardType = reserve.CardType,
                        CreateBy = reserve.CreateBy,
                        CreateTime = reserve.CreateTime,
                        Days = reserve.Days,
                        ExpiredTime = reserve.ExpiredTime,
                        IsDeleted = false,
                        LeavingTime = reserve.LeavingTime,
                        RoomPrice = row.RoomPrice,
                        RoomId = row.Id,
                        Status = (int)RecordStatus.Reserve,
                        UserName = reserve.UserName,
                        UserPhone = reserve.UserPhone,
                        Deposit = row.Deposit
                    });

                    try
                    {
                        db.Ado.BeginTran();

                        var count = db.Insertable<T_Room_Records>(objs).ExecuteCommand();

                        db.Ado.CommitTran();

                        return count > 0;
                    }
                    catch (Exception ex)
                    {
                        db.Ado.RollbackTran();
                        throw ex;
                    }
                }
            }
        }

        public bool CheckIn(RoomCheckInDto checkin)
        {
            if (checkin == null)
                throw new CustomerException("入住信息为null");

            if (string.IsNullOrWhiteSpace(checkin.UserPhone))
                throw new CustomerException("客人联系电话不能为空");

            if (!checkin.ArrvingTime.HasValue)
                throw new CustomerException("预抵时间不能为空");
            if (!checkin.LeavingTime.HasValue)
                throw new CustomerException("预离时间不能为空");

            if (checkin.ArrvingTime.Value.Ticks >= checkin.LeavingTime.Value.Ticks)
                throw new CustomerException("预离时间必须晚于预抵时间");

            if (checkin.Days == 0)
                throw new CustomerException("入住天数必须大于零");

            throw new NotImplementedException();
        }
    }
}
