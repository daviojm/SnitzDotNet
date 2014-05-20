﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using Snitz.Entities;
using Snitz.IDAL;
using Snitz.OLEDbDAL.Helpers;

namespace Snitz.OLEDbDAL
{
    public class PrivateMessage : IPrivateMessage
    {
        public PrivateMessageInfo GetById(int id)
        {
            PrivateMessageInfo pm = new PrivateMessageInfo();
            const string strSql = "UPDATE FORUM_PM SET M_READ=M_READ+1 WHERE M_ID=@MessageId " +
                                  "SELECT M_ID,M_SUBJECT,M_FROM,M_TO,M_SENT,M_MESSAGE,M_PMCOUNT,M_READ,M_MAIL,M_OUTBOX,ToMember.M_NAME AS ToMemberName, FromMember.M_NAME AS FromMemberName FROM (FORUM_PM LEFT OUTER JOIN " +
                                  "FORUM_MEMBERS AS FromMember ON FORUM_PM.M_FROM = FromMember.MEMBER_ID) LEFT OUTER JOIN " +
                                  "FORUM_MEMBERS AS ToMember ON FORUM_PM.M_TO = ToMember.MEMBER_ID " +
                                  "WHERE M_ID=@MessageId";

            using (OleDbDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnString, CommandType.Text, strSql, new OleDbParameter("@MessageId", OleDbType.Integer) { Value = id }))
            {
                while (rdr.Read())
                {
                    pm = BoHelper.CopyPrivateMessageToBO(rdr);
                }
            }
            return pm;
        }

