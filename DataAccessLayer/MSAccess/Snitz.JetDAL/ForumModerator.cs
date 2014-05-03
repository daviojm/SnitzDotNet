﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using Snitz.Entities;
using Snitz.IDAL;
using Snitz.OLEDbDAL.Helpers;

namespace Snitz.OLEDbDAL
{
    public class ForumModerator : IForumModerator
    {

        #region IBaseObject<ForumModeratorInfo> Members

        public ForumModeratorInfo GetById(int id)
        {
            ForumModeratorInfo moderator = null;
            const string sqlStr =
                "SELECT FM.MOD_ID, FM.FORUM_ID,FM.MEMBER_ID,FM.MOD_TYPE,M.M_NAME FROM " +
                "FORUM_MODERATOR FM LEFT OUTER JOIN FORUM_MEMBERS M ON FM.MEMBER_ID = M.MEMBER_ID " +
                "WHERE FM.MOD_ID=@ModId";
            OleDbParameter parm = new OleDbParameter("@ModId", OleDbType.VarChar) { Value = id };
            using (OleDbDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnString, CommandType.Text, sqlStr, parm))
            {
                while (rdr.Read())
                {
                    moderator = new ForumModeratorInfo
                    {
                        Id = rdr.GetInt32(0),
                        ForumId = rdr.GetInt32(1),
                        MemberId = rdr.GetInt32(2),
                        Type = rdr.GetInt32(3),
                        Name = rdr.SafeGetString(4)
                    };
                }
            }
            return moderator;
        }

        public IEnumerable<ForumModeratorInfo> GetByName(string name)
        {
            //not required
            return null;
        }

        public int Add(ForumModeratorInfo forumModerator)
        {
            const string sqlStr = "INSERT INTO FORUM_MODERATOR (FORUM_ID,MEMBER_ID,MOD_TYPE) VALUES (@ForumId,@MemberId,0)";
            List<OleDbParameter> parms = new List<OleDbParameter>
            {
                new OleDbParameter("@ForumId", OleDbType.Numeric) {Value = forumModerator.ForumId},
                new OleDbParameter("@MemberId", OleDbType.Numeric) {Value = forumModerator.MemberId}
            };

            return Convert.ToInt32(SqlHelper.ExecuteScalar(SqlHelper.ConnString, CommandType.Text, sqlStr, parms.ToArray()));
        }