        public IEnumerable<PrivateMessageInfo> GetByName(string name)
        {
            List<PrivateMessageInfo> pm = new List<PrivateMessageInfo>();
            const string strSql = "SELECT M_ID,M_SUBJECT,M_FROM,M_TO,M_SENT,M_MESSAGE,M_PMCOUNT,M_READ,M_MAIL,M_OUTBOX,ToMember.M_NAME AS ToMemberName, FromMember.M_NAME AS FromMemberName FROM (FORUM_PM LEFT OUTER JOIN " +
                                  "FORUM_MEMBERS AS FromMember ON FORUM_PM.M_FROM = FromMember.MEMBER_ID) LEFT OUTER JOIN " +
                                  "FORUM_MEMBERS AS ToMember ON FORUM_PM.M_TO = ToMember.MEMBER_ID " +
                                  "WHERE M_SUBJECT LIKE @Subject";

            using (OleDbDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnString, CommandType.Text, strSql, new OleDbParameter("@Subject", OleDbType.Integer) { Value = "%" + name + "%" }))
            {
                while (rdr.Read())
                {
                    pm.Add(BoHelper.CopyPrivateMessageToBO(rdr));
                }
            }
            return pm;
        }

        public int Add(PrivateMessageInfo message)
        {

            const string strSql = "INSERT INTO FORUM_PM (M_SUBJECT,M_FROM,M_TO,M_SENT,M_MESSAGE,M_READ,M_MAIL,M_OUTBOX) VALUES " +
                                  "(@Subject,@FromUser,@ToUser,@SentDate,@Message,0,@Email,@Outbox)";
            List<OleDbParameter> parms = new List<OleDbParameter>
                {
                    new OleDbParameter("@Subject", OleDbType.VarChar) {Value = message.Subject},
                    new OleDbParameter("@FromUser", OleDbType.Integer) {Value = message.FromMemberId},
                    new OleDbParameter("@ToUser", OleDbType.Integer) {Value = message.ToMemberId},
                    new OleDbParameter("@SentDate", OleDbType.VarChar) {Value = message.SentDate},
                    new OleDbParameter("@Message", OleDbType.VarChar) {Value = message.Message},
                    new OleDbParameter("@Email", OleDbType.VarChar) {Value = message.Mail},
                    new OleDbParameter("@Outbox", OleDbType.Integer) {Value = message.OutBox}
                };

            var res = SqlHelper.ExecuteScalar(SqlHelper.ConnString, CommandType.Text, strSql, parms.ToArray());
            return Convert.ToInt32(res);
        }

        public void Update(PrivateMessageInfo message)
        {
            const string strSql = "UPDATE FORUM_PM SET M_SUBJECT=,M_FROM=@Subject,M_TO=@ToUser,M_SENT=@FromUser,M_MESSAGE=@Message,M_READ=M_READ+1,M_MAIL=@Email,M_OUTBOX=@Outbox " +
                                  "WHERE M_ID=@PmId";
            List<OleDbParameter> parms = new List<OleDbParameter>
                {
                    new OleDbParameter("@PmId", OleDbType.Integer) {Value = message.Id},
                    new OleDbParameter("@Subject", OleDbType.VarChar) {Value = message.Subject},
                    new OleDbParameter("@FromUser", OleDbType.Integer) {Value = message.FromMemberId},
                    new OleDbParameter("@ToUser", OleDbType.Integer) {Value = message.ToMemberId},
                    new OleDbParameter("@SentDate", OleDbType.VarChar) {Value = message.SentDate},
                    new OleDbParameter("@Message", OleDbType.VarChar) {Value = message.Message},
                    new OleDbParameter("@Email", OleDbType.VarChar) {Value = message.Mail},
                    new OleDbParameter("@Outbox", OleDbType.Integer) {Value = message.OutBox}
                };
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnString, CommandType.Text, strSql, parms.ToArray());
        }

        public void Delete(PrivateMessageInfo pm)
        {
            const string strSql = "DELETE FROM FORUM_PM WHERE M_ID=@PmId";
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnString, CommandType.Text, strSql, new OleDbParameter("@PmId", OleDbType.Integer) { Value = pm.Id });
        }

        public void Dispose()
        {

        }

        public int MemberCount(string searchfor)
        {
            string strSql = "SELECT COUNT(M.MEMBER_ID) AS MemberCount " +
                            "FROM FORUM_MEMBERS AS M LEFT OUTER JOIN " +
                            "ProfileData AS P ON M.MEMBER_ID = P.UserId " +
                            "WHERE (M.M_STATUS = 1) AND (IIF (P.PMReceive IS NULL, 1,P.PMReceive) <> 0) ";
            OleDbParameter search = null;
            if (!String.IsNullOrEmpty(searchfor))
            {
                strSql = strSql + "AND M_NAME LIKE @SearchFor ";
                search = new OleDbParameter("@SearchFor", OleDbType.VarChar) { Value = searchfor + "%" };
            }

            var res = SqlHelper.ExecuteScalar(SqlHelper.ConnString, CommandType.Text, strSql, search);
            return Convert.ToInt32(res);
        }

        public IEnumerable<PrivateMessageInfo> GetMessages(int memberid)
        {
            const string strSql = "SELECT M_ID,M_SUBJECT,M_FROM,M_TO,M_SENT,M_MESSAGE,M_PMCOUNT,M_READ,M_MAIL,M_OUTBOX,ToMember.M_NAME AS ToMemberName, FromMember.M_NAME AS FromMemberName FROM (FORUM_PM LEFT OUTER JOIN " +
                                  "FORUM_MEMBERS AS FromMember ON FORUM_PM.M_FROM = FromMember.MEMBER_ID) LEFT OUTER JOIN " +
                                  "FORUM_MEMBERS AS ToMember ON FORUM_PM.M_TO = ToMember.MEMBER_ID " +
                                  "WHERE M_TO=@MemberId ORDER BY M_SENT DESC";
            List<PrivateMessageInfo> pm = new List<PrivateMessageInfo>();
            using (OleDbDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnString, CommandType.Text, strSql, new OleDbParameter("@MemberId", OleDbType.Integer) { Value = memberid }))
            {
                while (rdr.Read())
                {
                    pm.Add(BoHelper.CopyPrivateMessageToBO(rdr));
                }
            }
            return pm;
        }

        public IEnumerable<PrivateMessageInfo> GetSentMessages(int memberid)
        {
            const string strSql = "SELECT M_ID,M_SUBJECT,M_FROM,M_TO,M_SENT,M_MESSAGE,M_PMCOUNT,M_READ,M_MAIL,M_OUTBOX,ToMember.M_NAME AS ToMemberName, FromMember.M_NAME AS FromMemberName FROM (FORUM_PM LEFT OUTER JOIN " +
                                  "FORUM_MEMBERS AS FromMember ON FORUM_PM.M_FROM = FromMember.MEMBER_ID) LEFT OUTER JOIN " +
                                  "FORUM_MEMBERS AS ToMember ON FORUM_PM.M_TO = ToMember.MEMBER_ID " +
                                  "WHERE M_FROM=@MemberId AND M_OUTBOX=1 ORDER BY M_SENT DESC";
            List<PrivateMessageInfo> pm = new List<PrivateMessageInfo>();
            using (OleDbDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnString, CommandType.Text, strSql, new OleDbParameter("@MemberId", OleDbType.Integer) { Value = memberid }))
            {
                while (rdr.Read())
                {
                    pm.Add(BoHelper.CopyPrivateMessageToBO(rdr));
                }
            }
            return pm;
        }

        public IEnumerable<MemberInfo> GetMemberListPaged(int page, string searchfor)
        {
            
            string SELECT_OVER = "WITH MemberEntities AS (SELECT ROW_NUMBER() OVER (ORDER BY M_NAME) AS Row, MEMBER_ID FROM FORUM_MEMBERS LEFT OUTER JOIN ProfileData ON ProfileData.UserId=FORUM_MEMBERS.MEMBER_ID WHERE M_STATUS=1 AND IIF(PMEmail IS NULL,1,PMEmail)=1 ";
            if (!String.IsNullOrEmpty(searchfor))
            {
                SELECT_OVER += " AND M_NAME LIKE @SearchFor ";
            }
            const string MEMBER_SELECT = ") SELECT M.MEMBER_ID,M.M_STATUS,M.M_NAME,M.M_USERNAME,M.M_EMAIL,M.M_COUNTRY,M.M_HOMEPAGE,M.M_SIG" +
                                         ",M.M_LEVEL,M.M_AIM,M.M_YAHOO,M.M_ICQ,M.M_MSN,M.M_POSTS,M.M_DATE,M.M_LASTHEREDATE,M.M_LASTPOSTDATE" +
                                         ",M.M_TITLE,M.M_SUBSCRIPTION,M.M_HIDE_EMAIL,M.M_RECEIVE_EMAIL,M.M_IP,M.M_VIEW_SIG,M.M_SIG_DEFAULT" +
                                         ",M.M_VOTED,M.M_ALLOWEMAIL,M.M_AVATAR,M.M_THEME,M.M_TIMEOFFSET,M.M_DOB,M_AGE,M_PASSWORD,M_KEY,M_VALID,M_LASTUPDATED " +
                                         ",M_MARSTATUS,M_FIRSTNAME,M_LASTNAME,M_OCCUPATION,M_SEX,M_HOBBIES,M_LNEWS,M_QUOTE,M_BIO,M_LINK1,M_LINK2,M_CITY,M_STATE ";
            const string SELECT_OVER_FROM = "FROM MemberEntities ME INNER JOIN FORUM_MEMBERS M on ME.MEMBER_ID = M.MEMBER_ID " +
                                            " WHERE ME.Row Between @Start AND @MaxRows ORDER BY ME.Row ASC ";
            List<MemberInfo> members = new List<MemberInfo>();
            List<OleDbParameter> parms = new List<OleDbParameter>
                {
                    new OleDbParameter("@Start", OleDbType.Integer) {Value = 1 + (page*11)},
                    new OleDbParameter("@MaxRows", OleDbType.Integer) {Value = (page*11) + 11}
                };

            if (!String.IsNullOrEmpty(searchfor))
                parms.Add(new OleDbParameter("@SearchFor", OleDbType.VarChar) { Value = searchfor + "%" });
            using (OleDbDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnString, CommandType.Text, SELECT_OVER + MEMBER_SELECT + SELECT_OVER_FROM, parms.ToArray()))
            {
                while (rdr.Read())
                {
                    members.Add(BoHelper.CopyMemberToBO(rdr));
                }
            }
            return members;
        }

        public int UnreadPMCount(int memberid)
        {
            const string strSql = "SELECT COUNT(M_ID) FROM FORUM_PM WHERE M_TO=@MemberId AND M_READ=0";
            return Convert.ToInt32(SqlHelper.ExecuteScalar(SqlHelper.ConnString, CommandType.Text, strSql, new OleDbParameter("@MemberId", OleDbType.Integer) { Value = memberid }));
        }
    }
}