        public void Update(ForumModeratorInfo forumModerator)
        {
            const string sqlStr = "UPDATE FORUM_MODERATOR SET FORUM_ID=@ForumId,MEMBER_ID=@MemberId WHERE MOD_ID=@ModId";
            List<OleDbParameter> parms = new List<OleDbParameter>
            {
                new OleDbParameter("@ModId", OleDbType.Numeric) {Value = forumModerator.Id},
                new OleDbParameter("@ForumId", OleDbType.Numeric) {Value = forumModerator.ForumId},
                new OleDbParameter("@MemberId", OleDbType.Numeric) {Value = forumModerator.MemberId}
            };

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnString, CommandType.Text, sqlStr, parms.ToArray());
        }

        public void Delete(ForumModeratorInfo forumModerator)
        {
            const string sqlStr = "DELETE FROM FORUM_MODERATOR WHERE MOD_ID=@Id";
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnString, CommandType.Text, sqlStr, new OleDbParameter("@Id", OleDbType.Numeric) { Value = forumModerator.Id });
        }

        public void Dispose()
        {

        }

        #endregion

        /// <summary>
        /// returns a list of All Moderators
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ForumModeratorInfo> GetAll()
        {
            List<ForumModeratorInfo> moderators = new List<ForumModeratorInfo>();
            const string sqlStr =
                "SELECT M.MEMBER_ID,M.M_NAME FROM FORUM_MEMBERS M WHERE M.M_LEVEL=2 AND M_STATUS=1";

            using (OleDbDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnString, CommandType.Text, sqlStr, null))
            {
                while (rdr.Read())
                {
                    moderators.Add(new ForumModeratorInfo { MemberId = rdr.GetInt32(0), Name = rdr.SafeGetString(1) });
                }
            }
            return moderators;
        }

        public IEnumerable<ForumModeratorInfo> GetByParent(int forumid)
        {
            List<ForumModeratorInfo> moderators = new List<ForumModeratorInfo>();
            const string sqlStr =
                "SELECT FM.MOD_ID, M.MEMBER_ID,M.M_NAME FROM FORUM_MODERATOR FM LEFT OUTER JOIN  FORUM_MEMBERS AS M ON FM.MEMBER_ID = M.MEMBER_ID WHERE M.M_LEVEL=2 AND FM.FORUM_ID=@ForumId";

            using (OleDbDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnString, CommandType.Text, sqlStr, new OleDbParameter("@ForumId", OleDbType.Numeric) { Value = forumid }))
            {
                while (rdr.Read())
                {
                    moderators.Add(new ForumModeratorInfo { MemberId = rdr.GetInt32(1), Name = rdr.SafeGetString(2), Id = rdr.GetInt32(0), ForumId = forumid });
                }
            }
            return moderators;
        }

        public List<ForumInfo> GetUnModeratedForums(int memberId)
        {
            const string strSql = "SELECT F.FORUM_ID,F.CAT_ID,F.F_STATUS,F.F_SUBJECT,F.F_URL,F.F_TOPICS" +
                                  ",F.F_COUNT,F.F_LAST_POST,F.F_PRIVATEFORUMS,F.F_TYPE,F.F_LAST_POST_AUTHOR,F.F_A_TOPICS,F.F_A_COUNT,F.F_MODERATION" +
                                  ",F.F_SUBSCRIPTION,F.F_ORDER, F.F_COUNT_M_POSTS,F.F_LAST_POST_TOPIC_ID,F.F_LAST_POST_REPLY_ID,F.F_POLLS,F.F_DESCRIPTION" +
                                  ",F.F_L_ARCHIVE,F.F_ARCHIVE_SCHED,T.T_SUBJECT,M.M_NAME " +
                                  "FROM " +
                                  "(FORUM_FORUM  F LEFT OUTER JOIN " +
                                  "FORUM_MEMBERS M ON F.F_LAST_POST_AUTHOR = M.MEMBER_ID) LEFT OUTER JOIN " +
                                  "FORUM_TOPICS T ON F.F_LAST_POST_TOPIC_ID = T.TOPIC_ID " +
                                  "WHERE F.FORUM_ID NOT IN (SELECT FORUM_ID FROM FORUM_MODERATOR WHERE MEMBER_ID=@MemberId)";

            List<ForumInfo> forums = new List<ForumInfo>();

            OleDbParameter parm = new OleDbParameter("@MemberId", OleDbType.Numeric) { Value = memberId };

            using (OleDbDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnString, CommandType.Text, strSql, parm))
            {
                while (rdr.Read())
                {
                    forums.Add(BoHelper.CopyForumToBO(rdr));
                }
            }
            return forums;
        }

        public List<ForumInfo> GetModeratedForums(int memberId)
        {
            const string strSql = "SELECT F.FORUM_ID,F.CAT_ID,F.F_STATUS,F.F_SUBJECT,F.F_URL,F.F_TOPICS" +
                                  ",F.F_COUNT,F.F_LAST_POST,F.F_PRIVATEFORUMS,F.F_TYPE,F.F_LAST_POST_AUTHOR,F.F_A_TOPICS,F.F_A_COUNT,F.F_MODERATION" +
                                  ",F.F_SUBSCRIPTION,F.F_ORDER, F.F_COUNT_M_POSTS,F.F_LAST_POST_TOPIC_ID,F.F_LAST_POST_REPLY_ID,F.F_POLLS,F.F_DESCRIPTION" +
                                  ",F.F_L_ARCHIVE,F.F_ARCHIVE_SCHED,T.T_SUBJECT,M.M_NAME " +
                                  "FROM " +
                                  "(FORUM_FORUM  F LEFT OUTER JOIN " +
                                  "FORUM_MEMBERS M ON F.F_LAST_POST_AUTHOR = M.MEMBER_ID) LEFT OUTER JOIN " +
                                  "FORUM_TOPICS T ON F.F_LAST_POST_TOPIC_ID = T.TOPIC_ID " +
                                  "WHERE F.FORUM_ID  IN (SELECT FORUM_ID FROM FORUM_MODERATOR WHERE MEMBER_ID=@MemberId)";

            List<ForumInfo> forums = new List<ForumInfo>();

            OleDbParameter parm = new OleDbParameter("@MemberId", OleDbType.Numeric) { Value = memberId };

            using (OleDbDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnString, CommandType.Text, strSql, parm))
            {
                while (rdr.Read())
                {
                    forums.Add(BoHelper.CopyForumToBO(rdr));
                }
            }
            return forums;
        }

        public bool IsUserForumModerator(int memberid, int forumid)
        {
            const string strSql = "SELECT MOD_ID FROM FORUM_MODERATOR WHERE FORUM_ID=@ForumId AND MEMBER_ID=@MemberId";
            List<OleDbParameter> parms = new List<OleDbParameter>
                                       {
                                           new OleDbParameter("@MemberId", OleDbType.Numeric)
                                           {
                                               Value
                                                   =
                                                   memberid
                                           },
                                           new OleDbParameter("@ForumId", OleDbType.Numeric)
                                           {
                                               Value
                                                   =
                                                   forumid
                                           }
                                       };

            var res = SqlHelper.ExecuteScalar(SqlHelper.ConnString, CommandType.Text, strSql, parms.ToArray());
            return res != null;
        }

        public List<ForumModeratorInfo> GetAvailableModerators(int forumId)
        {
            var forummoderators = GetByParent(forumId);
            //peopleList2.Where(p => !peopleList1.Any(p2 => p2.ID == p.ID));
            var moderators = GetAll().Where(m => !forummoderators.Any(fm => fm.MemberId == m.MemberId));
            return new List<ForumModeratorInfo>(moderators);
        }

        public void SetForumModerators(int forumId, int[] userList)
        {
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnString, CommandType.Text, "DELETE FROM FORUM_MODERATOR WHERE FORUM_ID=@ForumId", new OleDbParameter("@ForumId", OleDbType.Numeric) { Value = forumId });
            foreach (int user in userList)
            {
                ForumModeratorInfo mod = new ForumModeratorInfo { ForumId = forumId, MemberId = user };
                Add(mod);
            }
        }

        public void SetUserAsModeratorForForums(int memberId, int[] forumList)
        {
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnString, CommandType.Text, "DELETE FROM FORUM_MODERATOR WHERE MEMBER_ID=@MemberId", new OleDbParameter("@MemberId", OleDbType.Numeric) { Value = memberId });
            foreach (int forum in forumList)
            {
                ForumModeratorInfo mod = new ForumModeratorInfo { ForumId = forum, MemberId = memberId };
                Add(mod);
            }
        }
    }
}
